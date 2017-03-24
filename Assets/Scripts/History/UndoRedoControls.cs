using UnityEngine;

public class UndoRedoControls : MonoBehaviour
{
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
            if (Input.GetKeyDown(flipMaster.arcadeButton[9]))
                historyManager.Undo();
            if (Input.GetKeyDown(flipMaster.arcadeButton[11]))
                historyManager.Redo();
        }
    }
}
