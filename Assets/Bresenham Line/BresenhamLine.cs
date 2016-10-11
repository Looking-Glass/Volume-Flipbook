using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BresenhamLine : MonoBehaviour
{
    MeshRenderer mr;
    public bool reset;
    public bool plot;
    public Vector2 xy1;
    public Vector2 xy2;
    public Transform xy1Sphere;
    public Transform xy2Sphere;
    Texture2D tex;

    // Use this for initialization
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        tex = new Texture2D(64, 64);
        mr.material.mainTexture = tex;
        Reset();
    }

    void Reset()
    {
        var colors = new Color[tex.width * tex.height];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }
        tex.SetPixels(colors);
        tex.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        xy1 = xy1Sphere.localPosition * tex.width;
        xy2 = xy2Sphere.localPosition * tex.width;

        if (plot || Input.GetKeyDown(KeyCode.A))
        {
            var linePoints = MakeLine(xy1, xy2);
            var color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            foreach (var linePoint in linePoints)
            {
                tex.SetPixel(linePoint[0], linePoint[1], color);
            }
            plot = false;
            tex.Apply();
        }

        if (reset || Input.GetKeyDown(KeyCode.R))
        {
            Reset();
            reset = false;
        }
    }

    public static List<int[]> MakeLine(int x, int y, int x2, int y2)
    {
        List<int[]> points = new List<int[]>();
        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            points.Add(new int[]{x, y});
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
        return points;
    }

    public static List<int[]> MakeLine(Vector2 _xy1, Vector2 _xy2)
    {
        var x1 = Mathf.FloorToInt(_xy1.x);
        var y1 = Mathf.FloorToInt(_xy1.y);
        var x2 = Mathf.FloorToInt(_xy2.x);
        var y2 = Mathf.FloorToInt(_xy2.y);
        var linePoints = MakeLine(x1, y1, x2, y2);
        return linePoints;
    }
}
