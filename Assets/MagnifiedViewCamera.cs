using UnityEngine;
using System.Collections;

public class MagnifiedViewCamera : MonoBehaviour
{
    public Transform hypercube;
    public Transform cursor;
    hypercubeCamera hypercubeCam;
    Camera cam;

    // Use this for initialization
    void Start()
    {
        hypercubeCam = hypercube.GetComponent<hypercubeCamera>();
        transform.localScale = hypercube.transform.localScale;
        cam = GetComponent<Camera>();
        var camClipDist = transform.localScale.z * 0.5f / hypercubeCam.localCastMesh.slices;
        cam.nearClipPlane = camClipDist * -0.5f;
        cam.farClipPlane = camClipDist * 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.zero + Vector3.forward * cursor.position.z;
    }
}
