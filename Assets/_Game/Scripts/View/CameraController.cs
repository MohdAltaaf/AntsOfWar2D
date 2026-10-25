using UnityEngine;

/// Put on the Main Camera. Right or middle mouse drag pans the map.
/// Clamped to MapData.worldSize. Starts centred on the player's hill.
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    Camera cam;
    Vector2 lastMouse;
    bool dragging;
    bool centred;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void LateUpdate()
    {
        if (GameBootstrap.I == null || GameBootstrap.I.State == null) return;

        if (!centred) { CentreOnPlayer(); centred = true; }
        if (GameInput.PanHeld)
        {
            if (!dragging)
            {
                lastMouse = GameInput.MousePos;
                dragging = true;
            }
            else
            {
                Vector2 currentMouse = GameInput.MousePos;
                Vector2 screenDelta = currentMouse - lastMouse;
                float worldUnitsPerPixel = (cam.orthographicSize * 2f) / Screen.height;
                Vector3 position = transform.position;
                position -= new Vector3(screenDelta.x, screenDelta.y, 0f) * worldUnitsPerPixel;
                transform.position = Clamp(position);
                lastMouse = currentMouse;
            }
            return;
        }
        dragging = false;

        var scoutController = GameBootstrap.I.GetComponent<ScoutController>();
        if (scoutController != null && scoutController.HasActiveArmy)
            transform.position = Clamp(new Vector3(scoutController.FocusPosition.x, scoutController.FocusPosition.y, -10f));
        else if (scoutController != null && scoutController.HasActiveScout)
            transform.position = Clamp(new Vector3(scoutController.FocusPosition.x, scoutController.FocusPosition.y, -10f));
    }

    void CentreOnPlayer()
    {
        foreach (var h in GameBootstrap.I.State.hills)
            if (h.ownedByPlayer) { transform.position = Clamp(new Vector3(h.pos.x, h.pos.y, -10f)); return; }
    }

    Vector3 Clamp(Vector3 p)
    {
        Vector2 size = GameBootstrap.I.map.worldSize;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float maxX = Mathf.Max(0f, size.x / 2f - halfW);
        float maxY = Mathf.Max(0f, size.y / 2f - halfH);
        p.x = Mathf.Clamp(p.x, -maxX, maxX);
        p.y = Mathf.Clamp(p.y, -maxY, maxY);
        p.z = -10f;
        return p;
    }

    void OnDrawGizmosSelected()
    {
        if (GameBootstrap.I == null || GameBootstrap.I.map == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, GameBootstrap.I.map.worldSize);
    }
}
