using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures;
using UnityEngine;

public class InnerSetting : MonoBehaviour
{
    [Autohook]
    public CardSetting Card;

    [Autohook]
    public TextMeshPro CardNameText;
    [Autohook]
    public TextMeshPro CardDescriptionText;


    public void SetByCard(Card card)
    {
        Card.SetByCard(card);
        //CardNameText.text = card.name + (card.isGold?"(金色)": "") + "******".Substring(0, card.star);
        //string keywordstr = card.GetAllKeywords()
        //    .Map(BIF.BIFStaticTool.GetEnumDescriptionSaved)
        //    .Map(str=>"<b>"+str+"</b>")
        //    .Join();
        //if (!string.IsNullOrEmpty(keywordstr)) keywordstr += "\n";
        //CardDescriptionText.text = keywordstr +  CardBuilder.GetCardDescription(card);
        //print(CardDescriptionText.text);
        CardNameText.text = CardBuilder.GetCardNameContainsStar(card);
        CardDescriptionText.text = CardBuilder.GetCardDescriptionContainsKeyword(card);
    }

    #region tap
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
    private void TapGesture_Tapped(object sender, System.EventArgs e)
    {
        tapped?.Invoke();
    }

   

    public void EnableTap(bool isOk, Action tapped)
    {
        if (isOk)
        {
            this.tapped = tapped;
        }
        else
        {
            this.tapped = null;
        }
    }

    #endregion tap


}
