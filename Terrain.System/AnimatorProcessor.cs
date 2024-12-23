using Stride.CommunityToolkit.Engine;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Rendering;
using System;

namespace Terrain;

public class AnimatorProcessor : EntityProcessor<TerrainGrid>
{
    public AnimatorProcessor() : base(typeof(TerrainGrid)) { }
    public override void Draw(RenderContext context)
    {
        base.Draw(context);

        var sceneSystem = context.Services.GetService<SceneSystem>();
        var camera = TryGetMainCamera(sceneSystem);

        if (camera == null)
            return;

        var input = context.Services.GetService<Stride.Input.InputManager>();
        if (input == null)
            throw new Exception();
        // Handle mouse input for height editing
        if (input.Mouse.IsButtonDown(MouseButton.Left))
        {
            HandleMouseClick(context, camera, input.MousePosition);
        }
    }

    private void HandleMouseClick(RenderContext context, CameraComponent camera, Vector2 mousePosition)
    {
        foreach (var terrain in ComponentDatas.Keys)
        {
            // Step 1: Use ScreenPointToRay to get the near and far points in world space
            var (vectorNear, vectorFar) = camera.ScreenPointToRay(mousePosition);

            var rayOrigin = new Vector3(vectorNear.X, vectorNear.Y, vectorNear.Z);
            var rayDirection = Vector3.Normalize(new Vector3(vectorFar.X, vectorFar.Y, vectorFar.Z) - rayOrigin);

            // Step 2: Intersect the ray with the terrain's plane
            if (IntersectRayWithTerrain(rayOrigin, rayDirection, terrain, out var terrainPoint))
            {
                ModifyTerrain(terrain, terrainPoint, 3); // Modify terrain with a radius of 3
            }
        }
    }
    private void ModifyTerrain(TerrainGrid terrain, Vector3 hitPoint, int radius)
    {
        // Find the corresponding grid cell
        var col = (int)(hitPoint.X / terrain.CellSize);
        var row = (int)(hitPoint.Z / terrain.CellSize);

        // Modify a square region around the clicked point
        for (var r = row - radius; r <= row + radius; r++)
        {
            for (var c = col - radius; c <= col + radius; c++)
            {
                if (r >= 0 && r < terrain.Size && c >= 0 && c < terrain.Size)
                {
                    var currentHeight = terrain.GetVertexHeight(c, r);
                    terrain.SetVertexHeight(c, r, currentHeight + 1f); // Increase height
                }
            }
        }

        // Update the terrain mesh
        // terrain.UpdateMesh();
    }

    private bool IntersectRayWithTerrain(Vector3 rayOrigin, Vector3 rayDirection, TerrainGrid terrain, out Vector3 terrainPoint)
    {
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
        if (hitPoint.X < 0 || hitPoint.X > terrain.TotalWidth ||
            hitPoint.Z < 0 || hitPoint.Z > terrain.TotalHeight)
            return false;

        terrainPoint = hitPoint;
        return true;
    }
    public static CameraComponent TryGetMainCamera(SceneSystem sceneSystem)
    {
        CameraComponent camera = null;
        if (sceneSystem.GraphicsCompositor.Cameras.Count == 0)
        {
            // The compositor wont have any cameras attached if the game is running in editor mode
            // Search through the scene systems until the camera entity is found
            // This is what you might call "A Hack"
            foreach (var system in sceneSystem.Game.GameSystems)
            {
                if (system is SceneSystem editorSceneSystem)
                {
                    foreach (var entity in editorSceneSystem.SceneInstance.RootScene.Entities)
                    {
                        if (entity.Name == "Camera Editor Entity")
                        {
                            camera = entity.Get<CameraComponent>();
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            camera = sceneSystem.GraphicsCompositor.Cameras[0].Camera;
        }

        return camera;
    }
}
