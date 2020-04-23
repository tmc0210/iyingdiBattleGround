using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionPage : UIPanel
{
    public override void ShowPanel()
    {
        //TODO:初始化要显示的卡
    }

    public void ClickBack()
    {
        Hide();
        UIManager.Instance.ShowPanel<TitlePage>();
    }

}
