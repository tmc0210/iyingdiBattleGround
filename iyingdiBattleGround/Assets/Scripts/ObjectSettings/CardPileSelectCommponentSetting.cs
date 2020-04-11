
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures;
using UnityEngine;
using UnityEngine.Assertions;

public class CardPileSelectCommponentSetting : MonoBehaviour
{
    #region public var

    [Autohook]
    public CardSetting Card0;
    [Autohook]
    public CardSetting Card1;
    [Autohook]
    public CardSetting Card2;
    [Autohook]
    public SpriteRenderer Star;
    [Autohook]
    public SpriteRenderer Moon;
    [Autohook]
    public SpriteRenderer Glow;

    #endregion

    #region private var

    private TapGesture tapGesture;
    private Action tapped = null;

    #endregion


    public void SetByCardPile(List<Card> cards)
    {
        Assert.IsTrue(cards.Count == 3, "牌堆必须含3张牌");
        Card0.SetByCard(cards[0]);
        Card1.SetByCard(cards[1]);
        Card2.SetByCard(cards[2]);
    }


    public void ShowStarOrMoon(bool isStar)
    {
        if (isStar)
        {
            Star.gameObject.SetActive(true);
            Moon.gameObject.SetActive(false);
        }
        else
        {
            Star.gameObject.SetActive(false);
            Moon.gameObject.SetActive(true);
        }
    }


    public void EnableTap(bool isOk, Action callback = null)
    {
        if (isOk) tapped = callback;
        else tapped = null;
    }

    public void ShowGlow(bool isShow = true)
    {
        Glow.gameObject.SetActive(isShow);
    }



    #region  unity function
    private void OnEnable()
    {
        tapGesture = GetComponent<TapGesture>();
        tapGesture.Tapped -= TapGesture_Tapped;
        tapGesture.Tapped += TapGesture_Tapped;
    }


    private void TapGesture_Tapped(object sender, EventArgs e)
    {
        tapped?.Invoke();
    }

    #endregion
}


