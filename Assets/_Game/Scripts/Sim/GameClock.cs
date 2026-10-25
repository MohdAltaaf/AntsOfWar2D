using UnityEngine;

/// Single source of truth for sim time. All sim ticks use GameClock.Dt.
public static class GameClock
{
    public static bool Paused;
    public static float Speed = 1f;                 // 1x, 2x later
    public static float Dt => Paused ? 0f : Time.deltaTime * Speed;
}
