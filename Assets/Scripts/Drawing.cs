using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Brush
{
    public List<int[]> pixels;

    public Brush()
    {
        pixels = new List<int[]>();
    }
}

public class DrawHistory : HistoryAction
{
    //int is x, y. color[] is oldColor, newColor
    public Dictionary<int, Color[]> pixels;
    public FlipMaster flipMaster;
    public int fsIndex;
    public int fpIndex;

    public DrawHistory(FlipMaster flipMaster)
    {
        pixels = new Dictionary<int, Color[]>();
        this.flipMaster = flipMaster;
        fsIndex = flipMaster.currentSlice;
        fpIndex = flipMaster.currentFrame;
    }

    public override void PerformAction()
    {
        var flipPanel = flipMaster.flipSlices[fsIndex].fp[fpIndex];
        foreach (var pixel in pixels)
        {
            int px;
            int py = Math.DivRem(pixel.Key, flipPanel.tex.width, out px);
            flipPanel.tex.SetPixel(px, py, pixel.Value[1]);
        }
        flipPanel.tex.Apply();
    }

    public override void UndoAction()
    {
        var flipPanel = flipMaster.flipSlices[fsIndex].fp[fpIndex];
        foreach (var pixel in pixels)
        {
            int px;
            int py = Math.DivRem(pixel.Key, flipPanel.tex.width, out px);
            flipPanel.tex.SetPixel(px, py, pixel.Value[0]);
        }
        flipPanel.tex.Apply();
    }
}

public class Drawing : MonoBehaviour
{
    public GameObject cursor;
    public Texture2D tex;
    public MeshRenderer mr;
    Vector2 mousePos;

    FlipMaster flipMaster;
    //so that it triggers a slice and frame reset
    int currentSlice = -1;
    int currentFrame = -1;
    HistoryManager history;

    float deadspaceW;
    float deadspaceH;

    Brush b1;
    Brush b2;
    Brush b3;

    public Brush currentBrush;
    public Color currentColor = Color.white;
    DrawHistory drawHistory;

    void Start()
    {
        flipMaster = GetComponent<FlipMaster>();
        history = FindObjectOfType<HistoryManager>();

        //Because the screen's aspect ratio won't match the texture's, adjust for that.
        var screenRatio = (float)Screen.width / Screen.height;
        var hypercube = FindObjectOfType<hypercubeCamera>();
        var texRatio = (float)hypercube.transform.localScale.x / hypercube.transform.localScale.y;

        var screenMaxW = (float)Screen.width;
        var screenMaxH = (float)Screen.height;
        if (texRatio < screenRatio)
            screenMaxH = Screen.width / texRatio;
        if (screenRatio > texRatio)
            screenMaxW = Screen.height * texRatio;

        deadspaceW = (Screen.width - screenMaxW) / 2;
        deadspaceH = (Screen.height - screenMaxH) / 2;

        //Brushes
        b1 = new Brush();
        b1.pixels.Add(new[] { 0, 0 });

        b2 = new Brush();
        b2.pixels.Add(new[] { 0, 0 });
        b2.pixels.Add(new[] { 1, 0 });
        b2.pixels.Add(new[] { -1, 0 });
        b2.pixels.Add(new[] { 0, 1 });
        b2.pixels.Add(new[] { 0, -1 });

        b3 = new Brush();
        b3.pixels.Add(new[] { 0, 0 });
        b3.pixels.Add(new[] { 1, 0 });
        b3.pixels.Add(new[] { -1, 0 });
        b3.pixels.Add(new[] { 0, 1 });
        b3.pixels.Add(new[] { 0, -1 });
        b3.pixels.Add(new[] { 1, 1 });
        b3.pixels.Add(new[] { 1, -1 });
        b3.pixels.Add(new[] { -1, -1 });
        b3.pixels.Add(new[] { -1, 1 });
        b3.pixels.Add(new[] { -2, 1 });
        b3.pixels.Add(new[] { -2, 0 });
        b3.pixels.Add(new[] { -2, -1 });
        b3.pixels.Add(new[] { 2, 1 });
        b3.pixels.Add(new[] { 2, 0 });
        b3.pixels.Add(new[] { 2, -1 });
        b3.pixels.Add(new[] { 1, -2 });
        b3.pixels.Add(new[] { 0, -2 });
        b3.pixels.Add(new[] { -1, -2 });
        b3.pixels.Add(new[] { 1, 2 });
        b3.pixels.Add(new[] { 0, 2 });
        b3.pixels.Add(new[] { -1, 2 });

        currentBrush = b2;
    }

    void Update()
    {
        if (currentSlice != flipMaster.currentSlice || currentFrame != flipMaster.currentFrame)
        {
            ResetRefs();
        }

        var mpX = Input.mousePosition.x;
        var mpY = Input.mousePosition.y;
        mousePos = new Vector2((mpX + deadspaceW) / (Screen.width - deadspaceW), (mpY + deadspaceH) / (Screen.height - deadspaceH));
        //mousePos = new Vector2((mpX) / (Screen.width), (mpY) / (Screen.height));
        mousePos = mousePos.SetX(Mathf.Clamp01(mousePos.x));
        mousePos = mousePos.SetY(Mathf.Clamp01(mousePos.y));

        cursor.transform.position = transform.position + Vector3.Scale(mousePos - Vector2.one * 0.5f, transform.localScale) + Vector3.forward * cursor.transform.position.z;

        if (flipMaster.flipControls == FlipMaster.FlipControls.General)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine("DrawLine");
            }

            if (Input.GetMouseButtonUp(0))
            {
                history.PerformAndRecord(drawHistory, true);
                StopCoroutine("DrawLine");
            }

            //Change brushes
            if (Input.GetKeyDown(KeyCode.Alpha1))
                currentBrush = b1;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                currentBrush = b2;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                currentBrush = b3;
        }
    }

    public void ResetRefs()
    {
        if (flipMaster == null)
            return;

        currentSlice = flipMaster.currentSlice;
        currentFrame = flipMaster.currentFrame;
        var cursorPos = flipMaster.flipControls == FlipMaster.FlipControls.General
            ? flipMaster.flipSlices[currentSlice].transform.position
            : flipMaster.flipSlices[0].transform.position;
        cursor.transform.position = cursorPos - Vector3.forward * 0.01f;
        mr = flipMaster.flipSlices[currentSlice].fp[currentFrame].mr;
        tex = (Texture2D)mr.material.mainTexture;
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

        //Save the overwritten pixels for history
        drawHistory = new DrawHistory(flipMaster);

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

                DrawBrush(px, py, ref drawHistory);
                dist += spacing;

                //Bresenham line
                if (segmentCount > 0)
                {
                    var linePoints = BresenhamLine.MakeLine(previousPx, previousPy, px, py);
                    foreach (var linePoint in linePoints)
                    {
                        DrawBrush(linePoint[0], linePoint[1], ref drawHistory);
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

    void DrawBrush(int x, int y, ref DrawHistory drawHistoryIn)
    {
        foreach (var pixel in currentBrush.pixels)
        {
            var px = x + pixel[0];
            var py = y + pixel[1];
            px = px == tex.width ? tex.width - 1 : px;
            py = py == tex.height ? tex.height - 1 : py;

            if (px < 0 || px >= tex.width)
                return;
            if (py < 0 || py >= tex.height)
                return;

            //if there isn't alread an entry for this pixel, record the old and new colors here.
            var pixelNum = px + py * tex.width;
            if (!drawHistoryIn.pixels.ContainsKey(pixelNum))
                drawHistoryIn.pixels.Add(pixelNum, new[] { tex.GetPixel(px, py), currentColor });

            tex.SetPixel(px, py, currentColor);
        }
    }
}
