using InterviewDemo.Combat;
using UnityEngine;

namespace InterviewDemo.Player
{
    /// <summary>
    /// 第二阶段玩家控制器。
    /// 这个脚本负责把键鼠输入转换成俯视角角色移动、鼠标朝向、普通攻击和闪避。
    /// 目前使用最小输入封装读取键鼠，是为了快速得到稳定可演示手感；项目已开启 Both 输入后端，后续可以平滑迁移到 Input Actions。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class TopDownPlayerController : MonoBehaviour
    {
        private const float GroundedVerticalVelocity = -1f;
        private const int HitBufferSize = 16;

        [SerializeField]
        private float moveSpeed = 5.8f;

        [SerializeField]
        private float rotateSpeed = 18f;

        [SerializeField]
        private float attackDamage = 25f;

        [SerializeField]
        private float attackRadius = 1.15f;

        [SerializeField]
        private float attackForwardOffset = 1.25f;

        [SerializeField]
        private float attackCooldown = 0.42f;

        [SerializeField]
        private float dodgeSpeed = 13f;

        [SerializeField]
        private float dodgeDuration = 0.16f;

        [SerializeField]
        private float dodgeCooldown = 0.75f;

        [SerializeField]
        private float dodgeInvulnerableDuration = 0.22f;

        [SerializeField]
        private float gravity = -22f;

        private readonly Collider[] _hitBuffer = new Collider[HitBufferSize];

        private CharacterController _characterController;
        private Camera _mainCamera;
        private Health _health;
        private Vector3 _moveDirection;
        private Vector3 _dodgeDirection;
        private float _verticalVelocity;
        private float _attackCooldownRemaining;
        private float _dodgeCooldownRemaining;
        private float _dodgeTimeRemaining;
        private string _lastFeedback = "准备战斗";

        /// <summary>
        /// 攻击冷却剩余比例。
        /// HUD 只关心展示进度，不需要知道内部冷却秒数如何递减。
        /// </summary>
        public float AttackCooldownNormalized => attackCooldown <= 0f ? 0f : Mathf.Clamp01(_attackCooldownRemaining / attackCooldown);

        /// <summary>
        /// 闪避冷却剩余比例。
        /// 数值为 0 表示闪避已经可用。
        /// </summary>
        public float DodgeCooldownNormalized => dodgeCooldown <= 0f ? 0f : Mathf.Clamp01(_dodgeCooldownRemaining / dodgeCooldown);

        /// <summary>
        /// HUD 展示用的短反馈文案。
        /// 这里刻意保持为只读字符串，避免 UI 反向影响玩家逻辑。
        /// </summary>
        public string LastFeedback => _lastFeedback;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _health = GetComponent<Health>();
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// 由 Bootstrap 注入必要引用。
        /// 这样玩家控制器不需要在 Update 里反复 Find，也方便未来测试时传入替代相机或生命组件。
        /// </summary>
        public void Configure(Camera gameplayCamera, Health health)
        {
            _mainCamera = gameplayCamera != null ? gameplayCamera : Camera.main;
            _health = health != null ? health : GetComponent<Health>();
        }

        private void Update()
        {
            if (_health != null && _health.IsDead)
            {
                _lastFeedback = "玩家已经倒下";
                return;
            }

            TickCooldowns();
            ReadMovementInput();
            FaceMousePosition();
            TryStartAttack();
            TryStartDodge();
            MoveCharacter();
        }

        /// <summary>
        /// 读取移动输入。
        /// 同时支持 WASD 和方向键，确保不同键盘习惯下都能立即操作。
        /// </summary>
        private void ReadMovementInput()
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                horizontal -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                horizontal += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                vertical -= 1f;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                vertical += 1f;
            }

            _moveDirection = new Vector3(horizontal, 0f, vertical);
            if (_moveDirection.sqrMagnitude > 1f)
            {
                _moveDirection.Normalize();
            }
        }

        /// <summary>
        /// 根据鼠标位置旋转角色。
        /// 使用水平面射线，而不是读取屏幕坐标差值，可以让角色始终准确朝向鼠标所在的世界位置。
        /// </summary>
        private void FaceMousePosition()
        {
            if (_mainCamera == null)
            {
                return;
            }

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (!groundPlane.Raycast(ray, out float distance))
            {
                return;
            }

            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 lookDirection = targetPoint - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        private void TryStartAttack()
        {
            if (_attackCooldownRemaining > 0f)
            {
                return;
            }

            if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.J))
            {
                return;
            }

            _attackCooldownRemaining = attackCooldown;
            int hitCount = ResolveAttackHit();
            _lastFeedback = hitCount > 0 ? $"普通攻击命中 {hitCount} 个目标" : "普通攻击挥空";
            SpawnAttackPreview(hitCount > 0);
        }

        /// <summary>
        /// 处理普通攻击命中。
        /// 这里使用 NonAlloc 版本的范围检测，避免攻击频繁触发时产生短生命周期 GC 分配。
        /// </summary>
        private int ResolveAttackHit()
        {
            Vector3 center = transform.position + transform.forward * attackForwardOffset + Vector3.up * 0.7f;
            int colliderCount = Physics.OverlapSphereNonAlloc(center, attackRadius, _hitBuffer);
            int hitCount = 0;

            for (int i = 0; i < colliderCount; i++)
            {
                Collider candidate = _hitBuffer[i];
                if (candidate == null || candidate.transform.IsChildOf(transform))
                {
                    continue;
                }

                Health targetHealth = candidate.GetComponentInParent<Health>();
                if (targetHealth == null || targetHealth == _health)
                {
                    continue;
                }

                if (targetHealth.ApplyDamage(attackDamage))
                {
                    hitCount++;
                }
            }

            return hitCount;
        }

        private void TryStartDodge()
        {
            if (_dodgeCooldownRemaining > 0f || _dodgeTimeRemaining > 0f)
            {
                return;
            }

            if (!Input.GetKeyDown(KeyCode.Space) && !Input.GetMouseButtonDown(1))
            {
                return;
            }

            _dodgeDirection = _moveDirection.sqrMagnitude > 0.001f ? _moveDirection.normalized : transform.forward;
            _dodgeTimeRemaining = dodgeDuration;
            _dodgeCooldownRemaining = dodgeCooldown;
            _health?.SetBriefInvulnerable(dodgeInvulnerableDuration);
            _lastFeedback = "闪避";
        }

        private void MoveCharacter()
        {
            Vector3 velocity = _moveDirection * moveSpeed;

            if (_dodgeTimeRemaining > 0f)
            {
                velocity += _dodgeDirection * dodgeSpeed;
                _dodgeTimeRemaining = Mathf.Max(0f, _dodgeTimeRemaining - Time.deltaTime);
            }

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = GroundedVerticalVelocity;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }

            velocity.y = _verticalVelocity;
            _characterController.Move(velocity * Time.deltaTime);
        }

        private void TickCooldowns()
        {
            _attackCooldownRemaining = Mathf.Max(0f, _attackCooldownRemaining - Time.deltaTime);
            _dodgeCooldownRemaining = Mathf.Max(0f, _dodgeCooldownRemaining - Time.deltaTime);
        }

        /// <summary>
        /// 生成一次很短的攻击范围预览。
        /// 这是第二阶段的阶段性表现层反馈，不参与命中判定；之后可以替换为正式特效或对象池。
        /// </summary>
        private void SpawnAttackPreview(bool hasHit)
        {
            GameObject preview = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            preview.name = hasHit ? "普通攻击命中特效" : "普通攻击范围提示";
            preview.transform.position = transform.position + transform.forward * attackForwardOffset + Vector3.up * 0.25f;
            preview.transform.localScale = Vector3.one * attackRadius * 1.35f;

            Collider previewCollider = preview.GetComponent<Collider>();
            if (previewCollider != null)
            {
                previewCollider.enabled = false;
            }

            Renderer renderer = preview.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = hasHit ? new Color(1f, 0.75f, 0.18f, 0.55f) : new Color(0.75f, 0.75f, 0.75f, 0.35f);
            }

            Destroy(preview, 0.12f);
        }
    }
}
