using UnityEngine;

public static class PlaceholderSprite
{
    static Sprite circle;
    static Sprite square;

    public static Sprite Circle
    {
        get
        {
            if (circle == null) circle = Create(true);
            return circle;
        }
    }

    public static Sprite Square
    {
        get
        {
            if (square == null) square = Create(false);
            return square;
        }
    }

    static Sprite Create(bool isCircle)
    {
        const int size = 32;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool filled = !isCircle ||
                    Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) <= 15f;
                pixels[y * size + x] = filled ? Color.white : Color.clear;
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size);
    }
}
