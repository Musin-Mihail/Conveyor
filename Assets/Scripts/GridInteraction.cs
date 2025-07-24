using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Обрабатывает клики мыши по сетке и размещает объекты на ячейках,
/// используя новую систему ввода (Input System).
/// </summary>
public class GridInteraction : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Префаб объекта, который будет размещаться поверх ячейки (ваш дополнительный спрайт)")]
    public GameObject overlayPrefab;

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
            }
        }

        if (!overlayPrefab)
        {
            Debug.LogError("Префаб для наложения (overlayPrefab) не назначен в инспекторе!");
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && overlayPrefab && mainCamera)
        {
            HandleMouseClick();
        }
    }

    /// <summary>
    /// Обрабатывает клик мыши, определяет ячейку и размещает на ней объект.
    /// </summary>
    private void HandleMouseClick()
    {
        var mouseScreenPos = Mouse.current.position.ReadValue();
        var mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0;
        var gridWidth = GridGenerator.Grid.GetLength(0);
        var gridHeight = GridGenerator.Grid.GetLength(1);
        var cellSize = GridGenerator.CellSize;
        var shiftedX = mouseWorldPos.x + cellSize / 2f;
        var shiftedY = mouseWorldPos.y + cellSize / 2f;
        var x = Mathf.FloorToInt(shiftedX / cellSize);
        var y = Mathf.FloorToInt(shiftedY / cellSize);
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        {
            Debug.Log("Клик за пределами сетки.");
            return;
        }

        var targetCell = GridGenerator.Grid[x, y];
        var overlayObject = Instantiate(overlayPrefab, targetCell.transform.position, Quaternion.identity, targetCell.transform);
        overlayObject.transform.localPosition = new Vector3(0, 0, -0.1f);
        Debug.Log($"Размещен объект на ячейке: ({x}, {y})");
    }
}