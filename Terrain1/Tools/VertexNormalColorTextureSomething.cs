using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Materials;
using Stride.Shaders;
using System;
using System.Runtime.InteropServices;
using Stride.Core;

namespace Terrain1.Tools;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DataContract]
[DataStyle(DataStyle.Compact)]
public record struct TerrainVertex : IEquatable<TerrainVertex>, IVertex
{

    public TerrainVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate) : this()
    {
        Position = position;
        Normal = normal;
        TextureCoordinate = textureCoordinate;
    }

    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TextureCoordinate;

    /// <summary>
    /// Defines structure byte size.
    /// </summary>
    public static readonly int Size = 36;

    /// <summary>
    /// The vertex layout of this struct.
    /// </summary>
    public static readonly VertexDeclaration Layout = new VertexDeclaration(
        VertexElement.Position<Vector3>(),
        VertexElement.Normal<Vector3>(),
        VertexElement.TextureCoordinate<Vector2>()
        );

    public VertexDeclaration GetLayout()
    {
        return Layout;
    }

    public void FlipWinding()
    {
        TextureCoordinate.X = (1.0f - TextureCoordinate.X);
    }
}
[DataContract("TerrainMaskComputeColor")]
[Display("Terrain Mask")]
public class TerrainMaskComputeColor : ComputeNode, IComputeScalar
{
    [DataMember(0), DataMemberRange(0, 8)] // Could be increaed to larger than 8 if more colors are added to the vertex stream
    public int TerrainLayerIndex { get; set; }

    private (string, string) GetSemanticNameAndChannel()
    {
        // Calculate the name of the correct semantic from COLOR0 to COLOR(n) and channel
        // 4 channels per color.
        var semanticIndex = TerrainLayerIndex / 4;
        var channel = (TerrainLayerIndex % 4) switch
        {
            0 => "r",
            1 => "g",
            2 => "b",
            _ => "a"
        };
        
        return ($"COLOR{semanticIndex}", channel);
    }

    public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
    {
        var (semanticName, channel) = GetSemanticNameAndChannel();
        return new ShaderClassSource("ComputeColorFromStream", semanticName, channel);
    }
}