using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Обрабатывает клики мыши по сетке и размещает объекты на ячейках,
/// используя новую систему ввода (Input System).
/// Взаимодействует с GridGenerator через его экземпляр (Singleton).
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
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && overlayPrefab && mainCamera && GridGenerator.Instance)
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
        var gridGenerator = GridGenerator.Instance;
        var gridPos = gridGenerator.WorldToGridPosition(mouseWorldPos);
        if (!gridGenerator.IsValidGridPosition(gridPos))
        {
            Debug.Log("Клик за пределами сетки.");
            return;
        }

        var targetCell = gridGenerator.Grid[gridPos.x, gridPos.y];
        if (targetCell.transform.childCount > 0)
        {
            Debug.Log($"Ячейка ({gridPos.x}, {gridPos.y}) уже занята.");
            return;
        }

        var overlayObject = Instantiate(overlayPrefab, targetCell.transform.position, Quaternion.identity, targetCell.transform);
        overlayObject.transform.localPosition = new Vector3(0, 0, -0.1f);
        Debug.Log($"Размещен объект на ячейке: ({gridPos.x}, {gridPos.y})");
    }
}