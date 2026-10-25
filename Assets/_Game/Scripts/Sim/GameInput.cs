using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// One place for all input. Works with the new Input System or the legacy one,
/// so no project settings need changing.
public static class GameInput
{
    public static Vector2 MousePos
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }
    }

    public static bool LeftClickDown
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }
    }

    /// Right or middle mouse held (camera drag).
    public static bool PanHeld
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null &&
                   (Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed);
#else
            return Input.GetMouseButton(1) || Input.GetMouseButton(2);
#endif
        }
    }

    /// WASD / arrow keys as a vector.
    public static Vector2 Move
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k == null) return Vector2.zero;
            float x = ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1f : 0f) - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1f : 0f);
            float y = ((k.wKey.isPressed || k.upArrowKey.isPressed) ? 1f : 0f) - ((k.sKey.isPressed || k.downArrowKey.isPressed) ? 1f : 0f);
            return new Vector2(x, y);
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }
    }

}
