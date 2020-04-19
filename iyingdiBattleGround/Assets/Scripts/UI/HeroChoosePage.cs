using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroChoosePage : UIPanel
{
    

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
