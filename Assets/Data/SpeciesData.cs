using UnityEngine;

[CreateAssetMenu(menuName = "AntWar/Species")]
public class SpeciesData : ScriptableObject
{
    public string displayName = "Species";
    [Header("Art slots (teammate fills these)")]
    public Sprite icon;          // class select portrait
    public Sprite hillSprite;    // optional override; else default prefab sprite
    public Sprite antSprite;     // scout and army ant sprite
    public Color tint = Color.white;

    [Header("Stats")]
    public float attack = 1f;              // power per ant when attacking
    public float defense = 1f;             // power per ant when defending
    public float recoverySeconds = 30f;    // injured batch recovery time
    public float growthPerSec = 0.05f;     // max capacity growth per second

    [Header("Start")]
    public int startingMaxCapacity = 30;

    [Header("Fixed relations")]
    public SpeciesData[] allies;
    public SpeciesData[] hostiles;

    public bool IsAlly(SpeciesData o) => System.Array.IndexOf(allies, o) >= 0;
    public bool IsHostile(SpeciesData o) => System.Array.IndexOf(hostiles, o) >= 0;
}
