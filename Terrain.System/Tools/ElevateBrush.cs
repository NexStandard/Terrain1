using Stride.Core.Mathematics;
using Stride.Games;
using Stride.Input;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Terrain.Tools.Areas;

namespace Terrain.Tools;
public class ElevateBrush : TerrainEditorTool
{
    public Area Area { get; init; } = new Circle();

    public override void Update(GameTime time)
    {
        base.Update(time);

        if (Area == null)
        {
            return;
        }

        // Handle mouse input for height editing
        if (EditorInput.Mouse.IsButtonDown(MouseButton.Left))
        {
            if (IntersectMouseRayWithTerrain(out var vector))
            {
                ModifyTerrain(ConvertToGridCell(vector), (float)time.Elapsed.TotalSeconds);
            }
        }
    }

    private void ModifyTerrain(Int2 center, float delta)
    {
        foreach (var cell in Area.GetAffectedCells(center, Terrain.Size, Terrain.CellSize))
        {
            if (cell.Y >= 0 && cell.Y < Terrain.Size && cell.X >= 0 && cell.X < Terrain.Size)
            {
                var currentHeight = Terrain.GetVertexHeight(cell.X, cell.Y);
                Terrain.SetVertexHeight(cell.X, cell.Y, currentHeight + ((float)Strength/100) * delta); // Increase height
            }
        }
    }
}
