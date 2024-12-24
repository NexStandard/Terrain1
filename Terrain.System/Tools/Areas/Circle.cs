using System.Collections.Generic;
using Stride.Core.Mathematics;
using Terrain.Tools.Areas;
namespace Terrain.Tools.Areas;
public class Circle : Area
{
    public int Radius { get; set; } = 3;  // Radius of the circle
    public float OuterBuffer { get; set; } = 0.5f;  // Buffer to smooth the outer edge of the circle

    public override IEnumerable<Int2> GetAffectedCells(Int2 center, int terrainSize, int terrainCellSize)
    {
        var gridRadius = Radius;

        // Loop over the grid and check each cell
        for (var r = center.Y - gridRadius; r <= center.Y + gridRadius; r++)
        {
            for (var c = center.X - gridRadius; c <= center.X + gridRadius; c++)
            {
                // Calculate the center of the cell
                var cellCenter = new Vector2(c * terrainCellSize + terrainCellSize / 2, r * terrainCellSize + terrainCellSize / 2);

                // Calculate the distance from the circle center to the cell center
                var distance = Vector2.Distance(new Vector2(center.X * terrainCellSize + terrainCellSize / 2, center.Y * terrainCellSize + terrainCellSize / 2), cellCenter);

                // Use a softened distance check: Allow a buffer region beyond the strict radius
                if (distance <= gridRadius * terrainCellSize + OuterBuffer * terrainCellSize)
                {
                    // Ensure the cell is within bounds
                    if (c >= 0 && c < terrainSize && r >= 0 && r < terrainSize)
                    {
                        yield return new Int2(c, r);
                    }
                }
            }
        }
    }
}
