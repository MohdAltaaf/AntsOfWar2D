using System.Text;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// Runtime-built anthill information panel. Opening the panel pauses simulation time.
public class AnthillMenu : MonoBehaviour
{
    Canvas canvas;
    GameObject panel;
    TMP_Text title;
    TMP_Text details;
    Button actionButton;
    Button scoutButton;
    Slider countSlider;
    TMP_Text actionLabel;
    bool previousPaused;

    void Awake()
    {
        GameEvents.HillClicked += Open;
        BuildUi();
        panel.SetActive(false);
    }

    void Update()
    {
        if (!panel.activeSelf) return;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Close();
#else
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();
#endif
    }

    void OnDestroy()
    {
        GameEvents.HillClicked -= Open;
        if (panel != null && panel.activeSelf)
            GameClock.Paused = previousPaused;
    }

    void BuildUi()
    {
        EnsureEventSystem();

        var canvasObject = new GameObject("AnthillMenuCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(620f, 620f);
        panelRect.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.055f, 0.07f, 0.1f, 0.97f);

        var header = CreateText("Header", panel.transform, 34f, TextAlignmentOptions.Center);
        SetRect(header.rectTransform, new Vector2(0f, -35f), new Vector2(540f, 55f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        title = header;

        var body = CreateText("Details", panel.transform, 24f, TextAlignmentOptions.TopLeft);
        body.rectTransform.anchorMin = Vector2.zero;
        body.rectTransform.anchorMax = Vector2.one;
        body.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        body.rectTransform.offsetMin = new Vector2(40f, 150f);
        body.rectTransform.offsetMax = new Vector2(-40f, -80f);
        body.fontSize = 18f;
        body.enableAutoSizing = true;
        body.fontSizeMin = 12f;
        body.fontSizeMax = 18f;
        body.verticalAlignment = VerticalAlignmentOptions.Top;
        body.textWrappingMode = TextWrappingModes.Normal;
        body.overflowMode = TextOverflowModes.Truncate;
        details = body;

        actionButton = CreateButton("ActionButton", panel.transform, "Action unavailable");
        SetRect(actionButton.GetComponent<RectTransform>(), new Vector2(-170f, 48f),
            new Vector2(240f, 54f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        actionLabel = actionButton.GetComponentInChildren<TMP_Text>();

        scoutButton = CreateButton("ScoutButton", panel.transform, "Send Scout");
        SetRect(scoutButton.GetComponent<RectTransform>(), new Vector2(-430f, 48f),
            new Vector2(240f, 54f), new Vector2(1f, 0f), new Vector2(1f, 0f));

        countSlider = CreateSlider(panel.transform);
        SetRect(countSlider.GetComponent<RectTransform>(), new Vector2(0f, 115f),
            new Vector2(500f, 30f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        countSlider.gameObject.SetActive(false);

        var closeButton = CreateButton("CloseButton", panel.transform, "Close");
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(-60f, 48f),
            new Vector2(100f, 54f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        closeButton.onClick.AddListener(Close);
    }

    void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }

    void Open(Anthill hill)
    {
        if (hill == null) return;

        previousPaused = GameClock.Paused;
        GameClock.Paused = true;
        panel.SetActive(true);

        title.text = hill.ownedByPlayer ? "Your Anthill" : "Scouted Anthill";
        title.color = hill.species.tint;

        var text = new StringBuilder();
        text.AppendLine(hill.label);
        text.AppendLine($"Owner: {(hill.ownedByPlayer ? "Player" : "NPC")}");
        text.AppendLine($"Species: {hill.species.displayName}");
        text.AppendLine();
        text.AppendLine($"Total ants: {hill.total}");
        text.AppendLine($"Available: {hill.Available}");
        text.AppendLine($"Away: {hill.away}");
        text.AppendLine($"Injured: {hill.Injured}");
        text.AppendLine($"Capacity: {Mathf.FloorToInt(hill.maxCapacity)}");
        text.AppendLine();
        text.AppendLine(hill.buff == null
            ? "Buff: None"
            : $"Buff: {hill.buff.displayName}\n{hill.buff.description}");
        text.AppendLine();
        text.AppendLine("Diplomacy");
        text.AppendLine($"Allies: {FormatSpecies(hill.species.allies)}");
        text.AppendLine($"Hostiles: {FormatSpecies(hill.species.hostiles)}");
        details.text = text.ToString();

        actionButton.onClick.RemoveAllListeners();
        scoutButton.onClick.RemoveAllListeners();
        var scoutController = GameBootstrap.I.GetComponent<ScoutController>();
        var npcThreat = GameBootstrap.I.GetComponent<NpcThreatController>();
        bool canSendScout = hill.ownedByPlayer && scoutController != null &&
            !scoutController.HasActiveScout && !scoutController.HasActiveArmy;
        bool canPrepareArmy = hill.ownedByPlayer && scoutController != null &&
            scoutController.CanPrepareArmyFromHome(hill);
        bool canReinforce = hill.ownedByPlayer && npcThreat != null &&
            npcThreat.IsUnderAttack(hill);
        scoutButton.GetComponentInChildren<TMP_Text>().text = "Send Scout";
        scoutButton.interactable = canSendScout;
        if (canSendScout)
            scoutButton.onClick.AddListener(() => SendScout(hill));
        actionLabel.text = canReinforce ? "Reinforce Defense" :
            (canPrepareArmy ? "Prepare Army" :
            (hill.ownedByPlayer ? "No action available" : "Prepare Army"));
        actionButton.interactable = canReinforce || canPrepareArmy ||
            (!hill.ownedByPlayer && scoutController != null && scoutController.CanOpenHill);
        if (!hill.ownedByPlayer)
        {
            if (actionButton.interactable)
                actionButton.onClick.AddListener(() => scoutController.BeginAttack(hill));
        }
        else if (canReinforce)
            actionButton.onClick.AddListener(() =>
            {
                npcThreat.TryReinforce(hill);
                Close();
            });
        else if (canPrepareArmy)
        {
            if (scoutController.HasPreparedTarget)
                actionButton.onClick.AddListener(scoutController.OpenArmyPrompt);
            else
                actionButton.onClick.AddListener(() => scoutController.PrepareArmy(hill));
        }
        countSlider.gameObject.SetActive(false);
    }

    void Close()
    {
        if (!panel.activeSelf) return;
        var scoutController = GameBootstrap.I == null
            ? null
            : GameBootstrap.I.GetComponent<ScoutController>();
        if (scoutController != null)
            scoutController.CancelArmyPreparation();
        panel.SetActive(false);
        countSlider.gameObject.SetActive(false);
        GameClock.Paused = previousPaused;
    }

    public void CloseMenu()
    {
        Close();
    }

    public void OpenHomePreparation(Anthill home, Anthill target)
    {
        Open(home);
        if (target != null)
            details.text += $"\n\nTarget selected: {target.label}\nChoose Prepare Army to continue.";
    }

    public void ShowSendPrompt(Anthill home, Anthill target, Action<int> send,
        string buttonPrefix = "Send")
    {
        previousPaused = false;
        GameClock.Paused = true;
        panel.SetActive(true);
        scoutButton.interactable = false;
        title.text = "Prepare Attack";
        title.color = home.species.tint;
        string destination = target == null ? "Free army" : $"Target: {target.label}";
        details.text = $"{home.label} - {destination}\n\nAvailable ants: {home.Available}\nChoose how many ants to send.\nAll sent ants return to {home.label}.";
        countSlider.minValue = 1;
        countSlider.maxValue = Mathf.Max(1, home.Available);
        countSlider.wholeNumbers = true;
        countSlider.value = Mathf.Max(1, home.Available / 2);
        countSlider.gameObject.SetActive(true);
        actionButton.interactable = home.Available > 0;
        actionLabel.text = $"{buttonPrefix} {Mathf.RoundToInt(countSlider.value)} ants";
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => send(Mathf.RoundToInt(countSlider.value)));
        countSlider.onValueChanged.RemoveAllListeners();
        countSlider.onValueChanged.AddListener(value =>
            actionLabel.text = $"{buttonPrefix} {Mathf.RoundToInt(value)} ants");
    }

    void SendScout(Anthill hill)
    {
        var controller = GameBootstrap.I.GetComponent<ScoutController>();
        if (controller == null || !controller.TrySendScout(hill)) return;
        Close();
    }

    static string FormatSpecies(SpeciesData[] species)
    {
        if (species == null || species.Length == 0) return "None";

        var result = new StringBuilder();
        for (int i = 0; i < species.Length; i++)
        {
            if (i > 0) result.Append(", ");
            result.Append(species[i] == null ? "Unknown" : species[i].displayName);
        }
        return result.ToString();
    }

    static TMP_Text CreateText(string name, Transform parent, float size, TextAlignmentOptions alignment)
    {
        var objectToCreate = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        objectToCreate.transform.SetParent(parent, false);
        var text = objectToCreate.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        return text;
    }

    static Button CreateButton(string name, Transform parent, string text)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color(0.15f, 0.22f, 0.3f, 1f);

        var label = CreateText("Label", buttonObject.transform, 20f, TextAlignmentOptions.Center);
        label.text = text;
        SetRect(label.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);
        label.rectTransform.offsetMin = new Vector2(8f, 4f);
        label.rectTransform.offsetMax = new Vector2(-8f, -4f);
        return buttonObject.GetComponent<Button>();
    }

    static Slider CreateSlider(Transform parent)
    {
        var sliderObject = new GameObject("CountSlider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);
        var slider = sliderObject.GetComponent<Slider>();
        var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderObject.transform, false);
        var backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.3f, 1f);
        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(background.transform, false);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.3f, 0.7f, 0.35f, 1f);
        slider.fillRect = fillRect;
        slider.targetGraphic = fill.GetComponent<Image>();
        return slider;
    }

    static void SetRect(RectTransform rect, Vector2 position, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
