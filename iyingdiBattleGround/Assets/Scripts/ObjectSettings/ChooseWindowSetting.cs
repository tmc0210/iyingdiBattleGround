using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using UnityEngine;

public class ChooseWindowSetting : MonoBehaviour
{

    [Autohook]
    public Transform Content;
    [Autohook]
    public Transform HideButton;
    [Autohook]
    public Transform ShowButton;


    [Autohook]
    public TapGesture Button;


    [Autohook]
    public InnerSetting inner1;
    [Autohook]
    public InnerSetting inner2;
    [Autohook]
    public InnerSetting inner3;
    [Autohook]
    public InnerSetting inner4;
    [Autohook]
    public InnerSetting inner5;

    private Action tapped = null;


    public Tween Show(Action<Tween, int> selected, params Card[] cardsO)
    {
        List<Card> cards = cardsO.Filter(card => card != null);
        //print("cards Count " + cards.Count);
        if (cards.Count == 3)
        {
            SetUp(inner1, cards[0], 0, selected);
            SetUp(inner2, cards[1], 1, selected);
            SetUp(inner3, cards[2], 2, selected);
        }
        else if (cards.Count == 2)
        {
            SetUp(inner4, cards[0], 0, selected);
            SetUp(inner5, cards[1], 1, selected);
        }
        else if (cards.Count == 1)
        {
            SetUp(inner2, cards[0], 0, selected);
        }
        else
        {
            throw new Exception("发现的卡牌数目不正确" + cards.Count);
        }

        tapped = CloseContentCallBack;

        return Open();
    }

    private void CloseContentCallBack()
    {
        tapped = OpenContentCallBack;
        Content.transform.DOLocalMove(new Vector3(-2f, -2f, 0), 0.2f).SetRelative();
        Content.transform.DOScale(Vector3.zero, 0.2f);
        HideButton.gameObject.SetActive(false);
        ShowButton.gameObject.SetActive(true);
    }
    private void OpenContentCallBack()
    {
        tapped = CloseContentCallBack;
        Content.transform.DOLocalMove(new Vector3(2f, 2f, 0), 0.2f).SetRelative();
        Content.transform.DOScale(Vector3.one, 0.2f);
        HideButton.gameObject.SetActive(true);
        ShowButton.gameObject.SetActive(false);
    }

    private void SetUp(InnerSetting inner, Card card, int no, Action<Tween, int> selected)
    {
        //print(inner.name);
        inner.gameObject.SetActive(true);
        inner.SetByCard(card);
        inner.EnableTap(true, () => {
            // 使用全局position
            selected.Invoke(DOTween.Sequence()
                .PrependCallback(() => {
                    CardSetting setting = BattleBoardSetting.instance.GetCard(card).setting;
                    BattleBoardSetting.instance.SetCard(setting, card);
                    setting.transform.position = inner.Card.transform.position;
                    setting.SetSortingOrder(100);
                })
                .Append(Close())
                , no);
        });
    }

    Tween Open()
    {
        gameObject.SetActive(true);
        //print("in open");
        transform.localScale = Vector3.zero;
        return DOTween.Sequence().Append(transform.DOScale(Vector3.one, 0.2f));
    }
    Tween Close()
    {
        gameObject.SetActive(false);
        inner1.gameObject.SetActive(false);
        inner2.gameObject.SetActive(false);
        inner3.gameObject.SetActive(false);
        inner4.gameObject.SetActive(false);
        inner5.gameObject.SetActive(false);
        //return DOTween.Sequence().Append(transform.DOScale(Vector3.zero, 0.05f)).AppendCallback(()=> {
        //});
        return null;
    }




    #region tap

    private void OnEnable()
    {
        Button.Tapped += Button_Tapped;
    }
    private void OnDisable()
    {
        Button.Tapped -= Button_Tapped;
    }

    private void Button_Tapped(object sender, EventArgs e)
    {
        tapped?.Invoke();
    }

    #endregion
}
