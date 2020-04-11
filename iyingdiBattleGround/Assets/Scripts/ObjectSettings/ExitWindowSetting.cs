using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitWindowSetting : MonoBehaviour
{
    [Autohook]
    public Transform Content;

    [Autohook]
    public SpriteButton Btn确定;
    [Autohook]
    public SpriteButton Btn取消;

    public void Show()
    {
        gameObject.SetActive(true);
        Content.transform.localScale = Vector3.zero;
        Content.transform.DOScale(Vector3.one, 0.05f).OnComplete(()=> {
            Btn确定.gameObject.SetActive(true);
            Btn取消.gameObject.SetActive(true);
        });
    }
    public void Hide()
    {
        Btn确定.gameObject.SetActive(false);
        Btn取消.gameObject.SetActive(false);
        Content.transform.DOScale(Vector3.zero, 0.05f).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

    public void OnCancel()
    {
        Hide();
    }
    public void OnConfirm()
    {
        Application.Quit();
    }
}
