using System.Numerics;

namespace raylib_cs_utils.Spatial;

public interface ISpatialEntity
{
    Vector2 Position { get; }
    float Radius { get; }
}