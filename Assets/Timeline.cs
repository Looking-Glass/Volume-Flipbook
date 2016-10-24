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
    public Transform cursor;

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
            if (Input.GetMouseButtonDown(0))
            {
                var ray = new Ray(cursor.position + Vector3.back, Vector3.forward);
                var rayHit = new RaycastHit();

                var rayBool = Physics.Raycast(ray, out rayHit);
                if (rayBool)
                {
                    StartCoroutine("SelectorFollowMouse");
                }
            }

            if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Escape))
            {
                flipMaster.RevertControlsToGeneral();
                dipShow.ForceToggleAnim();
            }
        }

        if (Input.GetMouseButtonUp(0) || Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Escape))
        {
            StopCoroutine("SelectorFollowMouse");
            ResetSelector();
        }
    }

    IEnumerator SelectorFollowMouse()
    {
        var waitEOF = new WaitForEndOfFrame();
        while (true)
        {
            if (cursor.position.x < timelineSize && cursor.position.x > -timelineSize)
                selectionMarker.position = selectionMarker.position.SetX(cursor.position.x);
            else if (cursor.position.x > timelineSize)
                selectionMarker.position = selectionMarker.position.SetX(timelineSize);
            else if (cursor.position.x < timelineSize)
                selectionMarker.position = selectionMarker.position.SetX(-timelineSize);


            var currentFramePos = (selectionMarker.position.x + timelineSize) / (timelineSize + timelineSize);
            currentFramePos = Mathf.Lerp(-0.5f / flipMaster.GetFrameCount(), (flipMaster.GetFrameCount() + 0.5f) / flipMaster.GetFrameCount(), currentFramePos);
            currentFramePos = Mathf.Clamp01(currentFramePos);
            currentFramePos = currentFramePos * flipMaster.GetFrameCount();
            var newCurrentFrame = Mathf.FloorToInt(currentFramePos);
            newCurrentFrame = newCurrentFrame >= flipMaster.GetFrameCount() ? newCurrentFrame - 1 : newCurrentFrame;
            flipMaster.currentFrame = newCurrentFrame;
            flipMaster.UpdateFrames();
            yield return waitEOF;
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
