using System.Collections.Generic;
using Stride.Core.Mathematics;
namespace Terrain.Tools.Areas;
public class Rectangle : Area
{
    public int Width { get; set; } = 6;   // Width in world units
    public int Height { get; set; } = 3;  // Height in world units

    public override IEnumerable<Int2> GetAffectedCells(Int2 center, int terrainSize, int terrainCellSize)
    {
        // Determine the number of cells that correspond to the width and height in world units
        var gridWidth = Width;    // Calculate the grid width in terms of cells
        var gridHeight = Height;  // Calculate the grid height in terms of cells

        // Adjust the affected area by grid size
        for (var r = center.Y - gridHeight / 2; r <= center.Y + gridHeight / 2; r++)
        {
            for (var c = center.X - gridWidth / 2; c <= center.X + gridWidth / 2; c++)
            {
                // Ensure the affected cells are within bounds
                if (c >= 0 && c < terrainSize && r >= 0 && r < terrainSize)
                {
                    yield return new Int2(c, r);
                }
            }
        }
    }
}