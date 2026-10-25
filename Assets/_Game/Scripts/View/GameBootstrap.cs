using UnityEngine;

/// Lives in the World scene. Builds the GameState from MapData, spawns views, ticks the sim.
public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap I { get; private set; }

    [Header("Assign in Inspector")]
    public MapData map;
    public AnthillView anthillPrefab;
    public Sprite scoutSprite;
    public Sprite armySprite;
    public Sprite npcScoutSprite;
    public Sprite npcArmySprite;

    public GameState State { get; private set; }
    GameEndController gameEnd;

    void Awake()
    {
        I = this;
        GameClock.Paused = false;
        GameClock.Speed = 1f;
        if (GetComponent<AnthillMenu>() == null)
            gameObject.AddComponent<AnthillMenu>();
        var scoutController = GetComponent<ScoutController>();
        if (scoutController == null)
            scoutController = gameObject.AddComponent<ScoutController>();
        scoutController.Configure(scoutSprite, armySprite);
        var npcThreat = GetComponent<NpcThreatController>();
        if (npcThreat == null)
            npcThreat = gameObject.AddComponent<NpcThreatController>();
        npcThreat.scoutSprite = npcScoutSprite != null ? npcScoutSprite : scoutSprite;
        npcThreat.armySprite = npcArmySprite != null ? npcArmySprite : armySprite;
        gameEnd = GetComponent<GameEndController>();
        if (gameEnd == null)
            gameEnd = gameObject.AddComponent<GameEndController>();

        if (map == null) { Debug.LogError("GameBootstrap: 'Map' is not assigned.", this); enabled = false; return; }
        if (anthillPrefab == null) { Debug.LogError("GameBootstrap: 'Anthill Prefab' is not assigned.", this); enabled = false; return; }
        if (map.hills == null || map.hills.Length == 0) { Debug.LogError("GameBootstrap: Map has no hills.", map); enabled = false; return; }

        Build();
        for (int i = 0; i < State.hills.Count; i++)
            if (State.hills[i].ownedByPlayer)
            {
                gameEnd.Configure(State, State.hills[i]);
                break;
            }
    }

    void Build()
    {
        State = new GameState();

        int id = 0;
        for (int i = 0; i < map.hills.Length; i++)
        {
            var s = map.hills[i];
            var species = s.species;
            if (s.isPlayerStart && GameSession.PlayerSpecies != null)
                species = GameSession.PlayerSpecies;

            if (species == null)
            {
                Debug.LogError($"GameBootstrap: Map hill #{i} ('{s.label}') has no Species assigned. Skipping it.", map);
                continue;
            }
            if (species.hillSprite == null)
                Debug.LogWarning($"GameBootstrap: {species.displayName} has no hill sprite.", species);
            if (species.antSprite == null)
                Debug.LogWarning($"GameBootstrap: {species.displayName} has no ant sprite.", species);

            var hill = new Anthill
            {
                id = id++,
                label = s.label,
                species = species,
                ownedByPlayer = s.isPlayerStart,
                scouted = s.isPlayerStart,          // fog: only own hill visible
                pos = s.position,
                buff = s.buff,
                total = s.startingAnts,
                maxCapacity = Mathf.Max(species.startingMaxCapacity, s.startingAnts)
            };
            State.hills.Add(hill);

            var view = Instantiate(anthillPrefab, hill.pos, Quaternion.identity, transform);
            view.name = "Hill_" + hill.label;
            view.Bind(hill);
        }
    }

    void Update()
    {
        if (State == null) return;
        State.Tick(GameClock.Dt);
    }
}