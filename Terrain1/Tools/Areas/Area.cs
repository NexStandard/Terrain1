using System.Collections.Generic;
using System;
using Stride.Core.Mathematics;
using Stride.Core;
namespace Terrain.Tools.Areas;

[DataContract(Inherited = true)]
public abstract class Area
{
    public abstract IEnumerable<Int2> GetAffectedCells(Int2 center, int terrainSize, int terrainCellSize);
}
