using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Обрабатывает взаимодействие игрока с сеткой, например, размещение объектов.
/// Взаимодействует с GridGenerator для получения информации о ячейках.
/// </summary>
public class GridInteraction : MonoBehaviour
{
    [Header("Настройки размещения")]
    [Tooltip("Префаб объекта, который будет размещаться на ячейке")]
    public GameObject conveyorPrefab;
    [Tooltip("Смещение по оси Z для размещаемых объектов, чтобы они были видны над сеткой.")]
    [SerializeField] private float placedObjectZOffset = -0.1f;
    [Header("Ссылки")]
    [Tooltip("Ссылка на основную камеру. Если не указать, будет найдена автоматически")]
    public Camera mainCamera;
    private GridGenerator _gridGenerator;

    private void Start()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
            if (!mainCamera)
            {
                Debug.LogError("Камера не найдена! Убедитесь, что у вас есть камера с тегом 'MainCamera' или назначьте ее вручную в инспекторе.");
                enabled = false;
                return;
            }
        }

        if (!conveyorPrefab)
        {
            Debug.LogError("Префаб для размещения (conveyorPrefab) не назначен в инспекторе!");
            enabled = false;
            return;
        }

        _gridGenerator = GridGenerator.Instance;
        if (!_gridGenerator)
        {
            Debug.LogError("Экземпляр GridGenerator не найден на сцене!");
            enabled = false;
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseClick();
        }
    }

    /// <summary>
    /// Обрабатывает клик мыши, определяет ячейку и размещает на ней объект, если она свободна.
    /// Логика была переработана для использования публичных методов GridGenerator.
    /// </summary>
    private void HandleMouseClick()
    {
        var mouseScreenPos = Mouse.current.position.ReadValue();
        var mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        var gridPosition = _gridGenerator.WorldToGridPosition(mouseWorldPos);
        var targetCell = _gridGenerator.GetCell(gridPosition);
        if (targetCell == null)
        {
            return;
        }

        if (targetCell.IsOccupied)
        {
            Debug.Log($"Ячейка {targetCell.GridPosition} уже занята.");
            return;
        }

        PlaceObjectInCell(targetCell);
    }

    /// <summary>
    /// Создает и размещает префаб в указанной ячейке.
    /// </summary>
    /// <param name="cell">Целевая ячейка для размещения.</param>
    private void PlaceObjectInCell(Cell cell)
    {
        var spawnPosition = cell.WorldPosition;
        spawnPosition.z = placedObjectZOffset;
        var placedObject = Instantiate(
            conveyorPrefab,
            spawnPosition,
            Quaternion.identity,
            _gridGenerator.placedObjectsContainer
        );
        cell.SetPlacedObject(placedObject);
        Debug.Log($"Размещен объект на ячейке: {cell.GridPosition}");
    }
}