using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using hypercube;

public class FlipPanes : MonoBehaviour
{
    public bool centered;
    public List<float> paneZs;
    public int currentFrame;
    public GameObject flipSlice;
    public List<GameObject> flipSlices;
    hypercubeCamera hypercube;
    castMesh castmesh;


    void Start()
    {
        hypercube = FindObjectOfType<hypercubeCamera>();
        castmesh = FindObjectOfType<castMesh>();
        paneZs = new List<float>();
        flipSlices = new List<GameObject>();

        //Mesh creation
        for (int i = 0; i < castmesh.slices; i++)
        {
            //Record the z
            paneZs.Add((float) i / castmesh.slices - 0.5f + 0.5f / castmesh.slices);
            flipSlices.Add((GameObject)Instantiate(flipSlice, transform));
            flipSlices[i].transform.localPosition += Vector3.forward * paneZs[i];
            flipSlices[i].transform.localScale = Vector3.one;
        }
    }
}