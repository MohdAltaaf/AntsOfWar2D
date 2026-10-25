using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEndController : MonoBehaviour
{
    GameState state;
    GameObject overlay;
    TMP_Text resultText;
    bool ended;

    public void Configure(GameState gameState, Anthill playerHome)
    {
        state = gameState;
    }

    void Update()
    {
        if (ended || state == null || state.hills.Count == 0) return;
        bool hasPlayerHill = false;
        for (int i = 0; i < state.hills.Count; i++)
            if (state.hills[i].ownedByPlayer)
            {
                hasPlayerHill = true;
                break;
            }
        if (!hasPlayerHill)
        {
            EndGame(false);
            return;
        }

        for (int i = 0; i < state.hills.Count; i++)
            if (!state.hills[i].ownedByPlayer)
                return;
        EndGame(true);
    }

    void EndGame(bool won)
    {
        ended = true;
        GameClock.Paused = true;
        BuildOverlay(won);
    }

    void BuildOverlay(bool won)
    {
        var canvasObject = new GameObject("GameEndCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        overlay = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        var panelRect = overlay.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(620f, 360f);
        overlay.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.06f, 0.98f);

        var textObject = new GameObject("ResultText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(overlay.transform, false);
        resultText = textObject.GetComponent<TextMeshProUGUI>();
        resultText.text = won ? "VICTORY" : "DEFEAT";
        resultText.fontSize = 64f;
        resultText.color = won ? new Color(0.3f, 1f, 0.45f) : new Color(1f, 0.35f, 0.3f);
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        resultText.rectTransform.anchorMax = new Vector2(1f, 1f);
        resultText.rectTransform.offsetMin = new Vector2(30f, 40f);
        resultText.rectTransform.offsetMax = new Vector2(-30f, -30f);

        var buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(overlay.transform, false);
        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.sizeDelta = new Vector2(220f, 60f);
        buttonRect.anchoredPosition = new Vector2(0f, 55f);
        buttonObject.GetComponent<Image>().color = new Color(0.15f, 0.3f, 0.4f, 1f);
        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(Restart);

        var buttonTextObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        buttonTextObject.transform.SetParent(buttonObject.transform, false);
        var buttonText = buttonTextObject.GetComponent<TextMeshProUGUI>();
        buttonText.text = "Restart";
        buttonText.fontSize = 24f;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;
    }

    void Restart()
    {
        GameClock.Paused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
