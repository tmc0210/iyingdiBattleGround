using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures;
using UnityEngine;

public class TrasureSelectCommponentSetting : MonoBehaviour
{
    #region public var
    [Autohook]
    public CardSetting Card;

    [Autohook]
    public TextMeshPro CardNameText;

    [Autohook]
    public TextMeshPro CardDescriptionText;


    [Autohook]
    public SpriteRenderer Glows;
    #endregion

    #region private var

    private TapGesture tapGesture;
    private Action tapped = null;

    #endregion


    public void SetByCard(Card card)
    {
        Card.SetByCard(card);
        CardNameText.text = CardBuilder.GetCardNameContainsStar(card);
        CardDescriptionText.text = CardBuilder.GetCardDescriptionContainsKeyword(card);
    }

    public void EnableTap(bool isOk, Action callback = null)
    {
        if (isOk) tapped = callback;
        else tapped = null;
    }
    public void Glow(bool isGlow = true)
    {
        Glows.gameObject.SetActive(isGlow);
    }

    #region  unity function
    private void OnEnable()
    {
        tapGesture = GetComponent<TapGesture>();
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
    #endregion
}
