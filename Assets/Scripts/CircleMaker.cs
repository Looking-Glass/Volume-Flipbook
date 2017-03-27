using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CircleMaker : MonoBehaviour
{
    public float circleLength = 0;
    public float tailLength = 0;
    float lineSegmentLength = 0.01f;
    public float radius = 1;
    public LineRenderer lr;

    // Use this for initialization
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (lr == null)
        {
            Debug.LogWarning(gameObject.name + " Circle maker should have line renderer component!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCircle();
    }

    void UpdateCircle()
    {
        List<Vector3> positions = new List<Vector3>();
        for (float i = tailLength; i < circleLength; i += lineSegmentLength)
        {
            positions.Add(new Vector3(Mathf.Sin(i * Mathf.PI * 2) * radius, Mathf.Cos(i * Mathf.PI * 2) * radius, 0f) + transform.position);
        }
        positions.Add(new Vector3(Mathf.Sin(circleLength * Mathf.PI * 2) * radius, Mathf.Cos(circleLength * Mathf.PI * 2) * radius, 0f) + transform.position);
        lr.numPositions = positions.Count;
        lr.SetPositions(positions.ToArray());
    }
}
