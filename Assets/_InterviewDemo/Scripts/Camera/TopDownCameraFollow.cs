using UnityEngine;

namespace InterviewDemo.CameraRig
{
    /// <summary>
    /// 简单俯视角相机跟随。
    /// 第二阶段只需要稳定观察玩家移动和攻击，因此这里保留固定偏移和轻微平滑，不引入 Cinemachine 依赖。
    /// </summary>
    public sealed class TopDownCameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Vector3 offset = new Vector3(0f, 10.5f, -8.5f);

        [SerializeField]
        private float followSmoothTime = 0.08f;

        [SerializeField]
        private Vector3 eulerAngles = new Vector3(52f, 0f, 0f);

        private Transform _target;
        private Vector3 _velocity;

        /// <summary>
        /// 设置跟随目标。
        /// Bootstrap 在进入战斗场景时调用，后续如果玩家重生，只需要重新传入新的玩家 Transform。
        /// </summary>
        public void Configure(Transform target)
        {
            _target = target;
            transform.rotation = Quaternion.Euler(eulerAngles);

            if (_target != null)
            {
                transform.position = _target.position + offset;
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 targetPosition = _target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, followSmoothTime);
            transform.rotation = Quaternion.Euler(eulerAngles);
        }
    }
}
