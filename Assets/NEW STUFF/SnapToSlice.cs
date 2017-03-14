using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToSlice : MonoBehaviour
{
    [SerializeField]
    int Slice;
    public int slice
    {
        get { return Slice; }
        set
        {
            Slice = Mathf.Clamp(value, 0, FindObjectOfType<hypercubeCamera>().localCastMesh.slices - 1);
            SetZPosition();
        }
    }

    void OnValidate()
    {
        slice = Slice;
    }

    void SetZPosition()
    {
        var hypercube = FindObjectOfType<hypercubeCamera>();
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y,
            hypercube.transform.localScale.z / hypercube.localCastMesh.slices * (Slice + 1 - 0.5f * hypercube.localCastMesh.slices));
    }
}