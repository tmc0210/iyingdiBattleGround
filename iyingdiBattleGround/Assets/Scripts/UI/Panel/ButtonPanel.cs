using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPanel : UIPanel
{
    /// <summary>
    /// 点击设置按钮
    /// </summary>
    public void ClickOption()
    {
        if (UIManager.Instance.GetPanel<OptionPanel>().gameObject.activeInHierarchy)
        {
            UIManager.Instance.HidePanel<OptionPanel>();
        }
        else
        {
            UIManager.Instance.ShowPanel<OptionPanel>();
        }
    }

    /// <summary>
    /// 点击退出按钮
    /// </summary>
    public void ClickExit()
    {
        UIManager.Instance.ShowPanel<ConfirmPanel, ConfirmPanel.ConfirmUIData>
            (new ConfirmPanel.ConfirmUIData("确定要退出游戏？", () => Application.Quit()));
    }

}
