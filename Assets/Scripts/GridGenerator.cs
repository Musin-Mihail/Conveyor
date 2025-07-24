using UnityEngine;

/// <summary>
/// Создает и управляет логической сеткой и ее визуальным представлением.
/// Реализован с использованием паттерна Singleton.
/// </summary>
public class GridGenerator : MonoBehaviour
{
    [Header("Параметры сетки")]
    [Tooltip("Ширина сетки (количество ячеек по X)")]
    public int gridWidth = 100;
    [Tooltip("Высота сетки (количество ячеек по Y)")]
    public int gridHeight = 100;
    [Tooltip("Размер каждой ячейки")]
    public float cellSize = 1.0f;
    [Header("Префабы и контейнеры")]
    [Tooltip("Префаб, который будет использоваться для каждой ячейки сетки (земля)")]
    public GameObject cellPrefab;
    /// <summary>
    /// Контейнер для всех визуальных объектов, размещаемых на сетке.
    /// </summary>
    [Tooltip("Трансформ, который будет родительским для всех размещаемых объектов")]
    public Transform placedObjectsContainer;
    /// <summary>
    /// Статический экземпляр для доступа к GridGenerator из других скриптов.
    /// </summary>
    public static GridGenerator Instance { get; private set; }
    /// <summary>
    /// Двумерный массив, хранящий данные всех ячеек сетки.
    /// </summary>
    private Cell[,] _cellGrid;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Debug.LogWarning("Обнаружен еще один экземпляр GridGenerator. Новый экземпляр будет удален.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (!cellPrefab)
        {
            Debug.LogError("Ошибка: Префаб ячейки (cellPrefab) не назначен в инспекторе!");
            enabled = false;
            return;
        }

        GenerateGrid();
    }

    /// <summary>
    /// Генерирует сетку на основе заданных параметров.
    /// </summary>
    private void GenerateGrid()
    {
        _cellGrid = new Cell[gridWidth, gridHeight];
        var gridContainer = new GameObject("GridContainer")
        {
            transform =
            {
                parent = transform
            }
        };

        if (!placedObjectsContainer)
        {
            placedObjectsContainer = new GameObject("PlacedObjectsContainer").transform;
            placedObjectsContainer.parent = transform;
        }

        for (var x = 0; x < gridWidth; x++)
        {
            for (var y = 0; y < gridHeight; y++)
            {
                var cellPosition = new Vector3(x * cellSize, y * cellSize, 0);
                var groundTile = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                groundTile.name = $"Cell_{x}_{y}";
                groundTile.transform.parent = gridContainer.transform;
                _cellGrid[x, y] = new Cell(cellPosition, new Vector2Int(x, y));
            }
        }

        Debug.Log($"Сетка размером {gridWidth}x{gridHeight} успешно создана!");
    }

    /// <summary>
    /// Преобразует мировую позицию в координаты ячейки на сетке.
    /// </summary>
    /// <param name="worldPosition">Позиция в мировых координатах.</param>
    /// <returns>Координаты ячейки (x, y) в виде Vector2Int.</returns>
    private Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        var x = Mathf.RoundToInt(worldPosition.x / cellSize);
        var y = Mathf.RoundToInt(worldPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Проверяет, находятся ли указанные координаты в пределах сетки.
    /// </summary>
    /// <param name="gridPosition">Координаты ячейки (x, y).</param>
    /// <returns>True, если координаты находятся в пределах сетки, иначе false.</returns>
    private bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }

    /// <summary>
    /// Возвращает объект ячейки по ее координатам.
    /// </summary>
    /// <param name="gridPosition">Координаты ячейки.</param>
    /// <returns>Объект Cell или null, если координаты вне сетки.</returns>
    private Cell GetCell(Vector2Int gridPosition)
    {
        return !IsValidGridPosition(gridPosition) ? null : _cellGrid[gridPosition.x, gridPosition.y];
    }

    /// <summary>
    /// Возвращает объект ячейки по мировой позиции.
    /// </summary>
    /// <param name="worldPosition">Мировая позиция.</param>
    /// <returns>Объект Cell или null, если позиция вне сетки.</returns>
    public Cell GetCell(Vector3 worldPosition)
    {
        var gridPosition = WorldToGridPosition(worldPosition);
        return GetCell(gridPosition);
    }
}