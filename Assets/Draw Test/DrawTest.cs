using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public class DrawTest : MonoBehaviour
{
    public GameObject cursor;
    Vector2 mousePos;
    Texture2D tex;
    MeshRenderer mr;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        tex = new Texture2D(256, 128);
        tex.filterMode = FilterMode.Point;
        var blackPixels = new Color[tex.GetPixels().Length];
        for (int i = 0; i < blackPixels.Length; i++)
        {
            blackPixels[i] = Color.black;
        }
        tex.SetPixels(blackPixels);
        tex.Apply();
        mr.material.mainTexture = tex;
    }

    void Update()
    {
        var mpX = Mathf.Clamp(Input.mousePosition.x, 0, Screen.width);
        var mpY = Mathf.Clamp(Input.mousePosition.y, 0, Screen.height);
        mousePos = new Vector2(mpX / Screen.width, mpY / Screen.height);

        cursor.transform.position = transform.position + Vector3.Scale(mousePos - Vector2.one * 0.5f, transform.localScale) + Vector3.forward * cursor.transform.position.z;

        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine("DrawLine");
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopCoroutine("DrawLine");
        }
    }

    IEnumerator DrawLine()
    {
        var dist = -1f;
        var previousRawMouse = Input.mousePosition;

        //We can increase spacing to make sense if we want later, but for now just leave it at zero.
        var spacing = 0f;

        var segmentCount = 0;
        var previousPx = 0;
        var previousPy = 0;

        while (true)
        {
            dist -= Vector3.Distance(previousRawMouse, Input.mousePosition);
            if (dist < 0)
            {
                var pixelToSet = mousePos;
                pixelToSet.x *= tex.width;
                pixelToSet.y *= tex.height;

                var px = Mathf.FloorToInt(pixelToSet.x);
                var py = Mathf.FloorToInt(pixelToSet.y);

                px = px == tex.width ? tex.width - 1 : px;
                py = py == tex.height ? tex.height - 1 : py;

                tex.SetPixel(px, py, Color.white);
                dist += spacing;

                //Bresenham line
                if (segmentCount > 0)
                {
                    var linePoints = BresenhamLine.MakeLine(previousPx, previousPy, px, py);
                    foreach (var linePoint in linePoints)
                    {
                        tex.SetPixel(linePoint[0], linePoint[1], Color.white);
                    }
                }
                segmentCount += 1;

                previousPx = px;
                previousPy = py;

                tex.Apply();
            }
            previousRawMouse = Input.mousePosition;

            yield return new WaitForEndOfFrame();
        }
    }
}
