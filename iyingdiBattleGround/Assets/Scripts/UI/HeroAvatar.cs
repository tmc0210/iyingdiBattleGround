using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroAvatar : MonoBehaviour
{
    public Image img_Avatar;
    public Text txt_Hp;

    public void SetAvatar(Sprite sprite)
    {
        img_Avatar.sprite = sprite;
    }

    public void SetHp(int hp)
    {
        txt_Hp.text = hp.ToString();
    }

    /// <summary>
    /// 是否显示血量
    /// </summary>
    /// <param name="isShow"></param>
    public void SetHpActive(bool isShow)
    {
        txt_Hp.transform.parent.gameObject.SetActive(isShow);
    }
}
