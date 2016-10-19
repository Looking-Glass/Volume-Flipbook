using UnityEngine;
using System.Collections;

public class DipShow : MonoBehaviour
{
    FlipMaster flipMaster;
    public FlipMaster.FlipControls thisFlipControls;
    float timer;
    Vector3 startPosition;
    Vector3 endPosition;
    float speed = 3f;
    public AnimationCurve animCurve;

    void Start()
    {
        flipMaster = FindObjectOfType<FlipMaster>();
        startPosition = transform.position;
        endPosition = startPosition.SetY(0);
    }

    public void Toggle()
    {
        StopCoroutine("ToggleCR");
        StartCoroutine("ToggleCR");
    }

    IEnumerator ToggleCR()
    {
        var show = flipMaster.flipControls == thisFlipControls;

        foreach (FlipSlice flipSlice in flipMaster.flipSlices)
        {
            foreach (FlipPanel flipPanel in flipSlice.fp)
            {
                flipPanel.mr.material.color = show ? new Color(0.3f, 0.3f, 0.3f) : Color.white;
            }
        }

        var targetTime = show ? 1f : 0f;
        while (!Mathf.Approximately(timer, targetTime))
        {
            timer += Time.deltaTime * (show ? 1 : -1) * speed;
            timer = Mathf.Clamp01(timer);
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, animCurve.Evaluate(timer));
            yield return new WaitForEndOfFrame();
        }
    }
}
