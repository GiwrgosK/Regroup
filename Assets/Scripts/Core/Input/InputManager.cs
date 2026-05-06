using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Vector2 GetMousePosition() {
        if (Mouse.current == null) return Vector2.zero;
        return Mouse.current.position.ReadValue();
    }

    public bool IsLeftMouseButtonPressed() {
        if (Mouse.current == null) return false;
        return Mouse.current.leftButton.wasPressedThisFrame;
    }

    public Vector2 GetCameraMovement() {
        Vector2 inputMoveDir = new Vector2(0, 0);
        if (Keyboard.current == null) return inputMoveDir;

		if (Keyboard.current.wKey.isPressed) inputMoveDir.y = +1f;
		if (Keyboard.current.sKey.isPressed) inputMoveDir.y = -1f;
		if (Keyboard.current.dKey.isPressed) inputMoveDir.x = +1f;
		if (Keyboard.current.aKey.isPressed) inputMoveDir.x = -1f;
        return inputMoveDir;
    }

    public float GetCameraRotation() {
        float rotationAmount = 0;
        if (Keyboard.current == null) return rotationAmount;

        if (Keyboard.current.qKey.isPressed) rotationAmount = +1f;
		if (Keyboard.current.eKey.isPressed) rotationAmount = -1f;
        return rotationAmount;
    }

    public float GetCameraZoom() {
        float zoomAmount = 0f;
        if (Mouse.current == null) return zoomAmount;

        float scrollY = Mouse.current.scroll.ReadValue().y;
		if (scrollY > 0) zoomAmount = -1f;
		if (scrollY < 0) zoomAmount = 1f;
        return zoomAmount;
    }

    public bool IsShiftPressed() {
        if (Keyboard.current == null) return false;
        return Keyboard.current.leftShiftKey.isPressed;
    }

    public bool IsEscapePressed() {
        if (Keyboard.current == null) return false;
        return Keyboard.current.escapeKey.wasPressedThisFrame;
    }
}