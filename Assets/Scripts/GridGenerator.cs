using UnityEngine;

/// <summary>
/// Этот скрипт создает сетку из префабов в сцене Unity.
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
    [Tooltip("Префаб, который будет использоваться для каждой ячейки сетки")]
    public GameObject cellPrefab;

    private void Start()
    {
        if (!cellPrefab)
        {
            Debug.LogError("Ошибка: Префаб ячейки (Cell Prefab) не назначен в инспекторе!");
            return;
        }

        GenerateGrid();
    }

    /// <summary>
    /// Генерирует сетку на основе заданных параметров.
    /// </summary>
    private void GenerateGrid()
    {
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
            }
        }

        Debug.Log($"Сетка размером {gridWidth}x{gridHeight} успешно создана!");
    }
}