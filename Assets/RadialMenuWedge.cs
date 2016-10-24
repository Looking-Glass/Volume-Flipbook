using System;
using UnityEngine;
using System.Collections;

public class RadialMenuWedge : MonoBehaviour
{
    [Range(0, 1)]
    public float breakPoint;
    [Range(0, 1)]
    public float rotation;
    Vector3[] locs;
    GMesh gmesh;
    MeshFilter mf;
    public GameObject radialWedge;
    public Transform sprite;

    void Awake()
    {
        gmesh = new GMesh();
        mf = GetComponentInChildren<MeshFilter>();
        mf.mesh = new Mesh();
        GenerateMenu();
        UpdateMenu();
    }

    public void SetPointRot(float point, float rot)
    {
        breakPoint = point;
        rotation = rot;
        UpdateMenu();
    }

    void GenerateMenu()
    {
        locs = new[]
        {
            new Vector3(1, 0), //0
            new Vector3(0, 1), //1
            new Vector3(-1, 0), //2
            new Vector3(0, -1), //3
        };
        gmesh.verts.AddRange(locs);
        
        gmesh.verts.AddRange(new[]
        {
            new Vector3(1, 0), //4
            Vector3.zero //5
        });

        gmesh.tris.AddRange(new[]
        {
            5, 1, 0, //0
            5, 2, 1, //1
            5, 3, 2, //2
            5, 4, 3 //3
        });

        for (int i = 0; i < gmesh.verts.Count; i++)
        {
            gmesh.uvs.Add((gmesh.verts[i] + Vector3.one) * 0.5f);
        }

        gmesh.Apply(mf.mesh);
    }

    void UpdateMenu()
    {
        var newRot = (1 - rotation + 0.25f) * 2 * Mathf.PI;
        var newPoint = (1 - breakPoint) * 4;
        for (int i = 4; i >= 0; i--)
        {
            var v = i * 0.5f * Mathf.PI;
            var vi = v + newRot;
            if (i < newPoint) vi = (1 - breakPoint) * 2 * Mathf.PI + newRot;

            gmesh.verts[i] = new Vector3(Mathf.Cos(vi), Mathf.Sin(vi));

            //uvs
            gmesh.uvs[i] = (gmesh.verts[i] + Vector3.one) * 0.5f;
        }

        gmesh.Apply(mf.mesh);

        var spriteRot = newRot + (1 - breakPoint) * 2.5f * Mathf.PI;
        var sprDist = 0.3f;
        sprite.localPosition = new Vector3(Mathf.Cos(spriteRot) * sprDist, Mathf.Sin(spriteRot) * sprDist, sprite.localPosition.z);
    }
}