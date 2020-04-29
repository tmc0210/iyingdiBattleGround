using BIF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using System.Linq;
using OrderedJson.Core;

public class Card :ICloneable
{
    #region unique id
    // 每张卡都有唯一id
    public int uniqueId = 0;
    public static int curUniqueId = 0;
    private void Unique() { uniqueId = curUniqueId++; }
    #endregion

    #region public var

    public int id = -1;                                 // 卡牌id
    public string name = "";                            // 卡牌名称
    public string displayName = "";                     // 展示名称
    public string image = "";                           // 卡图url
    public int cost = 0;                                // 卡牌费用
    public bool isToken = false;
    public bool isGold = false;                         // 是否是金卡
    public bool isDead = false;                         // 是否死亡
    public string skillDescription = "";                // 卡牌能力描述（暂时不用）
    public string description = "";                     // 卡牌背景描述
    public string unlockDescription = "";               // 解锁条件描述
    public int[] counters = new int[3];                 // 计数器
    public bool Lock = true;                            // 锁定
    
    public Card creator = null;                         // 创建者
    
    /// <summary>
    /// 此卡对应的金色版本
    /// </summary>
    public int goldVersion = -2;
    /// <summary>
    /// 卡牌特效(用于属性值光环, 每次光环更新前清空对应光环特效)
    /// </summary>
    public List<Card> effects = new List<Card>();
    /// <summary>
    /// 永久特效
    /// </summary>
    public List<Card> effectsStay = new List<Card>();
    
    
    /// <summary>
    /// 打出时选择目标的类型 None表示不选择
    /// </summary>
    //public MinionType targetType = MinionType.None;
    
    public CardType cardType = CardType.Minion;
    
    public int star = 1;
    public int attack = 1;
    public int health = 1;
    public MinionType type = MinionType.General;
    public List<string> tag = new List<string>();
    public HashSet<Keyword> keyWords = new HashSet<Keyword>();
    public Map<ProxyEnum, IOJMethod> methods = new Map<ProxyEnum, IOJMethod>();
    public int Counter = 0;
    public IOJMethod initMethod;

    #endregion

    #region setter and getter

    public bool HasKeyword(Keyword keyword)
    {
        return effects.Concat(effectsStay).Append(this).Any(card => card.keyWords.Contains(keyword));
    }

    public void RemoveKeyWord(Keyword keyword)
    {
        effects.Concat(effectsStay).Append(this).Map(card => {
            if (card.keyWords.Contains(keyword)) {
                card.keyWords.Remove(keyword);
            }
        });
    }

    public HashSet<Keyword> GetAllKeywords()
    {
        HashSet<Keyword> ret = new HashSet<Keyword>();
        effects.Concat(effectsStay).Append(this).Map(card => {
            ret.UnionWith(card.keyWords);
        });

        return ret;
    }

    /// <summary>
    /// 获取目标委托，用于判定是否存在该委托
    /// 不要直接invoke获取到的委托!（这样不会自动为gameEvent.thisEffect赋值)
    /// </summary>
    public IOJMethod GetProxys(ProxyEnum proxyEnum)
    {
        OJMethods methods = new OJMethods();
        effects.Concat(effectsStay).Append(this).Map(card => {
            if (card.methods.ContainsKey(proxyEnum))
            {
                methods.Add(card.methods[proxyEnum]);
            }
        });

        if (methods.Any())
        {
            return methods;
        }
        return null;
    }

    public ProxyEffects GetProxysByEffect(ProxyEnum proxyEnum)
    {
        ProxyEffects proxysEffects = new ProxyEffects(proxyEnum);
        foreach (var card in effects.Concat(effectsStay).Append(this))
        {
            if (card.methods.ContainsKey(proxyEnum))
            {
                proxysEffects.Add(card);
            }
        }


        if (proxysEffects.Count == 0)
        {
            return null;
        }
        return proxysEffects;
    }

    /// <summary>
    /// 将方法绑定到卡牌本身
    /// </summary>
    public void AddProxy(ProxyEnum proxyEnum, IOJMethod method)
    {
        if (methods.ContainsKey(proxyEnum))
        {
            IOJMethod oJMethod = methods[proxyEnum];
            if (oJMethod is OJMethods oJMethods)
            {
                oJMethods.Add(method);
            }
            else
            {
                methods[proxyEnum] = new OJMethods(oJMethod, method);
            }
        }
        else
        {
            methods[proxyEnum] = method;
        }
    }

    public bool InvokeProxy(ProxyEnum proxyEnum, GameEvent gameEvent)
    {
        var proxys = GetProxysByEffect(proxyEnum);
        if (proxys == null) return false;
        object answer = false;

        foreach (var card in proxys)
        {
            if (card.methods.ContainsKey(proxyEnum))
            {
                gameEvent.thisEffect = card;
                try
                {
                    answer = card.methods[proxyEnum].Invoke(gameEvent);
                }
                catch (RuntimeException e)
                {
                    $"[Error]{this.name} {card.name}: {e.Message}".LogToFile();
                }
            }
        }
        return !(answer == null || answer == (object)false);
    }

    /// <summary>
    /// 获得随从身材
    /// </summary>
    /// <param name="minion"></param>
    /// <returns></returns>
    public Vector2Int GetMinionBody()
    {
        Vector2Int body = new Vector2Int(attack, health);
        effects.Concat(effectsStay).Map(card => {
            body.x += card.attack;
            body.y += card.health;
        });

        return body;
    }

    /// <summary>
    /// 获得随从身材(不计算光环影响)
    /// </summary>
    /// <param name="minion"></param>
    /// <returns></returns>
    public Vector2Int GetMinionBodyWithoutEffect()
    {
        Vector2Int body = new Vector2Int(attack, health);
        effectsStay.Map(card => {
            body.x += card.attack;
            body.y += card.health;
        });

        return body;
    }

    public Vector2Int GetExtraBody()
    {
        Vector2Int body = new Vector2Int();
        effects.Concat(effectsStay).Map(card => {
            body.x += card.attack;
            body.y += card.health;
        });
        return body;
    }

    public void RemoveAuraEffect()
    {
        effects.Clear();
    }
    
    
    /// <summary>
    /// 是否为该种族
    /// </summary>
    /// <param name="minionType"></param>
    public bool IsMinionType(MinionType minionType)
    {
        if (type.Equals(minionType) || type.Equals(MinionType.Any) || minionType.Equals(MinionType.Any))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region object


    /// <summary>
    /// 生成新卡（获得新的uniqueId）
    /// </summary>
    /// <returns></returns>
    public Card NewCard()
    {
        Card card = Clone() as Card;
        card.Unique();
        return card;
    }

    /// <summary>
    /// 获取一张卡的完整复制（具有相同的uniqueId）
    /// </summary>
    /// <returns></returns>
    public object Clone()
    {
        Card card = MemberwiseClone() as Card;

        card.keyWords = new HashSet<Keyword>();
        card.keyWords.UnionWith(keyWords);

        card.effects = new List<Card>();
        card.effects.AddRange(effects);

        card.effectsStay = new List<Card>();
        card.effectsStay.AddRange(effectsStay);

        card.methods = new Map<ProxyEnum, IOJMethod>();
        card.methods.Update(methods);

        return card;
    }

    public void TransformToNewCardWithEffectsForBoss(Card card)
    {
        id = card.id;
        name = card.name;
        displayName = card.displayName;
        image = card.image;
        cost = card.cost;
        isToken = card.isToken;
        isGold = card.isGold;
        skillDescription = card.skillDescription;
        goldVersion = card.goldVersion;
        star = card.star;
        attack = card.attack;
        health = card.health;
        tag = card.tag;
        keyWords = card.keyWords;
        methods = new Map<ProxyEnum, IOJMethod>();
        methods.Update(card.methods);
        effectsStay = card.effectsStay;
        type = card.type;
    }

    public void TransformToNewCardWithEffects(Card card)
    {
        id = card.id;
        name = card.name;
        image = card.image;
        cost = card.cost;
        isToken = card.isToken;
        isGold = card.isGold;
        skillDescription = card.skillDescription;
        goldVersion = card.goldVersion;
        star = card.star;
        attack = card.attack;
        health = card.health;
        tag = card.tag;
        keyWords = card.keyWords;
        methods = card.methods;

        effects = new List<Card>();
        effects.AddRange(card.effects);

        effectsStay = new List<Card>();
        foreach (Card c in card.effectsStay)
        {
            effectsStay.Add(c);
        }
        type = card.type;
    }

    public void TransformToNewCardWithoutEffects(Card card)
    {
        TransformToNewCardWithEffects(card);
        effects = new List<Card>();
        effectsStay = new List<Card>();
    }

    public int GetPositionTag()
    {
        foreach(string item in tag)
        {
            for (int i = 0; i < 5; i++)
            {
                if (item.Equals("" + i))
                {
                    return i;
                }
            }
        }
        return 2;
    }

    public override string ToString()
    {
        Map<string, string> retMap = new Map<string, string>();
        var fieldInfos = GetType().GetFields();
        foreach (var field in fieldInfos)
        {
            object value = field.GetValue(this);
            if (value != null)
            {
                retMap[field.Name] = value.ToString();
            }
            else
            {
                retMap[field.Name] = "Null";
            }
        }
        return retMap.ToString();
    }


    #endregion


}



public enum MinionType
{
    [Description("")]
    General,    // 无类型
    [Description("野兽")]
    Beasts,
    [Description("恶魔")]
    Demons,
    [Description("机械")]
    Mechs,
    [Description("鱼人")]
    Murlocs,
    //[Description("海盗")]
    //Pirates,
    [Description("龙")]
    Dragons,
    //[Description("图腾")]
    //Totems,
    [Description("融合怪")]
    Any,    // 融合怪
    //[Description("")]
    //None,    // 空(不是无类型) 用作targetType的"无需指定target"
}

public enum Keyword
{
    [Description("剧毒")]
    Poisonous,
    [Description("嘲讽")]
    Taunt,
    [Description("风怒")]
    Windfury,
    [Description("圣盾")]
    DivineShield,
    [Description("潜行")]
    Stealth,
    [Description("复生")]
    Reborn,
    [Description("磁力")]
    Magnetic,
    [Description("超级风怒")]
    MegaWindfury,
    [Description("狂战斧")]
    Cleave,
    [Description("免疫")]
    Immune,
    [Description("冻结")]
    Freeze,
    [Description("主动")]
    Active,
    [Description("被动")]
    Passive,
    [Description("变幻")]
    Changing,
    [Description("")]
    None,
}

public enum CardType
{
    [Description("随从")]
    Minion,
    [Description("法术")]
    Spell,
    [Description("英雄")]
    Hero,
    [Description("Buff")]
    Buff,
}