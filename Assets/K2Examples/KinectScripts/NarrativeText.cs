using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NarrativeText : MonoBehaviour
{
    private static string text;
    private static string delayedText;
    private static bool stay;

    private static TextMeshProUGUI infoText;
    private static TextMeshProUGUI danceMoveInfoText;
    private static Image infoImage;

    private static float timeLeftInfoText;
    private static float timeLeftDanceInfoText;
    private static float delay;

    // voice recognition
    private WindowsVoice windowsVoice;

    // singleton instance
    public static NarrativeText instance = null;

    /// <summary>
    /// Gets the single NarrativeText instance.
    /// </summary>
    /// <value>The NarrativeText instance.</value>
    public static NarrativeText Instance
    {
        get
        {
            return instance;
        }
    }

    // Use this for initialization
    public void Awake() 
    {
        stay = false;

        windowsVoice = GameObject.Find("KinectController").GetComponent<WindowsVoice>();
        infoText = GameObject.Find("Canvas/Tip/InfoText").GetComponent<TextMeshProUGUI>();
        danceMoveInfoText = GameObject.Find("Canvas/DanceMoveInfo").GetComponent<TextMeshProUGUI>();
        infoImage = GameObject.Find("Canvas/Tip").GetComponent<Image>();

        infoText.gameObject.SetActive(false);
        danceMoveInfoText.gameObject.SetActive(false);
        infoImage.gameObject.SetActive(false);

        instance = this;
    }
	
	// Update is called once per frame
	void Update () 
    {

        if (!stay)
        {
            windowsVoice.speak(text);
            stay = true;
        }

        if (timeLeftInfoText > 0)
        {
            timeLeftInfoText -= Time.deltaTime;
            
            if(timeLeftInfoText <= 0)
            {
                hideInfoText();
                timeLeftInfoText = 0;
            }
        }

        if (timeLeftDanceInfoText > 0)
        {
            timeLeftDanceInfoText -= Time.deltaTime;

            if (timeLeftDanceInfoText <= 0)
            {
                hideDanceMoveInfoText();
                timeLeftDanceInfoText = 0;
            }
        }

        if (delay > 0)
        {
            delay -= Time.deltaTime;

            if (delay <= 0)
            {
                stay = false;
                text = delayedText;
                delay = 0;
            }
        }
    }

    //voce intarziat
    public static void setTextDelayed(string input, float countdown)
    {
        delayedText = input;
        delay = countdown;
    }
    
    //voce imediat
    public static void setText(string input)
    {
        stay = false;
        text = input;
    }

    //text hint jos
    public static void showInfoText(string input, float countdown)
    {
        infoText.SetText(input);
        infoText.gameObject.SetActive(true);
        infoImage.gameObject.SetActive(true);

        timeLeftInfoText = countdown;
    }

    public static void hideInfoText()
    {
        infoText.SetText("");
        infoText.gameObject.SetActive(false);
        infoImage.gameObject.SetActive(false);
    }

    //text hint sus
    public static void showDanceMoveInfoText(string input, float countdown)
    {
        danceMoveInfoText.SetText(input);
        danceMoveInfoText.gameObject.SetActive(true);

        timeLeftDanceInfoText = countdown;
    }

    public static void hideDanceMoveInfoText()
    {
        danceMoveInfoText.SetText("");
        danceMoveInfoText.gameObject.SetActive(false);
    }
}
