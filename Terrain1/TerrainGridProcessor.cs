namespace Terrain;

using Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewModels;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Core.Quantum;
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
            grid.Key.TerrainVertexDraw.Grid ??= grid.Key;
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
            if (grid.Key.TerrainVertexDraw is null)
            {
                return;
            }

            grid.Key.TerrainVertexDraw.TerrainGraphicsDevice ??= graphicsDevice;
            grid.Key.TerrainVertexDraw.TerrainGraphicsCommandList ??= commandList;
            grid.Key.TerrainVertexDraw.Grid ??= grid.Key;
            grid.Key.TerrainVertexDraw.GridRenderData ??= grid.Value;

            if (grid.Value.Size != grid.Key.Size || grid.Value.CellSize != grid.Key.CellSize || (grid.Value.IndexBuffer is null && grid.Value.VertexBuffer is null))
            {
                grid.Key.TerrainVertexDraw.Rebuild();
            }
            
            if(grid.Key.TerrainVertexDraw.Save)
            {
                OnResetPrefabNextYPositionButtonClicked(grid.Key);
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
                grid.TerrainVertexDraw.SaveTransaction(sceneEditorVm, levelEditCompNode);
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