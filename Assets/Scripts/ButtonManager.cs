using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ButtonProperties
{
    public Texture2D buttonTexture;
    public string buttonText;
    public AudioClip buttonAudioClip;
    public Color buttonColor = Color.white;
}

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager instance;

    [Header("arcade buttons")]
    public KeyCode[] arcadeButton =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Q, KeyCode.W, KeyCode.E,
        KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.Z, KeyCode.X, KeyCode.C,

        KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0,
        KeyCode.I, KeyCode.O, KeyCode.P,
        KeyCode.K, KeyCode.L, KeyCode.Semicolon,
        KeyCode.Comma, KeyCode.Period, KeyCode.Slash,
    };

    [Header("references to the button press visual")]
    public GameObject buttonGameObject;
    public RawImage buttonImage;
    public Text buttonText;
    public AudioSource buttonAudioSource;

    [Header("set of button properties")]
    public ButtonProperties[] buttonPropertiesSet;
    public bool playing;

    [Header("misc")]
    public float[] buttonPressSequence;
    public Vector3 defaultButtonPosition;
    public float buttonDownMovement;
    public static Action<int> buttonPressAction;

    [Header("held button visuals")]
    public GameObject resetText;
    public GameObject saveAndCloseText;
    public CircleMaker circleMaker;
    public float holdDownTimeForCircleMaker = 5f;

    [Header("Timeline")]
    public float timelineShowTime;

    Coroutine buttonPressCoroutine;
    Coroutine buttonHeldCoroutine;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        defaultButtonPosition = buttonGameObject.transform.position;
    }

    void Update()
    {
        for (int i = 0; i < arcadeButton.Length; i++)
        {

            if (Input.GetKeyDown(arcadeButton[i]))
            {

                //held button checks
                if (FlipbookManager.instance.flipbookState == FlipbookManager.FlipbookState.DRAWING)
                {
                    switch (i)
                    {
                        case 0:
                        case 1:
                            if (buttonHeldCoroutine != null)
                                StopCoroutine(buttonHeldCoroutine);
                            buttonHeldCoroutine = StartCoroutine(buttonPressHeld(i));
                            break;
                    }
                }

                //Regular checks
                switch (i)
                {
                    //play case is play or pause depending on state
                    case 7:
                        if (!FlipbookManager.instance.playing) //todo: replace with state.paused true or false
                        {
                            ButtonPress(i);
                        }
                        else
                        {
                            ButtonPress(i, 24);
                        }
                        break;
                    default:
                        ButtonPress(i);
                        break;
                }

            }
        }
    }

    /// <summary>
    /// visual replacement is used in some cases, ie. pausing uses a different graphic but sends the same button number
    /// </summary>
    void ButtonPress(int i, int visualReplacement = -99)
    {
        //set visual and audio properties of the button press visual
        int buttonPropertiesIndex = visualReplacement == -99 ? i : visualReplacement;
        SetButtonProperties(buttonPropertiesSet[buttonPropertiesIndex]);

        //send action event
        if (buttonPressAction != null)
        {
            buttonPressAction(i);
        }

        //play sound
        buttonAudioSource.Play();

        //start animation
        if (buttonPressCoroutine != null)
            StopCoroutine(buttonPressCoroutine);
        buttonPressCoroutine = StartCoroutine(buttonPressAnimation(i));
    }

    IEnumerator buttonPressAnimation(int i)
    {
        //start button press
        buttonGameObject.transform.position = defaultButtonPosition;
        buttonGameObject.SetActive(true);

        yield return new WaitForSeconds(buttonPressSequence[0]);

        //button down motion
        buttonGameObject.transform.position = defaultButtonPosition + Vector3.down * buttonDownMovement;
        yield return new WaitForSeconds(buttonPressSequence[1]);

        //button back up
        buttonGameObject.transform.position = defaultButtonPosition;
        yield return new WaitForSeconds(buttonPressSequence[2]);

        //button disapear
        buttonGameObject.SetActive(false);

        yield return new WaitForSeconds(buttonPressSequence[2]);
        FlipbookManager.instance.timelineParentTransform.gameObject.SetActive(false);
    }

    void SetButtonProperties(ButtonProperties buttonProperties)
    {
        buttonImage.texture = buttonProperties.buttonTexture;
        buttonText.text = buttonProperties.buttonText;
        buttonAudioSource.clip = buttonProperties.buttonAudioClip;
        buttonImage.color = buttonProperties.buttonColor;
    }

    IEnumerator buttonPressHeld(int i)
    {
        resetText.SetActive(false);
        saveAndCloseText.SetActive(false);
        circleMaker.circleLength = 0;
        circleMaker.gameObject.SetActive(true);
        if (i == 0)
        {
            resetText.SetActive(true);
        }
        if (i == 1)
        {
            saveAndCloseText.SetActive(true);
        }

        float timer = 0f;
        while (Input.GetKey(arcadeButton[i]) && timer < 1)
        {
            timer += Time.deltaTime * (1f / holdDownTimeForCircleMaker);
            circleMaker.circleLength = timer;

            if (timer >= 1)
            {
                //if it's a new flipbook
                if (i == 0)
                {
                    Destroy(FlipbookManager.instance.currentFlipbookObject.gameObject);
                    FlipbookManager.instance.MakeNewFlipbook();
                }
                if (i == 1)
                {
                    FlipbookManager.instance.SwitchToDemoMode();
                }

                resetText.SetActive(false);
                saveAndCloseText.SetActive(false);
            }
            yield return new WaitForEndOfFrame();
        }



        circleMaker.circleLength = 0;
        circleMaker.gameObject.SetActive(true);


        while (timer <= GetButtonSequenceTime() / holdDownTimeForCircleMaker)
        {
            timer += Time.deltaTime * (1 / holdDownTimeForCircleMaker);
            yield return new WaitForEndOfFrame();
        }
        resetText.SetActive(false);
        saveAndCloseText.SetActive(false);
    }

    float GetButtonSequenceTime()
    {
        float total = 0f;
        for (int i = 0; i < buttonPressSequence.Length; i++)
        {
            total += buttonPressSequence[i];
        }
        return total;
    }
}
