using UnityEngine;
using System.Collections.Generic;

public class ScoutController : MonoBehaviour
{
    static readonly Vector2 UnitOffset = new Vector2(0.9f, 0.6f);
    [Range(0.2f, 2f)] public float scoutScale = 0.8f;
    [Range(0.2f, 2f)] public float armyScale = 0.8f;
    [Range(0f, 0.35f)] public float movementJitter = 0.16f;
    [Range(0.5f, 4f)] public float movementWobbleSpeed = 1.8f;
    enum Phase { Roaming, Returning, HomeReady, ArmyRoaming, Battle, ArmyReturning }

    ScoutView scoutView;
    Scout scout;
    Anthill armyHome;
    Anthill attackTarget;
    Phase phase;
    ArmyView armyView;
    Vector2 armyPosition;
    int armyCount;
    float battleAttackers;
    float battleDefenders;
    float battleAttackerPower;
    float battleDefenderPower;
    bool battleResolved;
    int enemyReinforcements;
    BattleView battleView;
    Sprite scoutSprite;
    Sprite armySprite;
    readonly HashSet<Vector2Int> discoveredCells = new HashSet<Vector2Int>();
    const float DiscoveryCellSize = 1.5f;
    FogOfWarView fogOfWar;

    public bool HasActiveScout => scout != null;
    public bool HasActiveArmy => armyView != null;
    public bool CanOpenHill => scout != null && phase == Phase.Roaming;
    public bool CanControlArmy => phase == Phase.ArmyRoaming;
    public bool CanPrepareArmy => scout == null && armyView == null && armyHome == null;
    public bool HasPreparedTarget => phase == Phase.HomeReady && attackTarget != null;
    public bool IsPreparingArmy => phase == Phase.HomeReady && armyHome != null;
    public Vector2 FocusPosition => armyView != null ? armyPosition :
        (scout != null ? scout.position : Vector2.zero);

    public bool CanPrepareArmyFromHome(Anthill home)
    {
        if (home == null || !home.ownedByPlayer || armyView != null || armyHome != null)
            return false;
        if (HasPreparedTarget)
            return home == GetScoutHome();
        return CanPrepareArmy || (scout != null && phase == Phase.Roaming &&
            scout.home == home && Vector2.Distance(scout.position, home.pos) <=
            UnitOffset.magnitude + 0.35f);
    }

    public void Configure(Sprite scoutVisual, Sprite armyVisual)
    {
        scoutSprite = scoutVisual;
        armySprite = armyVisual;
    }

    void Start()
    {
        if (GameBootstrap.I == null || GameBootstrap.I.State == null) return;
        fogOfWar = FogOfWarView.Create(GameBootstrap.I.map.worldSize, DiscoveryCellSize);
        foreach (var hill in GameBootstrap.I.State.hills)
            if (hill.ownedByPlayer)
                MarkDiscoveredArea(hill.pos, 3.5f);
        fogOfWar.Refresh(IsInDiscoveredArea);
    }

    public bool TrySendScout(Anthill home)
    {
        if (home == null || !home.ownedByPlayer || HasActiveScout || HasActiveArmy) return false;
        scout = new Scout(home);
        scout.position = home.pos + UnitOffset;
        MarkDiscoveredArea(scout.position, scout.revealRadius);
        phase = Phase.Roaming;
        var scoutObject = new GameObject("Player Scout");
        scoutObject.transform.SetParent(transform, false);
        scoutView = scoutObject.AddComponent<ScoutView>();
        scoutView.sprite = scout.home.species.antSprite != null
            ? scout.home.species.antSprite
            : scoutSprite;
        scoutView.Bind(scout, scoutScale);
        Debug.Log($"Scout sent from {home.label}.");
        return true;
    }

    public bool IsNear(Anthill hill)
    {
        return scout != null && hill != null &&
               Vector2.Distance(scout.position, hill.pos) <= scout.revealRadius;
    }

    public bool IsInDiscoveredArea(Vector2 position)
    {
        return discoveredCells.Contains(WorldToDiscoveryCell(position));
    }

    public void BeginAttack(Anthill target)
    {
        if (scout == null || phase != Phase.Roaming || target == null ||
            target.ownedByPlayer || !target.scouted || !IsNear(target)) return;
        attackTarget = target;
        phase = Phase.Returning;
        GameBootstrap.I.GetComponent<AnthillMenu>().CloseMenu();
        Debug.Log($"Target selected: {target.label}. Scout returning to {scout.home.label}.");
    }

    public void PrepareArmy(Anthill home)
    {
        if (!CanPrepareArmyFromHome(home)) return;
        armyHome = home;
        phase = Phase.HomeReady;
        attackTarget = null;
        GameBootstrap.I.GetComponent<AnthillMenu>().ShowSendPrompt(
            home, null, StartArmy, "Prepare Army");
    }

    Anthill GetScoutHome()
    {
        return scout == null ? null : scout.home;
    }

    public bool SelectArmyTarget(Anthill target)
    {
        if (!CanControlArmy || target == null || target.ownedByPlayer ||
            !target.scouted) return false;
        attackTarget = target;
        Debug.Log($"Army target selected: {target.label}.");
        return true;
    }

    public bool IsArmyAt(Anthill hill)
    {
        return phase == Phase.ArmyRoaming && hill == attackTarget &&
               Vector2.Distance(armyPosition, hill.pos) <= 0.35f;
    }

    public void BeginArmyBattle()
    {
        if (!IsArmyAt(attackTarget)) return;
        BeginBattle();
    }

    void Update()
    {
        if (GameClock.Paused || (scout == null && armyView == null)) return;
        float dt = GameClock.Dt;
        if (phase == Phase.Roaming)
        {
            Vector2 input = GameInput.Move;
            if (input.sqrMagnitude >= 0.01f)
                scout.MoveBy(NaturalDirection(input, scout.position), dt);
            MarkDiscoveredArea(scout.position, scout.revealRadius);
            scout.RevealNearby(GameBootstrap.I.State);
        }
        else if (phase == Phase.Returning)
            MoveScoutHome(dt);
        else if (phase == Phase.ArmyRoaming)
        {
            Vector2 direction = attackTarget != null
                ? attackTarget.pos - armyPosition
                : GameInput.Move;
            if (direction.sqrMagnitude >= 0.01f)
            {
                Vector2 previousPosition = armyPosition;
                direction = NaturalDirection(direction, armyPosition);
                armyPosition += direction * 2f * dt;
                if (attackTarget == null && !IsInDiscoveredArea(armyPosition))
                    armyPosition = previousPosition;
            }
            armyView.SetPosition(armyPosition);
            if (attackTarget != null && IsArmyAt(attackTarget))
                BeginArmyBattle();
        }
        else if (phase == Phase.Battle)
            TickBattle(dt);
        else if (phase == Phase.ArmyReturning)
            MoveArmyHome(dt);
    }

    void MoveScoutHome(float dt)
    {
        scout.position = Vector2.MoveTowards(scout.position, scout.home.pos, scout.moveSpeed * dt);
        if (Vector2.Distance(scout.position, scout.home.pos) > 0.02f) return;
        phase = Phase.HomeReady;
        GameBootstrap.I.GetComponent<AnthillMenu>().OpenHomePreparation(scout.home, attackTarget);
    }

    public void OpenArmyPrompt()
    {
        if (phase != Phase.HomeReady || (scout == null && armyHome == null)) return;
        Anthill home = armyHome != null ? armyHome : scout.home;
        GameBootstrap.I.GetComponent<AnthillMenu>().ShowSendPrompt(
            home, attackTarget, StartArmy, "Prepare Army");
    }

    public void CancelArmyPreparation()
    {
        if (!IsPreparingArmy) return;
        armyHome = null;
        if (scout == null)
        {
            attackTarget = null;
            phase = Phase.Roaming;
        }
    }

    void StartArmy(int count)
    {
        Anthill home = armyHome != null ? armyHome : scout.home;
        if (home == null || count <= 0 || count > home.Available) return;
        MarkDiscoveredArea(home.pos, scout != null ? scout.revealRadius : 3.5f);
        armyCount = count;
        home.away += count;
        armyHome = home;
        armyPosition = home.pos + UnitOffset;
        phase = Phase.ArmyRoaming;
        var armyObject = new GameObject("Player Army");
        armyObject.transform.SetParent(transform, false);
        armyView = armyObject.AddComponent<ArmyView>();
        armyView.sprite = home.species.antSprite != null
            ? home.species.antSprite
            : armySprite;
        armyView.Bind(armyPosition, home.species.tint, armyScale, armyCount);
        if (scoutView != null)
            scoutView.SetVisible(false);
        GameBootstrap.I.GetComponent<AnthillMenu>().CloseMenu();
        Debug.Log($"Army prepared at {home.label}. Move it through discovered territory.");
    }

    Vector2 NaturalDirection(Vector2 direction, Vector2 position)
    {
        Vector2 forward = direction.normalized;
        Vector2 side = new Vector2(-forward.y, forward.x);
        float phase = position.x * 0.43f + position.y * 0.71f;
        float sway = Mathf.Sin(Time.time * movementWobbleSpeed + phase) * movementJitter;
        return (forward + side * sway).normalized;
    }

    void MarkDiscoveredArea(Vector2 center, float radius)
    {
        int cellRadius = Mathf.CeilToInt(radius / DiscoveryCellSize);
        Vector2Int centerCell = WorldToDiscoveryCell(center);
        for (int x = -cellRadius; x <= cellRadius; x++)
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                Vector2Int cell = centerCell + new Vector2Int(x, y);
                Vector2 cellCenter = DiscoveryCellToWorld(cell);
                if ((cellCenter - center).sqrMagnitude <= radius * radius)
                    discoveredCells.Add(cell);
            }
        fogOfWar?.Refresh(IsInDiscoveredArea);
    }

    static Vector2Int WorldToDiscoveryCell(Vector2 position)
    {
        Vector2 origin = -GameBootstrap.I.map.worldSize * 0.5f;
        return new Vector2Int(
            Mathf.FloorToInt((position.x - origin.x) / DiscoveryCellSize),
            Mathf.FloorToInt((position.y - origin.y) / DiscoveryCellSize));
    }

    static Vector2 DiscoveryCellToWorld(Vector2Int cell)
    {
        Vector2 origin = -GameBootstrap.I.map.worldSize * 0.5f;
        return origin + (Vector2)cell * DiscoveryCellSize +
            Vector2.one * (DiscoveryCellSize * 0.5f);
    }

    void BeginBattle()
    {
        phase = Phase.Battle;
        battleResolved = false;
        battleAttackers = armyCount;
        battleDefenders = attackTarget.Available;
        var npcThreat = GameBootstrap.I.GetComponent<NpcThreatController>();
        if (npcThreat != null)
            enemyReinforcements = npcThreat.ReinforceDefense(attackTarget);
        battleDefenders += enemyReinforcements;
        Anthill home = armyHome != null ? armyHome : scout.home;
        battleAttackerPower = battleAttackers * home.species.attack * Random.Range(0.9f, 1.1f);
        battleDefenderPower = battleDefenders * attackTarget.species.defense * Random.Range(0.9f, 1.1f);
        var battleObject = new GameObject("Player Battle");
        battleView = battleObject.AddComponent<BattleView>();
        battleView.Bind(attackTarget.pos);
        battleView.SetCounts(Mathf.CeilToInt(battleAttackers), Mathf.CeilToInt(battleDefenders));
    }

    void TickBattle(float dt)
    {
        battleAttackers -= battleDefenderPower * 0.06f * dt;
        battleDefenders -= battleAttackerPower * 0.06f * dt;
        battleView.SetCounts(Mathf.CeilToInt(battleAttackers), Mathf.CeilToInt(battleDefenders));
        if (battleAttackers > 0f && battleDefenders > 0f) return;
        ResolveBattle(battleAttackers > 0f);
        phase = Phase.ArmyReturning;
    }

    void ResolveBattle(bool won)
    {
        if (battleResolved) return;
        battleResolved = true;
        int attackerInjured = Mathf.Clamp(Mathf.RoundToInt(armyCount * (won ? 0.2f : 0.7f)), 0, armyCount);
        int defenderInjured = Mathf.Clamp(Mathf.RoundToInt(attackTarget.Available * (won ? 0.7f : 0.2f)), 0, attackTarget.Available);
        Anthill home = armyHome != null ? armyHome : scout.home;
        home.AddInjured(attackerInjured);
        if (attackTarget != null)
            attackTarget.AddInjured(defenderInjured);
        if (won && attackTarget != null)
        {
            attackTarget.ownedByPlayer = true;
            attackTarget.species = home.species;
            attackTarget.RestoreAllAvailable();
        }
        var npcThreat = GameBootstrap.I.GetComponent<NpcThreatController>();
        if (npcThreat != null)
            npcThreat.ResolveDefense(attackTarget, !won);
        Debug.Log($"{(won ? "Victory" : "Defeat")} at {attackTarget.label}.");
    }

    void MoveArmyHome(float dt)
    {
        Anthill home = armyHome != null ? armyHome : scout.home;
        armyPosition = Vector2.MoveTowards(armyPosition, home.pos, 4f * dt);
        armyView.SetPosition(armyPosition);
        if (Vector2.Distance(armyPosition, home.pos) > 0.02f) return;
        home.away -= armyCount;
        Destroy(battleView == null ? null : battleView.gameObject);
        Destroy(armyView.gameObject);
        if (scoutView != null)
            Destroy(scoutView.gameObject);
        armyView = null;
        scoutView = null;
        scout = null;
        armyHome = null;
        attackTarget = null;
        phase = Phase.Roaming;
    }
}
