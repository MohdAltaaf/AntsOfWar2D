using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class WorldClickHandler : MonoBehaviour
{
    Camera cam;

    void Awake() => cam = GetComponent<Camera>();

    void Update()
    {
        if (GameClock.Paused) return;
        if (!GameInput.LeftClickDown) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // clicked UI

        Vector2 world = cam.ScreenToWorldPoint(GameInput.MousePos);
        var scoutController = GameBootstrap.I.GetComponent<ScoutController>();
        var hits = Physics2D.OverlapPointAll(world);
        AnthillView view = null;
        float closestDistance = float.MaxValue;
        foreach (var hit in hits)
        {
            var candidate = hit.GetComponentInParent<AnthillView>();
            if (candidate == null || !candidate.Hill.scouted) continue;
            float distance = Vector2.Distance(world, candidate.Hill.pos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                view = candidate;
            }
        }
        if (scoutController != null && scoutController.CanControlArmy)
        {
            if (view != null && !view.Hill.ownedByPlayer && view.Hill.scouted &&
                scoutController.SelectArmyTarget(view.Hill))
                Debug.Log($"Army marching to {view.Hill.label}.");
            return;
        }
        bool canClickHill = view != null && view.Hill.scouted &&
            (scoutController == null || !scoutController.HasActiveScout ||
             (scoutController.CanOpenHill && scoutController.IsNear(view.Hill)));
        if (canClickHill)
        {
            view.HandleClick();
            return;
        }

    }
}
