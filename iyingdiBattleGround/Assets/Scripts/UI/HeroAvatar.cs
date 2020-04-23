using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroAvatar : MonoBehaviour
{
    public Image img_Avatar;
    public Text txt_Hp;

    public HeroAvatar SetAvatar(Sprite sprite)
    {
        img_Avatar.sprite = sprite;
        return this;
    }

    public HeroAvatar SetHp(int hp)
    {
        txt_Hp.text = hp.ToString();
        return this;
    }
}
