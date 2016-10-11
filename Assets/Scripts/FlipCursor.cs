using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipCursor : MonoBehaviour
{
    public Canvas canvas;
    public FlipPanes flipPanes;
    public int cursorSlice;
    RectTransform canvasRect;
    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        var normalizedMousePos = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
            );
        normalizedMousePos -= Vector2.one * 0.5f;
        var canvasSize = new Vector2(canvasRect.rect.width, canvasRect.rect.height);
        rectTransform.localPosition = Vector3.Scale(normalizedMousePos, canvasSize) + Vector3.forward * (flipPanes.paneZs[cursorSlice] - 0.01f);
    }
}
