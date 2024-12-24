using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.UI.Controls;
using Stride.UI.Events;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Terrain1.UIProcessorTest;
internal class EditorExtensionProcessor : EntityProcessor<EditorExtensionComponent, EditorExtensionProcessor.AssociatedData>
{
    internal static readonly UIElementKey<Button> CreatePrefabButton = new UIElementKey<Button>("TestButton");

    protected override AssociatedData GenerateComponentData([NotNull] Entity entity, [NotNull] EditorExtensionComponent component)
    {
        return new AssociatedData
        {
            UIComponent = entity.Get<UIComponent>(),
        };
    }

    protected override void OnEntityComponentRemoved(Entity entity, [NotNull] EditorExtensionComponent component, [NotNull] AssociatedData data)
    {
        // Ensure we unregister all event handlers
        if (data.CreatePrefabButton != null)
        {
            data.CreatePrefabButton.Click -= OnCreatePrefabButtonClicked;
        }
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var kv in ComponentDatas)
        {
            var assocData = kv.Value;
            var uiComp = assocData.UIComponent;
            if (uiComp?.Page?.RootElement == null)
            {
                continue;
            }

            var levelEditComp = kv.Key;

            if (assocData.CreatePrefabButton == null && uiComp.TryGetUI(CreatePrefabButton, out var button))
            {
                button.Click += OnCreatePrefabButtonClicked;
                assocData.CreatePrefabButton = button;
            }
        }
    }

    private void OnCreatePrefabButtonClicked(object sender, RoutedEventArgs e)
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
            var sceneVm = sceneEditorVm?.Asset;
            if (sceneEditorVm != null)
            {
                //var sp = sceneVm?.ServiceProvider;
                var package = sceneVm.AssetItem.Package;
                var boxPrefabPath = levelEditComp.BoxPrefab;
                var boxPrefabAssetItem = package.Assets.FirstOrDefault(x => string.Equals(x.Location.FullPath, boxPrefabPath.Url));
                var boxPrefabAsset = boxPrefabAssetItem.Asset as Stride.Assets.Entities.PrefabAsset;
                var prefabVmRaw = sceneEditorVm.Session.GetAssetById(boxPrefabAsset.Id);
                var prefabVm = prefabVmRaw as Stride.Assets.Presentation.ViewModel.PrefabViewModel;
                var addChildMod = Stride.Core.Assets.Editor.ViewModel.AddChildModifiers.Alt;    // Create without a container entity

                /*
                // This adds the prefab to the root at origin, but can't be modified further
                {
                    var sceneRootVm = sceneEditorVm.RootPart as Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewModels.SceneRootViewModel;
                    var sceneRootAddChildViewModel = (Stride.Core.Assets.Editor.ViewModel.IAddChildViewModel)sceneRootVm;
                    if (sceneRootAddChildViewModel.CanAddChildren(new[] { prefabVm }, addChildMod, out string msg))
                    {
                        sceneRootAddChildViewModel.AddChildren(new[] { prefabVm }, addChildMod);
                    }
                }
                */

                var sceneRootVm = sceneEditorVm.RootPart as Stride.Assets.Presentation.AssetEditors.SceneEditor.ViewModels.SceneRootViewModel;
                // SceneRootViewModel has an internal method called AddEntitiesFromAssets which will be used to add a prefab into the scene
                var sceneRoot_AddEntitiesFromAssets_MethodInfo = sceneRootVm.GetType().GetMethod("AddEntitiesFromAssets", BindingFlags.NonPublic | BindingFlags.Instance);

                var root = sceneEditorVm.HierarchyRoot;
                using (var transaction = sceneEditorVm.UndoRedoService.CreateTransaction())
                {
                    var param_Assets = new[] { prefabVm };
                    var param_Index = root.Asset.Asset.Hierarchy.RootParts.Count;   // Add to the end
                    var param_Modifiers = addChildMod;
                    var param_RootPosition = new Vector3(0, levelEditComp.PrefabNextYPosition, 0);
                    var methodParams = new object[] { param_Assets, param_Index, param_Modifiers, param_RootPosition };
                    var methodReturnValue = sceneRoot_AddEntitiesFromAssets_MethodInfo.Invoke(sceneRootVm, methodParams);
                    var entitiesViewModels = methodReturnValue as IReadOnlyCollection<Stride.Assets.Presentation.AssetEditors.EntityHierarchyEditor.ViewModels.EntityViewModel>;
                    // Can look at the entities created from the prefab in entitiesViewModels

                    var levelEditorEntity = levelEditComp.Entity;
                    var levelEditorEntityAssetPart = root.Asset.Asset.Hierarchy.Parts.FirstOrDefault(x => x.Value.Entity.Id == levelEditorEntity.Id);
                    var vmLevelEditorEntity = levelEditorEntityAssetPart.Value.Entity;      // This is the entity on the 'master' version.
                    var vmLevelEditComp = vmLevelEditorEntity.Get<EditorExtensionComponent>();

                    var levelEditCompNode = sceneEditorVm.Session.AssetNodeContainer.GetNode(vmLevelEditComp);
                    var nextYPosNodeRaw = levelEditCompNode[nameof(EditorExtensionComponent.PrefabNextYPosition)];
                    var nextYPosNode = nextYPosNodeRaw as Stride.Core.Assets.Quantum.IAssetMemberNode;

                    nextYPosNode.Update(levelEditComp.PrefabNextYPosition + 1);     // Increment & update LevelEditComponent.PrefabNextYPosition

                    sceneEditorVm.UndoRedoService.SetName(transaction, "Level Editor create prefab");
                }
            }
        });
    }

    internal class AssociatedData
    {
        public UIComponent UIComponent;

        public Button CreatePrefabButton;
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