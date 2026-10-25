using UnityEngine;

public class ScoutView : MonoBehaviour
{
    Scout scout;
    SpriteRenderer spriteRenderer;
    Vector3 lastPosition;
    public Sprite sprite;
    public void Bind(Scout value, float scale)
    {
        scout = value;
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite != null ? sprite : PlaceholderSprite.Circle;
        spriteRenderer.color = value.home.species.tint;
        spriteRenderer.sortingOrder = 20;
        transform.localScale = Vector3.one * scale;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (scout == null) return;
        Vector3 nextPosition = new Vector3(scout.position.x, scout.position.y, -1f);
        Vector2 movement = nextPosition - lastPosition;
        if (movement.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f);
        transform.position = nextPosition;
        lastPosition = nextPosition;
    }

    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
    }
}
