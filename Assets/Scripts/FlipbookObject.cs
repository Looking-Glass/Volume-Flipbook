using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class FlipbookObject : MonoBehaviour
{
    RawImage[] rawImages;
    /// <summary>
    /// List of frames, array of layers. Access by [frameNumber][layerNumber]
    /// </summary>
    public List<Texture2D[]> flipbookTextures;
    int FrameIndex;
    public int maxFrames = 100;

    public int frameIndex
    {
        get { return FrameIndex; }
        set
        {
            //not sure what this was for, todo: figure out why this null check is necessary
            if (rawImages == null) return;

            //increment frame and make sure it's within the range of frames
            int val = value;
            while (val < 0)
                val += flipbookTextures.Count;
            while (val >= flipbookTextures.Count)
                val -= flipbookTextures.Count;

            if (val != FrameIndex)
            {
                bool drawing = DrawingManager.instance.currentDrawingHistory != null;
                DrawingManager.instance.EndDrawHistory();
                FrameIndex = val;
                UpdateTexturesToFrameIndex();
                if (!FlipbookManager.instance.playing)
                {
                    FlipbookManager.instance.GenerateTimelineVisuals(flipbookTextures.Count);
                    FlipbookManager.instance.HighlightCurrentFrame(FrameIndex);
                    FlipbookManager.instance.ShowTimeline();
                }
                if (drawing)
                    DrawingManager.instance.StartDrawHistory();
            }
        }
    }

    void Awake()
    {
        rawImages = GetComponentsInChildren<RawImage>();
        for (int i = 0; i < rawImages.Length; i++)
        {
            rawImages[i].transform.position = Vector3.forward * FlipbookManager.GetSliceZPos(i);
        }
    }

    void Update()
    {

    }

    void OnEnable()
    {
        FlipbookManager.advanceFrameEvent += AdvanceFrame;
        ButtonManager.buttonPressAction += ButtonPress;
    }

    void OnDisable()
    {
        FlipbookManager.advanceFrameEvent -= AdvanceFrame;
        ButtonManager.buttonPressAction -= ButtonPress;
    }

    public void SetFlipbookTextures(List<Texture2D[]> ft)
    {
        flipbookTextures = ft;
        frameIndex = 0;
        UpdateTexturesToFrameIndex();
    }

    void AdvanceFrame(int increment)
    {
        frameIndex += increment;
    }

    void UpdateTexturesToFrameIndex()
    {
        //get out of this if there aren't enough flipbook textures loaded yet to update them.
        if (flipbookTextures.Count <= frameIndex)
        {
            print("count of flipbook textures is less than the frame index.");
            print(flipbookTextures.Count + " flipbook textures count");
            return;
        }
        for (int i = 0; i < rawImages.Length; i++)
        {
            rawImages[i].texture = flipbookTextures[frameIndex][i];
        }
        FlipbookManager.instance.HighlightCurrentFrame(frameIndex);
    }

    void ButtonPress(int i)
    {
        if (FlipbookManager.instance.flipbookState == FlipbookManager.FlipbookState.DRAWING &&
            DrawingManager.instance.currentDrawingHistory == null) //make sure there isn't a drawing going on right now
        {

            switch (i)
            {
                //duplicate frame
                case 2:
                    if (flipbookTextures.Count < maxFrames)
                        HistoryManager.instance.PerformAndRecord(new AddFrameHistory(frameIndex, false));
                    break;

                //delete frame
                case 3:
                    if (flipbookTextures.Count > 1)
                    {
                        HistoryManager.instance.PerformAndRecord(new DeleteFrameHistory(frameIndex));
                    }
                    break;

                //add frame
                case 5:
                    if (flipbookTextures.Count < maxFrames)
                        HistoryManager.instance.PerformAndRecord(new AddFrameHistory(frameIndex, true));
                    break;
            }
        }
    }

    public void DrawPixelOnTexture(int frameToDrawOn, int layerToDrawOn, int x, int y, Color color)
    {
        if (x < 0 || x >= flipbookTextures[frameToDrawOn][layerToDrawOn].width) return;
        if (y < 0 || y >= flipbookTextures[frameToDrawOn][layerToDrawOn].height) return;
        flipbookTextures[frameToDrawOn][layerToDrawOn].SetPixel(x, y, color);
    }

    //do not do this outside of the history managet (use historymanager.performandrecord new framehistory)
    public void AddFrame(bool blank, Texture2D[] customTextures = null)
    {
        //stop the movie from playing
        FlipbookManager.instance.playing = false;

        //is it not blank? if it's not blank, feed a texture2d[] to MakeFlipbookFrameTextures
        //are there customTextures? if so use those, otherwise duplicate current frame
        flipbookTextures.Insert(frameIndex + 1,
            !blank
                ? FlipbookManager.instance.MakeFlipbookFrameTextures(customTextures ?? flipbookTextures[frameIndex])
                : FlipbookManager.instance.MakeFlipbookFrameTextures());

        frameIndex++;
    }
    public void DeleteFrame()
    {
        //stop the movie from playing
        FlipbookManager.instance.playing = false;
        flipbookTextures.RemoveAt(frameIndex);

        if (frameIndex != 0)
        {
            frameIndex--;
        }
        else
        {
            UpdateTexturesToFrameIndex();
            FlipbookManager.instance.GenerateTimelineVisuals(flipbookTextures.Count);
            FlipbookManager.instance.HighlightCurrentFrame(FrameIndex);
            FlipbookManager.instance.ShowTimeline();
        }
    }
}

public class AddFrameHistory : HistoryAction
{
    public int frame;
    public bool blank;

    public AddFrameHistory(int frame, bool blank = true)
    {
        this.frame = frame;
        this.blank = blank;
    }

    public override void PerformAction()
    {
        var fb = FlipbookManager.instance.currentFlipbookObject;
        fb.frameIndex = frame;
        fb.AddFrame(blank);

    }

    public override void UndoAction()
    {
        var fb = FlipbookManager.instance.currentFlipbookObject;
        fb.frameIndex = frame + 1;
        fb.DeleteFrame();
    }
}

public class DeleteFrameHistory : HistoryAction
{
    public int frame;
    public Texture2D[] textures;

    public DeleteFrameHistory(int frame)
    {
        this.frame = frame;
        var currentFrameTextures = FlipbookManager.instance.currentFlipbookObject.flipbookTextures[frame];
        this.textures = new Texture2D[currentFrameTextures.Length];
        for (int i = 0; i < this.textures.Length; i++)
        {
            this.textures[i] = new Texture2D(currentFrameTextures[i].width, currentFrameTextures[i].height);
            this.textures[i].SetPixels32(currentFrameTextures[i].GetPixels32());
        }
    }

    public override void PerformAction()
    {
        var fb = FlipbookManager.instance.currentFlipbookObject;
        fb.frameIndex = frame;
        fb.DeleteFrame();
    }

    public override void UndoAction()
    {
        var fb = FlipbookManager.instance.currentFlipbookObject;

        if (frame != 0)
        {
            fb.frameIndex = frame - 1;
        }
        //todo: need to modify addframe to accept a texture
        fb.AddFrame(false, textures);

        //if the deleted frame was frame 0, addframe puts it in front so now we have to move it back
        if (frame == 0)
        {
            var item = fb.flipbookTextures[0];
            fb.flipbookTextures.RemoveAt(0);
            fb.flipbookTextures.Insert(1, item);
            fb.frameIndex = 0;
        }
    }
}