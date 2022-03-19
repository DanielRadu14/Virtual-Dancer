using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Toggle MaleToggle;
    public Toggle FemaleToggle;

    public void PlayGame()
    {
        if(MaleToggle.isOn && !FemaleToggle.isOn)
        {
            SceneManager.LoadScene(1);
        }
        else if(!MaleToggle.isOn && FemaleToggle.isOn)
        {
            //SceneManager.LoadScene(2);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
