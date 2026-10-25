using System.Collections.Generic;
using UnityEngine;

public class NpcThreatController : MonoBehaviour
{
    static readonly Vector2 UnitOffset = new Vector2(0.9f, 0.6f);
    enum Phase { Scouting, Returning, Preparing, Attacking, Battle, ReturningArmy }

    class Attack
    {
        public Anthill source;
        public Anthill target;
        public int count;
        public Vector2 position;
        public Phase phase;
        public GameObject scoutObject;
        public GameObject armyObject;
        public ArmyView armyView;
        public BattleView battleView;
        public float attackers;
        public float defenders;
        public float attackerPower;
        public float defenderPower;
        public int reinforcementCount;
        public int localReinforcementCount;
        public float prepareTimer;
    }

    public float decisionInterval = 30f;
    [Range(0f, 0.8f)] public float reserveFraction = 0.3f;
    public float attackMargin = 1.1f;
    public float scoutSpeed = 2f;
    public float armySpeed = 1.5f;
    [Range(0.2f, 2f)] public float scoutScale = 0.8f;
    [Range(0.2f, 2f)] public float armyScale = 0.8f;
    [Range(0f, 0.35f)] public float movementJitter = 0.16f;
    [Range(0.5f, 4f)] public float movementWobbleSpeed = 1.8f;
    public Sprite scoutSprite;
    public Sprite armySprite;

    float decisionTimer;
    readonly List<Attack> attacks = new List<Attack>();
    class DefenseReinforcement
    {
        public Anthill source;
        public Anthill target;
        public int count;
        public Vector2 position;
        public ArmyView view;
        public LineRenderer route;
        public bool returning;
    }
    readonly List<DefenseReinforcement> defenseReinforcements = new List<DefenseReinforcement>();

    void Start()
    {
        decisionTimer = decisionInterval;
    }

    void Update()
    {
        if (GameClock.Paused || GameBootstrap.I == null || GameBootstrap.I.State == null) return;
        float dt = GameClock.Dt;
        decisionTimer -= dt;
        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionInterval;
            TryStartAttack();
        }

        for (int i = attacks.Count - 1; i >= 0; i--)
        {
            TickAttack(attacks[i], dt);
            if (attacks[i].phase == Phase.ReturningArmy &&
                Vector2.Distance(attacks[i].position, attacks[i].source.pos) <= 0.02f)
            {
                FinishAttack(attacks[i]);
                attacks.RemoveAt(i);
            }
        }
        TickDefenseReinforcements(dt);
    }

    void TryStartAttack()
    {
        if (attacks.Count > 0) return;

        Attack best = null;
        float bestScore = float.MinValue;
        foreach (var source in GameBootstrap.I.State.hills)
        {
            if (source.ownedByPlayer || source.Available <= 1) continue;
            int reserve = Mathf.CeilToInt(source.total * reserveFraction);
            int send = source.Available - reserve;
            if (send <= 0) continue;

            foreach (var target in GameBootstrap.I.State.hills)
            {
                if (target == source || !IsHostile(source.species, target.species))
                    continue;
                float attackPower = send * source.species.attack;
                float defensePower = target.Available * target.species.defense;
                if (attackPower < defensePower * attackMargin) continue;

                float score = (attackPower / Mathf.Max(1f, defensePower)) -
                              Vector2.Distance(source.pos, target.pos) * 0.01f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = new Attack
                    {
                        source = source,
                        target = target,
                        count = send,
                        position = source.pos + UnitOffset,
                        phase = Phase.Scouting
                    };
                }

            }
        }

        if (best == null) return;
        best.source.away += best.count;
        best.scoutObject = CreateScout(best);
        attacks.Add(best);
        best.reinforcementCount = ReinforceDefense(best.target);
        Debug.Log($"NPC scout sent from {best.source.label} toward {best.target.label}.");
    }

    public bool IsUnderAttack(Anthill hill)
    {
        foreach (var attack in attacks)
            if (attack.target == hill &&
                (attack.phase == Phase.Preparing || attack.phase == Phase.Attacking ||
                 attack.phase == Phase.Battle))
                return true;
        return false;
    }

    public bool TryReinforce(Anthill hill)
    {
        if (hill == null) return false;
        foreach (var attack in attacks)
        {
            if (attack.target != hill ||
                (attack.phase != Phase.Preparing && attack.phase != Phase.Attacking &&
                 attack.phase != Phase.Battle))
                continue;

            int count = hill.Available;
            if (count <= 0) return false;
            hill.away += count;
            attack.reinforcementCount += count;
            attack.localReinforcementCount += count;
            if (attack.phase == Phase.Battle)
                attack.defenders += count;
            Debug.Log($"Reinforced {hill.label} with {count} ants.");
            return true;
        }
        return false;
    }

    public int ReinforceDefense(Anthill target)
    {
        if (target == null) return 0;
        int total = 0;
        foreach (var source in GameBootstrap.I.State.hills)
        {
            if (source.ownedByPlayer || source.Available < 2 ||
                !IsAlly(source.species, target.species) ||
                Vector2.Distance(source.pos, target.pos) > 8f)
                continue;
            int count = Mathf.Max(1, source.Available / 2);
            source.away += count;
            defenseReinforcements.Add(new DefenseReinforcement
            {
                source = source,
                target = target,
                count = count,
                position = source.pos + UnitOffset
            });
            var reinforcement = defenseReinforcements[defenseReinforcements.Count - 1];
            var reinforcementObject = new GameObject(
                $"NPC Reinforcements ({source.label})");
            reinforcementObject.transform.SetParent(transform, false);
            reinforcement.view = reinforcementObject.AddComponent<ArmyView>();
            reinforcement.view.sprite = source.species.antSprite != null
                ? source.species.antSprite
                : armySprite;
            reinforcement.view.Bind(reinforcement.position, source.species.tint, armyScale, count);
            reinforcement.route = CreateReinforcementRoute(reinforcement);
            total += count;
        }

        if (total > 0)
            Debug.Log($"Enemy reinforcements joined the defense of {target.label}: {total} ants.");
        return total;
    }

    static bool IsHostile(SpeciesData source, SpeciesData target)
    {
        return source != null && target != null &&
            source != target && source.IsHostile(target) &&
            !source.IsAlly(target);
    }

    static bool IsAlly(SpeciesData source, SpeciesData target)
    {
        return source != null && target != null &&
            (source == target || source.IsAlly(target) || target.IsAlly(source));
    }

    public void ResolveDefense(Anthill target, bool defendersWon)
    {
        for (int i = defenseReinforcements.Count - 1; i >= 0; i--)
        {
            var reinforcement = defenseReinforcements[i];
            if (reinforcement.target != target) continue;
            reinforcement.source.away = Mathf.Max(0, reinforcement.source.away - reinforcement.count);
            reinforcement.source.AddInjured(Mathf.RoundToInt(
                reinforcement.count * (defendersWon ? 0.2f : 0.7f)));
            reinforcement.returning = true;
        }
    }

    void TickDefenseReinforcements(float dt)
    {
        for (int i = defenseReinforcements.Count - 1; i >= 0; i--)
        {
            var reinforcement = defenseReinforcements[i];
            Vector2 destination = reinforcement.returning
                ? reinforcement.source.pos
                : reinforcement.target.pos;
            reinforcement.position = Vector2.MoveTowards(
                reinforcement.position, destination, armySpeed * dt);
            reinforcement.view.SetPosition(reinforcement.position);
            if (reinforcement.returning &&
                Vector2.Distance(reinforcement.position, reinforcement.source.pos) <= 0.02f)
            {
                Destroy(reinforcement.view.gameObject);
                Destroy(reinforcement.route.gameObject);
                defenseReinforcements.RemoveAt(i);
            }
        }
    }

    LineRenderer CreateReinforcementRoute(DefenseReinforcement reinforcement)
    {
        var routeObject = new GameObject(
            $"Reinforcement Route ({reinforcement.source.label})");
        routeObject.transform.SetParent(transform, false);
        var route = routeObject.AddComponent<LineRenderer>();
        route.useWorldSpace = true;
        route.positionCount = 2;
        route.SetPosition(0, new Vector3(
            reinforcement.source.pos.x, reinforcement.source.pos.y, 0.2f));
        route.SetPosition(1, new Vector3(
            reinforcement.target.pos.x, reinforcement.target.pos.y, 0.2f));
        route.startWidth = 0.045f;
        route.endWidth = 0.045f;
        route.startColor = new Color(1f, 0.8f, 0.2f, 0.65f);
        route.endColor = new Color(1f, 0.8f, 0.2f, 0.15f);
        route.sortingOrder = 18;
        route.material = new Material(Shader.Find("Sprites/Default"));
        return route;
    }

    GameObject CreateScout(Attack attack)
    {
        var scoutObject = new GameObject($"NPC Scout ({attack.source.label})");
        var renderer = scoutObject.AddComponent<SpriteRenderer>();
        renderer.sprite = attack.source.species.antSprite != null
            ? attack.source.species.antSprite
            : (scoutSprite != null ? scoutSprite : PlaceholderSprite.Circle);
        renderer.color = attack.source.species.tint;
        renderer.sortingOrder = 19;
        scoutObject.transform.localScale = Vector3.one * scoutScale;
        scoutObject.transform.position = new Vector3(attack.position.x, attack.position.y, -1f);
        return scoutObject;
    }

    void TickAttack(Attack attack, float dt)
    {
        if (attack.phase == Phase.Scouting)
        {
            Move(attack, attack.target.pos, scoutSpeed, dt);
            SetScoutPosition(attack);
            if (Reached(attack.position, attack.target.pos))
            {
                attack.phase = Phase.Returning;
                Debug.Log($"NPC scout reached {attack.target.label} and is returning.");
            }
        }
        else if (attack.phase == Phase.Returning)
        {
            Move(attack, attack.source.pos, scoutSpeed, dt);
            SetScoutPosition(attack);
            if (Reached(attack.position, attack.source.pos))
            {
                Destroy(attack.scoutObject);
                attack.phase = Phase.Preparing;
                attack.prepareTimer = 3f;
                attack.position = attack.source.pos + new Vector2(0.9f, 0.6f);
                Debug.Log($"NPC army preparing to attack {attack.target.label}.");
            }
        }
        else if (attack.phase == Phase.Preparing)
        {
            attack.prepareTimer -= dt;
            if (attack.prepareTimer <= 0f)
            {
                attack.phase = Phase.Attacking;
                attack.armyObject = CreateArmy(attack);
                Debug.Log($"NPC army marching from {attack.source.label} to {attack.target.label}.");
            }
        }
        else if (attack.phase == Phase.Attacking)
        {
            Move(attack, attack.target.pos, armySpeed, dt);
            attack.armyView.SetPosition(attack.position);
            if (Reached(attack.position, attack.target.pos))
            {
                BeginBattle(attack);
            }
        }
        else if (attack.phase == Phase.Battle)
        {
            TickBattle(attack, dt);
        }
        else
        {
            Move(attack, attack.source.pos, armySpeed, dt);
            attack.armyView.SetPosition(attack.position);
        }
    }

    GameObject CreateArmy(Attack attack)
    {
        var armyObject = new GameObject($"NPC Army ({attack.source.label})");
        attack.armyView = armyObject.AddComponent<ArmyView>();
        attack.armyView.sprite = attack.source.species.antSprite != null
            ? attack.source.species.antSprite
            : armySprite;
        attack.armyView.Bind(attack.position, attack.source.species.tint, armyScale, attack.count);
        return armyObject;
    }

    static void SetScoutPosition(Attack attack)
    {
        if (attack.scoutObject == null) return;
        Vector2 movement = attack.position -
            new Vector2(attack.scoutObject.transform.position.x, attack.scoutObject.transform.position.y);
        if (movement.sqrMagnitude > 0.0001f)
            attack.scoutObject.transform.rotation = Quaternion.Euler(
                0f, 0f, Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg - 90f);
        attack.scoutObject.transform.position =
            new Vector3(attack.position.x, attack.position.y, -1f);
    }

    void BeginBattle(Attack attack)
    {
        attack.phase = Phase.Battle;
        attack.attackers = attack.count;
        attack.defenders = attack.target.Available + attack.reinforcementCount;
        attack.attackerPower = attack.attackers * attack.source.species.attack * Random.Range(0.9f, 1.1f);
        attack.defenderPower = attack.defenders * attack.target.species.defense * Random.Range(0.9f, 1.1f);
        var battleObject = new GameObject($"NPC Battle ({attack.target.label})");
        attack.battleView = battleObject.AddComponent<BattleView>();
        attack.battleView.Bind(attack.target.pos);
        attack.battleView.SetCounts(Mathf.CeilToInt(attack.attackers), Mathf.CeilToInt(attack.defenders));
    }

    void TickBattle(Attack attack, float dt)
    {
        attack.attackers -= attack.defenderPower * 0.06f * dt;
        attack.defenders -= attack.attackerPower * 0.06f * dt;
        attack.battleView.SetCounts(Mathf.CeilToInt(attack.attackers), Mathf.CeilToInt(attack.defenders));
        if (attack.attackers > 0f && attack.defenders > 0f) return;

        ResolveBattle(attack, attack.attackers > 0f);
        attack.phase = Phase.ReturningArmy;
    }

    void ResolveBattle(Attack attack, bool won)
    {
        int attackerInjured = Mathf.Clamp(Mathf.RoundToInt(attack.count * (won ? 0.2f : 0.7f)), 0, attack.count);
        int defenderPool = attack.target.Available + attack.reinforcementCount;
        int defenderInjured = Mathf.Clamp(Mathf.RoundToInt(defenderPool * (won ? 0.7f : 0.2f)), 0, defenderPool);
        attack.source.AddInjured(attackerInjured);
        attack.target.AddInjured(defenderInjured);
        attack.target.away = Mathf.Max(0, attack.target.away - attack.localReinforcementCount);
        ResolveDefense(attack.target, !won);

        if (won)
        {
            attack.target.ownedByPlayer = false;
            attack.target.species = attack.source.species;
            attack.target.RestoreAllAvailable();
        }

        Debug.Log($"NPC {(won ? "captured" : "lost at")} {attack.target.label}.");
    }

    void FinishAttack(Attack attack)
    {
        attack.source.away -= attack.count;
        Destroy(attack.armyObject);
        Destroy(attack.battleView.gameObject);
        Debug.Log($"NPC army returned to {attack.source.label}.");
    }

    void Move(Attack attack, Vector2 target, float speed, float dt)
    {
        Vector2 direction = target - attack.position;
        if (direction.sqrMagnitude <= 0.0001f) return;
        Vector2 forward = direction.normalized;
        Vector2 side = new Vector2(-forward.y, forward.x);
        float phase = attack.source.id * 0.83f + attack.position.x * 0.43f +
            attack.position.y * 0.71f;
        float sway = Mathf.Sin(Time.time * movementWobbleSpeed + phase) * movementJitter;
        Vector2 destination = attack.position + (forward + side * sway).normalized;
        attack.position = Vector2.MoveTowards(attack.position, destination, speed * dt);
    }

    static bool Reached(Vector2 position, Vector2 target)
    {
        return Vector2.Distance(position, target) <= 0.02f;
    }
}
