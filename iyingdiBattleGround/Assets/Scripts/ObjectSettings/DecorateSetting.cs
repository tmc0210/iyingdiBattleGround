using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures;
using UnityEngine;
using UnityEngine.Rendering;

public class DecorateSetting : MonoBehaviour
{
    [Autohook]
    public Transform 冻结;
    [Autohook]
    public Transform 刷新;
    [Autohook]
    public Transform 升星;
    [Autohook]
    public Transform 铸币;
    [Autohook]
    public Transform 战斗;
    [Autohook]
    public Transform 英雄技能;



    [Autohook]
    public TextMeshPro FlushCost;
    [Autohook]
    public TextMeshPro FreezeCost;
    [Autohook]
    public TextMeshPro UpgrateCost;
    [Autohook]
    public TextMeshPro CoinNumber;
    [Autohook]
    public TextMeshPro HeroPowerCost;

    [Autohook]
    public SpriteRenderer 招揽随从Image;
    [Autohook]
    public SpriteRenderer 开始战斗Image;
    [Autohook]
    public SortingGroup ChangeBoard;
    [Autohook]
    public SpriteRenderer ChangeBoardMask;
    [Autohook]
    public SpriteRenderer 背面;
    [Autohook]
    public SpriteRenderer 正面;

    [Autohook]
    public SpriteRenderer Battle技能激活;
    [Autohook]
    public SpriteRenderer Freeze技能激活;
    [Autohook]
    public SpriteRenderer Flush技能激活;
    [Autohook]
    public SpriteRenderer Upgrate技能激活;
    [Autohook]
    public SpriteRenderer HeroPower技能激活;


    private Action UpgrateCostCallback;
    private Action FreezeCostCallback;
    private Action FlushCostCallback;
    private Action BattleCallback;
    private Action HeroPowerCallback;
    private Action HeroPowerLongPressCallback;



    private void OnEnable()
    {
        刷新.GetComponent<TapGesture>().Tapped += FlushCost_Tapped;
        冻结.GetComponent<TapGesture>().Tapped += FreezeCost_Tapped;
        升星.GetComponent<TapGesture>().Tapped += UpgrateCost_Tapped;
        战斗.GetComponent<TapGesture>().Tapped += Battle_Tapped;
        英雄技能.GetComponent<TapGesture>().Tapped += HeroPower_Tapped;
        英雄技能.GetComponent<LongPressGesture>().LongPressed += HeroPower_LongPressed;
    }

    private void HeroPower_LongPressed(object sender, EventArgs e)
    {
        HeroPowerLongPressCallback?.Invoke();
    }

    private void HeroPower_Tapped(object sender, EventArgs e)
    {
        HeroPowerCallback?.Invoke();
    }

    private void Battle_Tapped(object sender, EventArgs e)
    {
        BattleCallback?.Invoke();
    }

    private void OnDisable()
    {
        刷新.GetComponent<TapGesture>().Tapped -= FlushCost_Tapped;
        冻结.GetComponent<TapGesture>().Tapped -= FreezeCost_Tapped;
        升星.GetComponent<TapGesture>().Tapped -= UpgrateCost_Tapped;
        战斗.GetComponent<TapGesture>().Tapped -= Battle_Tapped;
        英雄技能.GetComponent<TapGesture>().Tapped -= HeroPower_Tapped;
        英雄技能.GetComponent<LongPressGesture>().LongPressed -= HeroPower_LongPressed;
    }

    private void UpgrateCost_Tapped(object sender, System.EventArgs e)
    {
        UpgrateCostCallback?.Invoke();
    }

    private void FreezeCost_Tapped(object sender, System.EventArgs e)
    {
        FreezeCostCallback?.Invoke();
    }
    private void FlushCost_Tapped(object sender, System.EventArgs e)
    {
        FlushCostCallback?.Invoke();
    }

    public bool battleLock = false;

    public void SetBattle(bool isOk, Action action = null)
    {
        if (battleLock)
        {
            isOk = false;
            action = null;
        }

        战斗.gameObject.SetActive(isOk);
        BattleCallback = action;
        if (action != null)
        {
            ShowGameObject(Battle技能激活.gameObject);
        }
        else
        {
            HideGameObject(Battle技能激活.gameObject);
        }
    }

   
    public void SetFlushCost(int n, Action callback = null)
    {
        刷新.gameObject.SetActive(n >= 0);
        if (n >= 0)
        {
            FlushCost.text = n.ToString();
        }
        FlushCostCallback = callback;

        if (callback != null && IsEnoughCoin(n))
        {
            ShowGameObject(Flush技能激活.gameObject);
        }
        else
        {
            HideGameObject(Flush技能激活.gameObject);
        }
        
    }
    public bool IsEnoughCoin(int coin)
    {
        if (BattleBoardSetting.instance.curChangeMessage != null)
        {
            if (BattleBoardSetting.instance.curChangeMessage.data != null)
            {
                return BattleBoardSetting.instance.curChangeMessage.data.leftCoins >= coin;
            }
        }
        return false;
    }

    private readonly Dictionary<GameObject, Tween> tweens = new Dictionary<GameObject, Tween>();
    public void ShowGameObject(GameObject go)
    {
        if (tweens.ContainsKey(go))
        {
            tweens[go].Kill();
        }
        tweens[go] = DOTween.Sequence().AppendInterval(0.3f).AppendCallback(() => {
            go.SetActive(true);
        });
    }
    public void HideGameObject(GameObject go)
    {
        if (tweens.ContainsKey(go))
        {
            tweens[go].Kill();
        }
        tweens[go] = DOTween.Sequence().AppendInterval(0.3f).AppendCallback(() => {
            go.SetActive(false);
        });
    }



    public void SetCoinNumber(int n)
    {
        铸币.gameObject.SetActive(n >= 0);
        if (n >= 0)
        {
            CoinNumber.text = "×" + n.ToString();
        }
    }


    public void SetFreezeCost(int n, Action callback = null)
    {
        冻结.gameObject.SetActive(n >= 0);
        if (n >= 0)
        {
            FreezeCost.text = n.ToString();
        }
        FreezeCostCallback = callback;

        if (callback != null)
        {
            ShowGameObject(Freeze技能激活.gameObject);
        }
        else
        {
            HideGameObject(Freeze技能激活.gameObject);
        }
    }
    public void SetUpgrateCost(int n, Action callback = null)
    {
        升星.gameObject.SetActive(n >= 0);
        if (n >= 0)
        {
            UpgrateCost.text = n.ToString();
        }
        UpgrateCostCallback = callback;
        if (callback != null && IsEnoughCoin(n))
        {
            ShowGameObject(Upgrate技能激活.gameObject);
        }
        else
        {
            HideGameObject(Upgrate技能激活.gameObject);
        }
    }

    public void SetHeroPower(int n, Action tapped = null, Action longPress = null)
    {
        英雄技能.gameObject.SetActive(true);
        HeroPowerCost.gameObject.SetActive(true);
        背面.gameObject.SetActive(false);
        正面.gameObject.SetActive(true);

        if (n >= 0)
        {
            英雄技能.gameObject.SetActive(true);
            HeroPowerCost.gameObject.SetActive(true);
            HeroPowerCost.text = n.ToString();
            HeroPowerCallback = tapped;
            HeroPowerLongPressCallback = longPress;
        }
        else if (n == -1) // 被动
        {
            英雄技能.gameObject.SetActive(true);
            HeroPowerCost.gameObject.SetActive(false);
            HeroPowerCallback = null;
            HeroPowerLongPressCallback = null;
        }
        else if (n == -2) // 用过
        {
            英雄技能.gameObject.SetActive(true);
            背面.gameObject.SetActive(true);
            正面.gameObject.SetActive(false);
            HeroPowerCost.gameObject.SetActive(false);
            HeroPowerCallback = null;
            HeroPowerLongPressCallback = null;
        }
        else // 消失
        {
            英雄技能.gameObject.SetActive(false);
            HeroPowerCallback = null;
            HeroPowerLongPressCallback = null;
        }


        if (tapped != null && n>=0 && IsEnoughCoin(n))
        {
            ShowGameObject(HeroPower技能激活.gameObject);
        }
        else
        {
            HideGameObject(HeroPower技能激活.gameObject);
        }
    }

    public Tween ShowChangeBoard(bool isBattle)
    {
        var it = isBattle ? 开始战斗Image : 招揽随从Image;

        return DOTween.Sequence().
            AppendCallback(() =>
            {
                if (isBattle)
                {
                    开始战斗Image.gameObject.SetActive(true);
                    招揽随从Image.gameObject.SetActive(false);
                }
                else
                {
                    开始战斗Image.gameObject.SetActive(false);
                    招揽随从Image.gameObject.SetActive(true);
                }
                it.gameObject.SetActive(true);
                it.transform.localScale = Vector3.zero;
                ChangeBoardMask.gameObject.SetActive(true);
                ChangeBoardMask.color = new Color(0, 0, 0, 0);
            })
            .Append(it.transform.DOScale(Vector3.one, 0.2f))
            .Join(ChangeBoardMask.DOFade(0.5f, 0.2f))
            .AppendInterval(1f)
            .Append(it.transform.DOScale(Vector3.zero, 0.1f))
            .Join(ChangeBoardMask.DOFade(0f, 0.1f))
            .AppendCallback(() => {
                ChangeBoardMask.gameObject.SetActive(false);
            })
            ;
    }
}
