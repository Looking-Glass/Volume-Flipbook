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

    void Start()
    {
        flipMaster = FindObjectOfType<FlipMaster>();
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
