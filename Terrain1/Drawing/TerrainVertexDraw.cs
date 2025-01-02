using Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewModels;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Core.Quantum;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terrain;

namespace Terrain1.Drawing;

/// <summary>
/// Class to create and manage the <see cref="VertexBufferBinding"/> for the Terrain to allow custom kinds of <see cref="IVertex"/>
/// </summary>
public abstract class TerrainVertexDraw
{
    /// <summary>
    /// The linked <see cref="TerrainGrid"/> on this <see cref="Entity"/> set by the <see cref="TerrainGridProcessor"/>
    /// </summary>
    public TerrainGrid Grid { get; set; }
    
    /// <summary>
    /// The assigned <see cref="TerrainGridRenderData"/> of the <see cref="TerrainGrid"/>
    /// </summary>
    /// <remarks>
    /// There is a race condition in the Editor if this is not <see cref="DataMemberIgnoreAttribute"/>
    /// </remarks>
    [DataMemberIgnore]
    public TerrainGridRenderData GridRenderData { get; internal set; }
    
    /// <summary>
    /// The <see cref="GraphicsDevice"/> of the Editor and ingame it changes to the games <see cref="GraphicsDevice"/>
    /// </summary>
    /// <remarks>
    /// There is a race condition in the Editor if this is not <see cref="DataMemberIgnoreAttribute"/>
    /// </remarks>
    [DataMemberIgnore]
    public GraphicsDevice TerrainGraphicsDevice { get; internal set; }

    /// <summary>
    /// The <see cref="CommandList"/> of the Editor and ingame it changes to the games <see cref="CommandList"/> of the <see cref="TerrainVertexDraw.TerrainGraphicsDevice"/>
    /// </summary>
    /// <remarks>
    /// There is a race condition in the Editor if this is not <see cref="DataMemberIgnoreAttribute"/>
    /// </remarks>
    [DataMemberIgnore]
    public CommandList TerrainGraphicsCommandList { get; internal set; }

    /// <summary>
    /// Is invoked if the Terrains Render Data has significant changes that require an entire rebuild of the Terrain
    /// </summary>
    public abstract void Rebuild();

    /// <summary>
    /// Is invoked if a single Cell of the <see cref="TerrainGrid"/> is out of sync
    /// </summary>
    /// <param name="cell">The terrain grid cell that requires changes ( not normalized with <see cref="TerrainGrid.CellSize"/> )</param>
    public abstract void Rebuild(Int2 cell, IVertex vertex);

    public virtual bool CanCommit { get; } = false;
    [DataMember]
    public virtual bool Save { get; internal set; }
    
    public virtual string Path { get; set; }

    public virtual bool Load { get; internal set; }

    public abstract void SaveTransaction(SceneEditorViewModel sceneEditorVm, IObjectNode levelEditCompNode);
    public abstract void LoadTransaction(SceneEditorViewModel sceneEditorVm, IObjectNode levelEditCompNode);

}
