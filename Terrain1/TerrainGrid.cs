﻿

using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Core.Serialization;
using Stride.Core.Serialization.Contents;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Terrain1.Drawing;
using Terrain1.Tools;

namespace Terrain;
[Display("Terrain", Expand = ExpandRule.Once)]
[ComponentCategory("Terrain")]
[DefaultEntityComponentRenderer(typeof(TerrainGridProcessor))]
public class TerrainGrid : StartupScript
{
    [DataMemberRange(1, 1024,1,124,0)]
    public int Size { get; init; } = 256;

    [DataMemberRange(1, 1024,1 , 124,0)]
    public int CellSize { get; init; } = 1;

    /// <summary>
    /// Total width of the terrain.
    /// </summary>
    public float TotalWidth => Size * CellSize;

    /// <summary>
    /// Total height of the terrain.
    /// </summary>
    public float TotalHeight => Size * CellSize;

    public Material Material { get; set; }
    public TerrainVertexDraw TerrainVertexDraw { get; set; } = new TerrainStandardVertexDraw();

    [DataMemberIgnore]
    public Dictionary<Int2, Color> VertexColors { get; } = new();
    public Dictionary<Int2, int> VertexColorMaterialMapping { get; } = new();

    [DataMember()]
    public List<KeyValuePair<Int2,float>> VertexHeightsE { get => VertexHeights.ToList(); set => VertexHeights = value.ToDictionary(x => x.Key, y => y.Value); }
    [DataMemberIgnore]
    public Dictionary<Int2, float> VertexHeights { get; private set; } = new();
    public HashSet<Int2> ModifiedVertices = new();
    public Dictionary<Int2, (Color,Color)> VertexColor = new();
    public override void Start()
    {
        Entity.Add(new ModelComponent());
    }
    public List<TerrainVertex> GenerateVertices()
    {
        var vertexCount = (Size + 1) * (Size + 1);
        var vertices = new TerrainVertex[vertexCount];

        var actualCellWidth = TotalWidth / Size;
        var actualCellHeight = TotalHeight / Size;

        for (var row = 0; row <= Size; row++)
        {
            for (var col = 0; col <= Size; col++)
            {
                var index = (row * (Size + 1)) + col;

                var x = col * actualCellWidth;
                var z = row * actualCellHeight;

                var position = new Vector3(x, GetVertexHeight(col, row), z);

                var color = Color.Black; // Default or calculated color
                VertexColors[new Int2(col, row)] = color;

                vertices[index] = new TerrainVertex()
                {
                    Position = position,
                    Normal = Vector3.UnitY,
                };
            }
        }

        return vertices.ToList();
    }
    public int[] GenerateIndices()
    {
        var quadCount = Size * Size;
        var indices = new int[quadCount * 6];

        var index = 0;
        for (var row = 0; row < Size; row++)
        {
            for (var col = 0; col < Size; col++)
            {
                // The four corners of the quad (2x2 square)
                var topLeft = (row * (Size + 1)) + col;
                var topRight = topLeft + 1;
                var bottomLeft = topLeft + Size + 1;
                var bottomRight = bottomLeft + 1;

                // Determine the winding for this quad (alternates based on (row + col) % 2)
                if ((row + col) % 2 == 0)
                {
                    indices[index++] = topRight;
                    indices[index++] = bottomRight;
                    indices[index++] = topLeft;

                    indices[index++] = bottomLeft;
                    indices[index++] = topLeft;
                    indices[index++] = bottomRight;
                }
                else
                {
                    indices[index++] = topLeft;
                    indices[index++] = topRight;
                    indices[index++] = bottomLeft;

                    indices[index++] = bottomLeft;
                    indices[index++] = topRight;
                    indices[index++] = bottomRight;
                }
            }
        }

        return indices;
    }
    public void SetVertexHeight(int col, int row, float height)
    {
        var key = new Int2(col, row);
        VertexHeights[key] = height;
        ModifiedVertices.Add(key);
    }


    public float GetVertexHeight(int col, int row)
    {
        var key = new Int2(col, row);
        return VertexHeights.TryGetValue(key, out var height) ? height : 0f;
    }


    // Get the height of a point in the terrain
    public float GetHeightAtPosition(float x, float z)
    {
        var subColumns = Size;
        var subRows = Size;

        var actualCellWidth = TotalWidth / subColumns;
        var actualCellHeight = TotalHeight / subRows;

        // Determine the column and row of the cell containing the point
        var column = (int)(x / actualCellWidth);
        var row = (int)(z / actualCellHeight);

        // Check if the point is within the terrain bounds
        if (column < 0 || column >= subColumns || row < 0 || row >= subRows)
        {
            throw new ArgumentOutOfRangeException($"The point ({x}, {z}) is outside the terrain bounds.");
        }

        // Local coordinates within the cell
        var localX = x - (column * actualCellWidth);
        var localZ = z - (row * actualCellHeight);

        // Retrieve heights for the vertices of the cell
        var topLeftHeight = GetVertexHeight(column, row);
        var topRightHeight = GetVertexHeight(column + 1, row);
        var bottomLeftHeight = GetVertexHeight(column, row + 1);
        var bottomRightHeight = GetVertexHeight(column + 1, row + 1);

        // Determine which triangle the point lies in
        if (localX + localZ <= actualCellWidth)
        {
            // Top-left triangle
            return InterpolateBarycentric(
                localX, localZ,
                new Vector3(0, topLeftHeight, 0),
                new Vector3(actualCellWidth, topRightHeight, 0),
                new Vector3(0, bottomLeftHeight, actualCellHeight)
            );
        }
        else
        {
            // Bottom-right triangle
            return InterpolateBarycentric(
                localX, localZ,
                new Vector3(actualCellWidth, topRightHeight, 0),
                new Vector3(0, bottomLeftHeight, actualCellHeight),
                new Vector3(actualCellWidth, bottomRightHeight, actualCellHeight)
            );
        }
    }

    private float InterpolateBarycentric(float x, float z, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var fullArea = Vector3.Cross(p2 - p1, p3 - p1).Length() / 2.0f;

        var area1 = Vector3.Cross(p2 - new Vector3(x, 0, z), p3 - new Vector3(x, 0, z)).Length() / 2.0f;
        var area2 = Vector3.Cross(p3 - new Vector3(x, 0, z), p1 - new Vector3(x, 0, z)).Length() / 2.0f;
        var area3 = Vector3.Cross(p1 - new Vector3(x, 0, z), p2 - new Vector3(x, 0, z)).Length() / 2.0f;

        var w1 = area1 / fullArea;
        var w2 = area2 / fullArea;
        var w3 = area3 / fullArea;

        return (w1 * p1.Y) + (w2 * p2.Y) + (w3 * p3.Y);
    }
    #region Testing
    private static readonly Random RandomGenerator = new Random();
    public bool Randomize { get; set; }
    public bool Flatten { get; set; }

    public void SetRandomHeights()
    {
        var subColumns = Size;
        var subRows = Size;

        for (var row = 0; row <= subRows; row++)
        {
            for (var col = 0; col <= subColumns; col++)
            {
                var randomHeight = (float)((RandomGenerator.NextDouble() * 20) - 10); // Random value between -10 and 10
                SetVertexHeight(col, row, randomHeight);
            }
        }
    }
    public void FlattenAll()
    {
        var subColumns = Size;
        var subRows = Size;
        for (var row = 0; row <= subRows; row++)
        {
            for (var col = 0; col <= subColumns; col++)
            {
                var randomHeight = 0f;
                SetVertexHeight(col, row, randomHeight);
            }
        }
    }




    #endregion
}