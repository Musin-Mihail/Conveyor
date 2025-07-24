using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управляет состоянием, типом и ориентацией одного конвейера.
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
    /// Флаг, который указывает, был ли этот конвейер изменен
    /// в результате размещения соседнего конвейера.
    /// Используется для опциональной блокировки изменений.
    /// </summary>
    public bool WasModifiedByNeighbor { get; private set; }

    /// <summary>
    /// Хранит последнюю примененную маску соединений.
    /// Это позволяет другим конвейерам проверять, в какие стороны открыт данный конвейер.
    /// </summary>
    public int ConnectionMask { get; private set; }

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
            [(int)DirectionFlags.None] = new(straightSprite, 0),

            // 1 соединение
            [(int)DirectionFlags.Up] = new(straightSprite, 0),
            [(int)DirectionFlags.Down] = new(straightSprite, 180),
            [(int)DirectionFlags.Left] = new(straightSprite, 90),
            [(int)DirectionFlags.Right] = new(straightSprite, -90),

            // 2 соединения (прямые)
            [(int)(DirectionFlags.Up | DirectionFlags.Down)] = new(straightSprite, 0),
            [(int)(DirectionFlags.Left | DirectionFlags.Right)] = new(straightSprite, 90),

            // 2 соединения (угловые)
            [(int)(DirectionFlags.Up | DirectionFlags.Right)] = new(cornerSprite, 0),
            [(int)(DirectionFlags.Up | DirectionFlags.Left)] = new(cornerSprite, 90),
            [(int)(DirectionFlags.Down | DirectionFlags.Left)] = new(cornerSprite, 180),
            [(int)(DirectionFlags.Down | DirectionFlags.Right)] = new(cornerSprite, -90),

            // 3 соединения (Т-образные)
            [(int)(DirectionFlags.Up | DirectionFlags.Left | DirectionFlags.Right)] = new(tJunctionSprite, 0),
            [(int)(DirectionFlags.Up | DirectionFlags.Down | DirectionFlags.Left)] = new(tJunctionSprite, 90),
            [(int)(DirectionFlags.Down | DirectionFlags.Left | DirectionFlags.Right)] = new(tJunctionSprite, 180),
            [(int)(DirectionFlags.Up | DirectionFlags.Down | DirectionFlags.Right)] = new(tJunctionSprite, -90),

            // 4 соединения (крестовина)
            [(int)(DirectionFlags.Up | DirectionFlags.Down | DirectionFlags.Left | DirectionFlags.Right)] = new(crossSprite, 0)
        };
    }

    /// <summary>
    /// Помечает этот конвейер как измененный соседом.
    /// </summary>
    public void MarkAsModifiedByNeighbor()
    {
        WasModifiedByNeighbor = true;
    }

    /// <summary>
    /// Обновляет внешний вид конвейера на основе маски его соединений.
    /// </summary>
    /// <param name="connectionMask">Битовуя маска, представляющая соединения с соседями.</param>
    public void UpdateState(int connectionMask)
    {
        ConnectionMask = connectionMask;
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