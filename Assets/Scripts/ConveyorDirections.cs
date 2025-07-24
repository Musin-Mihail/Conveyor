using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Определяет направления для соединений конвейеров с помощью битовых флагов.
/// Атрибут [System.Flags] позволяет комбинировать значения (например, Up | Left).
/// </summary>
[Flags]
public enum DirectionFlags
{
    None = 0,
    Up = 1, // Двоичное представление: 0001
    Down = 2, // Двоичное представление: 0010
    Left = 4, // Двоичное представление: 0100
    Right = 8, // Двоичное представление: 1000
}

/// <summary>
/// Статический класс-помощник для работы с направлениями конвейеров.
/// Содержит удобные сопоставления между флагами направлений и векторами.
/// </summary>
public static class ConveyorDirections
{
    /// <summary>
    /// Словарь, сопоставляющий флаг направления с соответствующим вектором смещения в сетке.
    /// Является статическим и доступным только для чтения для обеспечения безопасности.
    /// </summary>
    public static readonly Dictionary<DirectionFlags, Vector2Int> DirectionVectors = new()
    {
        [DirectionFlags.Up] = Vector2Int.up,
        [DirectionFlags.Down] = Vector2Int.down,
        [DirectionFlags.Left] = Vector2Int.left,
        [DirectionFlags.Right] = Vector2Int.right
    };

    /// <summary>
    /// Возвращает противоположное направление для заданного.
    /// </summary>
    /// <param name="direction">Исходное направление.</param>
    /// <returns>Противоположное направление.</returns>
    public static DirectionFlags GetOppositeDirection(DirectionFlags direction)
    {
        return direction switch
        {
            DirectionFlags.Up => DirectionFlags.Down,
            DirectionFlags.Down => DirectionFlags.Up,
            DirectionFlags.Left => DirectionFlags.Right,
            DirectionFlags.Right => DirectionFlags.Left,
            _ => DirectionFlags.None
        };
    }
}