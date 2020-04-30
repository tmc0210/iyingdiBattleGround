using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICard : MonoBehaviour
{
    //引用
    public Image img_Card;
    public Image img_Effect;
    public Image img_Race;
    public Text txt_Attack;
    public Text txt_Health;
    public Image img_Bottom;
    public UILevel uiLevel;

    public Image[] img_Reborns;

    //资源
    public Sprite[] raceSprites; 
    public Sprite[] effectSprites;
    public Sprite[] bottomSprites;

    private readonly Dictionary<Keyword, int> effectIndexDict = new Dictionary<Keyword, int>()
    {
        {Keyword.Poisonous, 0 },
        {Keyword.Taunt, 1 },
        {Keyword.Windfury, 2 },
        {Keyword.DivineShield, 3 },
        {Keyword.Stealth,4 },
        {Keyword.Cleave,5 },
    };

    public void SetCard(Card card)
    {
        //TODO:img_Card.sprite
        //设置攻防
        txt_Attack.text = card.attack.ToString();
        txt_Health.text = card.health.ToString();
        //设置等级
        uiLevel.SetLevel(card.star);
        //设置种族
        if (card.type == MinionType.General)
        {
            img_Race.gameObject.SetActive(false);
        }
        else
        {
            img_Race.gameObject.SetActive(true);
            img_Race.sprite = raceSprites[(int)card.type - 1];
        }
        //设置关键词特效
        if (card.keyWords.Count <= 0)
        {
            img_Effect.gameObject.SetActive(false);
            img_Bottom.gameObject.SetActive(false);
        }
        foreach (var keyword in card.keyWords)
        {
            if(effectIndexDict.ContainsKey(keyword))
            {
                img_Effect.sprite = effectSprites[effectIndexDict[keyword]];
            }
            else if (keyword == Keyword.Reborn)
            {
                img_Reborns[0].gameObject.SetActive(true);
                img_Reborns[1].gameObject.SetActive(true);
            }
        }

    }

}
