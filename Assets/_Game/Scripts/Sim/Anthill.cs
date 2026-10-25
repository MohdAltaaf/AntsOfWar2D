using System.Collections.Generic;
using UnityEngine;

public class InjuryBatch
{
    public int count;
    public float timeRemaining;
}

/// Pure simulation. No MonoBehaviour, no sprites.
public class Anthill
{
    public int id;
    public string label;
    public SpeciesData species;
    public bool ownedByPlayer;
    public bool scouted;
    public Vector2 pos;
    public BuffData buff;

    public int total;                 // all ants (available + away + injured)
    public float maxCapacity;
    public int away;
    public List<InjuryBatch> injured = new List<InjuryBatch>();

    float fillAccumulator;

    public int Injured { get { int n = 0; foreach (var b in injured) n += b.count; return n; } }
    public int Available => Mathf.Max(0, total - away - Injured);

    float BuffMult(BuffType t) => (buff != null && buff.type == t) ? buff.magnitude : 1f;

    public void Tick(float dt)
    {
        if (dt <= 0f) return;

        // Growth: capacity rises slowly, ants fill toward it.
        float growth = species.growthPerSec * BuffMult(BuffType.Warren);
        maxCapacity += growth * dt;
        if (total < maxCapacity)
        {
            fillAccumulator += growth * 2f * dt;      // fill rate: tweak
            while (fillAccumulator >= 1f && total < maxCapacity)
            {
                total++; fillAccumulator -= 1f;
            }
        }

        // Recovery: per-batch timers.
        for (int i = injured.Count - 1; i >= 0; i--)
        {
            injured[i].timeRemaining -= dt * BuffMult(BuffType.HealingGrounds);
            if (injured[i].timeRemaining <= 0f) injured.RemoveAt(i);
        }
    }

    public void AddInjured(int count)
    {
        if (count <= 0) return;
        injured.Add(new InjuryBatch { count = count, timeRemaining = species.recoverySeconds });
    }

    public void RestoreAllAvailable()
    {
        injured.Clear();
        away = 0;
    }
}
