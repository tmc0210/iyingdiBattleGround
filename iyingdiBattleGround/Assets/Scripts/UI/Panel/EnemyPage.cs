using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyPage : UIPanel
{
    public Text txt_Title;
    public HeroAvatar heroAvatar;
    public Text txt_Description;
    public Text txt_Skill;

    /// <summary>
    /// 需要敌人信息
    /// </summary>
    /// <param name="data"></param>
    public override void ShowPanel(UIData data)
    {
        base.ShowPanel(data);
    }

    public void SetEnemy(Player player, int level)
    {
        txt_Title.text = "挑战 " + level + "/4";
        //TODO:设置头像

    }




}
