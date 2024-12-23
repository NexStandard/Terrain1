using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;

namespace Terrain;

public class TerrainGridProcessor : EntityProcessor<TerrainGrid, TerrainGridRenderData>, IEntityComponentRenderProcessor
{
    public VisibilityGroup VisibilityGroup { get; set; }

    protected override TerrainGridRenderData GenerateComponentData([NotNull] Entity entity, [NotNull] TerrainGrid component)
    {
        return new TerrainGridRenderData() { ModelComponent = new() { Model = new() 
            } 
        };
    }
    public override void Draw(RenderContext context)
    {
        base.Draw(context);

        var graphicsDevice = Services.GetService<IGraphicsDeviceService>().GraphicsDevice;
        var sceneSystem = context.Services.GetService<SceneSystem>();
        var commandList = sceneSystem.Game.GraphicsContext.CommandList;
        foreach (var grid in ComponentDatas)
        {
            var vertices = grid.Key.GenerateVertices();
            var indices = grid.Key.GenerateIndices();
            if( grid.Value.Size != grid.Key.Size)
            {
                grid.Value.Size = grid.Key.Size;
                var indexBuffer = Stride.Graphics.Buffer.Index.New(graphicsDevice, indices, GraphicsResourceUsage.Dynamic);
                grid.Value.IndexBuffer = indexBuffer;
                var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(graphicsDevice, vertices, GraphicsResourceUsage.Dynamic);
                grid.Value.VertexBuffer = vertexBuffer;
            }
            if(grid.Value.IndexBuffer is null)
            {
                var indexBuffer = Stride.Graphics.Buffer.Index.New(graphicsDevice, indices, GraphicsResourceUsage.Dynamic);
                grid.Value.IndexBuffer = indexBuffer;
            }
            else
            {
                grid.Value.IndexBuffer.SetData(commandList, indices);
            }
            if(grid.Value.VertexBuffer is null)
            {
                var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(graphicsDevice, vertices, GraphicsResourceUsage.Dynamic);
                grid.Value.VertexBuffer = vertexBuffer;
            }
            else
            {
                grid.Value.VertexBuffer.SetData(commandList,vertices);
            }
            
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
