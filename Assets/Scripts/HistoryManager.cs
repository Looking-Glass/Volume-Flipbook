using System.Collections.Generic;
using UnityEngine;

public class HistoryAction
{
    public virtual void PerformAction() { }
    public virtual void UndoAction() { }
}

public class HistoryManager : MonoBehaviour
{
    public static HistoryManager instance;
    public List<HistoryAction> actionStack;
    int index;
    public int maxUndos;

    void OnEnable()
    {
        ButtonManager.buttonPressAction += ButtonPress;
    }

    void OnDisable()
    {
        ButtonManager.buttonPressAction -= ButtonPress;
    }

    void ButtonPress(int i)
    {
        if (FlipbookManager.instance.flipbookState == FlipbookManager.FlipbookState.DRAWING)
        {
            switch (i)
            {
                case 9:
                    Undo();
                    break;
                case 11:
                    Redo();
                    break;
            }
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        actionStack = new List<HistoryAction>();
    }

    public void PerformAndRecord(HistoryAction action, bool onlyRecord = false)
    {
        //If there are undos, erase them
        if (actionStack.Count != index)
        {
            actionStack.RemoveRange(index, actionStack.Count - index);
            Debug.Log("Erased saved Undos because an a new action has been performed");
        }

        Debug.Log("Performed and recorded " + action.GetType() + " to the actionStack");
        actionStack.Add(action);

        if (!onlyRecord)
            actionStack[index].PerformAction();


        index++;

        //cap the number of recorded actions
        while (actionStack.Count > maxUndos)
        {
            Debug.Log("Removed last entry of the history manager, as it's full");
            actionStack.RemoveAt(0);
            index--;
        }
    }

    public void Redo()
    {
        if (actionStack.Count > index)
        {
            actionStack[index++].PerformAction();
            Debug.Log("Redid " + actionStack[index - 1].GetType());
        }
        else
            Debug.Log("Attempting Redo() but no actions to redo!");
    }

    public void Undo()
    {
        if (index - 1 >= 0)
        {
            actionStack[--index].UndoAction();
            Debug.Log("Undid " + actionStack[index].GetType());
        }
        else
            Debug.Log("Attempting Undo() but no actions to undo!");
    }

    public void ClearHistory()
    {
        actionStack.Clear();
        index = 0;
    }
}