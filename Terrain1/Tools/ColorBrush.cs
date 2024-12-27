using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrain;
using Terrain.Tools.Areas;

namespace Terrain1.Tools;
internal class ColorBrush : TerrainEditorTool
{
    public override string UIName { get; set; } = nameof(ColorBrush);
    public Area Area { get; set; } = new Circle();
    public Color Color { get; set; }
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
                    // Terrain.SetVertexColor(c.X, c.Y, Color);
                }
            }

        }
    }

}
