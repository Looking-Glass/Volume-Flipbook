using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using UnityEngine;

public class AttractMode : MonoBehaviour
{

    public enum AttractModeState
    {
        DEMO,
        DRAW,
        OPEN
    }

    FlipMaster flipMaster;
    SaveUI saveui;
    List<string> newFilesList;
    public float timeBetweenScenes;
    public GameObject thumbnailPrefab;

    // Use this for initialization
    void Awake()
    {
        flipMaster = FindObjectOfType<FlipMaster>();
        saveui = FindObjectOfType<SaveUI>();
    }

    void Start()
    {
        StartDemoMode();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartDemoMode()
    {
        StartCoroutine(DemoMode());
    }

    void ShuffleFilelist()
    {
        var savedPath = Path.GetFullPath(".");
        savedPath = Path.Combine(savedPath, "Saved");
        if (!Directory.Exists(savedPath))
        {
            print("No saved flipbooks yet!");
            return;
        }
        var files = Directory.GetFiles(savedPath);
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
        List<string> filesList = files.ToList();
        if (newFilesList == null)
            newFilesList = new List<string>();
        newFilesList.Clear();
        for (int i = 0; i < filesList.Count; i++)
        {
            int randomIndex = Random.Range(0, filesList.Count);
            newFilesList.Add(filesList[randomIndex]);
            filesList.RemoveAt(randomIndex);
        }
    }

    IEnumerator DemoMode()
    {
        ShuffleFilelist();
        int index = 0;
        yield return new WaitForEndOfFrame();
        flipMaster.flipControls = FlipMaster.FlipControls.DemoMode;
        CreateThumbnail();
        while (true)
        {
            if (index == newFilesList.Count) index = 0;
            saveui.LoadFile(newFilesList[index++]);
            flipMaster.StartCoroutine("PlayAnimation");
            yield return new WaitForSeconds(timeBetweenScenes);
        }
    }

    void ShowThumbnails()
    {

    }

    void CreateThumbnail()
    {
        saveui.LoadFile(newFilesList[0]);
    }
}
