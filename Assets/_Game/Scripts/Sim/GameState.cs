using System.Collections.Generic;

/// Root of the simulation. Later: scouts, armies, diplomacy, NPC brains.
public class GameState
{
    public List<Anthill> hills = new List<Anthill>();
    public float timePlayed;

    public void Tick(float dt)
    {
        timePlayed += dt;
        foreach (var h in hills) h.Tick(dt);
    }
}
