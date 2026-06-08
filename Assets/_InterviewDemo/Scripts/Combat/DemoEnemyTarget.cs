using System.Collections;
using UnityEngine;

namespace InterviewDemo.Combat
{
    /// <summary>
    /// 第二阶段使用的训练靶目标。
    /// 它不是正式敌人 AI，只负责展示“能被攻击、会扣血、死亡后会复活”的基础战斗反馈。
    /// 第三阶段接入敌人状态机后，可以把它替换为真正的 EnemyController。
    /// </summary>
    public sealed class DemoEnemyTarget : MonoBehaviour
    {
        [SerializeField]
        private float respawnDelay = 1.6f;

        private Health _health;
        private Renderer _renderer;
        private Color _aliveColor = new Color(0.9f, 0.2f, 0.18f);
        private Color _hitColor = new Color(1f, 0.85f, 0.2f);
        private Color _deadColor = new Color(0.16f, 0.16f, 0.16f);
        private float _hitFlashUntilTime;
        private Coroutine _respawnRoutine;

        /// <summary>
        /// 当前训练靶生命组件。
        /// HUD 通过这个属性读取敌人血量，不需要直接查找场景对象。
        /// </summary>
        public Health Health => _health;

        /// <summary>
        /// 运行时装配入口。
        /// Bootstrap 会把已经创建好的 Health 传进来，保证训练靶和 HUD 指向同一个生命状态。
        /// </summary>
        public void Configure(Health health)
        {
            _health = health;
            _renderer = GetComponentInChildren<Renderer>();

            if (_health == null)
            {
                Debug.LogWarning("训练靶缺少 Health 组件，无法展示受击和死亡反馈。");
                return;
            }

            _health.Damaged += HandleDamaged;
            _health.Died += HandleDied;
            ApplyColor(_aliveColor);
        }

        private void OnDestroy()
        {
            if (_health == null)
            {
                return;
            }

            _health.Damaged -= HandleDamaged;
            _health.Died -= HandleDied;
        }

        private void Update()
        {
            if (_health == null || _health.IsDead)
            {
                return;
            }

            if (Time.time >= _hitFlashUntilTime)
            {
                ApplyColor(_aliveColor);
            }
        }

        private void HandleDamaged(float damage)
        {
            _hitFlashUntilTime = Time.time + 0.12f;
            ApplyColor(_hitColor);
        }

        private void HandleDied(Health health)
        {
            ApplyColor(_deadColor);

            if (_respawnRoutine != null)
            {
                StopCoroutine(_respawnRoutine);
            }

            _respawnRoutine = StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);

            if (_health != null)
            {
                _health.RestoreFull();
            }

            ApplyColor(_aliveColor);
            _respawnRoutine = null;
        }

        private void ApplyColor(Color color)
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.material.color = color;
        }
    }
}
