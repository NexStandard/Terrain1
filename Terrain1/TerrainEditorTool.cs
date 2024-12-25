using Stride.CommunityToolkit.Engine;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Input;
using Stride.Rendering;
using System;

namespace Terrain;

[ComponentCategory("Terrain")]
public abstract class TerrainEditorTool : StartupScript
{
    [DataMember(1)]
    public abstract string UIName { get; set; }

    [DataMemberIgnore]
    public TerrainGrid Terrain { get; internal set; }

    [DataMemberIgnore]
    public CameraComponent EditorCamera { get; internal set; }

    [DataMemberIgnore]
    public InputManager EditorInput {  get; internal set; }

    public bool Active { get; set; }

    [DataMemberRange(0, 100)]
    public int Strength { get; set; } = 100;

    public override void Start()
    {
    }

    public virtual void Update(GameTime gameTime) 
    {
        if (EditorCamera is null)
        {
            return;
        }
        if (Terrain is null)
        {
            return;
        }
        if (EditorInput is null)
        {
            return;
        }
        if (EditorInput.IsKeyDown(Keys.LeftAlt))
        {
            Strength = Math.Abs(Strength) * -1;
        }
        else 
        {
            Strength = Math.Abs(Strength);
        }
    }

    protected Int2 ConvertToGridCell(Vector3 worldPosition)
    {
        // Convert world coordinates to grid coordinates
        var gridX = (int)Math.Round(worldPosition.X / Terrain.CellSize);
        var gridZ = (int)Math.Round(worldPosition.Z / Terrain.CellSize);

        return new Int2(gridX, gridZ);
    }
    protected bool IntersectMouseRayWithTerrain(out Vector3 terrainPoint)
    {
        var (vectorNear, vectorFar) = EditorCamera.ScreenPointToRay(EditorInput.MousePosition);

        var rayOrigin = new Vector3(vectorNear.X, vectorNear.Y, vectorNear.Z);
        var rayDirection = Vector3.Normalize(new Vector3(vectorFar.X, vectorFar.Y, vectorFar.Z) - rayOrigin);

        terrainPoint = Vector3.Zero;

        // Intersect with Y=0 plane (terrain height at 0)
        var planeNormal = Vector3.UnitY;
        var planeD = 0f;
        var rayDotNormal = Vector3.Dot(rayDirection, planeNormal);

        if (Math.Abs(rayDotNormal) < 1e-6)
            return false; // No intersection

        var t = -(Vector3.Dot(rayOrigin, planeNormal) + planeD) / rayDotNormal;
        if (t < 0)
            return false; // Intersection is behind the camera

        var hitPoint = rayOrigin + (rayDirection * t);

        // Ensure the point lies within the terrain bounds
        if (hitPoint.X < 0 || hitPoint.X > Terrain.TotalWidth ||
            hitPoint.Z < 0 || hitPoint.Z > Terrain.TotalHeight)
            return false;

        terrainPoint = hitPoint;
        return true;
    }
}
