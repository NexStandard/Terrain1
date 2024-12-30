namespace Terrain;

using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terrain1.Tools;

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
    SpriteFont _font;
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
        if (_font is null)
        {
            _font = sceneSystem.Game.Content.Load<SpriteFont>("StrideDefaultFont");
        }
        foreach (var grid in ComponentDatas)
        {
            // Process TerrainEditorTools
            foreach (var component in grid.Key.Entity.Components)
            {
                if (component is TerrainEditorTool tool)
                {
                    if (tool.Active)
                    {
                        tool.EditorInput = inputManager;
                        tool.Terrain = grid.Key;
                        tool.EditorCamera = editorCamera;
                        tool.Update(time);
                    }
                }
            }
            // Process UIComponent for managing buttons
            foreach (var component in grid.Key.Entity.Components)
            {
                if (component is TerrainUITool f)
                {
                    f.EditorInput = inputManager;
                    f.Terrain = grid.Key;
                    f.EditorFont = _font;
                    f.EditorCamera = editorCamera;
                    f.Update(time);
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
                        VertexBuffers = new[] { new VertexBufferBinding(grid.Value.VertexBuffer, VertexPositionNormalColorTexture.Layout, grid.Value.VertexBuffer.ElementCount) },
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
                    var updatedVertex = new VertexPositionNormalColorTexture
                    {
                        Position = new Vector3(x, grid.Key.GetVertexHeight(location.X, location.Y), z),
                        Normal = Vector3.UnitY,
                        TextureCoordinate = new Vector2(
                            (float)location.X / grid.Key.Size,
                            (float)location.Y / grid.Key.Size
                        )
                        , Color = Color.Black,
                        Color1 = Color.Black
                    };
                    if (grid.Key.VertexColorMaterialMapping.TryGetValue(new Int2(x,z), out var target))
                    {
                        var x1 = target / 4;
                        var y = target % 4;
                        var color = y switch
                        {
                            0 => Color.Red,
                            1 => Color.Green,
                            2 => Color.Blue,
                            _ => Color.Black,
                        };
                        if (x1 == 0)
                        {
                            updatedVertex.Color = color;
                        }
                        else if (x1 == 1)
                        {
                            updatedVertex.Color1 = color;
                        }
                    }
                    File.WriteAllText("D:\\Colors", updatedVertex.Color.ToString() + updatedVertex.Color1 + target);
                    // Update the vertex directly in the GPU buffer at the correct offset
                    grid.Value.VertexBuffer.SetData(
                        commandList,
                        ref updatedVertex, // Update only this single vertex
                        index * VertexPositionNormalColorTexture.Layout.CalculateSize()
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
                OnResetPrefabNextYPositionButtonClicked(grid.Key);
                grid.Key.Flatten = false;
            }
        }
    }
    private void OnResetPrefabNextYPositionButtonClicked(TerrainGrid grid)
    {
        var kv = ComponentDatas.FirstOrDefault();
        var levelEditComp = kv.Key;

        var editorVm = Stride.Core.Assets.Editor.ViewModel.EditorViewModel.Instance;
        var gsVm = Stride.GameStudio.ViewModels.GameStudioViewModel.GameStudio;
        gsVm.StrideAssets.Dispatcher.Invoke(() =>
        {
            // Application.Current must be accessed on the UI thread
            var window = System.Windows.Application.Current.MainWindow as Stride.GameStudio.View.GameStudioWindow;

            var sceneEditorView = window.GetChildOfType<Stride.Assets.Presentation.AssetEditors.SceneEditor.Views.SceneEditorView>();
            var sceneEditorVm = sceneEditorView?.DataContext as Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewModels.SceneEditorViewModel;
            if (sceneEditorVm != null)
            {
                var levelEditorEntity = levelEditComp.Entity;

                var root = sceneEditorVm.HierarchyRoot;
                var levelEditorEntityAssetPart = root.Asset.Asset.Hierarchy.Parts.FirstOrDefault(x => x.Value.Entity.Id == levelEditorEntity.Id);
                var vmLevelEditorEntity = levelEditorEntityAssetPart.Value.Entity;

                var vmLevelEditComp = vmLevelEditorEntity.Get<TerrainGrid>();

                var levelEditCompNode = sceneEditorVm.Session.AssetNodeContainer.GetNode(vmLevelEditComp);

                var nextYPosNodeRaw = levelEditCompNode[nameof(TerrainGrid.VertexHeightsE)];
                var nextYPosNode = nextYPosNodeRaw as Stride.Core.Assets.Quantum.IAssetMemberNode;

                using (var transaction = sceneEditorVm.UndoRedoService.CreateTransaction())
                {
                    nextYPosNode.Update(grid.VertexHeightsE);
                    sceneEditorVm.UndoRedoService.SetName(transaction, "Level Editor Reset prefab next Y position");
                }
            }
        });
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

static class WpfExt
{
    public static void GetChildrenOfType<T>(this System.Windows.DependencyObject depObj, List<T> foundChildren)
        where T : System.Windows.DependencyObject
    {
        if (depObj == null) return;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);

            if (child is T matchedChild)
            {
                foundChildren.Add(matchedChild);
            }
            GetChildrenOfType(child, foundChildren);
        }
    }

    public static T GetChildOfType<T>(this System.Windows.DependencyObject depObj)
        where T : System.Windows.DependencyObject
    {
        if (depObj == null) return null;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);

            var result = (child as T) ?? GetChildOfType<T>(child);
            if (result != null) return result;
        }
        return null;
    }
}