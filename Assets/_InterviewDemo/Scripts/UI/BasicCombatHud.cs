using InterviewDemo.Combat;
using InterviewDemo.Player;
using UnityEngine;
using UnityEngine.UI;

namespace InterviewDemo.UI
{
    /// <summary>
    /// 第二阶段战斗 HUD。
    /// 这个脚本用代码创建基础 UI，是为了在当前场景无需手动摆 UI 的情况下，快速得到可演示的血条、冷却和操作说明。
    /// 后续进入打磨阶段时，可以把这些控件整理为 Prefab 或 UI Toolkit 布局。
    /// </summary>
    public sealed class BasicCombatHud : MonoBehaviour
    {
        private Health _playerHealth;
        private Health _enemyHealth;
        private TopDownPlayerController _playerController;
        private Text _playerHealthText;
        private Text _enemyHealthText;
        private Text _cooldownText;
        private Text _feedbackText;
        private Image _playerHealthFill;
        private Image _enemyHealthFill;
        private Image _attackCooldownFill;
        private Image _dodgeCooldownFill;

        /// <summary>
        /// 初始化 HUD。
        /// HUD 只持有展示所需的引用，不负责创建或控制玩家、敌人，保持 UI 层职责单一。
        /// </summary>
        public void Initialize(Health playerHealth, Health enemyHealth, TopDownPlayerController playerController)
        {
            _playerHealth = playerHealth;
            _enemyHealth = enemyHealth;
            _playerController = playerController;

            EnsureCanvasSettings();
            BuildHud();
            RefreshAll();

            if (_playerHealth != null)
            {
                _playerHealth.Changed += HandlePlayerHealthChanged;
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.Changed += HandleEnemyHealthChanged;
            }
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
            {
                _playerHealth.Changed -= HandlePlayerHealthChanged;
            }

            if (_enemyHealth != null)
            {
                _enemyHealth.Changed -= HandleEnemyHealthChanged;
            }
        }

        private void Update()
        {
            if (_playerController == null)
            {
                return;
            }

            _attackCooldownFill.fillAmount = _playerController.AttackCooldownNormalized;
            _dodgeCooldownFill.fillAmount = _playerController.DodgeCooldownNormalized;
            _cooldownText.text = $"攻击冷却：{FormatReadyState(_playerController.AttackCooldownNormalized)}    闪避冷却：{FormatReadyState(_playerController.DodgeCooldownNormalized)}";
            _feedbackText.text = $"状态：{_playerController.LastFeedback}";
        }

        private void EnsureCanvasSettings()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void BuildHud()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject root = CreateRect("第二阶段战斗 HUD", transform, new Vector2(0f, 0f), new Vector2(1280f, 720f));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            _playerHealthText = CreateText(root.transform, "玩家生命文字", "玩家生命", font, 22, new Vector2(28f, -28f), new Vector2(420f, 30f), TextAnchor.MiddleLeft);
            _playerHealthFill = CreateBar(root.transform, "玩家生命条", new Vector2(28f, -66f), new Vector2(310f, 18f), new Color(0.2f, 0.85f, 0.42f));

            _enemyHealthText = CreateText(root.transform, "训练靶生命文字", "训练靶生命", font, 22, new Vector2(-28f, -28f), new Vector2(420f, 30f), TextAnchor.MiddleRight);
            _enemyHealthText.rectTransform.anchorMin = new Vector2(1f, 1f);
            _enemyHealthText.rectTransform.anchorMax = new Vector2(1f, 1f);
            _enemyHealthText.rectTransform.pivot = new Vector2(1f, 1f);
            _enemyHealthFill = CreateBar(root.transform, "训练靶生命条", new Vector2(-338f, -66f), new Vector2(310f, 18f), new Color(0.95f, 0.28f, 0.22f));
            RectTransform enemyBarRect = _enemyHealthFill.transform.parent.GetComponent<RectTransform>();
            enemyBarRect.anchorMin = new Vector2(1f, 1f);
            enemyBarRect.anchorMax = new Vector2(1f, 1f);
            enemyBarRect.pivot = new Vector2(0f, 1f);

            _cooldownText = CreateText(root.transform, "冷却文字", "攻击冷却：就绪    闪避冷却：就绪", font, 20, new Vector2(0f, 54f), new Vector2(620f, 32f), TextAnchor.MiddleCenter);
            _cooldownText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            _cooldownText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            _cooldownText.rectTransform.pivot = new Vector2(0.5f, 0f);

            _attackCooldownFill = CreateBar(root.transform, "攻击冷却遮罩", new Vector2(-150f, 28f), new Vector2(180f, 12f), new Color(1f, 0.78f, 0.25f));
            _dodgeCooldownFill = CreateBar(root.transform, "闪避冷却遮罩", new Vector2(150f, 28f), new Vector2(180f, 12f), new Color(0.35f, 0.72f, 1f));
            SetBottomCenter(_attackCooldownFill.transform.parent.GetComponent<RectTransform>());
            SetBottomCenter(_dodgeCooldownFill.transform.parent.GetComponent<RectTransform>());

            _feedbackText = CreateText(root.transform, "战斗反馈文字", "状态：准备战斗", font, 22, new Vector2(0f, 92f), new Vector2(760f, 32f), TextAnchor.MiddleCenter);
            _feedbackText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            _feedbackText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            _feedbackText.rectTransform.pivot = new Vector2(0.5f, 0f);

            Text inputHelpText = CreateText(root.transform, "操作说明文字", "WASD/方向键移动    鼠标控制朝向    左键/J 普通攻击    空格/右键 闪避", font, 20, new Vector2(0f, 18f), new Vector2(900f, 32f), TextAnchor.MiddleCenter);
            SetBottomCenter(inputHelpText.rectTransform);
        }

        private void HandlePlayerHealthChanged(float current, float max)
        {
            RefreshHealth(_playerHealthText, _playerHealthFill, "玩家生命", current, max);
        }

        private void HandleEnemyHealthChanged(float current, float max)
        {
            RefreshHealth(_enemyHealthText, _enemyHealthFill, "训练靶生命", current, max);
        }

        private void RefreshAll()
        {
            if (_playerHealth != null)
            {
                HandlePlayerHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            }

            if (_enemyHealth != null)
            {
                HandleEnemyHealthChanged(_enemyHealth.CurrentHealth, _enemyHealth.MaxHealth);
            }
        }

        private static void RefreshHealth(Text label, Image fill, string title, float current, float max)
        {
            float ratio = max <= 0f ? 0f : Mathf.Clamp01(current / max);
            label.text = $"{title}：{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            fill.fillAmount = ratio;
        }

        private static string FormatReadyState(float normalizedCooldown)
        {
            return normalizedCooldown <= 0.01f ? "就绪" : $"{Mathf.CeilToInt(normalizedCooldown * 100f)}%";
        }

        private static Image CreateBar(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color fillColor)
        {
            GameObject background = CreateRect($"{name}背景", parent, anchoredPosition, size);
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0f, 0f, 0f, 0.55f);

            GameObject fill = CreateRect(name, background.transform, Vector2.zero, size);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 1f;

            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            return fillImage;
        }

        private static Text CreateText(Transform parent, string name, string content, Font font, int fontSize, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment)
        {
            GameObject textObject = CreateRect(name, parent, anchoredPosition, size);
            Text text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = font;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);

            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            return gameObject;
        }

        private static void SetBottomCenter(RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
        }
    }
}
