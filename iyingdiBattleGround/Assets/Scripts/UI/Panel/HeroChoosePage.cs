using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroChoosePage : UIPanel
{
    public override void ShowPanel<Data>(Data data)
    {
        
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

public class HeroData
{
    public List<Card> unLockedHero;
    public List<Card> lockedHero;
}
