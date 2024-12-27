using Stride.Core.Mathematics;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Terrain1.Tools;

/// <summary>
/// Describes a custom vertex format structure that contains position, normal and texture information.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct VertexPositionNormalColorTexture : IEquatable<VertexPositionNormalColorTexture>, IVertex
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VertexPositionNormalTexture"/> struct.
    /// </summary>
    /// <param name="position">The position of this vertex.</param>
    /// <param name="normal">The vertex normal.</param>
    /// <param name="textureCoordinate">UV texture coordinates.</param>
    public VertexPositionNormalColorTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate,Color color) : this()
    {
        Position = position;
        Normal = normal;
        TextureCoordinate = textureCoordinate;
        Color = color;
    }

    /// <summary>
    /// XYZ position.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The vertex normal.
    /// </summary>
    public Vector3 Normal;

    /// <summary>
    /// UV texture coordinates.
    /// </summary>
    public Vector2 TextureCoordinate;

    public Color Color;
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
        VertexElement.TextureCoordinate<Vector2>(),
        VertexElement.Color<Color>());

    public bool Equals(VertexPositionNormalColorTexture other)
    {
        return Position.Equals(other.Position) && Normal.Equals(other.Normal) && TextureCoordinate.Equals(other.TextureCoordinate) && Color.Equals(other.Color);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is VertexPositionNormalColorTexture && Equals((VertexPositionNormalColorTexture)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = Position.GetHashCode();
            hashCode = (hashCode * 397) ^ Normal.GetHashCode();
            hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
            hashCode = (hashCode * 397) ^ Color.GetHashCode();
            return hashCode;
        }
    }

    public VertexDeclaration GetLayout()
    {
        return Layout;
    }

    public void FlipWinding()
    {
        TextureCoordinate.X = (1.0f - TextureCoordinate.X);
    }

    public static bool operator ==(VertexPositionNormalColorTexture left, VertexPositionNormalColorTexture right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VertexPositionNormalColorTexture left, VertexPositionNormalColorTexture right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return string.Format("Position: {0}, Normal: {1}, Texcoord: {2}", Position, Normal, TextureCoordinate);
    }
}