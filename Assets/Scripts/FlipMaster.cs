using System;
using System.Collections;
using System.Collections.Generic;
using hypercube;
using UnityEngine;

public class FlipPanel
{
    public GameObject gameObject;
    public Transform transform;
    public MeshFilter mf;
    public MeshRenderer mr;
    public Texture2D tex;
}

public class FlipSlice
{
    public GameObject gameObject;
    public Transform transform;
    public List<FlipPanel> fp;
}

public class FlipMaster : MonoBehaviour
{
    public GameObject flipSlicePrefab;
    public GameObject flipPanelPrefab;
    [Range(2, 1024)] public int panelWidth = 256;
    [Range(2, 1024)] public int panelHeight = 128;
    hypercubeCamera hypercube;
    castMesh castmesh;
    public List<FlipSlice> flipSlices;
    List<List<int>> timetable;
    //this range will have to adjust based on how many slices there are in newer machines
    [Range(0, 9)] public int currentSlice;

    //Controls
    public enum FlipControls
    {
        General,
        Palette
    }
    public FlipControls flipControls;
    public KeyCode changeSliceForward = KeyCode.RightBracket;
    public KeyCode changeSliceBack = KeyCode.LeftBracket;

    //Hacky mapping correction
    float correctionRatio = 0.8f;

    // Use this for initialization
    void Start()
    {
        hypercube = FindObjectOfType<hypercubeCamera>();
        castmesh = FindObjectOfType<castMesh>();

        NewFlipbook();
    }

    void Update()
    {
        if (flipControls == FlipControls.General)
        {
            if (Input.GetKeyDown(changeSliceForward))
                ChangeSlice(-1);
            if (Input.GetKeyDown(changeSliceBack))
                ChangeSlice(1);
        }
    }

    public void NewFlipbook()
    {
        //is hardcoded for now
        var flipSliceCount = 10;

        #region scaling to fit in hypercube

        transform.localScale = hypercube.transform.localScale;
        var hyperRatio = hypercube.transform.localScale.x / hypercube.transform.localScale.y;
        var panelRatio = (float) panelWidth / panelHeight;
        if (hyperRatio > panelRatio)
        {
            transform.localScale = transform.localScale.SetY(hypercube.transform.localScale.y);
            transform.localScale = transform.localScale.SetX(transform.localScale.y * panelRatio);
        }
        else
        {
            transform.localScale = transform.localScale.SetX(hypercube.transform.localScale.x);
            transform.localScale = transform.localScale.SetY(transform.localScale.x / panelRatio);
        }

        transform.localScale = transform.localScale.SetX(transform.localScale.x * correctionRatio);

        #endregion


        //slice distance is usually 1 slice distance unless the amount of slices in the machine is at least twice as many as in the animation, then it goes up to 2, etc. 
        var sliceDistance = Mathf.Floor((float) flipSliceCount / castmesh.slices) / castmesh.slices;

        //create the slices and their panels, and store them in flipSlices
        flipSlices = new List<FlipSlice>();
        timetable = new List<List<int>>();
        for (int i = 0; i < flipSliceCount; i++)
        {
            //make a slice
            var flipSlice = (GameObject) Instantiate(flipSlicePrefab, transform);
            flipSlice.transform.localPosition += Vector3.forward *
                                                 (sliceDistance * (i - flipSliceCount * 0.5f) + 0.5f * sliceDistance);
                //TODO: test whether this should be + or - .5*slicedistance
            flipSlice.transform.localScale = Vector3.one;
            flipSlices.Add(NewFlipSlice(flipSlice));

            //make a first panel for that slice
            var flipPanel = (GameObject) Instantiate(flipPanelPrefab, flipSlice.transform);
            flipPanel.transform.localPosition = Vector3.zero;
            flipPanel.transform.localScale = Vector3.one;
            flipSlices[i].fp.Add(NewFlipPanel(flipPanel));

            //add an entry to the timetable
            timetable.Add(new List<int>());
        }
    }

    FlipSlice NewFlipSlice(GameObject fsGameObject)
    {
        var flipSlice = new FlipSlice()
        {
            gameObject = fsGameObject,
            transform = fsGameObject.transform,
            fp = new List<FlipPanel>()
        };

        return flipSlice;
    }

    FlipPanel NewFlipPanel(GameObject fpGameObject)
    {
        var flipPanel = new FlipPanel
        {
            gameObject = fpGameObject,
            transform = fpGameObject.transform,
            mf = fpGameObject.GetComponent<MeshFilter>(),
            mr = fpGameObject.GetComponent<MeshRenderer>(),
            tex = new Texture2D(panelWidth, panelHeight, TextureFormat.ARGB32, false, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            }
        };
        var colors = new Color[flipPanel.tex.width * flipPanel.tex.height];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }
        flipPanel.tex.SetPixels(colors);
        flipPanel.tex.Apply();
        flipPanel.mr.material.mainTexture = flipPanel.tex;

        return flipPanel;
    }

    void ChangeSlice(int i)
    {
        currentSlice += i;
        if (currentSlice > castmesh.slices - 1)
            currentSlice = 0;
        if (currentSlice < 0)
            currentSlice = castmesh.slices - 1;
    }
}
