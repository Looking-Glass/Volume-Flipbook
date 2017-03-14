using System.Collections;
using System.Collections.Generic;
using hypercube;
using UnityEngine;
using UnityEngine.UI;

public class FlipbookObject : MonoBehaviour
{
    hypercubeCamera hypercube;
    castMesh castmesh;
    
    //keep a list of flip slices, even though these might not be necessary to hang on to
    public List<GameObject> flipSlices;
    public GameObject flipSlicePrefab;
    
    void Start()
    {
        hypercube = FindObjectOfType<hypercubeCamera>();
        castmesh = FindObjectOfType<castMesh>();
        InitiateFlipSlices();
    }
    
    void Update()
    {

    }

    void InitiateFlipSlices()
    {
        for (int i = 0; i < castmesh.slices; i++)
        {
            flipSlices.Add(Instantiate(flipSlicePrefab, transform));
            flipSlices[i].transform.localScale = Vector3.one;
            flipSlices[i].GetComponent<SnapToSlice>().slice = i;
            flipSlices[i].GetComponent<RawImage>().texture = new Texture2D(512, 256);
        }
    }
}