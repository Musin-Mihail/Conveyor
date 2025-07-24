using UnityEngine;

/// <summary>
/// Управляет состоянием, типом и ориентацией одного конвейера.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Conveyor : MonoBehaviour
{
    /// <summary>
    /// Перечисление возможных направлений для прямого конвейера.
    /// </summary>
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
    [Header("Sprites")]
    [Tooltip("Спрайт для прямого конвейера.")]
    public Sprite straightSprite;
    [Tooltip("Спрайт для углового конвейера. Базовая ориентация - соединяет Вверх и Вправо.")]
    public Sprite cornerSprite;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Устанавливает тип конвейера (прямой) и поворачивает его в соответствии с направлением.
    /// </summary>
    /// <param name="direction">Новое направление.</param>
    public void SetStraight(Direction direction)
    {
        if (_spriteRenderer.sprite != straightSprite)
        {
            _spriteRenderer.sprite = straightSprite;
        }

        transform.rotation = Quaternion.Euler(0, 0, GetRotationForDirection(direction));
    }

    /// <summary>
    /// НОВОЕ: Устанавливает тип конвейера (угловой) и поворачивает его на заданный угол.
    /// </summary>
    /// <param name="angle">Угол поворота в градусах.</param>
    public void SetCorner(float angle)
    {
        if (_spriteRenderer.sprite != cornerSprite)
        {
            _spriteRenderer.sprite = cornerSprite;
        }

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Возвращает угол поворота в градусах для указанного направления (для прямых конвейеров).
    /// </summary>
    /// <param name="direction">Направление.</param>
    /// <returns>Угол в градусах.</returns>
    private static float GetRotationForDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => 0,
            Direction.Right => -90,
            Direction.Down => 180,
            Direction.Left => 90,
            _ => 0
        };
    }
}