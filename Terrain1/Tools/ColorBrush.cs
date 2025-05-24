using SharpDX;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Input;
using Stride.Rendering;
using System.IO;
using System.Linq;
using Terrain;
using Terrain.Tools.Areas;
using Terrain1.Drawing;

namespace Terrain1.Tools;
public class ColorBrush : TerrainEditorTool
{
    public override string UIName { get; set; } = nameof(ColorBrush);
    public Area Area { get; set; } = new Circle();

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Area == null)
        {
            return;
        }

        if (EditorInput.Mouse.IsButtonDown(MouseButton.Left))
        {
            // If the target height is not set, calculate it
            if (IntersectMouseRayWithTerrain(out var vector))
            {
                
                var cell = ConvertToGridCell(vector);
                var cells = Area.GetAffectedCells(cell, Terrain.Size, Terrain.CellSize);
                

                foreach (var c in cells)
                {
                    if (Terrain.TerrainVertexDraw is TerrainStandardVertexDraw standard) ;
                }
            }
        }
        else if (EditorInput.Mouse.IsButtonReleased(MouseButton.Left))
        {
        }
    }

}
