using UnityEngine;
using TMPro;

/// Thin view. Prefab layout:
///   Anthill (this script + CircleCollider2D)
///     Visual (SpriteRenderer)              <- teammate swaps this
///     Label  (TextMeshPro 3D, not UGUI)    <- count text
[RequireComponent(typeof(Collider2D))]
public class AnthillView : MonoBehaviour
{
    public SpriteRenderer visual;
    public TMP_Text label;

    [Tooltip("Off = unscouted hills are completely hidden (real fog of war). On = show a '?' marker.")]
    public bool showUnscoutedMarker = false;
    public Color hiddenColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [Range(0.5f, 4f)] public float visualScale = 2f;
    [Tooltip("Vertical offset applied to the hill art, label, and clickable area.")]
    public float visualYOffset = -0.5f;

    Anthill hill;
    Collider2D col;

    public Anthill Hill => hill;

    void Awake() => col = GetComponent<Collider2D>();

    public void Bind(Anthill h)
    {
        hill = h;
        if (h.species.hillSprite != null) visual.sprite = h.species.hillSprite;
        visual.transform.localScale = Vector3.one * visualScale;
        visual.transform.localPosition = new Vector3(0f, visualYOffset, 0f);
        label.transform.localPosition = new Vector3(
            label.transform.localPosition.x,
            label.transform.localPosition.y + visualYOffset,
            label.transform.localPosition.z);
        if (col is CircleCollider2D circle)
        {
            circle.radius = visualScale * 0.5f;
            circle.offset = new Vector2(0f, visualYOffset);
        }
    }

    void Update()
    {
        if (hill == null) return;

        bool show = hill.scouted || showUnscoutedMarker;
        visual.enabled = show;
        label.enabled = show;
        col.enabled = hill.scouted;      // only scouted hills are clickable

        if (hill.scouted)
        {
            visual.color = hill.species.tint;
            label.text = $"{hill.Available}/{hill.total}";
        }
        else if (showUnscoutedMarker)
        {
            visual.color = hiddenColor;
            label.text = "?";
        }
    }

    /// Called by WorldClickHandler.
    public void HandleClick()
    {
        if (hill == null) return;
        Debug.Log($"Clicked {hill.label} ({hill.species.displayName}) ants={hill.total} injured={hill.Injured}");
        GameEvents.RaiseHillClicked(hill);   // Step B: menu opens from this
    }
}