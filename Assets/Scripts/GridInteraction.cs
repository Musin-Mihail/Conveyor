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
    /// Создает, размещает объект в ячейке и обновляет ориентацию
    /// нового конвейера и его соседей для автоматического соединения.
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
        Debug.Log($"Размещен конвейер на {cell.GridPosition}. Запускается обновление соседей.");
        UpdateSurroundingConveyors(cell);
    }

    /// <summary>
    /// Обновляет ориентацию конвейера в указанной ячейке и всех ее соседних конвейеров.
    /// Это гарантирует, что все конвейеры в области правильно соединены.
    /// </summary>
    /// <param name="centerCell">Ячейка, вокруг которой нужно произвести обновление.</param>
    private void UpdateSurroundingConveyors(Cell centerCell)
    {
        if (centerCell.IsOccupied)
        {
            UpdateSingleConveyorOrientation(centerCell);
        }

        foreach (var neighborCell in _gridGenerator.GetNeighbors(centerCell).Where(neighborCell => neighborCell.IsOccupied))
        {
            UpdateSingleConveyorOrientation(neighborCell);
        }
    }

    /// <summary>
    /// Определяет и устанавливает правильную ориентацию для одного конвейера
    /// на основе его текущих соседей.
    /// </summary>
    /// <param name="cell">Ячейка с конвейером для обновления.</param>
    private void UpdateSingleConveyorOrientation(Cell cell)
    {
        var conveyor = cell.GetConveyor();
        if (!conveyor) return;

        var neighborConveyors = _gridGenerator.GetNeighbors(cell)
            .Where(c => c.IsOccupied && c.GetConveyor())
            .ToList();

        switch (neighborConveyors.Count)
        {
            case 0:
                conveyor.SetStraight(Conveyor.Direction.Up);
                break;
            case 1:
            {
                var directionVector = neighborConveyors[0].GridPosition - cell.GridPosition;
                conveyor.SetStraight(GetDirectionFromVector(directionVector));
                break;
            }
            case 2:
            {
                var dirToFirst = neighborConveyors[0].GridPosition - cell.GridPosition;
                var dirToSecond = neighborConveyors[1].GridPosition - cell.GridPosition;

                if (dirToFirst == -dirToSecond)
                {
                    conveyor.SetStraight(GetDirectionFromVector(dirToFirst));
                }
                else
                {
                    conveyor.SetCorner(GetAngleForCorner(dirToFirst, dirToSecond));
                }

                break;
            }
            case 3:
            {
                var angle = GetAngleForTJunction(neighborConveyors, cell);
                conveyor.SetTJunction(angle);
                break;
            }
            case 4:
            {
                conveyor.SetCross();
                break;
            }
            default:
                conveyor.SetStraight(Conveyor.Direction.Up);
                break;
        }
    }

    /// <summary>
    /// Вычисляет угол для Т-образного конвейера.
    /// Находит единственное направление, где нет соседа, и поворачивает "открытую" часть конвейера туда.
    /// </summary>
    /// <param name="neighbors">Список из трех соседних ячеек с конвейерами.</param>
    /// <param name="center">Центральная ячейка, для которой вычисляется поворот.</param>
    /// <returns>Угол поворота в градусах.</returns>
    private float GetAngleForTJunction(List<Cell> neighbors, Cell center)
    {
        var neighborDirections = neighbors.Select(n => n.GridPosition - center.GridPosition).ToList();

        var allDirections = new List<Vector2Int>
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        var missingDirection = allDirections.FirstOrDefault(dir => !neighborDirections.Contains(dir));

        if (missingDirection == Vector2Int.up) return 180f;
        if (missingDirection == Vector2Int.down) return 0f;
        if (missingDirection == Vector2Int.left) return -90f;
        if (missingDirection == Vector2Int.right) return 90f;

        return 0f;
    }

    /// <summary>
    /// Вычисляет угол поворота для углового конвейера на основе векторов к его двум соседям.
    /// </summary>
    /// <param name="dir1">Вектор направления к первому соседу.</param>
    /// <param name="dir2">Вектор направления ко второму соседу.</param>
    /// <returns>Угол в градусах для поворота спрайта.</returns>
    private float GetAngleForCorner(Vector2Int dir1, Vector2Int dir2)
    {
        var cornerDirection = dir1 + dir2;
        if (cornerDirection == new Vector2Int(1, 1)) return 0;
        if (cornerDirection == new Vector2Int(1, -1)) return -90;
        if (cornerDirection == new Vector2Int(-1, -1)) return 180;
        if (cornerDirection == new Vector2Int(-1, 1)) return 90;
        Debug.LogWarning($"Не удалось определить угол для направлений {dir1} и {dir2}");
        return 0;
    }

    /// <summary>
    /// Преобразует вектор направления в перечисление Conveyor.Direction.
    /// </summary>
    private Conveyor.Direction GetDirectionFromVector(Vector2Int vector)
    {
        if (vector == Vector2Int.up) return Conveyor.Direction.Up;
        if (vector == Vector2Int.down) return Conveyor.Direction.Down;
        if (vector == Vector2Int.left) return Conveyor.Direction.Left;
        if (vector == Vector2Int.right) return Conveyor.Direction.Right;
        return Conveyor.Direction.None;
    }
}