using System.Collections.Generic;
using System.Linq;
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

        if (conveyorPrefab.GetComponent<Conveyor>() == null)
        {
            Debug.LogError("На префабе 'conveyorPrefab' отсутствует компонент 'Conveyor'! Добавьте его.");
            enabled = false;
            return;
        }

        _gridGenerator = GridGenerator.Instance;
        if (_gridGenerator) return;
        Debug.LogError("Экземпляр GridGenerator не найден на сцене!");
        enabled = false;
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
    /// ИЗМЕНЕНО: Создает, размещает и автоматически поворачивает префаб в указанной ячейке.
    /// Если рядом есть один соседний конвейер, они поворачиваются друг к другу.
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
        var newConveyor = placedObject.GetComponent<Conveyor>();
        var neighbors = _gridGenerator.GetNeighbors(cell);
        var neighborConveyors = (from neighborCell in neighbors let neighborConveyor = neighborCell.GetConveyor() where neighborConveyor select new KeyValuePair<Cell, Conveyor>(neighborCell, neighborConveyor)).ToList();
        if (neighborConveyors.Count == 1)
        {
            var (neighborCell, neighborConveyor) = neighborConveyors[0];
            var directionToNeighborVector = neighborCell.GridPosition - cell.GridPosition;
            var directionToNeighbor = GetDirectionFromVector(directionToNeighborVector);
            if (directionToNeighbor != Conveyor.Direction.None)
            {
                newConveyor.SetRotation(directionToNeighbor);
            }

            var directionToNewVector = cell.GridPosition - neighborCell.GridPosition;
            var directionToNew = GetDirectionFromVector(directionToNewVector);
            if (directionToNew != Conveyor.Direction.None)
            {
                neighborConveyor.SetRotation(directionToNew);
            }

            Debug.Log($"Конвейер на {cell.GridPosition} соединен с конвейером на {neighborCell.GridPosition}.");
        }
        else
        {
            newConveyor.SetRotation(Conveyor.Direction.Up);
            Debug.Log($"Размещен конвейер на {cell.GridPosition}. Подходящих соседей: {neighborConveyors.Count}. Установлено направление по умолчанию.");
        }
    }

    /// <summary>
    /// Преобразует вектор направления в перечисление Conveyor.Direction.
    /// </summary>
    private Conveyor.Direction GetDirectionFromVector(Vector2Int vector)
    {
        if (vector == Vector2Int.up) return Conveyor.Direction.Up;
        if (vector == Vector2Int.down) return Conveyor.Direction.Down;
        if (vector == Vector2Int.left) return Conveyor.Direction.Left;
        return vector == Vector2Int.right ? Conveyor.Direction.Right : Conveyor.Direction.None;
    }
}