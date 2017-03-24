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
    DipShow dipShow;
    public Color[] colors;
    public GameObject cursorFillGameObject;

    void Start()
    {
        flipMaster = FindObjectOfType<FlipMaster>();
        drawing = FindObjectOfType<Drawing>();
        pTex = (Texture2D)paletteQuad.GetComponent<MeshRenderer>().material.mainTexture;
        dipShow = GetComponent<DipShow>();

        colorCoord = new[] { 2, 3 };
        SetColorFromCoord();

    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(toggleShowPalette))
        {
            if (flipMaster.flipControls == FlipMaster.FlipControls.Palette)
            {
                flipMaster.RevertControlsToGeneral();
            }
            else if (flipMaster.flipControls == FlipMaster.FlipControls.General)
            {
                flipMaster.SetFlipControls(FlipMaster.FlipControls.Palette);
            }
            dipShow.ForceToggleAnim();
        }
        */
        
        //Choose a color
        /*
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
        */

        //colors determined by arcade buttons
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(flipMaster.arcadeButton[12 + i]))
            {
                currentColor = colors[i];
                drawing.currentColor = currentColor;
                cursorFillGameObject.GetComponent<MeshRenderer>().material.color = currentColor;
            }
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
