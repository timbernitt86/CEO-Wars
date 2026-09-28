using System;
using CEOWars.Game;
using UnityEngine;
using UnityEngine.UI;

namespace CEOWars.UI
{
    public class GameUI : MonoBehaviour
    {
        private GameController game;
        private Canvas canvas;
        private Font font;

        private Text companyText;
        private Text cashText;
        private Text gemsText;
        private Text levelText;
        private Text revenueText;
        private Text employeesText;
        private Text productsText;
        private Text officeText;
        private Text boostText;
        private Text toastText;
        private Text xpText;
        private Image xpFill;

        private Button hireButton;
        private Text hireButtonText;
        private Button productButton;
        private Text productButtonText;
        private Button officeButton;
        private Text officeButtonText;

        private GameObject onboardingPanel;
        private InputField companyInput;
        private float toastUntil;

        private readonly Color bg = new Color(0.045f, 0.055f, 0.075f, 0.94f);
        private readonly Color panel = new Color(0.09f, 0.105f, 0.14f, 0.95f);
        private readonly Color panel2 = new Color(0.13f, 0.15f, 0.19f, 0.96f);
        private readonly Color text = new Color(0.96f, 0.97f, 0.99f);
        private readonly Color muted = new Color(0.64f, 0.69f, 0.77f);
        private readonly Color accent = new Color(0.88f, 0.18f, 0.12f);
        private readonly Color green = new Color(0.20f, 0.72f, 0.44f);
        private readonly Color gold = new Color(1.0f, 0.74f, 0.20f);

        private void Start()
        {
            game = GameController.Instance;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            BuildCanvas();
            BuildTopBar();
            BuildLeftStats();
            BuildBottomActions();
            BuildPremiumPanel();
            BuildToast();
            BuildOnboarding();

            game.StateChanged += Refresh;
            game.ToastRequested += ShowToast;
            Refresh();

            onboardingPanel.SetActive(!game.State.onboardingComplete);
        }

        private void Update()
        {
            if (toastText != null && toastText.gameObject.activeSelf && Time.unscaledTime > toastUntil)
                toastText.gameObject.SetActive(false);

            if (boostText != null && game != null)
            {
                if (game.State.HasActiveBoost)
                {
                    var left = new DateTime(game.State.boostEndsUtcTicks, DateTimeKind.Utc) - DateTime.UtcNow;
                    boostText.text = $"EXECUTIVE BOOST  x2   {Math.Max(0, (int)left.TotalMinutes):00}:{Math.Max(0, left.Seconds):00}";
                    boostText.gameObject.SetActive(true);
                }
                else boostText.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (game == null) return;
            game.StateChanged -= Refresh;
            game.ToastRequested -= ShowToast;
        }

        private void BuildCanvas()
        {
            var root = new GameObject("Canvas");
            root.transform.SetParent(transform);
            canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
        }

        private void BuildTopBar()
        {
            var bar = Panel("TopBar", canvas.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, -150), bg);
            companyText = Label("Company", bar.transform, "CEO WARS", 34, FontStyle.Bold, TextAnchor.MiddleLeft, text,
                new Vector2(0.03f, 0.48f), new Vector2(0.45f, 0.96f));
            levelText = Label("Level", bar.transform, "CEO LVL 1", 21, FontStyle.Bold, TextAnchor.MiddleLeft, muted,
                new Vector2(0.03f, 0.12f), new Vector2(0.22f, 0.45f));
            cashText = Label("Cash", bar.transform, "$25K", 31, FontStyle.Bold, TextAnchor.MiddleRight, green,
                new Vector2(0.48f, 0.48f), new Vector2(0.76f, 0.96f));
            gemsText = Label("Gems", bar.transform, "◆ 120", 29, FontStyle.Bold, TextAnchor.MiddleRight, gold,
                new Vector2(0.77f, 0.48f), new Vector2(0.97f, 0.96f));

            var xpBack = Panel("XpBack", bar.transform, new Vector2(0.25f, 0.14f), new Vector2(0.97f, 0.31f), Vector2.zero, Vector2.zero, panel2);
            var xp = Panel("XpFill", xpBack.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, accent);
            xpFill = xp.GetComponent<Image>();
            xpFill.type = Image.Type.Filled;
            xpFill.fillMethod = Image.FillMethod.Horizontal;
            xpFill.fillAmount = 0.2f;
            xpText = Label("XpText", xpBack.transform, "CEO XP", 15, FontStyle.Bold, TextAnchor.MiddleCenter, text, Vector2.zero, Vector2.one);
        }

        private void BuildLeftStats()
        {
            var stats = Panel("Stats", canvas.transform, new Vector2(0.025f, 0.52f), new Vector2(0.40f, 0.88f), Vector2.zero, Vector2.zero, panel);
            Label("Header", stats.transform, "YOUR COMPANY", 24, FontStyle.Bold, TextAnchor.MiddleLeft, text, new Vector2(0.07f, 0.82f), new Vector2(0.93f, 0.97f));

            revenueText = Stat(stats.transform, "Revenue", "Revenue / sec", "$0", 0.62f);
            employeesText = Stat(stats.transform, "Employees", "Employees", "1", 0.43f);
            productsText = Stat(stats.transform, "Products", "Products", "0", 0.24f);
            officeText = Stat(stats.transform, "Office", "Office level", "1", 0.05f);
        }

        private Text Stat(Transform parent, string name, string label, string value, float y)
        {
            Label(name + "Label", parent, label.ToUpperInvariant(), 16, FontStyle.Normal, TextAnchor.MiddleLeft, muted,
                new Vector2(0.07f, y + 0.09f), new Vector2(0.93f, y + 0.18f));
            return Label(name + "Value", parent, value, 28, FontStyle.Bold, TextAnchor.MiddleLeft, text,
                new Vector2(0.07f, y), new Vector2(0.93f, y + 0.10f));
        }

        private void BuildBottomActions()
        {
            var actions = Panel("Actions", canvas.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 305), bg);
            Label("ActionsTitle", actions.transform, "BUILD YOUR EMPIRE", 20, FontStyle.Bold, TextAnchor.MiddleLeft, muted,
                new Vector2(0.035f, 0.78f), new Vector2(0.5f, 0.96f));

            hireButton = ActionButton(actions.transform, "Hire", "HIRE", "Employee", new Vector2(0.025f, 0.08f), new Vector2(0.325f, 0.75f), () => game.HireEmployee(), out hireButtonText);
            productButton = ActionButton(actions.transform, "Product", "LAUNCH", "Product", new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.75f), () => game.DevelopProduct(), out productButtonText);
            officeButton = ActionButton(actions.transform, "Office", "UPGRADE", "Office", new Vector2(0.675f, 0.08f), new Vector2(0.975f, 0.75f), () => game.UpgradeOffice(), out officeButtonText);
        }

        private Button ActionButton(Transform parent, string name, string title, string subtitle, Vector2 min, Vector2 max, Action onClick, out Text detailText)
        {
            var btn = Button(name, parent, min, max, panel2, onClick);
            Label("Title", btn.transform, title, 24, FontStyle.Bold, TextAnchor.UpperCenter, text, new Vector2(0.05f, 0.54f), new Vector2(0.95f, 0.92f));
            Label("Subtitle", btn.transform, subtitle, 17, FontStyle.Normal, TextAnchor.MiddleCenter, muted, new Vector2(0.05f, 0.36f), new Vector2(0.95f, 0.63f));
            detailText = Label("Price", btn.transform, "$0", 19, FontStyle.Bold, TextAnchor.LowerCenter, gold, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.38f));
            return btn;
        }

        private void BuildPremiumPanel()
        {
            var premium = Panel("Premium", canvas.transform, new Vector2(0.58f, 0.52f), new Vector2(0.975f, 0.88f), Vector2.zero, Vector2.zero, panel);
            Label("Header", premium.transform, "BOARDROOM", 24, FontStyle.Bold, TextAnchor.MiddleLeft, text, new Vector2(0.07f, 0.80f), new Vector2(0.93f, 0.96f));
            Label("Sub", premium.transform, "Use gems to accelerate growth.", 16, FontStyle.Normal, TextAnchor.UpperLeft, muted, new Vector2(0.07f, 0.67f), new Vector2(0.93f, 0.82f));

            var boost = Button("Boost", premium.transform, new Vector2(0.07f, 0.38f), new Vector2(0.93f, 0.64f), accent, () => game.BuyExecutiveBoost());
            Label("Text", boost.transform, "EXECUTIVE BOOST\nx2 revenue · 10 min · ◆20", 18, FontStyle.Bold, TextAnchor.MiddleCenter, text, Vector2.zero, Vector2.one);

            var cash = Button("InstantCash", premium.transform, new Vector2(0.07f, 0.08f), new Vector2(0.93f, 0.34f), panel2, () => game.BuyInstantCash());
            Label("Text", cash.transform, "INVESTOR CASH\n1h revenue · ◆50", 18, FontStyle.Bold, TextAnchor.MiddleCenter, text, Vector2.zero, Vector2.one);

            boostText = Label("BoostActive", canvas.transform, "EXECUTIVE BOOST x2", 18, FontStyle.Bold, TextAnchor.MiddleCenter, text,
                new Vector2(0.31f, 0.91f), new Vector2(0.69f, 0.95f));
            boostText.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            boostText.gameObject.SetActive(false);
            var boostBg = boostText.gameObject.AddComponent<Outline>();
            boostBg.effectColor = accent;
            boostBg.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private void BuildToast()
        {
            toastText = Label("Toast", canvas.transform, "", 21, FontStyle.Bold, TextAnchor.MiddleCenter, text,
                new Vector2(0.14f, 0.16f), new Vector2(0.86f, 0.22f));
            var image = toastText.gameObject.AddComponent<Image>();
            image.color = new Color(0.02f, 0.025f, 0.035f, 0.92f);
            image.raycastTarget = false;
            toastText.transform.SetAsLastSibling();
            toastText.gameObject.SetActive(false);
        }

        private void BuildOnboarding()
        {
            onboardingPanel = Panel("Onboarding", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.025f, 0.03f, 0.045f, 0.985f));
            onboardingPanel.transform.SetAsLastSibling();

            Label("Logo", onboardingPanel.transform, "CEO WARS", 58, FontStyle.Bold, TextAnchor.MiddleCenter, text,
                new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.83f));
            Label("Tagline", onboardingPanel.transform, "BUILD. SCALE. DOMINATE.", 24, FontStyle.Bold, TextAnchor.MiddleCenter, accent,
                new Vector2(0.10f, 0.64f), new Vector2(0.90f, 0.70f));
            Label("Prompt", onboardingPanel.transform, "Name your first company", 21, FontStyle.Normal, TextAnchor.MiddleCenter, muted,
                new Vector2(0.12f, 0.53f), new Vector2(0.88f, 0.60f));

            var inputGo = Panel("CompanyInput", onboardingPanel.transform, new Vector2(0.12f, 0.43f), new Vector2(0.88f, 0.51f), Vector2.zero, Vector2.zero, panel2);
            companyInput = inputGo.AddComponent<InputField>();
            companyInput.characterLimit = 24;
            var inputText = Label("InputText", inputGo.transform, "", 25, FontStyle.Bold, TextAnchor.MiddleLeft, text,
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.92f));
            var placeholder = Label("Placeholder", inputGo.transform, "e.g. Nova Dynamics", 25, FontStyle.Normal, TextAnchor.MiddleLeft, muted,
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.92f));
            companyInput.textComponent = inputText;
            companyInput.placeholder = placeholder;

            var start = Button("Start", onboardingPanel.transform, new Vector2(0.18f, 0.31f), new Vector2(0.82f, 0.39f), accent, StartCompany);
            Label("Text", start.transform, "FOUND COMPANY", 24, FontStyle.Bold, TextAnchor.MiddleCenter, text, Vector2.zero, Vector2.one);

            Label("Seed", onboardingPanel.transform, "Starting capital: $25,000  ·  1 employee  ·  ◆120 gems", 17, FontStyle.Normal, TextAnchor.MiddleCenter, muted,
                new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.28f));
        }

        private void StartCompany()
        {
            game.SetCompanyName(companyInput.text);
            onboardingPanel.SetActive(false);
            ShowToast("Company founded. Now build your empire.");
        }

        private void Refresh()
        {
            if (game == null || companyText == null) return;
            var s = game.State;
            companyText.text = s.companyName.ToUpperInvariant();
            cashText.text = GameController.Money(s.cash);
            gemsText.text = $"◆ {s.gems}";
            levelText.text = $"CEO LVL {s.level}";
            revenueText.text = GameController.Money(s.RevenuePerSecond) + "/s";
            employeesText.text = s.employees.ToString();
            productsText.text = s.products.ToString();
            officeText.text = s.officeLevel.ToString();
            hireButtonText.text = GameController.Money(s.HireCost);
            productButtonText.text = GameController.Money(s.ProductCost);
            officeButtonText.text = GameController.Money(s.OfficeUpgradeCost);
            xpFill.fillAmount = (float)Mathf.Clamp01((float)(s.xp / Math.Max(1d, s.XpForNextLevel)));
            xpText.text = $"CEO XP  {Math.Floor(s.xp):0} / {Math.Floor(s.XpForNextLevel):0}";
            SetAffordable(hireButton, s.cash >= s.HireCost);
            SetAffordable(productButton, s.cash >= s.ProductCost);
            SetAffordable(officeButton, s.cash >= s.OfficeUpgradeCost);
        }

        private void SetAffordable(Button button, bool affordable)
        {
            if (button == null) return;
            var colors = button.colors;
            colors.normalColor = affordable ? panel2 : new Color(0.09f, 0.095f, 0.11f);
            colors.highlightedColor = affordable ? new Color(0.18f, 0.20f, 0.25f) : new Color(0.11f, 0.11f, 0.12f);
            button.colors = colors;
        }

        private void ShowToast(string message)
        {
            if (toastText == null) return;
            toastText.text = message;
            toastText.gameObject.SetActive(true);
            toastText.transform.SetAsLastSibling();
            toastUntil = Time.unscaledTime + 3.2f;
        }

        private GameObject Panel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private Text Label(string name, Transform parent, string value, int size, FontStyle style, TextAnchor anchor, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var label = go.GetComponent<Text>();
            label.font = font;
            label.text = value;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = anchor;
            label.color = color;
            label.raycastTarget = false;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = Mathf.Max(11, size - 8);
            label.resizeTextMaxSize = size;
            return label;
        }

        private Button Button(string name, Transform parent, Vector2 min, Vector2 max, Color color, Action onClick)
        {
            var go = Panel(name, parent, min, max, Vector2.zero, Vector2.zero, color);
            var button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            if (onClick != null) button.onClick.AddListener(() => onClick());
            return button;
        }
    }
}
