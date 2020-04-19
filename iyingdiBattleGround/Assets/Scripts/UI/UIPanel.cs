using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIPanel : MonoBehaviour
{
    public virtual void ShowPanel()
    {

    }

    public virtual void HidePanel()
    {

    }

    public void Hide()
    {
        HidePanel();
        gameObject.SetActive(false);
    }

    public void Delay(float seconds, Action action)
    {
        StartCoroutine(DelayCoroutine(seconds, action));
        
    }

    IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action();
    }

}
