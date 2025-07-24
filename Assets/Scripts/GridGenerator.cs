using UnityEngine;

/// <summary>
/// Этот скрипт создает сетку из префабов, хранит ссылки на них и предоставляет централизованный доступ к сетке.
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
    [Header("Префаб")]
    [Tooltip("Префаб, который будет использоваться для каждой ячейки сетки (земля)")]
    public GameObject cellPrefab;
    /// <summary>
    /// Статический экземпляр для доступа к GridGenerator из других скриптов.
    /// </summary>
    public static GridGenerator Instance { get; private set; }
    /// <summary>
    /// Двумерный массив, хранящий все ячейки сетки.
    /// </summary>
    public GameObject[,] Grid { get; private set; }
    /// <summary>
    /// Размер ячейки сетки.
    /// </summary>
    private float CellSize { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Обнаружен еще один экземпляр GridGenerator. Новый экземпляр будет удален.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (!cellPrefab)
        {
            Debug.LogError("Ошибка: Префаб ячейки (cellPrefab) не назначен в инспекторе!");
            return;
        }

        GenerateGrid();
    }

    /// <summary>
    /// Генерирует сетку на основе заданных параметров.
    /// </summary>
    private void GenerateGrid()
    {
        Grid = new GameObject[gridWidth, gridHeight];
        CellSize = cellSize;
        var gridContainer = new GameObject("GridContainer")
        {
            transform =
            {
                parent = transform
            }
        };
        for (var x = 0; x < gridWidth; x++)
        {
            for (var y = 0; y < gridHeight; y++)
            {
                var cellPosition = new Vector3(x * cellSize, y * cellSize, 0);
                var newCell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                newCell.name = $"Cell_{x}_{y}";
                newCell.transform.parent = gridContainer.transform;
                Grid[x, y] = newCell;
            }
        }

        Debug.Log($"Сетка размером {gridWidth}x{gridHeight} успешно создана!");
    }

    /// <summary>
    /// Преобразует мировую позицию в координаты ячейки на сетке.
    /// </summary>
    /// <param name="worldPosition">Позиция в мировых координатах.</param>
    /// <returns>Координаты ячейки (x, y) в виде Vector2Int.</returns>
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        var shiftedX = worldPosition.x + CellSize / 2f;
        var shiftedY = worldPosition.y + CellSize / 2f;
        var x = Mathf.FloorToInt(shiftedX / CellSize);
        var y = Mathf.FloorToInt(shiftedY / CellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Проверяет, находятся ли указанные координаты в пределах сетки.
    /// </summary>
    /// <param name="gridPosition">Координаты ячейки (x, y).</param>
    /// <returns>True, если координаты находятся в пределах сетки, иначе false.</returns>
    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }
}