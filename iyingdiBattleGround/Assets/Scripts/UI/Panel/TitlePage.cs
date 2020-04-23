using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitlePage : UIPanel
{

    public void ToGame()
    {
        Hide();
        UIManager.Instance.ShowPanel<HeroChoosePage>();
    }

    public void ToCollection()
    {
        Hide();
        UIManager.Instance.ShowPanel<CollectionPage>();
    }
}
