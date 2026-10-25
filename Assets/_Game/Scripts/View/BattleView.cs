using TMPro;
using UnityEngine;

public class BattleView : MonoBehaviour
{
    TMP_Text label;

    public void Bind(Vector2 position)
    {
        label = gameObject.AddComponent<TextMeshPro>();
        label.fontSize = 2.4f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        var meshRenderer = label.GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = "Default";
        meshRenderer.sortingOrder = 1000;
        transform.position = new Vector3(position.x, position.y + 2.3f, -5f);
    }

    public void SetCounts(int attackers, int defenders)
    {
        if (label != null)
            label.text = $"ATK {Mathf.Max(0, attackers)}  DEF {Mathf.Max(0, defenders)}";
    }
}
