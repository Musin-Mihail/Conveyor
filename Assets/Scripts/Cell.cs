using UnityEngine;

/// <summary>
/// Представляет данные для одной ячейки сетки.
/// </summary>
public class Cell
{
    public Cell(Vector3 worldPosition, Vector2Int gridPosition)
    {
        WorldPosition = worldPosition;
        GridPosition = gridPosition;
    }

    /// <summary>
    /// Мировая позиция центра ячейки.
    /// </summary>
    public Vector3 WorldPosition { get; }

    /// <summary>
    /// Координаты ячейки в сетке.
    /// </summary>
    public Vector2Int GridPosition { get; }

    /// <summary>
    /// Объект, размещенный в этой ячейке (например, конвейер, здание).
    /// Доступен для чтения извне, но устанавливается только внутри логики ячейки.
    /// </summary>
    private GameObject PlacedObject { get; set; }

    /// <summary>
    /// Возвращает true, если в ячейке размещен какой-либо объект.
    /// </summary>
    public bool IsOccupied => PlacedObject;

    /// <summary>
    /// Устанавливает игровой объект в эту ячейку.
    /// </summary>
    /// <param name="placedObject">Объект для размещения.</param>
    public void SetPlacedObject(GameObject placedObject)
    {
        PlacedObject = placedObject;
    }

    /// <summary>
    /// Возвращает компонент Conveyor из размещенного объекта, если он есть.
    /// </summary>
    /// <returns>Компонент Conveyor или null.</returns>
    public Conveyor GetConveyor()
    {
        return IsOccupied ? PlacedObject.GetComponent<Conveyor>() : null;
    }
}