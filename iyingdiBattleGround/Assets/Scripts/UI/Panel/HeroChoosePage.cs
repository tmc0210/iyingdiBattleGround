using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroChoosePage : UIPanel
{
    public Transform content;
    public GameObject heroBarGO;

    public override void ShowPanel(UIData data)
    {
        HeroData heroData = data as HeroData;
        foreach (var card in heroData.unLockedHero)
        {
            Instantiate(heroBarGO, content).GetComponent<HeroBar>().SetHero(card, false);
        }
        foreach (var card in heroData.unLockedHero)
        {
            Instantiate(heroBarGO, content).GetComponent<HeroBar>().SetHero(card, false);
        }
    }

    public void ClickBack()
    {
        Hide();
        UIManager.Instance.ShowPanel<TitlePage>();
    }

    public void ClickConfirm()
    {
        //TODO:进入选牌
    }
}

public class HeroData : UIData
{
    public List<Card> unLockedHero;
    public List<Card> lockedHero;
}
