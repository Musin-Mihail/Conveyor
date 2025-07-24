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
    [Header("Визуальное представление")]
    [Tooltip("Материал, который будет использоваться для поверхности сетки (земли)")]
    public Material gridMaterial;
    [Header("Контейнеры")]
    [Tooltip("Трансформ, который будет родительским для всех размещаемых объектов")]
    public Transform placedObjectsContainer;
    /// <summary>
    /// Статический экземпляр для доступа к GridGenerator из других скриптов.
    /// </summary>
    public static GridGenerator Instance { get; private set; }
    /// <summary>
    /// Двумерный массив, хранящий данные всех ячеек сетки.
    /// Доступен для чтения другим системам.
    /// </summary>
    private Cell[,] CellGrid { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Обнаружен еще один экземпляр GridGenerator. Новый экземпляр будет удален.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (!gridMaterial)
        {
            Debug.LogError("Ошибка: Материал для сетки (gridMaterial) не назначен в инспекторе!");
            enabled = false;
            return;
        }

        GenerateGrid();
    }

    /// <summary>
    /// Генерирует логическую сетку и создает ее визуальное представление в виде единого меша.
    /// </summary>
    private void GenerateGrid()
    {
        CellGrid = new Cell[gridWidth, gridHeight];
        if (!placedObjectsContainer)
        {
            placedObjectsContainer = new GameObject("PlacedObjectsContainer").transform;
            placedObjectsContainer.parent = transform;
        }

        for (var x = 0; x < gridWidth; x++)
        {
            for (var y = 0; y < gridHeight; y++)
            {
                var cellPosition = new Vector3(x * cellSize + cellSize * 0.5f, y * cellSize + cellSize * 0.5f, 0);
                CellGrid[x, y] = new Cell(cellPosition, new Vector2Int(x, y));
            }
        }

        GenerateGridMesh();
        Debug.Log($"Сетка размером {gridWidth}x{gridHeight} успешно создана!");
    }

    /// <summary>
    /// Создает единый меш для визуального отображения всей сетки.
    /// </summary>
    private void GenerateGridMesh()
    {
        var gridMeshObject = new GameObject("GridMesh")
        {
            transform =
            {
                parent = transform
            }
        };
        var meshFilter = gridMeshObject.AddComponent<MeshFilter>();
        var meshRenderer = gridMeshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = gridMaterial;
        var mesh = new Mesh
        {
            name = "Procedural Grid"
        };

        var quadCount = gridWidth * gridHeight;
        var vertices = new Vector3[quadCount * 4];
        var uvs = new Vector2[quadCount * 4];
        var triangles = new int[quadCount * 6];

        for (var x = 0; x < gridWidth; x++)
        {
            for (var y = 0; y < gridHeight; y++)
            {
                var index = y * gridWidth + x;
                var vIndex = index * 4;
                var tIndex = index * 6;
                var center = CellGrid[x, y].WorldPosition;
                var halfSize = cellSize * 0.5f;
                vertices[vIndex + 0] = new Vector3(center.x - halfSize, center.y - halfSize, 0);
                vertices[vIndex + 1] = new Vector3(center.x - halfSize, center.y + halfSize, 0);
                vertices[vIndex + 2] = new Vector3(center.x + halfSize, center.y + halfSize, 0);
                vertices[vIndex + 3] = new Vector3(center.x + halfSize, center.y - halfSize, 0);
                uvs[vIndex + 0] = new Vector2(0, 0);
                uvs[vIndex + 1] = new Vector2(0, 1);
                uvs[vIndex + 2] = new Vector2(1, 1);
                uvs[vIndex + 3] = new Vector2(1, 0);
                triangles[tIndex + 0] = vIndex + 0;
                triangles[tIndex + 1] = vIndex + 1;
                triangles[tIndex + 2] = vIndex + 2;
                triangles[tIndex + 3] = vIndex + 0;
                triangles[tIndex + 4] = vIndex + 2;
                triangles[tIndex + 5] = vIndex + 3;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// Преобразует мировую позицию в координаты ячейки на сетке.
    /// </summary>
    /// <param name="worldPosition">Позиция в мировых координатах.</param>
    /// <returns>Координаты ячейки (x, y) в виде Vector2Int.</returns>
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        var x = Mathf.FloorToInt(worldPosition.x / cellSize);
        var y = Mathf.FloorToInt(worldPosition.y / cellSize);
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
    /// Возвращает объект ячейки по ее координатам в сетке.
    /// </summary>
    /// <param name="gridPosition">Координаты ячейки.</param>
    /// <returns>Объект Cell или null, если координаты вне сетки.</returns>
    public Cell GetCell(Vector2Int gridPosition)
    {
        return !IsValidGridPosition(gridPosition) ? null : CellGrid[gridPosition.x, gridPosition.y];
    }
}