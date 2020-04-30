using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICardPile : MonoBehaviour
{
    public UICard[] uiCards;
    public Image img_Title;
    public Image img_Selected;

    //资源
    public Sprite[] titleSprites;

    public void SetCardPile(Card[] cards)
    {
        for (int i = 0; i < uiCards.Length; i++)
        {
            if (i< cards.Length)
            {
                uiCards[i].SetCard(cards[i]);
            }
            else
            {
                Debug.LogError("输入的Card数组大小小于需要的大小");
                break;
            }
        }
    }

    public void SetTitle(TitleType type)
    {
        img_Title.sprite = titleSprites[(int)type];
    }

    public void Click()
    {
        img_Selected.color = new Color(0, 0, 0, 1);
    }

    public void CancelClick()
    {
        img_Selected.color = new Color(0, 0, 0, 0);
    }

    public enum TitleType
    {
        Sun,
        Moon,
    }
}
