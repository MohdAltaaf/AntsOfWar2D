using System;

/// Sim -> UI messaging. Sim never references UI directly.
public static class GameEvents
{
    public static event Action<Anthill> HillClicked;
    public static void RaiseHillClicked(Anthill h) => HillClicked?.Invoke(h);
}
