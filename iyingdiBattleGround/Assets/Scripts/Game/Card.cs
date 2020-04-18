using BIF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using System.Linq;

[Serializable]
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
    public List<Effect> effects = new List<Effect>();
    /// <summary>
    /// 永久特效
    /// </summary>
    public List<Effect> effectsStay = new List<Effect>();
    /// <summary>
    /// 自身特效
    /// </summary>
    public List<Effect> effectsOri = new List<Effect>();
    
    
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
    public Map<ProxyEnum, CardProxyDelegate> proxys = new Map<ProxyEnum, CardProxyDelegate>();

    #endregion

    #region setter and getter

    public bool HasKeyword(Keyword keyword)
    {
        if (keyWords.Contains(keyword)) return true;
        bool HasKeyword(Effect effect)
        {
            if (effect is KeyWordEffect keyWordEffect)
            {
                return keyWordEffect.keyword == keyword;
            }
            return false;
        }

        if (effects.Any(HasKeyword)) return true;
        return effectsStay.Any(HasKeyword);
    }

    public void RemoveKeyWord(Keyword keyword)
    {
        if (keyWords.Contains(keyword))
        {
            keyWords.Remove(keyword);
        }
        if (effects.Count != 0)
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                if (effects[i] is KeyWordEffect keyWordEffect)
                {
                    if (keyWordEffect.keyword == keyword)
                    {
                        effects.Remove(effects[i]);
                    }
                }
            }
        }
        if (effectsStay.Count != 0)
        {
            for (int i = effectsStay.Count - 1; i >= 0; i--)
            {
                if (effectsStay[i] is KeyWordEffect keyWordEffect)
                {
                    if (keyWordEffect.keyword == keyword)
                    {
                        effectsStay.Remove(effectsStay[i]);
                    }
                }
            }
        }
    }

    public HashSet<Keyword> GetAllKeywords()
    {
        HashSet<Keyword> ret = new HashSet<Keyword>();
        ret.UnionWith(keyWords);
        effects.Map<Effect, KeyWordEffect>()
            .Map(effect => effect.keyword)
            .Map(ret.Add);
        effectsStay.Map<Effect, KeyWordEffect>()
            .Map(effect=>effect.keyword)
            .Map(ret.Add); 

        return ret;
    }

    /// <summary>
    /// 获取目标委托，用于判定是否存在该委托
    /// 不要直接invoke获取到的委托!（这样不会自动为gameEvent.thisEffect赋值)
    /// </summary>
    /// <param name="proxyEnum"></param>
    /// <returns></returns>
    public CardProxyDelegate GetProxys(ProxyEnum proxyEnum)
    {
        CardProxyDelegate @delegate = null;
        @delegate += proxys.GetByDefault(proxyEnum);
        foreach (var effect in effects)
        {
            if (effect is ProxyEffect proxyEffect && proxyEffect.proxyEnum == proxyEnum)
                @delegate += proxyEffect.cardProxyDelegate;
        }
        foreach (var effect in effectsStay)
        {
            if (effect is ProxyEffect proxyEffect && proxyEffect.proxyEnum == proxyEnum)
                @delegate += proxyEffect.cardProxyDelegate;
        }
        foreach (var effect in effectsOri)
        {
            if (effect is ProxyEffect proxyEffect && proxyEffect.proxyEnum == proxyEnum)
                @delegate += proxyEffect.cardProxyDelegate;
        }
        return @delegate;
    }

    public ProxyEffects GetProxysByEffect(ProxyEnum proxyEnum)
    {
        ProxyEffects proxysEffects = new ProxyEffects();
        foreach (var effect in effects)
        {
            if (effect is ProxyEffect proxyEffect && proxyEffect.proxyEnum == proxyEnum)
                proxysEffects.Add(proxyEffect);
        }
        foreach (var effect in effectsStay)
        {
            if (effect is ProxyEffect proxyEffect && proxyEffect.proxyEnum == proxyEnum)
                proxysEffects.Add(proxyEffect);
        }
        foreach (var effect in effectsOri)
        {
            if (effect is ProxyEffect proxyEffect && proxyEffect.proxyEnum == proxyEnum)
                proxysEffects.Add(proxyEffect);
        }

        if (proxysEffects.Count == 0)
        {
            return null;
        }

        return proxysEffects;
    }

    /// <summary>
    /// dont use
    /// </summary>
    /// <param name="proxyEnum"></param>
    /// <param name="cardProxyDelegate"></param>
    public ProxyEffect AddProxyOri(ProxyEnum proxyEnum, CardProxyDelegate cardProxyDelegate, int setCounter = 0, bool makeGroup = false)
    {
        ProxyEffect effect = new ProxyEffect(proxyEnum, cardProxyDelegate)
        {
            Counter = setCounter
        };
        if (makeGroup && effectsOri.Count>0)
        {
            effect.groupAs = effectsOri.Map<Effect, ProxyEffect>().GetOne();
        }

        effectsOri.Add(effect);
        return effect;
    }

    public bool InvokeProxy(ProxyEnum proxyEnum, GameEvent gameEvent)
    {
        var proxys = GetProxysByEffect(proxyEnum);
        if (proxys == null) return false;
        bool answer = false;
        foreach(var proxy in proxys)
        {
            answer = proxy.Invoke(gameEvent);
        }
        return answer;
    }

    /// <summary>
    /// 获得随从身材
    /// </summary>
    /// <param name="minion"></param>
    /// <returns></returns>
    public Vector2Int GetMinionBody()
    {
        Vector2Int body = new Vector2Int(attack, health);
        foreach (var effect in effects)
        {
            if (effect is BodyPlusEffect bodyPlusEffect)
            {
                body.x += bodyPlusEffect.attack;
                body.y += bodyPlusEffect.health;
            }
        }
        foreach (var effect in effectsStay)
        {
            if (effect is BodyPlusEffect bodyPlusEffect)
            {
                body.x += bodyPlusEffect.attack;
                body.y += bodyPlusEffect.health;
            }
        }

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
        foreach (var effect in effectsStay)
        {
            if (effect is BodyPlusEffect bodyPlusEffect)
            {
                body.x += bodyPlusEffect.attack;
                body.y += bodyPlusEffect.health;
            }
        }

        return body;
    }

    public Vector2Int GetExtraBody()
    {
        Vector2Int body = new Vector2Int();
        foreach (var effect in effects)
        {
            if (effect is BodyPlusEffect bodyPlusEffect)
            {
                body.x += bodyPlusEffect.attack;
                body.y += bodyPlusEffect.health;
            }
        }
        foreach (var effect in effectsStay)
        {
            if (effect is BodyPlusEffect bodyPlusEffect)
            {
                body.x += bodyPlusEffect.attack;
                body.y += bodyPlusEffect.health;
            }
        }

        return body;
    }

    public void RemoveAuraEffect()
    {
        effects.Clear();
        //for(int i = effects.Count - 1; i >= 0; i--)
        //{
        //    if (effects[i] is BodyPlusEffect)
        //    {
        //        effects.Remove(effects[i]);
        //    }
        //}
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

        card.effects = new List<Effect>();
        card.effects.AddRange(effects);

        card.effectsStay = new List<Effect>();
        card.effectsStay.AddRange(effectsStay);

        card.effectsOri = new List<Effect>();
        card.effectsOri.AddRange(effectsOri.Cast<ProxyEffect>().Select(p=>p.Copy()));

        card.proxys = new Map<ProxyEnum, CardProxyDelegate>();
        card.proxys.Update(proxys);

        return card;
    }

    public void TransformToNewCardWithEffectsForBoss(Card card)
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
        proxys = card.proxys;
        
        effectsOri = new List<Effect>();
        effectsOri.AddRange(card.effectsOri);

        if (IsMinionType(card.type) && effectsStay.Count > 0)
        {
            for (int i = effectsStay.Count - 1;i >= 0;i--)
            { 
                if (!(effectsStay[i] is BodyPlusEffect))
                {
                    effectsStay.Remove(effectsStay[i]);
                }
            }
        }
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
        proxys = card.proxys;

        effects = new List<Effect>();
        effects.AddRange(card.effects);

        effectsStay = new List<Effect>();

        foreach (Effect effect in card.effectsStay)
        {
            if (effect is BodyPlusEffect)
            {
                effectsStay.Add(effect);
            }
        }

        effectsOri = new List<Effect>();
        effectsOri.AddRange(card.effectsOri);

        foreach (Effect effect in card.effectsStay)
        {
            if (!(effect is BodyPlusEffect))
            {
                effectsStay.Add(effect);
            }
        }

        type = card.type;
    }

    public void TransformToNewCardWithoutEffects(Card card)
    {
        TransformToNewCardWithEffects(card);
        effects = new List<Effect>();
        effectsStay = new List<Effect>();
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
    Hero
}