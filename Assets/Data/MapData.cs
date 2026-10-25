using UnityEngine;

[System.Serializable]
public class HillSpawn
{
    public string label = "Hill";
    public Vector2 position;
    public SpeciesData species;
    public int startingAnts = 10;
    public BuffData buff;
    public bool isPlayerStart;
}

[CreateAssetMenu(menuName = "AntWar/Map")]
public class MapData : ScriptableObject
{
    [Tooltip("Total world size in units, centred on (0,0). Camera is clamped to this.")]
    public Vector2 worldSize = new Vector2(48f, 28f);
    public HillSpawn[] hills;
}