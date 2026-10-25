using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "World";

    [Header("Text")]
    public string gameTitle = "ANTS OF WAR";
    public string playButtonText = "PLAY";
    public string quitButtonText = "QUIT";
    public string howToPlayTitle = "HOW TO PLAY";
    public string startGameButtonText = "START GAME";
    public string backButtonText = "BACK";
    [TextArea(8, 30)]
    public string howToPlayText =
        "YOUR OBJECTIVE\n" +
        "Capture every anthill to win. If you lose all of your anthills, the game ends.\n\n" +
        "CONTROLS\n" +
        "WASD or Arrow Keys: Move your scout or army.\n" +
        "Right or Middle Mouse Drag: Pan the camera.\n" +
        "Scroll Wheel: Zoom the camera.\n" +
        "Left Click: Select visible anthills and use their menus.\n\n" +
        "STARTING OUT\n" +
        "You begin with one visible home anthill. The number above it shows available ants / total ants.\n" +
        "Ants slowly grow until the colony reaches its capacity.\n" +
        "Click your anthill and choose Send Scout to begin exploring.\n\n" +
        "EXPLORATION AND FOG OF WAR\n" +
        "The scout reveals nearby territory as it moves. Explored territory stays visible.\n" +
        "Unexplored territory is covered by fog, and hidden anthills cannot be selected.\n" +
        "A discovered enemy anthill can be inspected by clicking it while your scout is nearby.\n\n" +
        "ARMIES AND BATTLES\n" +
        "Return your scout to your home anthill, then choose Prepare Army.\n" +
        "Choose how many available ants to send. Click a discovered enemy anthill while leading the army.\n" +
        "The army marches there automatically and starts a battle when it arrives.\n" +
        "Battle text shows the remaining attackers and defenders. Surviving armies return home.\n" +
        "Ants can become injured and recover over time. Away ants are not available at the anthill.\n\n" +
        "DIPLOMACY AND NPCS\n" +
        "Species have allies and hostiles. Hostile NPCs attack when they have enough strength.\n" +
        "Allies may send visible reinforcements to defend an anthill.\n" +
        "Watch the map and anthill counts before committing your army.\n\n" +
        "WIN AND LOSS\n" +
        "Capture all anthills for Victory. Lose your last player-owned anthill for Defeat.";

    [Header("Colors")]
    public Color backgroundColor = new Color(0.04f, 0.06f, 0.08f, 1f);
    public Color panelColor = new Color(0.1f, 0.13f, 0.16f, 0.95f);
    public Color buttonColor = new Color(0.24f, 0.48f, 0.25f, 1f);
    public Color buttonTextColor = Color.white;
    public Color titleColor = new Color(0.95f, 0.78f, 0.25f, 1f);

    [Header("Layout")]
    public int titleFontSize = 64;
    public int buttonFontSize = 28;
    public Vector2 buttonSize = new Vector2(320f, 72f);
    public float buttonSpacing = 18f;

    GameObject mainPanel;
    GameObject howToPlayPanel;

    void Awake()
    {
        CreateEventSystem();
        CreateMenu();
    }

    void CreateMenu()
    {
        var canvasObject = new GameObject("Main Menu Canvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var background = CreateImage("Background", canvasObject.transform, backgroundColor);
        Stretch(background.rectTransform);

        var panel = CreateImage("Menu Panel", canvasObject.transform, panelColor);
        mainPanel = panel.gameObject;
        panel.rectTransform.sizeDelta = new Vector2(620f, 540f);
        panel.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        panel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panel.rectTransform.anchoredPosition = Vector2.zero;

        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = buttonSpacing;
        layout.padding = new RectOffset(40, 40, 45, 45);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var title = CreateText("Game Title", panel.transform, gameTitle, titleFontSize, titleColor);
        var titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredWidth = 540f;
        titleLayout.preferredHeight = 150f;
        titleLayout.minWidth = 540f;
        titleLayout.minHeight = 150f;
        title.alignment = TextAnchor.MiddleCenter;
        title.fontStyle = FontStyle.Bold;
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        title.verticalOverflow = VerticalWrapMode.Overflow;

        CreateButton("Play Button", panel.transform, playButtonText, Play, buttonColor);
        CreateButton("Quit Button", panel.transform, quitButtonText, Quit, buttonColor);

        CreateHowToPlayPanel(canvasObject.transform);
    }

    void CreateHowToPlayPanel(Transform canvasTransform)
    {
        var panel = CreateImage("How To Play Panel", canvasTransform, panelColor);
        howToPlayPanel = panel.gameObject;
        Stretch(panel.rectTransform);
        panel.gameObject.SetActive(false);

        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = buttonSpacing;
        layout.padding = new RectOffset(140, 140, 45, 45);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var title = CreateText("How To Play Title", panel.transform,
            howToPlayTitle, titleFontSize, titleColor);
        var titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.minHeight = 90f;
        titleLayout.preferredHeight = 90f;
        title.alignment = TextAnchor.MiddleCenter;
        title.fontStyle = FontStyle.Bold;
        title.horizontalOverflow = HorizontalWrapMode.Overflow;

        var guide = CreateText("How To Play Text", panel.transform,
            howToPlayText, Mathf.Max(16, buttonFontSize - 8), buttonTextColor);
        var guideLayout = guide.gameObject.AddComponent<LayoutElement>();
        guideLayout.minHeight = 680f;
        guideLayout.preferredHeight = 680f;
        guide.alignment = TextAnchor.UpperLeft;
        guide.horizontalOverflow = HorizontalWrapMode.Wrap;
        guide.verticalOverflow = VerticalWrapMode.Truncate;

        var start = CreateButton("Start Game Button", panel.transform,
            startGameButtonText, StartGame, buttonColor);
        var startLayout = start.GetComponent<LayoutElement>();
        startLayout.minWidth = buttonSize.x;
        startLayout.preferredWidth = buttonSize.x;
        startLayout.minHeight = buttonSize.y;
        startLayout.preferredHeight = buttonSize.y;

        var back = CreateButton("Back Button", panel.transform,
            backButtonText, BackToMainMenu, buttonColor);
        var backLayout = back.GetComponent<LayoutElement>();
        backLayout.minWidth = buttonSize.x;
        backLayout.preferredWidth = buttonSize.x;
        backLayout.minHeight = buttonSize.y;
        backLayout.preferredHeight = buttonSize.y;
    }

    void CreateEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    Image CreateImage(string objectName, Transform parent, Color color)
    {
        var image = new GameObject(objectName, typeof(RectTransform), typeof(Image))
            .GetComponent<Image>();
        image.transform.SetParent(parent, false);
        image.color = color;
        return image;
    }

    Text CreateText(string objectName, Transform parent, string value, int size, Color color)
    {
        var text = new GameObject(objectName, typeof(RectTransform), typeof(Text))
            .GetComponent<Text>();
        text.transform.SetParent(parent, false);
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    Button CreateButton(string objectName, Transform parent, string value,
        UnityEngine.Events.UnityAction action, Color color)
    {
        var button = CreateImage(objectName, parent, color);
        var layout = button.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = buttonSize.x;
        layout.preferredHeight = buttonSize.y;
        layout.minWidth = buttonSize.x;
        layout.minHeight = buttonSize.y;
        var buttonComponent = button.gameObject.AddComponent<Button>();
        buttonComponent.targetGraphic = button;
        buttonComponent.onClick.AddListener(action);

        var text = CreateText("Text", button.transform, value, buttonFontSize, buttonTextColor);
        Stretch(text.rectTransform);
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        return buttonComponent;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void Play()
    {
        SetHowToPlayVisible(true);
    }

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("MainMenuController: Game scene name is empty.", this);
            return;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    public void BackToMainMenu()
    {
        SetHowToPlayVisible(false);
    }

    void SetHowToPlayVisible(bool visible)
    {
        if (mainPanel == null || howToPlayPanel == null)
        {
            Debug.LogError("MainMenuController: Menu panels could not be found.", this);
            return;
        }
        mainPanel.SetActive(!visible);
        howToPlayPanel.SetActive(visible);
    }

    public void Quit()
    {
        Debug.Log("MainMenuController: Quit requested.");
        Application.Quit();
    }
}
