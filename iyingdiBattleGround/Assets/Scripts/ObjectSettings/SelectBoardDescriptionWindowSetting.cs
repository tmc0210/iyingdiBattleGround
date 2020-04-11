using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectBoardDescriptionWindowSetting : MonoBehaviour
{
    [Autohook]
    public InnerSetting inner1;
    [Autohook]
    public InnerSetting inner2;
    [Autohook]
    public InnerSetting inner3;
    [Autohook]
    public Transform Content;
    [Autohook]
    public Transform Mask;
    [Autohook]
    public TextMeshPro TitleText;


    public Action closed = null;
    public Action confirm = null;

    private void Awake()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
    }

    public void Show(List<Card> cards, string name = "")
    {
        gameObject.SetActive(true);
        inner1.SetByCard(cards[0]);
        inner2.SetByCard(cards[1]);
        inner3.SetByCard(cards[2]);

        transform.DOScale(Vector3.one, 0.1f);

        TitleText.text = name;
    }

    public void OnCancel()
    {
        transform.DOScale(Vector3.zero, 0.1f).OnComplete(()=> {
            closed?.Invoke();
        });
    }
    public void OnConfirm()
    {
        transform.DOScale(Vector3.zero, 0.1f).OnComplete(() => {
            confirm?.Invoke();
        });
    }
}
