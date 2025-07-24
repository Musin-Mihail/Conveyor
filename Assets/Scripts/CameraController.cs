using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Управляет перемещением и масштабированием основной камеры.
/// Поддерживает движение с помощью клавиш WASD, панорамирование средней кнопкой мыши
/// и масштабирование колесиком мыши.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Скорость перемещения камеры с помощью клавиатуры.")]
    public float moveSpeed = 15f;
    [Header("Настройки масштабирования")]
    [Tooltip("Скорость приближения/отдаления камеры.")]
    public float zoomSpeed = 100f;
    [Tooltip("Минимальное значение ортографического размера (максимальное приближение).")]
    public float minZoom = 5f;
    [Tooltip("Максимальное значение ортографического размера (максимальное отдаление).")]
    public float maxZoom = 50f;
    private Camera _camera;
    private Vector3 _dragOrigin;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleKeyboardMovement();
        HandleMouseZoom();
        HandleMousePan();
    }

    /// <summary>
    /// Обрабатывает перемещение камеры с помощью клавиш W, A, S, D.
    /// </summary>
    private void HandleKeyboardMovement()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        var moveDirection = Vector3.zero;
        if (keyboard.wKey.isPressed) moveDirection += Vector3.up;
        if (keyboard.sKey.isPressed) moveDirection += Vector3.down;
        if (keyboard.aKey.isPressed) moveDirection += Vector3.left;
        if (keyboard.dKey.isPressed) moveDirection += Vector3.right;
        if (moveDirection.sqrMagnitude > 1)
        {
            moveDirection.Normalize();
        }

        transform.position += moveDirection * (moveSpeed * _camera.orthographicSize * Time.deltaTime);
    }

    /// <summary>
    /// Обрабатывает масштабирование (зум) с помощью колесика мыши.
    /// Работает для ортографической камеры.
    /// </summary>
    private void HandleMouseZoom()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        var scroll = mouse.scroll.ReadValue().y;

        if (!(Mathf.Abs(scroll) > 0.1f)) return;
        var newSize = _camera.orthographicSize - scroll * zoomSpeed * Time.deltaTime;
        _camera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }

    /// <summary>
    /// Обрабатывает панорамирование (перемещение) камеры с помощью средней кнопки мыши.
    /// </summary>
    private void HandleMousePan()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (mouse.middleButton.wasPressedThisFrame)
        {
            _dragOrigin = _camera.ScreenToWorldPoint(mouse.position.ReadValue());
        }

        if (!mouse.middleButton.isPressed) return;
        var difference = _dragOrigin - _camera.ScreenToWorldPoint(mouse.position.ReadValue());
        transform.position += difference;
    }
}