using BIF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;




public abstract class Effect
{
    public readonly bool isOnce = false;            // 一次性效果
}



/// <summary>
/// 修改身材的特效
/// </summary>
public class BodyPlusEffect:Effect
{
    public int attack = 0;
    public int health = 0;

    public BodyPlusEffect(int attack, int health)
    {
        this.attack = attack;
        this.health = health;
    }
}

/// <summary>
/// 特殊的特效
/// </summary>
public class KeyWordEffect : Effect
{
    public readonly Keyword keyword;

    public KeyWordEffect(Keyword keyword)
    {
        this.keyword = keyword;
    }
}

/// <summary>
/// 特殊的特效
/// </summary>
public class SpecialEffect : Effect
{

}
public class ProxyEffect : SpecialEffect
{
    private int counter = 0;
    /// <summary>
    /// this与groupAs为一组，且以groupAs为主
    /// </summary>
    public ProxyEffect groupAs = null;
    public readonly ProxyEnum proxyEnum;
    public CardProxyDelegate cardProxyDelegate;

    public int Counter {
        set {
            if (groupAs == null || groupAs == this)
            {
                counter = value;
            }
            else
            {
                groupAs.Counter = value;
            }
        }
        get
        {
            if (groupAs == null || groupAs == this)
            {
                return counter;
            }
            else
            {
                return groupAs.Counter;
            }
        }
    }

    public ProxyEffect(ProxyEnum proxyEnum, CardProxyDelegate cardProxyDelegate)
    {
        this.proxyEnum = proxyEnum;
        this.cardProxyDelegate = cardProxyDelegate;
    }

    public bool Invoke(GameEvent gameEvent)
    {
        //gameEvent.thisEffect = this;
        if (cardProxyDelegate == null) return false;
        return cardProxyDelegate.Invoke(gameEvent);
    }

    public ProxyEffect Copy()
    {
        ProxyEffect effect = MemberwiseClone() as ProxyEffect;
        return effect;
    }
}


public class ProxyEffects: List<Card>
{
    private readonly ProxyEnum proxy;

    public ProxyEffects(ProxyEnum proxy)
    {
        this.proxy = proxy;
    }

    public bool Invoke(GameEvent gameEvent)
    {
        bool returnValue = false;
        foreach (var card in this)
        {
            returnValue = card.InvokeProxy(proxy, gameEvent);
        }
        return returnValue;
    }
}




/// <summary>
/// 英雄免疫
/// </summary>
public class HeroImmunityEffect : SpecialEffect
{

}

/// <summary>
/// 双倍战吼
/// </summary>
public class DouleBattlecryEffect: SpecialEffect
{

}
public class TripleBattlecryEffect: SpecialEffect
{

}
public class DouleDeathrattleEffect : SpecialEffect
{

}
public class TripleDeathrattleEffect : SpecialEffect
{

}



public delegate bool CardProxyDelegate(GameEvent gameEvent);


/// <summary>
/// 可以通过<see cref="BIFStaticTool.GetEnumNameAndDescriptionSaved{T}"/>来获取反射值
/// </summary>
public enum ProxyEnum {
    None,

    [CommonDescription("战吼")]
    Battlecry,

    [Description("亡语")]
    [CommonDescription("亡语")]
    Deathrattle,

    [Description("光环")]
    [CommonDescription("光环")]
    Aura,

    [Description("闪电")]
    [CommonDescription("随从召唤时")]
    WhenMinionSummon,

    [Description("闪电")]
    [CommonDescription("随从召唤后")]
    AfterMinionSummon,

    [Description("闪电")]
    [CommonDescription("随从打出时")]
    WhenMinionPlayed,

    [Description("闪电")]
    [CommonDescription("随从打出后")]
    AfterMinionPlayed,

    [Description("闪电")]
    [CommonDescription("随从死亡后")]
    AfterMinionDeath,

    [Description("闪电")]
    [CommonDescription("受伤后")]
    AfterHurt,

    [Description("闪电")]
    [CommonDescription("受伤前")]
    PreHurt,

    [Description("闪电")]
    [CommonDescription("英雄受伤后")]
    AfterHeroHurt,

    [Description("闪电")]
    [CommonDescription("其他随从受伤后")]
    AfterMinionHurt,

    [Description("闪电")]
    [CommonDescription("攻击时")]
    WhenAttack,

    [Description("闪电")]
    [CommonDescription("攻击后")]
    AfterAttack,

    [Description("闪电")]
    [CommonDescription("其他随从攻击时")]
    WhenMinionAttack,

    [Description("闪电")]
    [CommonDescription("其他随从攻击后")]
    AfterMinionAttack,

    [Description("闪电")]
    [CommonDescription("失去圣盾后")]
    AfterDivineShieldBroken,

    [Description("闪电")]
    [CommonDescription("其他随从失去圣盾后")]
    AfterMinionDivineShieldBroken,

    [Description("闪电")]
    [CommonDescription("其他随从复生后")]
    AfterMinionReborn,

    [Description("闪电")]
    [CommonDescription("消灭随从后")]
    AfterKillEnemy,

    [Description("闪电")]
    [CommonDescription("超杀")]
    OverKill,

    [Description("闪电")]
    [CommonDescription("其他随从消灭随从后")]
    AfterMinionKillEnemy,

    [Description("闪电")]
    [CommonDescription("售出后")]
    AfterSold,

    [Description("闪电")]
    [CommonDescription("其他随从售出后")]
    AfterMinionSold,

    [Description("闪电")]
    [CommonDescription("购买随从后")]
    AfterBoughtMinion,

    [Description("闪电")]
    [CommonDescription("回合开始时")]
    TurnStart,

    [Description("闪电")]
    [CommonDescription("回合结束时")]
    TurnEnd,

    [CommonDescription("回合结束时,如果该随从在手牌中")]
    TurnEndInHand,

    [CommonDescription("回合开始时,如果该随从在手牌中")]
    TurnStartInHand,

    [CommonDescription("随从打出后,如果该随从在手牌中")]
    AfterMinionPlayedInHand,

    [Description("闪电")]
    [CommonDescription("每当你升级酒馆后")]
    AfterUpgrade,

    [Description("闪电")]
    [CommonDescription("每当你刷新酒馆后")]
    AfterFlush,

    [Description("闪电")]
    [CommonDescription("游戏开始时")]
    GameStart,

    [Description("闪电")]
    [CommonDescription("对战开始时")]
    CombatStart,

    [CommonDescription("英雄技能")]
    HeroPower,

    [CommonDescription("法术效果")]
    SpellEffect,

    [CommonDescription("选取时立即触发")]
    Selected
}

public class CommonDescriptionAttribute : Attribute
{
    public readonly string Description;

    public CommonDescriptionAttribute(string description)
    {
        Description = description;
    }
}
