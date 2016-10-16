using System.Collections.Generic;
using UnityEngine;

public class HistoryAction
{
    public virtual void PerformAction() { }
    public virtual void UndoAction() { }
}

public class HistoryManager : MonoBehaviour
{
    List<HistoryAction> actionStack;
    int index;

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
        index += 1;
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
}