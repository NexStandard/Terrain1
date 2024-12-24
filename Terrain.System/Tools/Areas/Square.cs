using System.Collections.Generic;
using Stride.Core.Mathematics;
namespace Terrain.Tools.Areas;
public class Square : Area
{
    public int SideLength { get; set; } = 3;

    public override IEnumerable<Int2> GetAffectedCells(Int2 center, int terrainSize, int terrainCellSize)
    {
        var gridSideLength = SideLength;

        for (var r = center.Y - gridSideLength; r <= center.Y + gridSideLength; r++)
        {
            for (var c = center.X - gridSideLength; c <= center.X + gridSideLength; c++)
            {
                if (c >= 0 && c < terrainSize && r >= 0 && r < terrainSize)
                {
                    yield return new Int2(c, r);
                }
            }
        }
    }
}
