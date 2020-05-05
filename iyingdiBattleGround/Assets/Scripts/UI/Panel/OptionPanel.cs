using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionPanel : UIPanel
{
    private bool isMusicOn;
    private bool isMusicOff;
    
    public void ResolutionChanged(int index)
    {

    }

    public void ClickMusicButton()
    {

    }

    public void ClickSoundButton()
    {

    }

    public void AdjustMusicValue(float value)
    {

    }

    public void AdjustSoundValue(float value)
    {

    }

    public void ClickConfirm()
    {
        Hide();
    }

    public void ClickBackToMain()
    {

        UIManager.Instance.ShowPanel<ConfirmPanel, ConfirmPanel.ConfirmUIData>
            (new ConfirmPanel.ConfirmUIData("确定要返回主界面？", () =>
            {
                //TODO:退出游戏的方法
                UIManager.Instance.HideAll();
                UIManager.Instance.ShowPanel<MainPanel>();
                UIManager.Instance.ShowPanel<TitlePage>();
                UIManager.Instance.ShowPanel<ButtonPanel>();
            }));
    }

    public void ClickExitGame()
    {
        UIManager.Instance.ShowPanel<ConfirmPanel, ConfirmPanel.ConfirmUIData>
            (new ConfirmPanel.ConfirmUIData("确定要退出游戏？", () => Application.Quit()));
    }


}
