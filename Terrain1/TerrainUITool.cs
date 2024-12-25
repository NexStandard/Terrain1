using Stride.Core;
using Stride.Engine;
using Stride.Input;
using Stride.Games;
using Stride.Graphics;

namespace Terrain;

[ComponentCategory("Terrain")]
public abstract class TerrainUITool : StartupScript
{
    [DataMemberIgnore]
    public TerrainGrid Terrain { get; internal set; }

    [DataMemberIgnore]
    public CameraComponent EditorCamera { get; internal set; }

    [DataMemberIgnore]
    public InputManager EditorInput { get; internal set; }

    [DataMemberIgnore]
    public SpriteFont EditorFont { get; internal set; }

    public SpriteFont Font { get; set; }
    public abstract void Update(GameTime gameTime);
}
