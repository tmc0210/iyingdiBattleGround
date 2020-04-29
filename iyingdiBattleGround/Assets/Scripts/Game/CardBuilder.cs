using BIF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using OrderedJson;
using OrderedJson.Core;

public class CardBuilder
{
    public static Map<int, Card> AllCards = null;
    public static int idCnt = 0;

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
    public static IEnumerator InitAllCards()
    {
        //ModManger.GetModsName();

        var path = GetCSVPath("Buildin");
        //Debug.Log(path);
        var request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();
        var text = request.downloadHandler.text;
        request.downloadHandler.Dispose();
        //Debug.Log(text);
        //var data = BIFStaticTool.ParseCsv(text);
        var data = CsvFileReader.Parse(text);
        AllCards = ReadCSV(data);


        GameEvent gameEvent = new GameEvent();
        foreach (var pair in AllCards)
        {
            var card = pair.Value;
            gameEvent.hostCard = card;
            card.initMethod?.Invoke(gameEvent);
        }
    }

    /// <summary>
    /// 读取csv文件，创建所有卡牌的原型
    /// </summary>
    /// <returns></returns>
    public static Map<int, Card> ReadCSV(List<List<string>> csvData, OJParser parser = null)
    {
        if (parser == null)
        {
            parser = new OJParser(typeof(CommonCommandDefiner));
        }

        Map<int, Card> map = new Map<int, Card>();
        //idCnt = 0;
        foreach (var data in csvData)
        {
            if (string.IsNullOrEmpty(data[0])) continue;
            if (data[0].StartsWith("//")) continue;

            Card card = NewCardByCsvData(data, parser);
            card.id = idCnt++;

            map[card.id] = card;
        }

        #region auto add gold versionCard
        foreach (var card in map.GetValues().Where(card => !card.isGold && card.cardType == CardType.Minion))
        {
            Card aimCard = map.FilterValue(c => c.isGold && c.name == card.name).GetOne();
            if (aimCard == null)
            {
                Card goldCard = _GetNewGoldenCard(card);
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

    public static Card NewEmptyBuffCard()
    {
        Card card = new Card
        {
            // 读取基本属性
            //id = idCnt++,
            name = "",
            displayName = "",
            cardType = CardType.Buff,
            type = MinionType.Any,
            isToken = true,
            image = "",
            star = 1,
            cost = 0,
            isGold = false,
            attack = 0,
            health = 0,
            skillDescription = "",
            description = "",
            unlockDescription = "",
        };
        //AllCards[card.id] = card;
        return card;
    }

    public static Card NewBodyBuffCard(int attack, int health)
    {
        Card buff = NewEmptyBuffCard();
        buff.attack = attack;
        buff.health = health;
        return buff;
    }

    public static Card _GetNewGoldenCard(Card card)
    {
        Card goldCard = card.Clone() as Card;
        goldCard.isGold = true;
        goldCard.isToken = true;
        goldCard.id = idCnt++;
        goldCard.health *= 2;
        goldCard.attack *= 2;
        goldCard.goldVersion = card.id;
        return goldCard;
    }

    public static Card NewCardByCsvData(List<string> data, OJParser parser = null)
    {
        if (parser == null) parser = ModManger.parser;
        Card card = new Card
        {
            // 读取基本属性
            //id = idCnt++,
            name = data[0],
            displayName = data[1],
            cardType = BIFStaticTool.GetEnumDescriptionEnumSaved(data[2], CardType.Minion),
            type = BIFStaticTool.GetEnumDescriptionEnumSaved(data[3], MinionType.General),
            isToken = BIFStaticTool.ParseInt(data[4]) > 0,
            image = data[5],
            star = BIFStaticTool.ParseInt(data[6]),
            cost = BIFStaticTool.ParseInt(data[7]),
            isGold = BIFStaticTool.ParseInt(data[8]) > 0,
            attack = BIFStaticTool.ParseInt(data[9]),
            health = BIFStaticTool.ParseInt(data[10]),
            skillDescription = data[13],
            description = data[14],
            unlockDescription = data[15],
        };

        // 读取keyword
        string[] keywordStrings = data[11].Split(';', '，');
        foreach (var keywordString in keywordStrings)
        {
            //KeyWord keyword = GetKeywordByDescription(keywordString);
            Keyword keyword = BIFStaticTool.GetEnumDescriptionEnumSaved(keywordString, Keyword.None);
            if (keyword != Keyword.None)
            {
                card.keyWords.Add(keyword);
            }
        }

        // 读取流派标签
        if (!string.IsNullOrEmpty(data[15]))
        {
            card.tag = new List<string>(data[15].Split('；', ';'));
        }

        // 读取长关键字
        string code = data[12];
        if (!string.IsNullOrEmpty(code))
        {
            //Debug.Log("parse:"+ code);

            try
            {
                var method = parser.Parse(code, card.name);
                card.initMethod = method;
            }
            catch (OJException e)
            {
                $"[Error] {e.Message}".LogToFile();
                Debug.LogWarning(e);
            }
        }

        return card;
    }


    private static string GetCSVPath(string v)
    {
//        string path =
//#if UNITY_ANDROID && !UNITY_EDITOR
//        Application.streamingAssetsPath
//#elif UNITY_IPHONE && !UNITY_EDITOR
//        "file://" + Application.streamingAssetsPath ;
//#elif UNITY_STANDLONE_WIN||UNITY_EDITOR
//        "file://" + Application.streamingAssetsPath;
//#else
//        string.Empty;
//#endif
        return Application.streamingAssetsPath + $"/Mods/{v}/Card/card.csv";
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
        return card.skillDescription;
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
            //AllCards = ReadCSV();
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
    public static Card SearchBuffByName(string name) //用于检索buff
    {
        return AllCards.FilterValue(card => card.name.Equals(name) && card.cardType == CardType.Buff).GetOne();
    }
}


