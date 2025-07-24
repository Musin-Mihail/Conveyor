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

        if (conveyorPrefab) return;
        Debug.LogError("Префаб для размещения (conveyorPrefab) не назначен в инспекторе!");
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
        if (!conveyorPrefab || !mainCamera || GridGenerator.Instance == null)
        {
            return;
        }

        var mouseScreenPos = Mouse.current.position.ReadValue();
        var mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        var gridGenerator = GridGenerator.Instance;
        mouseWorldPos.z = 0;
        var targetCell = gridGenerator.GetCell(mouseWorldPos);
        if (targetCell == null)
        {
            Debug.Log("Клик за пределами сетки.");
            return;
        }

        if (targetCell.IsOccupied)
        {
            Debug.Log($"Ячейка ({targetCell.GridPosition.x}, {targetCell.GridPosition.y}) уже занята.");
            return;
        }

        var spawnPosition = targetCell.WorldPosition;
        spawnPosition.z = placedObjectZOffset;
        var placedObject = Instantiate(
            conveyorPrefab,
            spawnPosition,
            Quaternion.identity,
            gridGenerator.placedObjectsContainer
        );
        targetCell.SetPlacedObject(placedObject);
        Debug.Log($"Размещен объект на ячейке: ({targetCell.GridPosition.x}, {targetCell.GridPosition.y})");
    }
}