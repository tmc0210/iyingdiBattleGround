using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionPage : UIPanel
{
    private bool isCardPage = true;

    public Transform content;

    public GameObject uiCardPrefab;
    private List<GameObject> uiCardList;

    public override void ShowPanel()
    {
        ShowCardPage();
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void ClickBack()
    {
        Hide();
        UIManager.Instance.ShowPanel<TitlePage>();
    }

    /// <summary>
    /// 显示卡牌页面
    /// </summary>
    public void ShowCardPage()
    {
        if (isCardPage)
        {
            return;
        }
        isCardPage = true;
        foreach (var go in uiCardList)
        {
            UIFactory.Instance.PushItem("UICard", go);
        }
        List<Card> cards = GameController.GetAllCard();
        foreach (var card in cards)
        {
            var go = UIFactory.Instance.GetItem("UICard", uiCardPrefab);
            go.GetComponent<UICard>().SetCard(card);
            uiCardList.Add(go);
        }
    }

    /// <summary>
    /// 显示宝藏页面
    /// </summary>
    public void ShowTreasurePage()
    {
        if (!isCardPage)
        {
            return;
        }
        isCardPage = false;
        foreach (var go in uiCardList)
        {
            UIFactory.Instance.PushItem("UICard", go);
        }
        List<Card> cards = GameController.GetAllTreasureCard();
        foreach (var card in cards)
        {
            var go = UIFactory.Instance.GetItem("UICard", uiCardPrefab);
            go.GetComponent<UICard>().SetTreasure(card);
            uiCardList.Add(go);
        }
    }

}
