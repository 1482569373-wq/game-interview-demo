using System;
using UnityEngine;

namespace InterviewDemo.Combat
{
    /// <summary>
    /// 通用生命值组件。
    /// 玩家、敌人、训练靶都可以复用它，避免把“扣血、死亡、满血恢复”这些规则散落在不同脚本里。
    /// 第二阶段先把它做成 MonoBehaviour，是为了方便直接挂到场景对象上；后续可以把纯规则继续拆成普通 C# 类做 EditMode 测试。
    /// </summary>
    public sealed class Health : MonoBehaviour
    {
        /// <summary>
        /// 当前生命值变化事件。
        /// 参数分别是当前生命值和最大生命值，UI 只需要订阅这个事件，不需要知道伤害来自哪里。
        /// </summary>
        public event Action<float, float> Changed;

        /// <summary>
        /// 受到有效伤害事件。
        /// 参数是本次实际扣除的生命值，受击闪烁、音效、飘字都可以从这里扩展。
        /// </summary>
        public event Action<float> Damaged;

        /// <summary>
        /// 死亡事件。
        /// 这里传回自身组件，方便波次管理器未来统计哪个目标死亡。
        /// </summary>
        public event Action<Health> Died;

        [SerializeField]
        private float maxHealth = 100f;

        [SerializeField]
        private float currentHealth = 100f;

        private float _invulnerableUntilTime;
        private bool _hasDied;

        /// <summary>
        /// 最大生命值。
        /// 使用属性暴露只读值，避免外部脚本绕过 Configure 直接改坏状态。
        /// </summary>
        public float MaxHealth => maxHealth;

        /// <summary>
        /// 当前生命值。
        /// UI 和调试脚本读取这个值即可，不应该直接写入。
        /// </summary>
        public float CurrentHealth => currentHealth;

        /// <summary>
        /// 是否已经死亡。
        /// 死亡后会拒绝继续扣血，防止同一帧多个命中重复触发死亡流程。
        /// </summary>
        public bool IsDead => _hasDied;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        /// <summary>
        /// 初始化生命值。
        /// Bootstrap 在运行时装配玩家和敌人时调用它，确保每次进入场景都有稳定的初始状态。
        /// </summary>
        public void Configure(float configuredMaxHealth)
        {
            maxHealth = Mathf.Max(1f, configuredMaxHealth);
            RestoreFull();
        }

        /// <summary>
        /// 施加伤害。
        /// 返回 true 表示本次攻击确实造成了扣血，攻击者可以据此播放命中特效或更新反馈文案。
        /// </summary>
        public bool ApplyDamage(float damage)
        {
            if (_hasDied || damage <= 0f || Time.time < _invulnerableUntilTime)
            {
                return false;
            }

            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            float appliedDamage = previousHealth - currentHealth;

            if (appliedDamage <= 0f)
            {
                return false;
            }

            Damaged?.Invoke(appliedDamage);
            Changed?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                _hasDied = true;
                Died?.Invoke(this);
            }

            return true;
        }

        /// <summary>
        /// 设置短暂无敌。
        /// 玩家闪避时会调用它，让闪避动作既有位移反馈，也有明确的防御收益。
        /// </summary>
        public void SetBriefInvulnerable(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            _invulnerableUntilTime = Mathf.Max(_invulnerableUntilTime, Time.time + duration);
        }

        /// <summary>
        /// 恢复满血并重置死亡状态。
        /// 第二阶段训练靶死亡后会自动复活，方便连续测试攻击手感。
        /// </summary>
        public void RestoreFull()
        {
            _hasDied = false;
            currentHealth = maxHealth;
            _invulnerableUntilTime = 0f;
            Changed?.Invoke(currentHealth, maxHealth);
        }
    }
}
