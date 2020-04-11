using BIF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class CardBuilder
{
    static public Map<int, Card> AllCards = null;

    /// <summary>
    /// 根据卡牌id创建Card
    /// </summary>
    /// <param name="no"></param>
    /// <returns></returns>
    public static Card NewCard(int no)
    {
        //Debug.Log("no= " + no);
        return GetCard(no).NewCard();
    }
    public static void InitAllCards()
    {
        if (AllCards == null) AllCards = ReadCSV();
    }

    /// <summary>
    /// 读取csv文件，创建所有卡牌的原型
    /// </summary>
    /// <returns></returns>
    public static Map<int, Card> ReadCSV()
    {
        Map<int, Card> map = new Map<int, Card>();

        List<List<string>> csvData = BIFStaticTool.ReadCSV("chess");
        csvData.RemoveAt(0);    // 删除英文描述
        csvData.RemoveAt(0);    // 删除中文描述

        int idCnt = 0;
        foreach (var data in csvData)
        {
            if (string.IsNullOrEmpty(data[0])) continue;
            if (data[0].StartsWith("//")) continue;

            Card card = new Card
            {
                // 读取基本属性
                id = idCnt++,
                name = data[0],
                isToken = BIFStaticTool.ParseInt(data[1]) > 0,
                image = data[2],
                star = BIFStaticTool.ParseInt(data[3]),
                cost = BIFStaticTool.ParseInt(data[4]),
                isGold = BIFStaticTool.ParseInt(data[5]) > 0,
                attack = BIFStaticTool.ParseInt(data[6]),
                health = BIFStaticTool.ParseInt(data[7]),
                type = BIFStaticTool.GetEnumDescriptionEnumSaved(data[10], MinionType.General),
                cardType = BIFStaticTool.GetEnumDescriptionEnumSaved(data[11], CardType.Minion),
                skillDescription = data[13],
                description = data[14],
                unlockDescription = data[15],
            };

            // 读取keyword
            string[] keywordStrings = data[8].Split(';', '，');
            foreach (var keywordString in keywordStrings)
            {
                //KeyWord keyword = GetKeywordByDescription(keywordString);
                Keyword keyword = BIFStaticTool.GetEnumDescriptionEnumSaved(keywordString, Keyword.None);
                if (keyword != Keyword.None)
                {
                    card.keyWords.Add(keyword);
                }
            }
            map[card.id] = card;

            // 读取LongKeyword
            if (!string.IsNullOrEmpty(data[9]))
            {
                SetLongKeywordNatural(card, data[9]);
            }

            // 读取流派标签
            if (!string.IsNullOrEmpty(data[12]))
            {
                card.tag = new List<string>(data[12].Split('；',';'));
            }

            //// 读取counters
            //if (!string.IsNullOrEmpty(data[15]))
            //{
            //    int i = 0;
            //    foreach(var counter in data[15].Split(';', '；').Select(str=> BIFStaticTool.ParseInt(str)))
            //    {
            //        card.counters[i++] = counter;
            //        if (i >= card.counters.Length) break;
            //    }
            //}
        }

        #region auto add gold versionCard
        foreach (var card in map.GetValues().Where(card=>!card.isGold && card.cardType == CardType.Minion))
        {
            Card aimCard = map.FilterValue(c => c.isGold && c.name == card.name).GetOne();
            if (aimCard == null)
            {
                Card goldCard = card.Clone() as Card;
                goldCard.isGold = true;
                goldCard.isToken = true;
                goldCard.id = idCnt++;
                goldCard.health *= 2;
                goldCard.attack *= 2;
                map[goldCard.id] = goldCard;
            }
        }
        #endregion

        #region update goldVersion
        foreach (var pair in map)
        {
            Card card = pair.Value;
            Card aimCard = map.FilterValue(c => c.isGold != card.isGold && c.name == card.name).GetOne();
            if (aimCard != null) card.goldVersion = aimCard.id;
        }

        #endregion

        return map;
    }

    private static Map<MinionType, string> minionTypeAndDescription = null;
    public static MinionType GetMinionTypeByDescription(string description, MinionType defaultType=MinionType.General)
    {
        if (minionTypeAndDescription == null)
        {
            minionTypeAndDescription = BIFStaticTool.GetEnumFieldAndDescription<MinionType>();
        }
        if (string.IsNullOrEmpty(description)) return defaultType;
        foreach (var pair in minionTypeAndDescription)
        {
            if (pair.Value == description)
            {
                return pair.Key;
            }
        }
        return defaultType;
    }


    private static void SetLongKeywordNatural(Card card, string str)
    {
        string[] longKeywordStrings = str.Split(';', '；');
        var map = BIFStaticTool.GetEnumNameAndDescriptionSaved<ProxyEnum>();
        foreach (var longKeywordString in longKeywordStrings)
        {
            string[] settings = longKeywordString.Split(':', '：');
            if (settings.Length != 2)
            {
                continue;
            }
            CardProxyDelegate action = GetStaticDelegate(settings[1]);
            if (action == null)
            {
                Debug.LogWarning("[未找到委托函数]" + settings[1]);
                continue;
            }

            bool isFind = false;
            foreach (var item in map)
            {
                if (item.Value.Key.Equals(settings[0]))
                {
                    var desc = GetProxyDescription(action);
                    card.AddProxyOri(item.Key, action, desc.Item5, true);

                    isFind = true;
                    break;
                }
            }
            if (!isFind)
                Debug.LogWarning("[未找到长关键字]" + card.name+ settings[0] + action);
        }
    }


    /// <summary>
    /// 委托，普通描述，金卡描述，是否隐藏前缀
    /// </summary>
    private static readonly Map<CardProxyDelegate, (string, string, bool, string, int)> ProxyDescriptionCache = new Map<CardProxyDelegate, (string, string, bool, string, int)>();
    public static (string, string, bool, string, int) GetProxyDescription(CardProxyDelegate cardProxy)
    {
        if (ProxyDescriptionCache.ContainsKey(cardProxy))
        {
            return ProxyDescriptionCache[cardProxy];
        }
        return ("", "", false, "", 0);
    }

    public static string GetCardDescription(Card card)
    {
        if (!string.IsNullOrEmpty(card.skillDescription))
        {
            return card.skillDescription;
        }
        var map = BIFStaticTool.GetEnumNameAndCommonDescriptionSaved<ProxyEnum>();
        var str = "";
        foreach (var pair in map)
        {
            var effects = card.GetProxysByEffect(pair.Key);
            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    var ac = effect.cardProxyDelegate;
                    if (ac == null) continue;
                    var p = GetProxyDescription(ac as CardProxyDelegate);
                    string desc = card.isGold ? p.Item2 : p.Item1;
                    string promopt = p.Item3 ? p.Item4 : pair.Value.Value;
                    if (!string.IsNullOrEmpty(promopt)) promopt += ": ";

                    if (!string.IsNullOrEmpty(desc))
                    {
                        string description = promopt + desc + "\n";
                        description = string.Format(description, effect.Counter);
                        str += description;
                    }
                }
            }
        }
        return str;
    }

    public static string GetCardDescriptionContainsKeyword(Card card)
    {
        string keywordstr = card.GetAllKeywords()
            .Map(BIF.BIFStaticTool.GetEnumDescriptionSaved)
            .Map(str => "<b>" + str + "</b>")
            .StringJoin();
        if (!string.IsNullOrEmpty(keywordstr)) keywordstr += "\n";
        return keywordstr + GetCardDescription(card);
    }
    public static string GetCardNameContainsStar(Card card)
    {
        return card.name + (card.isGold ? "(金色)" : "") + "******".Substring(0, card.star);
    }

    public static Map<string, CardProxyDelegate> cacheGetStaticDelegateCache = new Map<string, CardProxyDelegate>();
    public static CardProxyDelegate GetStaticDelegate(string name)
    {
        // 读取缓存
        if (cacheGetStaticDelegateCache.ContainsKey(name))
        {
            return cacheGetStaticDelegateCache[name];
        }


        Type type = typeof(CardLongKeywordAchievement);
        MethodInfo method = type.GetMethod(name);
        if (method != null)
        {
            CardProxyDelegate action = Delegate.CreateDelegate(typeof(CardProxyDelegate), method) as CardProxyDelegate;

            #region get description
            GoldDescriptionAttribute goldDescription = Attribute.GetCustomAttribute(method, typeof(GoldDescriptionAttribute), false) as GoldDescriptionAttribute;
            CommonDescriptionAttribute commonDescription = Attribute.GetCustomAttribute(method, typeof(CommonDescriptionAttribute), false) as CommonDescriptionAttribute;
            HidePromptAttribute hidePrompt = Attribute.GetCustomAttribute(method, typeof(HidePromptAttribute), false) as HidePromptAttribute;
            SetCounterAttribute setCounter = Attribute.GetCustomAttribute(method, typeof(SetCounterAttribute), false) as SetCounterAttribute;
            ProxyDescriptionCache[action] = (commonDescription?.Description, goldDescription?.Description, hidePrompt!=null, hidePrompt?.Description, setCounter?.Value??0);

            #endregion

            // 设置缓存
            cacheGetStaticDelegateCache[name] = action;
            return action;
        }
        return null;
    }



    private static TField GetField<TObj, TField>(TObj obj, string name, TField defalutValue=default)
    {
        Type type = obj.GetType();
        FieldInfo fieldInfo = type.GetField(name);
        if (fieldInfo != null)
        {
            return (TField)fieldInfo.GetValue(obj);
        }
        return defalutValue;
    }
    private static bool SetField<TObj, TField>(TObj obj, string name, TField setValue)
    {
        Type type = obj.GetType();
        FieldInfo fieldInfo = type.GetField(name);
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(obj, setValue);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 根据卡牌id得到原型卡牌
    /// </summary>
    /// <param name="no"></param>
    /// <returns></returns>
    public static Card GetCard(int no)
    {
        if (AllCards == null)
        {
            AllCards = ReadCSV();
        }

        Card card = AllCards.GetByDefault(no, null);
        return card;
    }

    public static Map<int,Card> GetAllCardsExceptTokens(Map<int, Card> originMap)
    {
        Map<int, Card> map = new Map<int, Card>();
        foreach (var item in originMap)
        {
            if (!item.Value.isToken)
            {
                map.Add(item.Key, item.Value);
            }
        }
        return map;
    }

    public static List<Card> GetCardsByStar(int star)
    {
        return AllCards.FilterValue(card => !card.isToken && card.star == star);
    }

    public static Card SearchCardByName(string name) //用于检索token
    {
        return AllCards.FilterValue(card => card.name.Equals(name)).GetOne();
    }
    public static Card SearchCardByName(string name, bool isGold) //用于检索token
    {
        return AllCards.FilterValue(card=>card.isGold==isGold).Filter(card => card.name.Equals(name)).GetOne();
    }
}


