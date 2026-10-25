using UnityEngine;

public enum BuffType { None, HealingGrounds, Fortified, Warren, WarCamp, Lookout }

[CreateAssetMenu(menuName = "AntWar/Buff")]
public class BuffData : ScriptableObject
{
    public string displayName = "Buff";
    public Sprite icon;                 // art slot
    [TextArea] public string description;
    public BuffType type;
    public float magnitude = 1.25f;     // multiplier
}
