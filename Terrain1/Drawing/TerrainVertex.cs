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
public struct TerrainVertex : IVertex
{
    public static readonly VertexDeclaration Layout = CreateVertexDeclaration();
    private static VertexDeclaration CreateVertexDeclaration()
    {
        var vertElems = new VertexElement[]
        {
            VertexElement.Position<Vector3>(),
            VertexElement.Normal<Vector3>(),
            VertexElement.Tangent<Vector3>(),
            VertexElement.Color<Color>(0),
            VertexElement.TextureCoordinate<Vector2>(0),
        };
        return new VertexDeclaration(vertElems);
    }

    public Vector3 Position;
    public Vector3 Normal;
    public Vector3 Tangent;

    public Color Color;

    public Vector2 TextureCoords0;

    public readonly VertexDeclaration GetLayout() => Layout;

    public void FlipWinding()
    {
        TextureCoords0.X = (1.0f - TextureCoords0.X);
    }
}