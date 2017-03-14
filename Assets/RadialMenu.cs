using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class RadialWedge
{
    public string wedgeName;
    public Color color;
    public Sprite sprite;
    public Transform trans;
    public RadialMenuWedge rw;
}

public class RadialMenu : MonoBehaviour
{
    public GameObject radialWedge;
    public Transform cursor;
    public AnimationCurve animCurve;
    public TextMesh tooltip;
    [Range(0f, 1f)]
    public float color;
    [Range(0.1f, 1f)]
    public float colorVariance = 1f;
    float wedgeSize;
    float timer;
    float speed = 3f;
    [HideInInspector]
    public bool showing;
    public RadialWedge[] wedges;
    public RadialButtons radialButtons;

    // Use this for initialization
    void Start()
    {
        GenerateMenu();
        EstablishColors();
    }

    public void ToggleShow()
    {
        showing = !showing;
        StopCoroutine("SpreadMenu");
        StartCoroutine("SpreadMenu");

        if (showing)
        {
            transform.position = new Vector3(cursor.position.x, cursor.position.y, transform.position.z);
        }
    }

    void Update()
    {
        var pos = cursor.position - transform.position;
        var dist = pos.magnitude - transform.localScale.x / 1.5f;
        var angle = Mathf.Atan2(pos.x, -pos.y);
        angle = 1f - (angle / Mathf.PI + 1) / 2f;
        var speedWedge = 3f;

        var w = Mathf.FloorToInt(wedges.Length * angle);
        for (int i = 0; i < wedges.Length; i++)
        {
            if (i != w || !showing || dist > 0)
                wedges[i].trans.localScale = Vector3.MoveTowards(wedges[i].trans.localScale, Vector3.one,
                    Time.deltaTime * speedWedge);
            if (Mathf.Approximately(timer, 0f))
            {
                wedges[i].trans.localScale = Vector3.one;
            }
        }

        if (Mathf.Approximately(timer, 1f) && dist <= 0)
        {
            wedges[w].trans.localScale = Vector3.MoveTowards(wedges[w].trans.localScale, Vector3.one * 1.2f,
                Time.deltaTime * speedWedge);
            tooltip.text = wedges[w].wedgeName;

            var tooltipMove = 0.75f;

            tooltip.transform.localPosition = new Vector3(0f, -tooltipMove);

            //Move the tooltip
            if (transform.position.y < -0.4f)
            {
                tooltip.transform.localPosition = tooltip.transform.localPosition.SetY(tooltipMove);
            }

            if (Mathf.Abs(transform.position.x) > 2.8f)
            {
                tooltip.transform.localPosition =
                    tooltip.transform.localPosition = new Vector3(tooltipMove * (transform.position.x > 0 ? -1f : 1f), 0f);
            }
        }
        else
        {
            tooltip.text = "";
        }

        if (showing && Input.GetMouseButtonDown(0) && dist <= 0)
        {
            radialButtons.ButtonActions(w);
        }
    }

    public void GenerateMenu()
    {
        if (wedges.Length == 0)
            return;

        wedgeSize = 1f / wedges.Length;

        for (int i = 0; i < wedges.Length; i++)
        {
            var wedge = (GameObject)Instantiate(radialWedge, transform, false);
            wedges[i].rw = wedge.GetComponent<RadialMenuWedge>();
            wedge.GetComponent<MeshRenderer>().material.color = wedges[i].color;
            wedges[i].trans = wedge.transform;
            wedges[i].rw.spriteSR.sprite = wedges[i].sprite;
        }
    }

    public void EstablishColors()
    {
        for (int i = 0; i < wedges.Length; i++)
        {
            wedges[i].rw.GetComponent<MeshRenderer>().material.color =
                Color.HSVToRGB((color + wedgeSize * i * colorVariance) % 1, 0.66f, 0.66f);
        }
    }

    public IEnumerator SpreadMenu()
    {
        while (!Mathf.Approximately(timer, showing ? 1 : 0))
        {
            timer += Time.deltaTime * speed * (showing ? 1 : -1);
            var animTime = animCurve.Evaluate(timer);

            for (int i = 0; i < wedges.Length; i++)
            {
                var ws = Mathf.Lerp(0f, wedgeSize, animTime);
                var wsi = Mathf.Lerp(0f, wedgeSize * i, animTime);
                wedges[i].rw.SetPointRot(ws, wsi);

                wedges[i].rw.spriteSR.enabled = !Mathf.Approximately(timer, 0f);
                if (wedges[i].rw.spriteSR.enabled)
                {
                    wedges[i].rw.spriteSR.color = new Color(1f, 1f, 1f, animTime);
                }
            }

            timer = Mathf.Clamp01(timer);

            yield return new WaitForEndOfFrame();
        }
    }
}
