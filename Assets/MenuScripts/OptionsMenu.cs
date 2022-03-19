using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Toggle MaleToggle;
    public Toggle FemaleToggle;
    private bool changed = false;

    void Update()
    {
        if (FemaleToggle.isOn && !changed)
        {
            MaleToggle.isOn = false;
            changed = true;
        }

        if (MaleToggle.isOn && changed)
        {
            FemaleToggle.isOn = false;
            changed = false;
        }
    }
}
