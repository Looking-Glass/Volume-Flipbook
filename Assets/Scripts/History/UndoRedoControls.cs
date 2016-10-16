using UnityEngine;

public class UndoRedoControls : MonoBehaviour
{
    public KeyCode undo = KeyCode.Z;
    public KeyCode redo = KeyCode.Y;

    HistoryManager historyManager;
    FlipMaster flipMaster;

    void Start()
    {
        historyManager = FindObjectOfType<HistoryManager>();
        flipMaster = FindObjectOfType<FlipMaster>();
    }

    void Update()
    {
        if (flipMaster.flipControls == FlipMaster.FlipControls.General)
        {
            if (Input.GetKeyDown(undo))
                historyManager.Undo();
            if (Input.GetKeyDown(redo))
                historyManager.Redo();
        }
    }
}
