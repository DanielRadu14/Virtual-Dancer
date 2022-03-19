using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using System.Linq;
using System.IO;
using TMPro;
using RefFiles;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class KinectListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    // The singleton instance of KinectListener
    protected static KinectListener instance = null;

    private KinectManager kinectManager;
    public static TextMeshProUGUI startText;
    private static bool gameStarted = false;
    private static bool sayBackTextInfo = true;
    public AudioSource audioSource;

    // voice recognition
    private WindowsVoice windowsVoice;
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    public static KinectListener Instance
    {
        get
        {
            return instance;
        }
    }

    public void UserDetected(long userId, int userIndex)
    {
        if (userIndex != playerIndex)
        {
            return;
        }

        // detect gestures
        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;
        }
        // 1
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);
        // 2
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
        // 3
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.RaiseBothHands);
        // 4
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.Push);
        // 5
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.LeanLeft);
        // 6
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.LeanRight);
        // 7
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.Run);
        // 8 
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.ShoulderLeftFront);
        // 9 
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.ShoulderRightFront);
        // 10
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.Tpose);
        // 11
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.KickLeft);
        // 12
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.KickRight);
        // 13
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.Jump);
        // 14
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);
        // 15
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);
        // 16
        kinectManager.DetectGesture(userId, KinectGestures.Gestures.Wave);
    }

    public void UserLost(long userId, int userIndex)
    {
        if (userIndex != playerIndex)
        {
            return;
        }
    }

    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (userIndex != playerIndex || KinectManager.gestureDetected)
        {
            return;
        }
    }

    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (KinectManager.gestureDetected)
        {
            return false;
        }

        if (userIndex != playerIndex)
        {
            return false;
        }

        if(KinectManager.gameModeStat == KinectManager.GameMode.Record || KinectManager.gameModeStat == KinectManager.GameMode.Play)
        {
            return false;
        }

        bool cond_start_program =
            (KinectManager.gameModeStat == KinectManager.GameMode.None) && gesture == KinectGestures.Gestures.RaiseRightHand && gameStarted;
        if (cond_start_program)
        {
            GameObject.Find("Canvas/RaiseHandImage").GetComponent<Image>().color = Color.green;
            GameObject.Find("Canvas/RaiseHandImage/Text").GetComponent<Text>().color = Color.black;
            KinectManager.gestureDetected = true;

            //delete all previous user records
            DTW.deleteFiles();

            StartCoroutine(GetReady());
        }

        bool cond_next_dance_move = gesture == KinectGestures.Gestures.SwipeLeft && gameStarted && !AvatarController.danceUnlocked &&
            (KinectManager.gameModeStat == KinectManager.GameMode.View || KinectManager.gameModeStat == KinectManager.GameMode.None);
        if(cond_next_dance_move)
        {
            NextMove();
        }

        bool cond_prev_dance_move = gesture == KinectGestures.Gestures.SwipeRight && gameStarted && !AvatarController.danceUnlocked &&
            (KinectManager.gameModeStat == KinectManager.GameMode.View || KinectManager.gameModeStat == KinectManager.GameMode.None);
        if (cond_prev_dance_move)
        {
            PreviousMove();
        }

        bool cond_start =
            (KinectManager.gameModeStat == KinectManager.GameMode.None) && gesture == KinectGestures.Gestures.Wave && !gameStarted;
        if (cond_start)
        {
            StartGame();
        }

        bool cond_feedback = gesture == KinectGestures.Gestures.RaiseLeftHand && AvatarController.allowFeedback;
        if (cond_feedback)
        {
            if (KinectManager.gameModeStat == KinectManager.GameMode.None)
            {
                ShowFeedback();
            }
            else if (KinectManager.gameModeStat == KinectManager.GameMode.Feedback)
            {
                HideFeedback();
            }
        }

        return true;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint)
    {
        if (userIndex != playerIndex || KinectManager.gestureDetected)
        {
            return false;
        }

        return true; 
    }

    public void Awake()
    {
        audioSource = GameObject.Find("KinectController/Song").GetComponent<AudioSource>();

        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;
        }

        windowsVoice = GameObject.Find("KinectController").GetComponent<WindowsVoice>();
        startText = GameObject.Find("Canvas/RecordingModeText").GetComponent<TextMeshProUGUI>();

        instance = this;

        keywords.Add("dance", () =>
        {
            Debug.Log("Start game!");
            StartGame();
        });

        keywords.Add("stop", () =>
        {
            Debug.Log("Stop game");
            StopGame();
        });

        keywords.Add("next", () =>
        {
            Debug.Log("Next move!");
            NextMove();
        });

        keywords.Add("back", () =>
        {
            Debug.Log("Previous move!");
            PreviousMove();
        });

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizerForPhrase;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizerForPhrase(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;

        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    public static void StartGame()
    {
        if (KinectManager.gameModeStat == KinectManager.GameMode.None)
        {
            gameStarted = true;

            KinectManager.gameModeStat = KinectManager.GameMode.View;
            AvatarController.view_file = AvatarController.playback_file;
            AvatarController.noReps = 0;
            AvatarController.raiseHandVoiceInfo = true;

            KinectManager.viewButton.SetActive(false);
            KinectManager.speedSlider.SetActive(false);
            KinectManager.speedText.SetActive(false);

            string refDanceMoveInfo = getDanceMoveInfo()["FinalDance.txt"];
            NarrativeText.setText(refDanceMoveInfo);
            NarrativeText.showDanceMoveInfoText(refDanceMoveInfo, 3.0f);
        }
    }

    IEnumerator GetReady()
    {
        if (!AvatarController.danceUnlocked)
        {
            KinectManager.progressCircle.SetActive(true);
            KinectManager.progressUI.SetFillerSizeAsPercentage(0);
        }
        else
        {
            KinectManager.progressCircle.SetActive(false);
        }

        //only starts after raising the right hand
        windowsVoice.speak("Get ready!");
        startText.SetText("Get ready!");
        yield return new WaitForSeconds(1.6f);

        windowsVoice.speak("3");
        startText.SetText("3");
        yield return new WaitForSeconds(1.6f);

        windowsVoice.speak("2");
        startText.SetText("2");
        yield return new WaitForSeconds(1.6f);

        windowsVoice.speak("1");
        startText.SetText("1");
        yield return new WaitForSeconds(1.6f);

        if (!AvatarController.danceUnlocked)
        {
            AvatarController.condition_for_writing_userFile = true;
            AvatarController.condition_for_creating_userFile = true;
        }
        else
        {
            //start song
            audioSource.Play();

            //load the final dance
            AvatarController.frames = null;
            AvatarController.framesCount = 0;

            AvatarController.playback_file = AvatarController.playback_directory + "FinalDance.txt";
            AvatarController.frames = AvatarController.loadFrames(AvatarController.playback_file, ref AvatarController.framesCount);

            AvatarController.condition_for_writing_userFile = false;
            AvatarController.condition_for_creating_userFile = false;
        }

        AvatarController.framesCount = 0;
        AvatarController.frames = AvatarController.loadFrames(AvatarController.playback_file, ref AvatarController.framesCount);
        KinectManager.gameModeStat = KinectManager.GameMode.Play;

        windowsVoice.speak("Let's dance");
        startText.SetText("Let's dance");
        yield return new WaitForSeconds(1.6f);

        startText.SetText("");
    }

    public void StopGame()
    {
        if (KinectManager.gameModeStat == KinectManager.GameMode.None)
        {
            return;
        }

        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;
        }

        AvatarController.condition_for_writing_userFile = false;
        AvatarController.condition_for_creating_userFile = false;
    }

    public void NextMove()
    {
        int noPlaybackFiles = Directory.GetFiles(AvatarController.playback_directory).Length;

        if (AvatarController.playbackFileNo < noPlaybackFiles - 1)
        {
            AvatarController.playbackFileNo++;
        }
        else
        {
            AvatarController.playbackFileNo = 0;
        }

        AvatarController.currentFrame = 0;
        AvatarController.playback_file = AvatarController.playback_directory + "Record" + AvatarController.playbackFileNo + ".txt";

        AvatarController.framesCount = 0;
        AvatarController.frames = AvatarController.loadFrames(AvatarController.playback_file, ref AvatarController.framesCount);

        //display dance info
        string refDanceMoveInfo;
        try
        {
            refDanceMoveInfo = getDanceMoveInfo()["Record" + AvatarController.playbackFileNo + ".txt"];
        }
        catch (KeyNotFoundException e)
        {
            //default info
            refDanceMoveInfo = "";
        }
        NarrativeText.showDanceMoveInfoText(refDanceMoveInfo, 3.0f);

        KinectManager.gameModeStat = KinectManager.GameMode.View;
        AvatarController.view_file = AvatarController.playback_file;
        AvatarController.noReps = 0;
        AvatarController.raiseHandVoiceInfo = false;

        if (sayBackTextInfo)
        {
            NarrativeText.showInfoText("Say back or swipe right to see the previous dance move", 5.0f);
            sayBackTextInfo = !sayBackTextInfo;
        }

        //reset score
        KinectManager.progressUI.SetFillerSizeAsPercentage(0);
    }

    public void PreviousMove()
    {
        int noPlaybackFiles = Directory.GetFiles(AvatarController.playback_directory).Length;

        if (AvatarController.playbackFileNo > 0)
        {
            AvatarController.playbackFileNo--;
        }
        else
        {
            AvatarController.playbackFileNo = noPlaybackFiles - 1;
        }

        AvatarController.currentFrame = 0;
        AvatarController.playback_file = AvatarController.playback_directory + "Record" + AvatarController.playbackFileNo + ".txt";

        AvatarController.framesCount = 0;
        AvatarController.frames = AvatarController.loadFrames(AvatarController.playback_file, ref AvatarController.framesCount);

        //display dance info
        string refDanceMoveInfo;
        try
        {
            refDanceMoveInfo = getDanceMoveInfo()["Record" + AvatarController.playbackFileNo + ".txt"];
        }
        catch (KeyNotFoundException e)
        {
            //default info
            refDanceMoveInfo = "";
        }
        NarrativeText.showDanceMoveInfoText(refDanceMoveInfo, 3.0f);

        KinectManager.gameModeStat = KinectManager.GameMode.View;
        AvatarController.view_file = AvatarController.playback_file;
        AvatarController.noReps = 0;
        AvatarController.raiseHandVoiceInfo = false;

        //reset score
        KinectManager.progressUI.SetFillerSizeAsPercentage(0);
    }

    public void ShowFeedback()
    {
        string[] comp = AvatarController.playback_file.Split(Path.DirectorySeparatorChar);
        string skeletonFile = comp[comp.Length - 1];
        AvatarController.skeleton_file = AvatarController.skeletons_directory + skeletonFile;
        AvatarController.feedback_file = AvatarController.user_records_directory + "UserRecord0.txt";

        AvatarController.skel_framesCount = 0;
        AvatarController.skel_frames = AvatarController.skel_loadFrames(AvatarController.skeleton_file, ref AvatarController.skel_framesCount);

        AvatarController.framesCount = 0;
        AvatarController.frames = AvatarController.loadFrames(AvatarController.feedback_file, ref AvatarController.framesCount);

        KinectManager.gameModeStat = KinectManager.GameMode.Feedback;
        KinectManager.readSkeletonFile = true;
        KinectManager.writeSkeletonFile = false;

        if (kinectManager == null)
        {
            kinectManager = KinectManager.Instance;
        }
        kinectManager.refreshAvatarControllers();
    }

    public void HideFeedback()
    {
        KinectManager.gameModeStat = KinectManager.GameMode.None;
        KinectManager.readSkeletonFile = false;
        KinectManager.writeSkeletonFile = false;

        if (KinectManager.skeletonStreamReader != null)
        {
            KinectManager.skeletonStreamReader.Close();
        }

        AvatarController.allowFeedback = false;

        //delete all user records
        DTW.deleteFiles();
    }

    public static Dictionary<string, string> danceMoveInfo = new Dictionary<string, string>();
    public static Dictionary<string, string> getDanceMoveInfo()
    {
        danceMoveInfo["FinalDance.txt"] = "Here is the dance you'll need to learn";
        danceMoveInfo["Record0.txt"] = "Starting with the basics";
        danceMoveInfo["Record1.txt"] = "Let me see your hands";
        danceMoveInfo["Record2.txt"] = "Now something more";
        danceMoveInfo["Record3.txt"] = "Keep your right hand up";
        danceMoveInfo["Record4.txt"] = "Now the left one";

        return danceMoveInfo;
    }

    public void Update()
    {
    }
}
