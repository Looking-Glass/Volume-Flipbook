using System;
using UnityEngine;
using System.Collections;

public class Palette : MonoBehaviour
{
    public KeyCode toggleShowPalette = KeyCode.P;
    public AnimationCurve movementCurve;
    public float speed = 3f;
    public Color currentColor;
    public int[] colorCoord;
    public GameObject paletteQuad;
    public Transform dot;
    public Transform cursor;
    Texture2D pTex;
    FlipMaster flipMaster;
    Drawing drawing;
    float timer;
    Vector3 startPosition;
    Vector3 endPosition;
    int tempSlice;

    void Start()
    {
        startPosition = transform.localPosition;
        endPosition = startPosition.SetY(0);
        flipMaster = FindObjectOfType<FlipMaster>();
        drawing = FindObjectOfType<Drawing>();
        pTex = (Texture2D)paletteQuad.GetComponent<MeshRenderer>().material.mainTexture;

        colorCoord = new[] { 2, 3 };
        SetColorFromCoord();

    }

    void Update()
    {
        if (Input.GetKeyDown(toggleShowPalette))
        {
            if (flipMaster.flipControls != FlipMaster.FlipControls.Palette)
            {
                flipMaster.RevertConstrolsToGeneral();
                flipMaster.flipControls = FlipMaster.FlipControls.Palette;
            }
            else
            {
                flipMaster.RevertConstrolsToGeneral();
            }
            TogglePalette();
        }
        
        //Choose a color
        if (flipMaster.flipControls == FlipMaster.FlipControls.Palette)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = new Ray(cursor.position + Vector3.back, Vector3.forward);
                var rayHit = new RaycastHit();

                var rayBool = Physics.Raycast(ray, out rayHit);
                if (rayBool)
                {

                    int px = Mathf.FloorToInt(rayHit.textureCoord.x * pTex.width);
                    int py = Mathf.FloorToInt(rayHit.textureCoord.y * pTex.height);

                    colorCoord = new[] { px, py };
                    SetColorFromCoord();
                }
            }
        }

    }

    public void TogglePalette()
    {
        StopCoroutine("TogglePaletteCR");
        StartCoroutine("TogglePaletteCR");
    }

    IEnumerator TogglePaletteCR()
    {
        var showPalette = flipMaster.flipControls == FlipMaster.FlipControls.Palette;

        foreach (FlipSlice flipSlice in flipMaster.flipSlices)
        {
            foreach (FlipPanel flipPanel in flipSlice.fp)
            {
                flipPanel.mr.material.color = showPalette ? new Color(0.3f, 0.3f, 0.3f) : Color.white;
            }
        }

        if (showPalette)
        {
            tempSlice = flipMaster.currentSlice;
            flipMaster.currentSlice = 0;
        }
        else
        {
            flipMaster.currentSlice = tempSlice;
        }

        var targetTime = showPalette ? 1f : 0f;
        while (!Mathf.Approximately(timer, targetTime))
        {
            timer += Time.deltaTime*(flipMaster.flipControls == FlipMaster.FlipControls.Palette ? 1 : -1)*speed;
            timer = Mathf.Clamp01(timer);
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, movementCurve.Evaluate(timer));
            yield return new WaitForEndOfFrame();
        }
    }

    void SetColorFromCoord()
    {
        int px = colorCoord[0];
        int py = colorCoord[1];
        currentColor = pTex.GetPixel(px, py);

        var tx = (px + 0.5f - pTex.width / 2f) / pTex.width;
        var ty = (py + 0.5f - pTex.height / 2f) / pTex.height;

        dot.localPosition = new Vector3(tx, ty, dot.localPosition.z);

        drawing.currentColor = currentColor;
    }
}
