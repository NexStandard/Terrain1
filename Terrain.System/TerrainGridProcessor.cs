namespace Terrain;

using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;

public class TerrainGridProcessor : EntityProcessor<TerrainGrid, TerrainGridRenderData>, IEntityComponentRenderProcessor
{
    public VisibilityGroup VisibilityGroup { get; set; }

    protected override TerrainGridRenderData GenerateComponentData([NotNull] Entity entity, [NotNull] TerrainGrid component)
    {
        return new TerrainGridRenderData() { ModelComponent = new() { Model = new() } };
    }
    GraphicsDevice graphicsDevice { get; set; }
    SceneSystem sceneSystem { get; set; }
    CameraComponent editorCamera { get; set; }
    Stride.Input.InputManager inputManager { get; set; }
    protected override void OnSystemAdd()
    {
        base.OnSystemAdd();
        graphicsDevice = Services.GetService<IGraphicsDeviceService>().GraphicsDevice;
        sceneSystem = Services.GetService<SceneSystem>();
        editorCamera = TryGetMainCamera(sceneSystem);
        inputManager = Services.GetService<Stride.Input.InputManager>();
    }
    public override void Update(GameTime time)
    {
        base.Update(time);
        foreach (var grid in ComponentDatas)
        {
            foreach (var component in grid.Key.Entity.Components)
            {
                if (component is TerrainEditorTool tool)
                {
                    tool.EditorInput = inputManager;
                    tool.Terrain = grid.Key;
                    tool.EditorCamera = editorCamera;
                    tool.Update(time);
                }
            }
        }
    }
    public override void Draw(RenderContext context)
    {
        base.Draw(context);
        var commandList = sceneSystem.Game.GraphicsContext.CommandList;
        foreach (var grid in ComponentDatas)
        {
            if (grid.Value.Size != grid.Key.Size || grid.Value.CellSize != grid.Key.CellSize || (grid.Value.IndexBuffer is null && grid.Value.VertexBuffer is null))
            {
                var vertices = grid.Key.GenerateVertices();
                var indices = grid.Key.GenerateIndices();
                grid.Value.Size = grid.Key.Size;
                grid.Value.CellSize = grid.Key.CellSize;
                var indexBuffer = Stride.Graphics.Buffer.Index.New(graphicsDevice, indices, GraphicsResourceUsage.Default);
                grid.Value.IndexBuffer = indexBuffer;
                var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(graphicsDevice, vertices, GraphicsResourceUsage.Default);
                grid.Value.VertexBuffer = vertexBuffer;
                var mesh = new Stride.Rendering.Mesh
                {
                    Draw = new Stride.Rendering.MeshDraw
                    {
                        PrimitiveType = Stride.Graphics.PrimitiveType.TriangleList,
                        DrawCount = indices.Length,
                        IndexBuffer = new IndexBufferBinding(grid.Value.IndexBuffer, true, indices.Length),
                        VertexBuffers = new[] { new VertexBufferBinding(grid.Value.VertexBuffer, VertexPositionNormalTexture.Layout, grid.Value.VertexBuffer.ElementCount) },
                    },
                    MaterialIndex = 0,
                };
                var model = new Model()
                {
                    Meshes = [mesh],
                };

                grid.Value.ModelComponent.Model.Meshes.Clear();
                grid.Value.ModelComponent.Model.Meshes.Add(mesh);
                var comp = grid.Key.Entity.Get<ModelComponent>();
                if (comp == null)
                {
                    comp = new ModelComponent()
                    {
                        Model = model
                    };
                    grid.Key.Entity.Add(comp);

                }
                else
                {
                    comp.Model = model;
                }
                if (grid.Key.Material != null)
                {
                    grid.Value.ModelComponent.Materials.Clear();
                    grid.Value.ModelComponent.Materials.Add(0, grid.Key.Material);
                    comp.Model.Meshes[0].MaterialIndex = 0;
                    comp.Model.Materials.Add(grid.Key.Material);
                }
            }
            if (grid.Key.ModifiedVertices.Count > 0)
            {
                foreach (var location in grid.Key.ModifiedVertices)
                {
                    var x = location.X * grid.Key.CellSize;
                    var z = location.Y * grid.Key.CellSize;

                    var index = (location.Y * (grid.Key.Size + 1)) + location.X;

                    // Prepare the updated vertex
                    var updatedVertex = new VertexPositionNormalTexture
                    {
                        Position = new Vector3(x, grid.Key.GetVertexHeight(location.X, location.Y), z),
                        Normal = Vector3.UnitY,
                        TextureCoordinate = new Vector2(
                            (float)location.X / grid.Key.Size,
                            (float)location.Y / grid.Key.Size
                        )
                    };
                    
                    // Update the vertex directly in the GPU buffer at the correct offset
                    grid.Value.VertexBuffer.SetData(
                        commandList,
                        ref updatedVertex, // Update only this single vertex
                        index * VertexPositionNormalTexture.Layout.CalculateSize()
                    );
                }

                // Clear the modified vertices list after updating
                grid.Key.ModifiedVertices.Clear();
            }

            if (grid.Key.Randomize)
            {
                grid.Key.SetRandomHeights();
                grid.Key.Randomize = false;
            }
            if (grid.Key.Flatten)
            {
                grid.Key.FlattenAll();
                grid.Key.Flatten = false;
            }
        }
    }

    protected override void OnEntityComponentRemoved(Entity entity, TerrainGrid component, TerrainGridRenderData data)
    {
        entity.Remove<ModelComponent>();
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