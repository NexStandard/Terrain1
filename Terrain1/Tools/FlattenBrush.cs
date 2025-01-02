using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrain.Tools.Areas;

namespace Terrain.Tools;

[ComponentCategory("Terrain")]
public class FlattenBrush : TerrainEditorTool
{
    public Area Area { get; init; } = new Circle();
    public override string UIName { get; set; } = nameof(FlattenBrush);
    
    private bool isDone = false;

    public override bool NeedsTransactionCommit { get => Terrain.TerrainVertexDraw.CanCommit && isDone; }
    private float? TargetHeight { get; set; } // Cached target height while the mouse is pressed

    public override void Update(GameTime time)
    {
        isDone = false;
        base.Update(time);

        if (Area == null)
        {
            return;
        }

        if (EditorInput.Mouse.IsButtonDown(MouseButton.Left))
        {
            // If the target height is not set, calculate it
            if (TargetHeight == null && IntersectMouseRayWithTerrain(out var vector))
            {
                TargetHeight = GetAverageHeightOfQuad(vector);
            }

            if (TargetHeight != null && IntersectMouseRayWithTerrain(out var clickVector))
            {
                FlattenTerrain(ConvertToGridCell(clickVector), TargetHeight.Value);
            }
        }
        else if (EditorInput.Mouse.IsButtonReleased(MouseButton.Left))
        {
            // Reset the cached target height when the mouse is released
            isDone = true;
            TargetHeight = null;
        }
    }

    private void FlattenTerrain(Int2 center, float targetHeight)
    {
        // Get the affected cells based on the area
        foreach (var cell in Area.GetAffectedCells(center, Terrain.Size, Terrain.CellSize))
        {
            if (cell.Y >= 0 && cell.Y < Terrain.Size && cell.X >= 0 && cell.X < Terrain.Size)
            {
                // Instantly set the height to the target height
                Terrain.SetVertexHeight(cell.X, cell.Y, targetHeight);
            }
        }
    }

    private float GetAverageHeightOfQuad(Vector3 worldPoint)
    {
        // Calculate the grid cell column and row
        var col = (int)(worldPoint.X / Terrain.CellSize);
        var row = (int)(worldPoint.Z / Terrain.CellSize);

        // Get the heights of the quad's corners
        float topLeftHeight = Terrain.GetVertexHeight(col, row);
        float topRightHeight = Terrain.GetVertexHeight(col + 1, row);
        float bottomLeftHeight = Terrain.GetVertexHeight(col, row + 1);
        float bottomRightHeight = Terrain.GetVertexHeight(col + 1, row + 1);

        // Calculate the average height
        return (topLeftHeight + topRightHeight + bottomLeftHeight + bottomRightHeight) / 4f;
    }
}
