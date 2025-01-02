using Stride.Core.Mathematics;
using Stride.Engine;
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
using Terrain1.Drawing;

namespace Terrain.Tools;

[ComponentCategory("Terrain")]
public class ElevateBrush : TerrainEditorTool
{
    public Area Area { get; init; } = new Circle();
    public override string UIName { get; set; } = nameof(ElevateBrush);
    public override bool NeedsTransactionCommit { get => Terrain.TerrainVertexDraw.CanCommit && isDone; }
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
            if (IntersectMouseRayWithTerrain(out var vector))
            {
                ModifyTerrain(ConvertToGridCell(vector), (float)time.Elapsed.TotalSeconds);
            }
        }
        else if (EditorInput.Mouse.IsButtonReleased(MouseButton.Left))
        {
            isDone = true;
        }
    }

    private void ModifyTerrain(Int2 center, float delta)
    {
        foreach (var cell in Area.GetAffectedCells(center, Terrain.Size, Terrain.CellSize))
        {
            if (cell.Y >= 0 && cell.Y < Terrain.Size && cell.X >= 0 && cell.X < Terrain.Size)
            {
                if( Terrain.TerrainVertexDraw is TerrainStandardVertexDraw std)
                {
                    std.SetVertexHeight(cell, ((float)Strength / 100) * delta); // Increase height

                }

            }
        }
    }
}
