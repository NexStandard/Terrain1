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
public class MaskedElevateBrush : TerrainEditorTool
{
    public Area Area { get; init; } = new Circle();
    public override string UIName { get; set; } = nameof(MaskedElevateBrush);
    private bool isDone = false;
    public override void Update(GameTime time)
    {
        isDone = false;
        base.Update(time);

        if (Area == null)
        {
            return;
        }

        // Handle mouse input for height editing
        if (EditorInput.Mouse.IsButtonDown(MouseButton.Left))
        {
            if (IntersectMouseRayWithTerrain(out var hitPoint))
            {
                // Convert hit point to grid coordinates
                var center = ConvertToGridCell(hitPoint);
                ModifyTerrain(center, (float)time.Elapsed.TotalSeconds);
            }
        }
        else if (EditorInput.Mouse.IsButtonReleased(MouseButton.Left))
        {
            isDone = true;
        }
    }

    private void ModifyTerrain(Int2 center, float delta)
    {
        // Get the affected cells based on the area
        var affectedCells = Area.GetAffectedCells(center, Terrain.Size, Terrain.CellSize);

        // Compute the maximum distance in world space from the center to the farthest affected cell
        var maxDistance = ComputeMaxDistance(center, affectedCells);

        foreach (var cell in affectedCells)
        {
            if (cell.Y >= 0 && cell.Y < Terrain.Size && cell.X >= 0 && cell.X < Terrain.Size)
            {
                // Calculate the distance from the center in world units
                var distance = Vector2.Distance(
                    new Vector2(center.X * Terrain.CellSize, center.Y * Terrain.CellSize),
                    new Vector2(cell.X * Terrain.CellSize, cell.Y * Terrain.CellSize)
                );

                // Normalize the distance based on the maximum range in world units
                var normalizedDistance = Math.Clamp(distance / maxDistance, 0f, 1f);

                // Falloff factor
                var falloff = 1f - normalizedDistance;

                // Slight reduction specifically for the exact center node
                if (cell == center)
                {
                    falloff *= 0.9f; // Reduce center node to 90% of its calculated value
                }

                // Calculate the adjusted strength
                var adjustment = ((float)Strength / 100) * delta * falloff;

                // Apply the adjustment
                var currentHeight = Terrain.GetVertexHeight(cell.X, cell.Y);
                Terrain.SetVertexHeight(cell.X, cell.Y, currentHeight + adjustment);
            }
        }
    }

    private float ComputeMaxDistance(Int2 center, IEnumerable<Int2> affectedCells)
    {
        var maxDistance = 0f;
        var centerWorld = new Vector2(center.X * Terrain.CellSize, center.Y * Terrain.CellSize);

        foreach (var cell in affectedCells)
        {
            var cellWorld = new Vector2(cell.X * Terrain.CellSize, cell.Y * Terrain.CellSize);
            var distance = Vector2.Distance(centerWorld, cellWorld);
            maxDistance = Math.Max(maxDistance, distance);
        }

        return maxDistance;
    }
}
