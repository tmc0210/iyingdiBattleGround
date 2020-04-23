using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIPanel : MonoBehaviour
{
    public virtual void ShowPanel()
    {

    }

    /// <summary>
    /// 当显示页面需要获取数据时，请重写该方法
    /// </summary>
    /// <typeparam name="Data"></typeparam>
    /// <param name="data"></param>
    public virtual void ShowPanel(UIData data)
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

    protected void Delay(float seconds, Action action)
    {
        StartCoroutine(DelayCoroutine(seconds, action));
        
    }

    IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action();
    }
}

public class UIData
{

}
