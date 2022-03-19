using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    protected static PauseMenu instance = null;
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;

    private float timeLeft = 10.0f;
    private bool start_countdown = false;

    protected KinectManager kinectManager;

    public static PauseMenu Instance
    {
        get
        {
            return instance;
        }
    }

    void Update()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            KinectManager.viewButton.SetActive(true);
            KinectManager.speedSlider.SetActive(true);
            KinectManager.speedText.SetActive(true);
        }

        //pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        //start game
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (KinectManager.gameModeStat == KinectManager.GameMode.View)
            {
                KinectManager.gameModeStat = KinectManager.GameMode.None;
            }
            else
            {
                KinectListener.StartGame();
            }
        }

        //start recording new dance move
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (KinectManager.gameModeStat == KinectManager.GameMode.None)
            {
                start_countdown = true;
                Debug.Log("Recording will start in 10 seconds...");

                if (kinectManager == null)
                {
                    kinectManager = KinectManager.Instance;
                }

                int noPlaybackFiles = Directory.GetFiles(AvatarController.playback_directory).Length;
                AvatarController.record_file = AvatarController.playback_directory + "Record" + noPlaybackFiles + ".txt";
                File.Create(AvatarController.record_file);

                //first record file created to be set for playback
                if (noPlaybackFiles == 0)
                {
                    AvatarController.playbackFileNo = 0;
                    AvatarController.playback_file = AvatarController.record_file;
                }
            }
            else if (KinectManager.gameModeStat == KinectManager.GameMode.Record)
            {
                StopRecording();
            }
        }

        if (start_countdown && timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            KinectListener.startText.SetText((timeLeft).ToString("0"));

            if(KinectListener.startText.text == "0")
            {
                KinectListener.startText.SetText("");
                NarrativeText.showDanceMoveInfoText("Started recording!", 1.0f);
                Debug.Log("Started recording!");
                start_countdown = false;

                AvatarController.condition_for_creating_danceMoveFile = true;
                AvatarController.condition_for_writing_danceMoveFile = true;

                //write skeleton data
                KinectManager.writeSkeletonFile = true;
                int noPlaybackFiles = Directory.GetFiles(AvatarController.skeletons_directory).Length;
                string skeleton_file = AvatarController.skeletons_directory + "Record" + noPlaybackFiles + ".txt";
                KinectManager.skeletonStreamWriter = new StreamWriter(skeleton_file, true);

                KinectManager.gameModeStat = KinectManager.GameMode.Record;
            }
        }
    }

    public void StopRecording()
    {
        timeLeft = 10.0f;
        NarrativeText.showDanceMoveInfoText("Recording stopped!", 1.0f);
        Debug.Log("Recording stopped!");

        if (AvatarController.streamWriter != null)
        {
            AvatarController.streamWriter.Close();
        }

        KinectManager.viewButton.SetActive(true);
        KinectManager.speedSlider.SetActive(true);
        KinectManager.speedText.SetActive(true);
        AvatarController.condition_for_writing_danceMoveFile = false;
        KinectManager.gameModeStat = KinectManager.GameMode.None;

        //stop writing skeleton data
        KinectManager.writeSkeletonFile = false;
        if (KinectManager.skeletonStreamWriter != null)
        {
            KinectManager.skeletonStreamWriter.Close();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        UnityEngine.Application.Quit();
    }
}
