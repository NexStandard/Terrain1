using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain1;

using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using System.Collections.Generic;
using System.Diagnostics;
using Terrain1.Tools;

public class TextureAtlasBuilder : SyncScript
{



    public override void Update() { }
}
public class NexMesh : SyncScript
{
    public Material DirtMaterial;
    public Material Blend;
    public Material Gras;
    public Material Terrain;
    public ModelComponent ModelComponent;
    public Texture Bob;
    public Texture Bob2;
    bool x;
    public List<Material> TileMaterials { get; set; } // List of unique materials (Grass, Dirt, Rock, etc.)
    public int TileSize = 256; // Size of one tile texture in pixels

    private Dictionary<Material, Vector2> uvOffsets = new Dictionary<Material, Vector2>(); // UV offsets for each material

    public override void Start()
    {

    }
    public override void Update()
    {
        if (!x && GraphicsDevice != null)
        {
            CreateMesh();
            x = true;
        }

    }

    private void CreateMesh()
    {
        List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
        List<int> indices = new List<int>();

        Vector3[] tilePositions = {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(2, 0, 0)
    };

        var tileMaterials = new List<Material>() { DirtMaterial, Blend, Gras };

        TileMaterials = tileMaterials;
        var mat = CreateTextureAtlas();
        Terrain.Passes[0].Parameters.Set(MaterialKeys.DiffuseMap, mat);

        for (int i = 0; i < tilePositions.Length; i++)
        {
            Vector3 basePos = tilePositions[i];

            Vector2 uvOffset = GetUVOffset(tileMaterials[i]);
            float uvTileSize = 1.0f / tileMaterials.Count;

            int baseIndex = vertices.Count;

            vertices.Add(new VertexPositionNormalTexture(basePos, Vector3.UnitY, new Vector2(1, 0)));
            vertices.Add(new VertexPositionNormalTexture(basePos + new Vector3(1, 0, 0), Vector3.UnitY, new Vector2(0,1)));
            vertices.Add(new VertexPositionNormalTexture(basePos + new Vector3(0, 0, 1), Vector3.UnitY, new Vector2(0, 0)));
            vertices.Add(new VertexPositionNormalTexture(basePos + new Vector3(1, 0, 1), Vector3.UnitY, new Vector2(0, 1)));

            indices.Add(baseIndex);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 2);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 3);
            indices.Add(baseIndex + 2);
        }
        var indexBuffer = Stride.Graphics.Buffer.Index.New(GraphicsDevice, indices.ToArray(), GraphicsResourceUsage.Default);
        var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(GraphicsDevice, vertices.ToArray(), GraphicsResourceUsage.Default);
        var mesh = new Mesh
        {
            Draw = new MeshDraw
            {
                PrimitiveType = PrimitiveType.TriangleList,
                DrawCount = indices.Count,
                IndexBuffer = new IndexBufferBinding(indexBuffer, true, indices.Count),
                VertexBuffers = new[] { new VertexBufferBinding(vertexBuffer, VertexPositionNormalTexture.Layout, vertexBuffer.ElementCount) },
            },
            MaterialIndex = 0,
        };
        var model = new Model()
        {
            Meshes = [mesh],
        };


        var amat = Material.New(GraphicsDevice, new()
        {
            Attributes = new()
            {
                Diffuse = new MaterialDiffuseMapFeature { DiffuseMap = new ComputeTextureColor(Bob) },
                DiffuseModel = new MaterialDiffuseLambertModelFeature(),
            }
        });
        // Terrain.Passes[0].Parameters.Set(MaterialKeys.DiffuseMap, mat);
        model.Materials.Add(amat);
        ModelComponent.Model = model;
    }
    public Texture CreateTextureAtlas()
    {
        int tileCount = TileMaterials.Count;
        int atlasWidth = TileSize * tileCount;
        int atlasHeight = TileSize;

        if (GraphicsDevice is null)
            throw new Exception("GraphicsDevice is null");

        var atlas = Texture.New2D(GraphicsDevice, atlasWidth, atlasHeight,
            PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);

        using (var commandList = Game.GraphicsContext.CommandList)
        {
            // commandList.Clear(atlas, new Color4(0, 0, 0, 0)); // Clear to transparent

            for (int i = 0; i < tileCount; i++)
            {
                Material material = TileMaterials[i];
                if (material.Passes.Count == 0) continue; // Ensure material has a valid pass

                var texture = material.Passes[0].Parameters.Get(MaterialKeys.DiffuseMap) as Texture;
                if (texture == null) texture = Bob2; // Skip if no texture

                int offsetX = i * TileSize;
                CopyTextureToAtlas(commandList, atlas, texture, offsetX);
                uvOffsets[material] = new Vector2((float)offsetX / atlasWidth, 0); // Store UV offset
            }
        }

        return atlas;
    }

    private void CopyTextureToAtlas(CommandList commandList, Texture atlas, Texture sourceTexture, int offsetX)
    {
        commandList.CopyRegion(sourceTexture, 0, new ResourceRegion(0, 0, 0, TileSize, TileSize, 1), atlas, 0, offsetX, 0, 0);
    }

    public Vector2 GetUVOffset(Material material)
    {
        return uvOffsets.TryGetValue(material, out var uv) ? uv : Vector2.Zero;
    }
}
