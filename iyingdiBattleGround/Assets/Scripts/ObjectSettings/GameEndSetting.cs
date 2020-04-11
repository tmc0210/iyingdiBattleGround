using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndSetting : MonoBehaviour
{
    [Autohook]
    public SpriteRenderer 胜利;
    [Autohook]
    public SpriteRenderer 失败;

    bool isWin = true;

    public void Win()
    {
        isWin = true;
        gameObject.SetActive(true);
        Show(胜利.gameObject);
    }

    public void Lose()
    {
        isWin = false;
        gameObject.SetActive(true);
        Show(失败.gameObject);
    }

    public void Show(GameObject go)
    {
        go.gameObject.SetActive(true);
        go.transform.localScale = Vector3.zero;
        go.transform.DOScale(Vector3.one, 0.1f);
    }
    public void Hide(GameObject go, Action callback = null)
    {
        go.transform.DOScale(Vector3.zero, 0.1f).OnComplete(()=> {
            callback?.Invoke();
        });
    }

    public void Close()
    {
        if (isWin)
        {
            Hide(胜利.gameObject, () => {
                GameAnimationSetting.instance.gameEndAction?.Invoke(true);
            });
        }
        else
        {
            Hide(失败.gameObject, () => {
                GameAnimationSetting.instance.gameEndAction?.Invoke(false);
            });
        }
    }
}
