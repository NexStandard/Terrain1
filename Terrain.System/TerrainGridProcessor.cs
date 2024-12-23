using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;

namespace Terrain;

using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Engine;
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
    protected override void OnSystemAdd()
    {
        base.OnSystemAdd();

        graphicsDevice = Services.GetService<IGraphicsDeviceService>().GraphicsDevice;
    }
    public override void Draw(RenderContext context)
    {
        base.Draw(context);
        var sceneSystem = context.Services.GetService<SceneSystem>();
        var commandList = sceneSystem.Game.GraphicsContext.CommandList;
        foreach (var grid in ComponentDatas)
        {
            if (grid.Value.Size != grid.Key.Size || grid.Value.CellSize != grid.Key.CellSize || (grid.Value.IndexBuffer is null && grid.Value.VertexBuffer is null))
            {
                var vertices = grid.Key.GenerateVertices();
                var indices = grid.Key.GenerateIndices();
                grid.Value.Size = grid.Key.Size;
                grid.Value.CellSize = grid.Key.CellSize;
                var indexBuffer = Stride.Graphics.Buffer.Index.New(graphicsDevice, indices, GraphicsResourceUsage.Dynamic);
                grid.Value.IndexBuffer = indexBuffer;
                var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(graphicsDevice, vertices, GraphicsResourceUsage.Dynamic);
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

            // update only the modified height vertices which couldnt be applied yet
            // improve the set data as it "laggs" with a big terrain, as the entire array is copied each time
            if(grid.Key.ModifiedVertices.Count > 0)
            {
                var vertexBuffer = grid.Value.VertexBuffer.GetData<VertexPositionNormalTexture>(commandList);
                foreach (var location in grid.Key.ModifiedVertices)
                {
                    var x = location.X * grid.Key.CellSize;
                    var z = location.Y * grid.Key.CellSize;

                    var index = (location.Y * (grid.Key.Size + 1)) + location.X;


                    var position = new Vector3(x, grid.Key.GetVertexHeight(location.X, location.Y), z);

                    vertexBuffer[index] = new VertexPositionNormalTexture
                    {
                        Position = position,
                        Normal = Vector3.UnitY,
                        TextureCoordinate = new Vector2(
                            (float)location.X / grid.Key.Size,
                            (float)location.Y / grid.Key.Size
                        )
                    };
                }
                // really needed?
                grid.Value.VertexBuffer.SetData(commandList, vertexBuffer);
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
}
