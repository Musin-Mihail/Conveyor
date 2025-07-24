using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет состоянием, типом и ориентацией одного конвейера.
/// Логика была переработана для использования битовой маски, что упрощает определение внешнего вида.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Conveyor : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("Спрайт для прямого конвейера.")]
    public Sprite straightSprite;
    [Tooltip("Спрайт для углового конвейера. Базовая ориентация - соединяет Вверх и Вправо.")]
    public Sprite cornerSprite;
    [Tooltip("Спрайт для Т-образного конвейера. Базовая ориентация - открыт Вверх, Влево и Вправо.")]
    public Sprite tJunctionSprite;
    [Tooltip("Спрайт для крестового конвейера.")]
    public Sprite crossSprite;
    private SpriteRenderer _spriteRenderer;

    /// <summary>
    /// Словарь, который сопоставляет битовую маску соединений с соответствующим типом конвейера (спрайт и поворот).
    /// </summary>
    private Dictionary<int, ConveyorType> _conveyorTypes;

    /// <summary>
    /// Структура для хранения данных о типе конвейера: спрайт и угол поворота.
    /// </summary>
    private struct ConveyorType
    {
        public readonly Sprite Sprite;
        public readonly float Rotation;

        public ConveyorType(Sprite sprite, float rotation)
        {
            Sprite = sprite;
            Rotation = rotation;
        }
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        InitializeConveyorTypes();
    }

    /// <summary>
    /// Инициализирует словарь, заполняя его всеми возможными комбинациями соединений (масками)
    /// и соответствующими им спрайтами и углами поворота.
    /// </summary>
    private void InitializeConveyorTypes()
    {
        _conveyorTypes = new Dictionary<int, ConveyorType>
        {
            // Маска 0: Нет соединений (изолированный конвейер)
            [0] = new(straightSprite, 0),
            // Маска 1: Только вверх
            [1] = new(straightSprite, 0),
            // Маска 2: Только вниз
            [2] = new(straightSprite, 180),
            // Маска 3: Вверх и Вниз (прямой вертикальный)
            [3] = new(straightSprite, 0),
            // Маска 4: Только влево
            [4] = new(straightSprite, 90),
            // Маска 5: Вверх и Влево (угол)
            [5] = new(cornerSprite, 90),
            // Маска 6: Вниз и Влево (угол)
            [6] = new(cornerSprite, 180),
            // Маска 7: Вверх, Вниз, Влево (Т-образный)
            [7] = new(tJunctionSprite, 90),
            // Маска 8: Только вправо
            [8] = new(straightSprite, -90),
            // Маска 9: Вверх и Вправо (угол)
            [9] = new(cornerSprite, 0),
            // Маска 10: Вниз и Вправо (угол)
            [10] = new(cornerSprite, -90),
            // Маска 11: Вверх, Вниз, Вправо (Т-образный)
            [11] = new(tJunctionSprite, -90),
            // Маска 12: Влево и Вправо (прямой горизонтальный)
            [12] = new(straightSprite, 90),
            // Маска 13: Вверх, Влево, Вправо (Т-образный)
            [13] = new(tJunctionSprite, 0),
            // Маска 14: Вниз, Влево, Вправо (Т-образный)
            [14] = new(tJunctionSprite, 180),
            // Маска 15: Все 4 направления (крестовина)
            [15] = new(crossSprite, 0)
        };
    }

    /// <summary>
    /// Обновляет внешний вид конвейера на основе маски его соединений.
    /// </summary>
    /// <param name="connectionMask">Битовуя маска, представляющая соединения с соседями.</param>
    public void UpdateState(int connectionMask)
    {
        if (_conveyorTypes.TryGetValue(connectionMask, out var type))
        {
            _spriteRenderer.sprite = type.Sprite;
            transform.rotation = Quaternion.Euler(0, 0, type.Rotation);
        }
        else
        {
            _spriteRenderer.sprite = straightSprite;
            transform.rotation = Quaternion.identity;
            Debug.LogWarning($"Не найдена конфигурация для маски соединений: {connectionMask}");
        }
    }
}