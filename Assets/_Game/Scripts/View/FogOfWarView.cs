using System;
using UnityEngine;

public sealed class FogOfWarView : MonoBehaviour
{
    Texture2D texture;
    SpriteRenderer spriteRenderer;
    Vector2 worldSize;
    float cellSize;
    int width;
    int height;

    public static FogOfWarView Create(Vector2 size, float discoveryCellSize)
    {
        var view = new GameObject("Fog of War").AddComponent<FogOfWarView>();
        view.worldSize = size;
        view.cellSize = discoveryCellSize;
        view.width = Mathf.CeilToInt(size.x / discoveryCellSize);
        view.height = Mathf.CeilToInt(size.y / discoveryCellSize);
        view.texture = new Texture2D(view.width, view.height, TextureFormat.RGBA32, false);
        view.texture.filterMode = FilterMode.Point;
        view.texture.wrapMode = TextureWrapMode.Clamp;
        view.spriteRenderer = view.gameObject.AddComponent<SpriteRenderer>();
        view.spriteRenderer.sortingOrder = 100;
        view.spriteRenderer.sprite = Sprite.Create(
            view.texture,
            new Rect(0f, 0f, view.width, view.height),
            new Vector2(0.5f, 0.5f),
            1f / discoveryCellSize);
        view.transform.position = Vector3.zero;
        view.transform.localScale = new Vector3(
            size.x / (view.width * discoveryCellSize),
            size.y / (view.height * discoveryCellSize),
            1f);
        return view;
    }

    public void Refresh(Func<Vector2, bool> isDiscovered)
    {
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(
                    -worldSize.x * 0.5f + (x + 0.5f) * cellSize,
                    -worldSize.y * 0.5f + (y + 0.5f) * cellSize);
                pixels[y * width + x] = isDiscovered(position)
                    ? new Color(0f, 0f, 0f, 0f)
                    : new Color(0f, 0f, 0f, 0.82f);
            }
        texture.SetPixels(pixels);
        texture.Apply(false);
    }
}
