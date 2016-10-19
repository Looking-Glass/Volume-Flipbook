using UnityEngine;
using System.Collections;
using System.IO;
using Ionic.Zip;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveUI : MonoBehaviour
{
    FlipMaster flipMaster;
    public GameObject saveUI;
    public GameObject loadUI;
    InputField saveInputField;
    InputField loadInputField;
    DipShow saveDipShow;
    DipShow loadDipShow;

    void Start()
    {
        flipMaster = FindObjectOfType<FlipMaster>();
        saveInputField = saveUI.GetComponentInChildren<InputField>();
        loadInputField = loadUI.GetComponentInChildren<InputField>();
        saveDipShow = saveUI.GetComponent<DipShow>();
        loadDipShow = loadUI.GetComponent<DipShow>();
    }

    void Update()
    {
        var editing = flipMaster.flipControls == FlipMaster.FlipControls.Save ||
                      flipMaster.flipControls == FlipMaster.FlipControls.Load;

        if (Input.GetKeyDown(KeyCode.S) && !editing)
        {
            flipMaster.SetFlipControls(FlipMaster.FlipControls.Save);
            saveDipShow.Toggle();
            saveInputField.enabled = true;
            saveInputField.ActivateInputField();
        }

        if (Input.GetKeyDown(KeyCode.L) && !editing)
        {
            flipMaster.SetFlipControls(FlipMaster.FlipControls.Load);
            loadDipShow.Toggle();
            loadInputField.enabled = true;
            loadInputField.ActivateInputField();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (flipMaster.flipControls == FlipMaster.FlipControls.Save)
            {
                SaveFile(saveInputField.text);
            }

            if (flipMaster.flipControls == FlipMaster.FlipControls.Load)
            {
                LoadFile(loadInputField.text);
            }
        }

        if (editing)
        {
            var mouseButtonDown = false;
            for (int i = 0; i <= 2; i++)
            {
                if (Input.GetMouseButtonDown(i))
                    mouseButtonDown = true;
            }
            if (Input.GetKeyDown(KeyCode.Escape) || mouseButtonDown)
            {
                flipMaster.RevertControlsToGeneral();
            }
        }
    }

    void SaveFile(string filename)
    {
        var finalPath = Path.GetFullPath(".");
        finalPath = Path.Combine(finalPath, "Saved");
        Directory.CreateDirectory(finalPath);
        finalPath = Path.Combine(finalPath, filename + ".vflip");

        if (File.Exists(finalPath))
            File.Delete(finalPath);

        var tempPath = Path.GetTempPath();
        print(tempPath);

        tempPath = Path.Combine(tempPath, "VFLIP");
        if (Directory.Exists(tempPath))
            DeleteDirectory(tempPath);
        Directory.CreateDirectory(tempPath);

        for (int i = 0; i < flipMaster.flipSlices.Count; i++)
        {
            var slicePath = Path.Combine(tempPath, i.ToString());
            Directory.CreateDirectory(slicePath);
            for (int j = 0; j < flipMaster.flipSlices[i].fp.Count; j++)
            {
                var panelPath = Path.Combine(slicePath, j + ".png");
                var panelStream = File.Create(panelPath);
                var panelBytes = flipMaster.flipSlices[i].fp[j].tex.EncodeToPNG();
                panelStream.Write(panelBytes, 0, panelBytes.Length);
                panelStream.Close();
            }
        }

        using (Ionic.Zip.ZipFile zipFile = new ZipFile(finalPath))
        {
            zipFile.AddDirectory(tempPath);
            zipFile.Save();
        }

        print("Saved!");
    }

    void LoadFile(string filename)
    {
        var savedPath = Path.GetFullPath(".");
        savedPath = Path.Combine(savedPath, "Saved");
        if (!Directory.Exists(savedPath))
        {
            print("No saved flipbooks yet!");
            return;
        }
        savedPath = Path.Combine(savedPath, filename + ".vflip");
        if (!File.Exists(savedPath))
        {
            print("File not found!");
            return;
        }

        var tempPath = Path.GetTempPath();
        tempPath = Path.Combine(tempPath, "VFLIP");
        if (Directory.Exists(tempPath))
            DeleteDirectory(tempPath);
        Directory.CreateDirectory(tempPath);

        using (Ionic.Zip.ZipFile zipFile = new ZipFile(savedPath))
        {
            zipFile.ExtractAll(tempPath);
        }

        var sliceDirectories = Directory.GetDirectories(tempPath);
        flipMaster.NewFlipbook(sliceDirectories);

        DeleteDirectory(tempPath);
    }

    public static void DeleteDirectory(string target_dir)
    {
        string[] files = Directory.GetFiles(target_dir);
        string[] dirs = Directory.GetDirectories(target_dir);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(target_dir, false);
    }
}
