using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroPanel : UIPanel
{
    public Button btn_Confirm;

    public override void ShowPanel()
    {
        Delay(3, () => btn_Confirm.gameObject.SetActive(true));
    }

    public override void HidePanel()
    {
        gameObject.SetActive(false);
        UIManager.Instance.ShowPanel<MainPanel>();
    }
}
