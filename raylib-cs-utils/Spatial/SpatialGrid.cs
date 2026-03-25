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
                _cells[x,y].Clear();
            }
        }

        foreach (var entity in allEntities)
        {
            int gridX = (int)(entity.Position.X /  _cellSize);
            int gridY = (int)(entity.Position.Y / _cellSize);

            if (isInside(gridX, gridY))
            {
                _cells[gridX, gridY].Add(entity);
            }
        }
    }

    public List<T>? GetEntitiesinCell(Vector2 position)
    {
        int gridX = (int)(position.X / _cellSize);
        int gridY = (int)(position.Y / _cellSize);

        if (isInside(gridX, gridY))
        {
            return _cells[gridX, gridY];
        }

        return null;
    }

    private bool isInside(int gridX, int gridY)
    {
        return gridX >= 0 && gridX < _gridWidth && gridY >= 0 && gridY < _gridHeight;
    }
}