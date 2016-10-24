using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// GMesh is an all-purpose class for assisting in mesh generation through code.
/// </summary>
public class GMesh
{
    public List<Vector3> verts;
    public List<int> tris;
    public List<Vector2> uvs;
    public List<Color> colors;

    public GMesh(Mesh mesh = null)
    {
        if (mesh == null)
        {
            verts = new List<Vector3>();
            tris = new List<int>();
            uvs = new List<Vector2>();
            colors = new List<Color>();
        }
        else
        {
            verts = mesh.vertices.ToList();
            tris = mesh.triangles.ToList();
            uvs = mesh.uv.ToList();
            colors = mesh.colors.ToList();
        }
    }

    public void Apply(Mesh meshRef)
    {
        meshRef.SetVertices(verts);
        meshRef.SetTriangles(tris, 0);
        meshRef.SetUVs(0, uvs);
        meshRef.SetColors(colors);
        meshRef.RecalculateNormals();
        meshRef.RecalculateBounds();
    }
}
