using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Timeline : MonoBehaviour
{
    FlipMaster flipMaster;
    public GameObject frameMarker;
    public List<Transform> frameMarkers;
    public Transform selectionMarker;
    int currentFrame = -1;
    int frameCount = -1;
    float timelineSize = 3.1f; //todo: don't hardcode this
    DipShow dipShow;

    void Start()
    {
        flipMaster = FindObjectOfType<FlipMaster>();
        dipShow = GetComponent<DipShow>();
    }

    void Update()
    {
        if (frameCount != flipMaster.GetFrameCount())
        {
            ResetTimeline();
        }

        if (currentFrame != flipMaster.currentFrame)
        {
            ResetSelector();
        }

        if (flipMaster.flipControls == FlipMaster.FlipControls.General)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                flipMaster.SetFlipControls(FlipMaster.FlipControls.Timeline);
                dipShow.ForceToggleAnim();
            }
        }
        else if (flipMaster.flipControls == FlipMaster.FlipControls.Timeline)
        {
            if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Escape))
            {
                flipMaster.RevertControlsToGeneral();
                dipShow.ForceToggleAnim();
            }
        }
    }

    public void ResetTimeline()
    {
        if (flipMaster != null)
        {
            //clear the framemarkers
            for (int i = 0; i < frameMarkers.Count; i++)
            {
                Destroy(frameMarkers[i].gameObject);
            }
            frameMarkers.RemoveRange(0, frameMarkers.Count);

            //Todo: use object pool for frame marker sprites
            frameCount = flipMaster.GetFrameCount();
            for (int i = 0; i < frameCount; i++)
            {
                var lerpVal = (i + 1f) / (frameCount + 1f);
                var newXPos = Mathf.Lerp(-timelineSize, timelineSize, lerpVal);
                var frameMarkerGO = (GameObject)Instantiate(frameMarker, transform, false);
                frameMarkers.Add(frameMarkerGO.transform);
                frameMarkerGO.transform.localPosition = frameMarkerGO.transform.localPosition.SetX(newXPos);
            }
            ResetSelector();
        }
    }

    public void ResetSelector()
    {
        currentFrame = flipMaster.currentFrame;
        selectionMarker.position = frameMarkers[currentFrame].position;
    }
}
