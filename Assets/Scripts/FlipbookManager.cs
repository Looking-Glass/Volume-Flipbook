using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FlipbookManager : MonoBehaviour
{
    public string savedFolderName = "SAVED";
    public int imageWidth = 512;
    public int imageHeight = 256;
    public int flipbookLayerCount = 7;//hardcoded to 7
    public static FlipbookManager instance;
    public static Action<int> advanceFrameEvent;
    public GameObject flipbookPrefab;
    Color32[] blackPixelSet;
    public enum FlipbookState
    {
        DEMO,
        DRAWING
    }
    public FlipbookState flipbookState = FlipbookState.DEMO;
    public FlipbookObject currentFlipbookObject;
    public int currentFlipbookFileNumber;
    public float framerate = 1 / 12f;
    public bool playing;

    [Header("timeline visuals")]
    public Transform timelineParentTransform;
    public GameObject frameIndicatorPrefab;
    public float distBetweenFrameIndicators;
    int currentlyHighlightedFrame; //holding this to make it easier to switch the indicator back to being non-highlighted
    public Color[] timelineHighlightColors;
    public float timelineShowTime;
    Coroutine timelineShowCoroutine;

    [Header("demo mode stuff")]
    int currentlyPlayingDemoIndex = 0;
    public int framesToPlayDemoBeforeSwitching = 200;
    bool cyclingDemos;
    float idleTimer;
    public float maxTimeBeforeDemoMode = 10f;
    public GameObject warningText;
    public Text countdownText;
    
    void OnEnable()
    {
        ButtonManager.buttonPressAction += ButtonPress;
    }

    void OnDisable()
    {
        ButtonManager.buttonPressAction -= ButtonPress;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //gets the demos started
        SwitchToDemoMode();
    }
    
    void Update()
    {
        if (flipbookState == FlipbookState.DRAWING)
        {
            if (Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0)
            {
                idleTimer += Time.deltaTime;
            }
            else
            {
                idleTimer = 0;
            }


            if (idleTimer > maxTimeBeforeDemoMode - 10)
            {
                warningText.SetActive(true);

                string timeLeft = Mathf.Ceil(maxTimeBeforeDemoMode - idleTimer).ToString("0");
                countdownText.text = timeLeft;
                if (idleTimer > maxTimeBeforeDemoMode)
                {
                    SwitchToDemoMode();
                }
            }
            else
            {
                warningText.SetActive(false);
            }
        }
    }
    
    void ButtonPress(int i)
    {
        idleTimer = 0;

        switch (i)
        {
            //make new flipbook
            case 0:
                if (flipbookState == FlipbookState.DEMO)
                {
                    MakeNewFlipbook();
                    SwitchToDrawMode();

                }
                break;

            //save flipbook
            case 1:
                if (flipbookState == FlipbookState.DRAWING)
                {
                    SaveFlipbook();
                }
                break;

            //forward / back frame
            case 6:
                if (flipbookState == FlipbookState.DRAWING)
                    advanceFrameEvent(-1);
                break;
            case 8:
                if (flipbookState == FlipbookState.DRAWING)
                    advanceFrameEvent(1);
                break;

            //play / pause
            case 7:
                if (flipbookState == FlipbookState.DRAWING)
                {
                    if (!playing)
                    {
                        StartCoroutine(PlayMovie());
                    }
                    else
                    {
                        playing = false;
                    }
                    ShowTimeline();
                }
                break;
        }
    }

    IEnumerator PlayMovie()
    {
        if (playing == true) yield break;

        playing = true;
        while (playing)
        {
            yield return new WaitForSeconds(framerate);
            if (playing)
                advanceFrameEvent(1);
        }
    }

    public static float GetSliceZPos(int slice, bool normalized = false)
    {
        float depthInterval = (float)1 / hypercubeCamera.mainCam.sliceTextures.Length;
        float depth = depthInterval * (slice + 1);
        if (normalized == false)
        {
            depth = depth * hypercubeCamera.mainCam.transform.localScale.z -
                    hypercubeCamera.mainCam.transform.localScale.z * 0.5f;
        }
        return depth;
    }

    /// <summary>
    /// leave path blank to start a new flipbook
    /// </summary>
    public void MakeNewFlipbook(string pathToLoadFrom = "")
    {
        //clear history
        HistoryManager.instance.ClearHistory();

        //clear any currently existing flipbooks
        var flipbooks = FindObjectsOfType<FlipbookObject>();
        for (int i = 0; i < flipbooks.Length; i++)
        {
            Destroy(flipbooks[i].gameObject);
        }

        //should be only one canvas, return error if not there
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No canvas detected! Can't make flipbook");
            return;
        }

        var flipbookGameObject = Instantiate(flipbookPrefab, canvas.transform);
        var flipbookComponent = flipbookGameObject.GetComponent<FlipbookObject>();
        List<Texture2D[]> flipbookFrames;
        if (pathToLoadFrom != "")
        {
            flipbookFrames = LoadFlipbookTexturesFromFile(pathToLoadFrom);
        }
        else
        {
            flipbookFrames = new List<Texture2D[]>() { MakeFlipbookFrameTextures() };
        }
        flipbookComponent.SetFlipbookTextures(flipbookFrames);
        currentFlipbookObject = flipbookComponent;

        //generate a timeline and set the highlight to the first frame
        GenerateTimelineVisuals(flipbookFrames.Count);
        HighlightCurrentFrame(0);
    }

    public void SwitchToDrawMode()
    {
        playing = false;
        cyclingDemos = false;
        //for saving
        var savedPath = GetSavedPath();
        string[] fileList = Directory.GetDirectories(savedPath);
        currentFlipbookFileNumber = fileList.Length;

        flipbookState = FlipbookState.DRAWING;
    }

    public void SwitchToDemoMode()
    {
        flipbookState = FlipbookState.DEMO;
        warningText.SetActive(false);
        StartCoroutine(PlayMovie());
        IncrementDemoIndex(0);
    }

    Texture2D MakeFlipbookTexture(Texture2D duplicateTexture = null)
    {
        Texture2D tex = new Texture2D(imageWidth, imageHeight);
        if (blackPixelSet == null)
        {
            blackPixelSet = new Color32[tex.width * tex.height];
            for (int i = 0; i < tex.width * tex.height; i++)
            {
                blackPixelSet[i] = Color.black;
            }
        }
        if (duplicateTexture == null)
        {
            tex.SetPixels32(blackPixelSet);
        }
        else
        {
            tex.SetPixels32(duplicateTexture.GetPixels32());
        }
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// default is make empty textures. pass a set of textures to make a duplicate
    /// </summary>
    /// <returns></returns>
    public Texture2D[] MakeFlipbookFrameTextures(Texture2D[] duplicateSet = null)
    {
        //make an array for the layers of this frame
        Texture2D[] layerTextures = new Texture2D[flipbookLayerCount];

        //start looping through layers of this frame, assigning a texture from PNG for each.
        for (int layer = 0; layer < flipbookLayerCount; layer++)
        {
            if (duplicateSet == null)
            {
                layerTextures[layer] = MakeFlipbookTexture();
            }
            else
            {
                layerTextures[layer] = MakeFlipbookTexture(duplicateSet[layer]);
            }

            layerTextures[layer].Apply();
        }

        return layerTextures;
    }

    public List<Texture2D[]> LoadFlipbookTexturesFromFile(string fullFlipbookDirectoryPath)
    {
        //get frame directories
        string[] frameDirectories = Directory.GetDirectories(fullFlipbookDirectoryPath);
        List<Texture2D[]> flipbookFrames = new List<Texture2D[]>();

        //start looping, frames first, then layers of each frame
        for (int frame = 0; frame < frameDirectories.Length; frame++)
        {
            //make an array for the layers of this frame
            Texture2D[] layerTextures = new Texture2D[flipbookLayerCount];
            string[] layerDirectories = Directory.GetFiles(frameDirectories[frame]);

            //start looping through layers of this frame, assigning a texture from PNG for each.
            for (int layer = 0; layer < flipbookLayerCount; layer++)
            {
                var imageBytes = File.ReadAllBytes(layerDirectories[layer]);
                layerTextures[layer] = MakeFlipbookTexture();
                layerTextures[layer].LoadImage(imageBytes);
                layerTextures[layer].Apply();
            }

            //Add the array of layers to the list of frames.
            flipbookFrames.Add(layerTextures);
        }

        return flipbookFrames;
    }

    public void GenerateTimelineVisuals(int frameCount)
    {
        for (int i = 0; i < frameCount; i++)
        {
            Transform fi;
            if (i < timelineParentTransform.childCount)
            {
                fi = timelineParentTransform.GetChild(i);
            }
            else
            {
                fi = Instantiate(frameIndicatorPrefab, timelineParentTransform).transform;
            }

            //reset position
            fi.position = timelineParentTransform.position;

            //add the distance to the right
            fi.position += Vector3.right * distBetweenFrameIndicators * (i % 10);

            //aligning on middle
            fi.position -= Vector3.up * distBetweenFrameIndicators * (i / 10);
            fi.position += Vector3.up * distBetweenFrameIndicators * ((frameCount - 1) / 10) * 0.5f;
        }

        //remove no longer-used frame indicators
        for (int i = frameCount; i < timelineParentTransform.childCount; i++)
        {
            Transform fi = timelineParentTransform.GetChild(i);
            Destroy(fi.gameObject);
        }
    }

    public void HighlightCurrentFrame(int currentFrame)
    {
        //this is checking if the timeline has been generated yet before updating
        if (timelineParentTransform.childCount <= Mathf.Max(currentFrame, currentlyHighlightedFrame))
        {
            return;
        }

        timelineParentTransform.GetChild(currentlyHighlightedFrame).GetComponent<RawImage>().color =
            timelineHighlightColors[0];
        timelineParentTransform.GetChild(currentFrame).GetComponent<RawImage>().color =
            timelineHighlightColors[1];
        currentlyHighlightedFrame = currentFrame;
    }

    string GetSavedPath()
    {
        var savedPath = Path.GetFullPath(".");
        savedPath = Path.Combine(savedPath, savedFolderName);
        if (!Directory.Exists(savedPath))
        {
            Directory.CreateDirectory(savedPath);
        }
        return savedPath;
    }

    void SaveFlipbook()
    {
        //Set up to save as a new file
        string fileName = currentFlipbookFileNumber.ToString("00000");
        string savedFilePath = Path.Combine(GetSavedPath(), fileName);
        if (!Directory.Exists(savedFilePath))
        {
            Directory.CreateDirectory(savedFilePath);
        }
        for (int frame = 0; frame < currentFlipbookObject.flipbookTextures.Count; frame++)
        {
            string framePath = Path.Combine(savedFilePath, frame.ToString());
            if (!Directory.Exists(framePath))
            {
                Directory.CreateDirectory(framePath);
            }
            for (int layer = 0; layer < currentFlipbookObject.flipbookTextures[frame].Length; layer++)
            {
                var pngBytes = currentFlipbookObject.flipbookTextures[frame][layer].EncodeToPNG();
                string layerPath = Path.Combine(framePath, layer.ToString() + ".png");
                File.WriteAllBytes(layerPath, pngBytes);
            }
        }

        //delete any extra frames still hanging around
        string[] framePaths = Directory.GetDirectories(savedFilePath);
        for (int frame = currentFlipbookObject.flipbookTextures.Count; frame < framePaths.Length; frame++)
        {
            Directory.Delete(framePaths[frame], true);
        }
    }

    IEnumerator CountdownToCycleDemos()
    {
        if (cyclingDemos) yield break;

        cyclingDemos = true;
        while (cyclingDemos)
        {
            yield return new WaitForSeconds(framerate * framesToPlayDemoBeforeSwitching);
            if (cyclingDemos)
                IncrementDemoIndex(1);
        }
    }

    void IncrementDemoIndex(int i)
    {
        var savedFlipbookDirectories = Directory.GetDirectories(GetSavedPath());
        currentlyPlayingDemoIndex += i;

        if (savedFlipbookDirectories.Length == 0)
        {
            Debug.LogWarning("no saved flipbooks!");
            MakeNewFlipbook();
            SwitchToDrawMode();
            return;
        }

        if (currentlyPlayingDemoIndex >= savedFlipbookDirectories.Length)
        {
            currentlyPlayingDemoIndex -= savedFlipbookDirectories.Length;
        }
        else if (currentlyPlayingDemoIndex < 0)
        {
            currentlyPlayingDemoIndex += savedFlipbookDirectories.Length;
        }
        
        MakeNewFlipbook(savedFlipbookDirectories[currentlyPlayingDemoIndex]);

        //Start the coroutine of counting down till we switch to the next demo
        StartCoroutine(CountdownToCycleDemos());
    }


    public void ShowTimeline()
    {
        if (timelineShowCoroutine != null)
        {
            StopCoroutine(timelineShowCoroutine);
        }
        timelineShowCoroutine = StartCoroutine(timelineShowAnimation());
    }

    IEnumerator timelineShowAnimation()
    {
        timelineParentTransform.gameObject.SetActive(true);
        yield return new WaitForSeconds(timelineShowTime);
        timelineParentTransform.gameObject.SetActive(false);
    }

}
