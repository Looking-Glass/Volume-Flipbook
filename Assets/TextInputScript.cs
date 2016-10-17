using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextInputScript : MonoBehaviour
{
    public KeyCode keyToSelect;
    InputField inputField;

    void Start()
    {
        inputField = GetComponent<InputField>();
    }

    void Update()
    {
        if (Input.GetKeyDown(keyToSelect))
            if (inputField.isFocused)
                inputField.DeactivateInputField();
            else
                inputField.ActivateInputField();
    }
}
