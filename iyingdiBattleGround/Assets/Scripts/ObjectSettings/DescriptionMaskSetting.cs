using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures;
using UnityEngine;

public class DescriptionMaskSetting : MonoBehaviour
{
    [Autohook]
    public TextMeshPro Text;

    private TapGesture tapGesture = null;
    private Action tapped = null;

    private void OnEnable()
    {
        if (tapGesture == null) tapGesture = GetComponent<TapGesture>();
        tapGesture.Tapped += TapGesture_Tapped;
    }
    private void OnDisable()
    {
        tapGesture.Tapped -= TapGesture_Tapped;
    }
    private void TapGesture_Tapped(object sender, EventArgs e)
    {
        tapped?.Invoke();
    }

    public void EnableTap(bool isOk, Action action = null)
    {
        if (isOk)
        {
            tapped = action;
        }
        else
        {
            tapped = null;
        }
    }
}
