using System.Numerics;

namespace raylib_cs_utils.Spatial;

public class SpatialGrid<T> where T: ISpatialEntity
{
    private List<T>[,] _cells;
    private float _cellSize;
    private int _gridWidth;
    private int _gridHeight;

    public SpatialGrid(int width, int height, float cellSize)
    {
        _gridWidth = width;
        _gridHeight = height;
        _cellSize = cellSize;
        _cells = new List<T>[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _cells[x, y] = new List<T>();
            }
        }
    }

    public void RebuildGrid(List<T> allEntities)
    {
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                 _cells[x, y].Clear();
            }
        }

        foreach (var entity in allEntities)
        {
            Insert(entity);
        }
    }

    private void Insert(T entity)
    {
        GetEntityCellBounds(entity, out int minX, out int maxX, out int minY, out int maxY);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (IsInside(x, y))
                    _cells[x, y].Add(entity);
            }
        }
    }
    
    public void Remove(T entity)
    {
        GetEntityCellBounds(entity, out int minX, out int maxX, out int minY, out int maxY);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (IsInside(x, y))
                    _cells[x, y].Remove(entity);
            }
        }
    }
    
    public List<T>? GetEntitiesInCell(Vector2 position)
    {
        int gridX = (int)(position.X / _cellSize);
        int gridY = (int)(position.Y / _cellSize);

        if (IsInside(gridX, gridY))
        {
            return _cells[gridX, gridY];
        }

        return null;
    }
    
    public IEnumerable<T> GetNearby(Vector2 position, int radiusCells = 1)
    {
        int centerX = (int)(position.X / _cellSize);
        int centerY = (int)(position.Y / _cellSize);

        var seen = new HashSet<T>();

        for (int x = centerX - radiusCells; x <= centerX + radiusCells; x++)
        {
            for (int y = centerY - radiusCells; y <= centerY + radiusCells; y++)
            {
                if (IsInside(x, y))
                {
                    foreach (var entity in _cells[x, y])
                    {
                        if (seen.Add(entity)) yield return entity;
                    }
                }
            }
        }
    }
    
    private bool IsInside(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < _gridWidth && gridY >= 0 && gridY < _gridHeight;
    }
    
    private void GetEntityCellBounds(T entity, out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = (int)((entity.Position.X - entity.Radius) / _cellSize);
        maxX = (int)((entity.Position.X + entity.Radius) / _cellSize);
        minY = (int)((entity.Position.Y - entity.Radius) / _cellSize);
        maxY = (int)((entity.Position.Y + entity.Radius) / _cellSize);
    }
}