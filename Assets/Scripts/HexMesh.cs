using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    /// <summary>The mesh generated to cover the entire grid.</summary>
    Mesh hexMesh;

    /// <summary>List of the vertices in the grid.</summary>
    static List<Vector3> vertices = new List<Vector3>();

    /// <summary>List of hex cell colours. </summary>
    static List<Color> colours = new List<Color>();

    /// <summary>List of the triangles in the grid.</summary>
    static List<int> triangles = new List<int>();

    /// <summary>The mesh collider for the generated mesh.</summary>
	MeshCollider meshCollider;

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";        
    }

    #region Triangulation
    /// <summary>Triangulates the vertices and triangles of the cells that make up the grid. Clears all previous data so that this can be done
    /// even if the grid has been previously triangulated. Triangulates each cell individually, then recalculates the mesh normals after the
    /// vertices and triangles have been assigned to the mesh.
    /// </summary>
    /// <param name="cells">Represents the HexCell array "cells" in HexGrid.</param>
    public void Triangulate(HexCell[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        colours.Clear();
        triangles.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        hexMesh.vertices = vertices.ToArray();
        hexMesh.colors = colours.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
    }

    /// <summary>Loops through the triangles in the cell, triangulating each one based on given direction.</summary>
    /// <param name="cell">The current Hex Cell being generated.</param>
    void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    /// <summary>Takes in a direction relative to the current cell. Determines the EdgeVertices of the next cell in the given direction.</summary>
    /// <param name="direction">The given direction.</param>
    /// <param name="cell">The current cell.</param>
    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.Position;
        EdgeVertices e = new EdgeVertices(center + HexMetrics.GetFirstSolidCorner(direction), center + HexMetrics.GetSecondSolidCorner(direction));

        TriangulateEdgeFan(center, e, cell.Colour);

        if (direction <= HexDirection.SE)
        {
            TriangulateConnection(direction, cell, e);
        }        
    }

    /// <summary>Triangulates the vertices in a given direction using the edgevertices provided.</summary>
    /// <param name="direction">The given direction.</param>
    /// <param name="cell">The current cell.</param>
    /// <param name="e1">The provided EdgeVertices from Triangulate().</param>
    void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1)
    {
        HexCell neighbour = cell.GetNeighbour(direction);
        if (neighbour == null)
        {
            return;
        }

        Vector3 bridge = HexMetrics.GetBridge(direction);
        bridge.y = neighbour.Position.y - cell.Position.y;

        EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v4 + bridge);

        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(e1, cell, e2, neighbour);
        }

        else
        {
            TriangulateEdgeStrip(e1, cell.Colour, e2, neighbour.Colour);
        }

        HexCell nextNeighbour = cell.GetNeighbour(direction.Next());
        if (direction <= HexDirection.E && nextNeighbour != null)
        {
            Vector3 v5 = e1.v4 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbour.Position.y;
            
            if (cell.Elevation <= neighbour.Elevation)
            {
                if (cell.Elevation <= nextNeighbour.Elevation)
                {
                    TriangulateCorner(e1.v4, cell, e2.v4, neighbour, v5, nextNeighbour);
                }

                else
                {
                    TriangulateCorner(v5, nextNeighbour, e1.v4, cell, e2.v4, neighbour);
                }
            }

            else if (neighbour.Elevation <= nextNeighbour.Elevation)
            {
                TriangulateCorner(e2.v4, neighbour, v5, nextNeighbour, e1.v4, cell);
            }

            else
            {
                TriangulateCorner(v5, nextNeighbour, e1.v4, cell, e2.v4, neighbour);
            }
        }        
    }

    /// <summary>Triangulates the corners of cell given it's neighbours' EdgeTypes and the vectors of their edges.</summary>
    /// <param name="bottom">The given vector of the bottom cell's EdgeType.</param>
    /// <param name="bottomCell">The cell below the current cell.</param>
    /// <param name="left">The given vector of the left cell's EdgeType.</param>
    /// <param name="leftCell">The cell to the left of the current cell.</param>
    /// <param name="right">The given vector of the right cell's EdgeType.</param>
    /// <param name="rightCell">The cell to the right of the current cell.</param>
    void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }

            else if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
            }

            else
            {
                TriangulateCornerTerracesCliff(
                    bottom, bottomCell, left, leftCell, right, rightCell
                );
            }
        }

        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }

            else
            {
                TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }

        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
            }

            else
            {
                TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            }
        }

        else
        {
            AddTriangle(bottom, left, right);
            AddTriangleColour(bottomCell.Colour, leftCell.Colour, rightCell.Colour);
        }        
    }

    void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell)
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.Colour, endCell.Colour, 1);

        TriangulateEdgeStrip(begin, beginCell.Colour, e2, c2);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(beginCell.Colour, endCell.Colour, i);
            TriangulateEdgeStrip(e1, c1, e2, c2);
        }

        TriangulateEdgeStrip(e2, c2, end, endCell.Colour);
    }

    void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceLerp(beginCell.Colour, leftCell.Colour, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.Colour, rightCell.Colour, 1);

        AddTriangle(begin, v3, v4);
        AddTriangleColour(beginCell.Colour, c3, c4);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.Colour, leftCell.Colour, i);
            c4 = HexMetrics.TerraceLerp(beginCell.Colour, rightCell.Colour, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColour(c1, c2, c3, c4);
        }

        AddQuad(v3, v4, left, right);
        AddQuadColour(c3, c4, leftCell.Colour, rightCell.Colour);
    }

    void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        
        if (b < 0)
        {
            b = -b;
        }

        Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
        Color boundaryColour = Color.Lerp(beginCell.Colour, rightCell.Colour, b);
        
        TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColour);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColour);
        }

        else
        {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColour(leftCell.Colour, rightCell.Colour, boundaryColour);
        }
    }

    void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);

        if (b < 0)
        {
            b = -b;
        }

        Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(left), b);
        Color boundaryColour = Color.Lerp(beginCell.Colour, leftCell.Colour, b);

        TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColour);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColour);
        }

        else
        {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColour(leftCell.Colour, rightCell.Colour, boundaryColour);
        }
    }

    void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 boundary, Color boundaryColour)
    {
        Vector3 v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        Color c2 = HexMetrics.TerraceLerp(beginCell.Colour, leftCell.Colour, 1);

        AddTriangleUnperturbed(Perturb(begin), v2, boundary);
        AddTriangleColour(beginCell.Colour, c2, boundaryColour);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.Colour, leftCell.Colour, i);
            AddTriangleUnperturbed(v1, v2, boundary);
            AddTriangleColour(c1, c2, boundaryColour);
        }

        AddTriangleUnperturbed(v2, Perturb(left), boundary);
        AddTriangleColour(c2, leftCell.Colour, boundaryColour);
    }

    void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color colour)
    {
        AddTriangle(center, edge.v1, edge.v2);
        AddTriangleColour(colour);
        AddTriangle(center, edge.v2, edge.v3);
        AddTriangleColour(colour);
        AddTriangle(center, edge.v3, edge.v4);
        AddTriangleColour(colour);
    }

    void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2)
    {
        AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        AddQuadColour(c1, c2);
        AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        AddQuadColour(c1, c2);
        AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        AddQuadColour(c1, c2);
    }

    #endregion

    #region Triangles
    /// <summary>Creates a triangle with vertices. The index of the first vertex is equal to the length of the list, and is saved before adding any vertices.
    /// </summary>
    /// <param name="v1">First vertex in the triangle.</param>
    /// <param name="v2">Second vertex in the triangle.</param>
    /// <param name="v3">Third vertex in the triangle.</param>
    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    /// <summary>Adds a single colour to the triangle.</summary>
    /// <param name="colour"></param>
    void AddTriangleColour(Color colour)
    {
        colours.Add(colour);
        colours.Add(colour);
        colours.Add(colour);
    }

    /// <summary>Variant of AddTriangleColour for three vertices.</summary>
    /// <param name="c1">Colour for vertex 1.</param>
    /// <param name="c2">Colour for vertex 2.</param>
    /// <param name="c3">Colour for vertex 3.</param>
    void AddTriangleColour(Color c1, Color c2, Color c3)
    {
        colours.Add(c1);
        colours.Add(c2);
        colours.Add(c3);
    }

    #endregion

    #region Quads
    /// <summary>Creates a quad with vertices. The index of the first vertex is equal to the length of the list, and is saved before adding any vertices.
    /// </summary>
    /// <param name="v1">First vertex in the quad.</param>
    /// <param name="v2">Second vertex in the quad.</param>
    /// <param name="v3">Third vertex in the quad.</param>
    /// <param name="v4">Fourth vertex in the quad.</param>
    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        vertices.Add(Perturb(v4));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    /// <summary>Adds the given colour to two vertices in the quad.</summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    void AddQuadColour(Color c1, Color c2)
    {
        colours.Add(c1);
        colours.Add(c1);
        colours.Add(c2);
        colours.Add(c2);
    }

    /// <summary>Variant of AddQuadColour for four vertices.</summary>
    /// <param name="c1">Colour for vertex 1.</param>
    /// <param name="c2">Colour for vertex 2.</param>
    /// <param name="c3">Colour for vertex 3.</param>
    /// <param name="c4">Colour for vertex 4.</param>
    void AddQuadColour(Color c1, Color c2, Color c3, Color c4)
    {
        colours.Add(c1);
        colours.Add(c2);
        colours.Add(c3);
        colours.Add(c4);
    }

    #endregion

    Vector3 Perturb(Vector3 position)
    {
        Vector4 sample = HexMetrics.SampleNoise(position);
        position.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
        position.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;
        return position;
    }
}