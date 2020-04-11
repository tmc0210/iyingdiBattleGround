using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures;
using UnityEngine;

public class HeroSelectCommponentSetting : MonoBehaviour
{
    #region public var
    [Autohook]
    public HeroSetting Hero;

    [Autohook]
    public TextMeshPro CardNameText;

    [Autohook]
    public TextMeshPro CardDescriptionText;

    [Autohook]
    public TextMeshPro SkillNameText;

    [Autohook]
    public TextMeshPro SkillDescriptionText;

    [Autohook]
    public SpriteRenderer LockChain;

    #endregion

    #region private var

    private TapGesture tapGesture;
    private Action tapped = null;
    private bool isLock = false;

    #endregion


    public void SetByCard(Card card)
    {
        Hero.SetByCard(card);
        CardNameText.text = card.name;
        string costText = card.cost >= 0 ? card.cost + "费" : "被动";
        costText = "(" + costText + ")";
        if (isLock)
        {
            SkillNameText.text = "英雄技能" + costText;
            SkillDescriptionText.text = "锁定中";
            CardDescriptionText.text = card.unlockDescription;
        }
        else
        {
            SkillNameText.text = "英雄技能" + costText;
            SkillDescriptionText.text = CardBuilder.GetCardDescription(card);
            CardDescriptionText.text = card.description;
        }

    }

    public void EnableTap(bool isOk, Action callback=null)
    {
        if (isOk) tapped = callback;
        else tapped = null;
    }


    public void Lock(bool isLock = true)
    {
        this.isLock = isLock;
        LockChain.gameObject.SetActive(isLock);
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

    public void Glow(bool v)
    {
        Hero.ShowBorder(v);
    }
    #endregion
}


