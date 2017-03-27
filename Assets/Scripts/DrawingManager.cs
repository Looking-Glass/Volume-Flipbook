using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawingManager : MonoBehaviour
{
    [Tooltip("Should be at least 1, 1")]
    public Vector2 correctionScale = Vector2.one;
    Vector2 hypercubeScale;

    //Don't have a better way of handling color changes right now, so I'll just import the colors from the Button Manager right now.
    Dictionary<int, Color> colors;
    public Color currentColor;
    Material cursorMaterial;
    public float zOffset;
    int LayerIndex;
    public int layerIndex
    {
        get { return LayerIndex; }
        set
        {
            if (value != LayerIndex)
            {
                //todo: add draw history logic
            }

            LayerIndex = value;
            int length = FlipbookManager.instance.flipbookLayerCount;
            while (LayerIndex < 0)
            {
                LayerIndex += length;
            }
            while (LayerIndex >= length)
            {
                LayerIndex -= length;
            }

            zPos = FlipbookManager.GetSliceZPos(layerIndex) + zOffset;
        }
    }
    float zPos;

    //Drawing
    Coroutine drawingCoroutine;

    //Brushes
    /// <summary>
    /// an array of the brushes, typically 3. [i, j] where i is the pixel # and j=0 is x, j=1 is y
    /// </summary>
    int[][,] brushes;
    public int[] brushSizes;
    [Range(0, 2)]
    public int brushSizeIndex;

    //mouse hiding
    float mouseHidingTimer;
    public float timeBeforeHidingMouse = 5f;
    public RawImage mouseRawImage;

    //history
    public DrawingHistory currentDrawingHistory;

    //making a singleton
    public static DrawingManager instance;

    void GenerateBrushes()
    {
        brushes = new int[3][,];
        for (int i = 0; i < 3; i++)
        {
            GenerateBrush(i, brushSizes[i]);
        }
    }

    void GenerateBrush(int index, int radius)
    {
        List<int[]> points = new List<int[]>();
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (Mathf.Sqrt(j * j + i * i) <= radius)
                {
                    points.Add(new[] { j, i });
                }
            }
        }
        brushes[index] = new int[points.Count, 2];
        for (int i = 0; i < points.Count; i++)
        {
            brushes[index][i, 0] = points[i][0];
            brushes[index][i, 1] = points[i][1];
        }
    }

    void OnEnable()
    {
        ButtonManager.buttonPressAction += ButtonPress;
    }

    void OnDisable()
    {
        ButtonManager.buttonPressAction -= ButtonPress;
    }

    void ButtonPress(int i)
    {
        if (colors.ContainsKey(i))
        {
            SetCursorColor(i);
        }
        if (FlipbookManager.instance.flipbookState == FlipbookManager.FlipbookState.DRAWING)
        {
            switch (i)
            {
                //Change layers
                case 4:
                    layerIndex++;
                    break;
                case 10:
                    layerIndex--;
                    break;

                //brush sizes
                case 21:
                    brushSizeIndex = 0;
                    break;
                case 22:
                    brushSizeIndex = 1;
                    break;
                case 23:
                    brushSizeIndex = 2;
                    break;
            }
        }
    }

    void SetCursorColor(int i)
    {
        currentColor = colors[i];
        cursorMaterial.color = currentColor;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cursorMaterial = GetComponent<RawImage>().material;
        hypercubeScale = hypercubeCamera.mainCam.transform.localScale;
        zPos = FlipbookManager.GetSliceZPos(layerIndex) + zOffset;
        ImportColorsFromButtonManager();
        GenerateBrushes();
        SetCursorColor(20); //set the color to white
        StartCoroutine(Drawing());
    }

    void Update()
    {

        //todo: this isn't the best way to handle drawing, but since there's nothing else using mouse input, i won't work on an event system for this yet.
        MousePositionUpdate();

        //handle mouse hiding
        HandleMouseHiding();

        //drawing history
        if (FlipbookManager.instance.flipbookState == FlipbookManager.FlipbookState.DRAWING)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartDrawHistory();
            }
            if (Input.GetMouseButtonUp(0))
            {
                EndDrawHistory();
            }
        }
    }

    public void StartDrawHistory()
    {
        currentDrawingHistory = new DrawingHistory(FlipbookManager.instance.currentFlipbookObject.frameIndex, layerIndex);
    }

    public void EndDrawHistory()
    {
        if (currentDrawingHistory != null)
        {
            currentDrawingHistory.SetFinalTexture();
            HistoryManager.instance.PerformAndRecord(currentDrawingHistory, true);
            currentDrawingHistory = null;
        }
    }

    void HandleMouseHiding()
    {
        if (Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0)
        {
            mouseHidingTimer += Time.deltaTime;
        }
        else
        {
            mouseHidingTimer = 0;
        }

        if (FlipbookManager.instance.flipbookState == FlipbookManager.FlipbookState.DRAWING &&
            !FlipbookManager.instance.playing)
        {
            mouseHidingTimer = 0;
        }

        if (mouseHidingTimer > timeBeforeHidingMouse)
        {
            mouseRawImage.enabled = false;
        }
        else
        {
            mouseRawImage.enabled = true;
        }
    }

    void MousePositionUpdate()
    {
        Vector2 mousePosNormalized =
            Vector2.Scale(Input.mousePosition, new Vector2(1f / Screen.width, 1f / Screen.height)) -
            Vector2.one * 0.5f;
        Vector2 mousePosHypercubeScaled = Vector2.Scale(mousePosNormalized, hypercubeScale);
        transform.position = Vector3.Scale(mousePosHypercubeScaled, correctionScale) + Vector3.forward * zPos;
    }

    void ImportColorsFromButtonManager()
    {
        colors = new Dictionary<int, Color>();
        for (int i = 12; i <= 20; i++)
        {
            if (i == 18) //make an exception for eraser color
            {
                colors.Add(i, Color.black);
            }
            else
            {
                colors.Add(i, ButtonManager.instance.buttonPropertiesSet[i].buttonColor);
            }
        }
    }

    IEnumerator Drawing()
    {
        int previousPosX = -9999;
        int previousPosY = -9999;
        int posX;
        int posY;
        FlipbookObject currentFlipbook;
        int[,] bresenhamLine;
        while (true)
        {
            if (!Input.GetMouseButton(0) && previousPosX != -9999)
            {
                previousPosX = -9999;
                previousPosY = -9999;
            }
            if (Input.GetMouseButton(0) && FlipbookManager.instance.flipbookState == FlipbookManager.FlipbookState.DRAWING)
            {
                posX =
                    Mathf.FloorToInt((transform.position.x + hypercubeCamera.mainCam.transform.localScale.x * 0.5f) /
                                     hypercubeCamera.mainCam.transform.localScale.x *
                                     FlipbookManager.instance.imageWidth);
                posY =
                    Mathf.FloorToInt((transform.position.y + hypercubeCamera.mainCam.transform.localScale.y * 0.5f) /
                                     hypercubeCamera.mainCam.transform.localScale.y *
                                     FlipbookManager.instance.imageHeight);

                if (previousPosX != -9999 && previousPosY != -9999)
                {
                    currentFlipbook = FlipbookManager.instance.currentFlipbookObject;
                    bresenhamLine = MakeBresenhamLine(previousPosX, previousPosY, posX, posY);
                    for (int i = 0; i < bresenhamLine.GetLength(0); i++)
                    {
                        for (int j = 0; j < brushes[brushSizeIndex].GetLength(0); j++)
                        {

                            currentFlipbook.DrawPixelOnTexture(
                                currentFlipbook.frameIndex,
                                layerIndex,
                                bresenhamLine[i, 0] + brushes[brushSizeIndex][j, 0],
                                bresenhamLine[i, 1] + brushes[brushSizeIndex][j, 1],
                                currentColor
                                );
                        }
                    }
                    currentFlipbook.flipbookTextures[currentFlipbook.frameIndex][layerIndex].Apply();
                }

                previousPosX = posX;
                previousPosY = posY;
            }
            yield return new WaitForSeconds(1f / 60);
        }
    }

    void RecordStateToHistoryBeforeDrawing()
    {

    }

    /// <summary>
    /// Returns an int[i, j] where i is the list of coords, j=0 is the x pos and j=1 is y pos
    /// </summary>
    public static int[,] MakeBresenhamLine(int x, int y, int x2, int y2)
    {
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
        int[,] points = new int[longest + 1, 2];
        for (int i = 0; i <= longest; i++)
        {
            points[i, 0] = x;
            points[i, 1] = y;
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
}

public class DrawingHistory : HistoryAction
{
    public Texture2D previousTexture2D;
    public Texture2D finalTexture2D;
    public int frame;
    public int layer;

    public DrawingHistory(int frame, int layer)
    {
        var tex = FlipbookManager.instance.currentFlipbookObject.flipbookTextures[frame][layer];

        previousTexture2D = new Texture2D(tex.width, tex.height);
        previousTexture2D.SetPixels32(tex.GetPixels32());

        this.frame = frame;
        this.layer = layer;
    }

    public void SetFinalTexture()
    {
        var tex = FlipbookManager.instance.currentFlipbookObject.flipbookTextures[frame][layer];

        finalTexture2D = new Texture2D(tex.width, tex.height);
        finalTexture2D.SetPixels32(tex.GetPixels32());
    }

    public override void PerformAction()
    {
        FlipbookManager.instance.currentFlipbookObject.frameIndex = frame;
        FlipbookManager.instance.currentFlipbookObject.flipbookTextures[frame][layer].SetPixels32(finalTexture2D.GetPixels32());
        FlipbookManager.instance.currentFlipbookObject.flipbookTextures[frame][layer].Apply();
    }

    public override void UndoAction()
    {
        FlipbookManager.instance.currentFlipbookObject.frameIndex = frame;
        FlipbookManager.instance.currentFlipbookObject.flipbookTextures[frame][layer].SetPixels32(previousTexture2D.GetPixels32());
        FlipbookManager.instance.currentFlipbookObject.flipbookTextures[frame][layer].Apply();
    }
}