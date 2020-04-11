using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectBoardTitleSetting : MonoBehaviour
{
    [Autohook]
    public SpriteRenderer Text选择英雄;
    [Autohook]
    public SpriteRenderer Text选择牌堆;
    [Autohook]
    public SpriteRenderer Text选择宝藏;
    [Autohook]
    public SpriteRenderer Text收藏馆;
    [Autohook]
    public SpriteRenderer Text战后总结;

    public void HideAll()
    {
        Text选择英雄.gameObject.SetActive(false);
        Text选择宝藏.gameObject.SetActive(false);
        Text选择牌堆.gameObject.SetActive(false);
        Text收藏馆.gameObject.SetActive(false);
        Text战后总结.gameObject.SetActive(false);
    }

    public void SetSelectHero()
    {
        Show();
        HideAll();
        Text选择英雄.gameObject.SetActive(true);
    }
    public void SetSelectPile()
    {
        Show();
        HideAll();
        Text选择牌堆.gameObject.SetActive(true);
    }
    public void SetSelectTreasure()
    {
        Show();
        HideAll();
        Text选择宝藏.gameObject.SetActive(true);
    }
    public void SetShowCollection()
    {
        Show();
        HideAll();
        Text收藏馆.gameObject.SetActive(true);
    }
    public void SetSummary()
    {
        Show();
        HideAll();
        Text战后总结.gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }

}
