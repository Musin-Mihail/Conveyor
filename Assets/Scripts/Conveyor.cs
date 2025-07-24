using UnityEngine;

/// <summary>
/// Управляет состоянием и ориентацией одного конвейера.
/// </summary>
public class Conveyor : MonoBehaviour
{
    /// <summary>
    /// Перечисление возможных направлений для конвейера.
    /// </summary>
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Устанавливает направление и поворачивает объект в соответствии с ним.
    /// </summary>
    /// <param name="direction">Новое направление.</param>
    public void SetRotation(Direction direction)
    {
        transform.rotation = Quaternion.Euler(0, 0, GetRotationForDirection(direction));
    }

    /// <summary>
    /// Возвращает угол поворота в градусах для указанного направления.
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