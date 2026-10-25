using UnityEngine;

public class Scout
{
    public Anthill home;
    public Vector2 position;
    public float revealRadius = 3.5f;
    public float moveSpeed = 2.5f;

    public Scout(Anthill home)
    {
        this.home = home;
        position = home.pos;
    }

    public void MoveBy(Vector2 direction, float dt)
    {
        if (direction.sqrMagnitude < 0.01f) return;
        position += direction.normalized * moveSpeed * dt;
    }

    public void RevealNearby(GameState state)
    {
        foreach (var hill in state.hills)
        {
            if (hill.scouted) continue;
            if ((position - hill.pos).sqrMagnitude <= revealRadius * revealRadius)
            {
                hill.scouted = true;
                Debug.Log($"Scout discovered {hill.label}.");
            }
        }
    }
}
