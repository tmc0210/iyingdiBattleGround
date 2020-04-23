using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroBar : MonoBehaviour
{
    public HeroAvatar heroAvatar;
    public Text txt_Name;
    public Text txt_SkillType;
    public Text txt_SkillInfo;
    public Text txt_CardInfo;
    public GameObject img_Lock;

    public void SetHero(Card card, bool isLock)
    {
        //TODO:heroAvatar
        txt_Name.text = card.name;
        string costText = card.cost >= 0 ? card.cost + "费" : "被动";
        costText = "(" + costText + ")";
        if (isLock)
        {
            txt_SkillType.text = "英雄技能" + costText;
            txt_SkillInfo.text = "锁定中";
            txt_CardInfo.text = card.unlockDescription;
            img_Lock.SetActive(true);
        }
        else
        {
            txt_SkillType.text = "英雄技能" + costText;
            txt_SkillInfo.text = CardBuilder.GetCardDescription(card);
            txt_CardInfo.text = card.description;
            img_Lock.SetActive(false);
        }
    }
}
