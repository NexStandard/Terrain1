using Stride.Core.Mathematics;
using Stride.Games;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrain.Tools.Areas;

namespace Terrain.Tools;
public class SmoothBrush : TerrainEditorTool
{
    public Area Area { get; init; } = new Circle();

    // Adjust these values to control the speed of smoothing
    public float SmoothingFactor { get; set; } = 2.0f; // Determines how fast the smoothing happens.

    public override void Update(GameTime time)
    {
        base.Update(time);

        if (Area == null)
        {
            return;
        }

        // Handle mouse input for smoothing the terrain
        if (EditorInput.Mouse.IsButtonDown(MouseButton.Left))
        {
            if (IntersectMouseRayWithTerrain(out var vector))
            {
                SmoothTerrain(ConvertToGridCell(vector), vector, (float)time.Elapsed.TotalSeconds);
            }
        }
    }

    private void SmoothTerrain(Int2 center, Vector3 worldPoint, float delta)
    {
        // Calculate the average height of the surrounding terrain for smoothing
        float targetHeight = GetAverageHeightInArea(center, worldPoint);

        // Get the affected cells based on the area
        foreach (var cell in Area.GetAffectedCells(center, Terrain.Size, Terrain.CellSize))
        {
            if (cell.Y >= 0 && cell.Y < Terrain.Size && cell.X >= 0 && cell.X < Terrain.Size)
            {
                // Get the current height of the cell
                var currentHeight = Terrain.GetVertexHeight(cell.X, cell.Y);

                // Gradually move the height towards the target height for smoothing
                var adjustment = (targetHeight - currentHeight) * ((float)Strength / 100) * delta * SmoothingFactor;

                // Set the new smoothed height for the cell
                Terrain.SetVertexHeight(cell.X, cell.Y, currentHeight + adjustment);
            }
        }
    }

    private float GetAverageHeightInArea(Int2 center, Vector3 worldPoint)
    {
        // Calculate the grid cell column and row based on the world position
        var col = (int)(worldPoint.X / Terrain.CellSize);
        var row = (int)(worldPoint.Z / Terrain.CellSize);

        // Get the heights of the quad's corners
        float topLeftHeight = Terrain.GetVertexHeight(col, row);
        float topRightHeight = Terrain.GetVertexHeight(col + 1, row);
        float bottomLeftHeight = Terrain.GetVertexHeight(col, row + 1);
        float bottomRightHeight = Terrain.GetVertexHeight(col + 1, row + 1);

        // Return the average height of the four corners of the quad for smoother terrain blending
        return (topLeftHeight + topRightHeight + bottomLeftHeight + bottomRightHeight) / 4f;
    }
}