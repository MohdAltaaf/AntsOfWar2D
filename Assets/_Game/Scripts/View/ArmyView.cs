using UnityEngine;

public class ArmyView : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    readonly System.Collections.Generic.List<Transform> followers = new System.Collections.Generic.List<Transform>();
    Vector2 lastPosition;
    Vector2 forward = Vector2.up;
    public Sprite sprite;
    public float followerCatchupSpeed = 8f;
    public void Bind(Vector2 position, Color color, float scale, int count = 1)
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite != null ? sprite : PlaceholderSprite.Square;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = 20;
        transform.localScale = Vector3.one * scale;
        lastPosition = position;
        int followerCount = Mathf.Clamp(count - 1, 0, 5);
        for (int i = 0; i < followerCount; i++)
        {
            var follower = new GameObject($"Army Ant {i + 1}");
            follower.transform.SetParent(transform.parent, false);
            var followerRenderer = follower.AddComponent<SpriteRenderer>();
            followerRenderer.sprite = sprite != null ? sprite : PlaceholderSprite.Square;
            followerRenderer.color = color;
            followerRenderer.sortingOrder = 19;
            follower.transform.localScale = Vector3.one * scale;
            follower.transform.position = new Vector3(
                position.x, position.y - 0.32f * (i + 1), -1.01f - i * 0.01f);
            followers.Add(follower.transform);
        }
        SetPosition(position);
    }

    public void SetPosition(Vector2 position)
    {
        Vector2 movement = position - lastPosition;
        if (movement.sqrMagnitude > 0.0001f)
        {
            forward = movement.normalized;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f);
        }
        transform.position = new Vector3(position.x, position.y, -1f);
        for (int i = 0; i < followers.Count; i++)
        {
            float sway = Mathf.Sin(Time.time * 3.2f + i * 1.4f) * 0.08f;
            Vector2 side = new Vector2(-forward.y, forward.x);
            Vector2 target = position - forward * (0.32f * (i + 1)) + side * sway;
            Vector2 current = followers[i].position;
            Vector2 next = Vector2.MoveTowards(current, target, followerCatchupSpeed * Time.deltaTime);
            Vector2 followerMovement = next - current;
            followers[i].position = new Vector3(next.x, next.y, -1.01f - i * 0.01f);
            if (followerMovement.sqrMagnitude > 0.000001f)
                followers[i].rotation = Quaternion.Euler(
                    0f, 0f, Mathf.Atan2(followerMovement.y, followerMovement.x) * Mathf.Rad2Deg - 90f);
        }
        lastPosition = position;
    }

    void OnDestroy()
    {
        for (int i = 0; i < followers.Count; i++)
            if (followers[i] != null)
                Destroy(followers[i].gameObject);
        followers.Clear();
    }
}
