using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class SelectCard
{
    private static List<School> schools;
    
    private static List<int> starList = new List<int>();

    public static Tuple<List<Card>, List<Card>,string,string> GetCards(int i)
    {
        if (schools == null)
        {
            Init();
        }
        List<Card> tmp1 = new List<Card>();
        List<Card> tmp2 = new List<Card>();
        string str1;
        string str2;
        var tmp = new List<School>();
        int counter = 0;
        do
        {
            counter++;
            tmp1 = new List<Card>();
            tmp2 = new List<Card>();
            schools.Shuffle();
            tmp = schools.Shuffle().Take(2).ToList();
            for (int j = 3 * i; j < 3 * i + 3; j++)
            {
                tmp1.Add(tmp[0].cards.Filter(card => card.star == starList[j]).GetOneRandomly());
            }
            for (int j = 3 * i; j < 3 * i + 3; j++)
            {
                tmp2.Add(tmp[1].cards.Filter(card => card.star == starList[j]).GetOneRandomly());
            }
        }
        while ((tmp1.Contains(null) || tmp2.Contains(null) || (tmp1.Distinct()).Count() < 3 || (tmp2.Distinct()).Count() < 3) && counter < 20);
        str1 = tmp[0].name;
        str2 = tmp[1].name;
        if (counter >= 20)
        {
            var tmpCards = new List<Card>();
            foreach (var scool in schools)
            {
                foreach (var item in scool.cards)
                {
                    tmpCards.Add(item);
                }
            }
            tmpCards.Distinct();
            do
            {
                if (tmp1.Contains(null) || tmp1.Distinct().Count() < 3)
                {
                    tmp1.Clear();
                    for (int j = 3 * i; j < 3 * i + 3; j++)
                    {
                        tmp1.Add(tmpCards.Filter(card => card.star == starList[j]).GetOneRandomly());
                    }
                    Debug.Log("牌堆1生成失败,随机发牌");
                    str1 = "混沌";
                }
                if (tmp2.Contains(null) || tmp2.Distinct().Count() < 3)
                {
                    tmp2.Clear();
                    for (int j = 3 * i; j < 3 * i + 3; j++)
                    {
                        tmp2.Add(tmpCards.Filter(card => card.star == starList[j]).GetOneRandomly());
                    }
                    Debug.Log("牌堆2生成失败,随机发牌");
                    str2 = "混沌";
                }
            }
            while ((tmp1.Contains(null) || tmp2.Contains(null) || (tmp1.Distinct()).Count() < 3 || (tmp2.Distinct()).Count() < 3));
        }
        return new Tuple<List<Card>, List<Card>, string, string>(tmp1, tmp2, str1, str2);
    }

    public static void Init()
    {
        CardBuilder.GetCard(0);
        CardPile cardPile = new CardPile();
        cardPile.InitCardPileFully();
        /*
            string minionTypeCountString = cardPile.cardPile
                .GroupBy(pair => pair.Key.type)
                .Select(x => (card: x.First().Key, count: x.Sum(pair => pair.Value)))
                .Select(x => (x.card.type, count: x.count / Const.numOfMinionsInCardPile[x.card.star - 1]))
                .Select(pair => (type: BIF.BIFStaticTool.GetEnumDescriptionSaved(pair.type), pair.count))
                .Where(pair => !string.IsNullOrEmpty(pair.type))
                .Map(pair => pair.type + ":" + pair.count)
                .Join(" ");
            Debug.Log(minionTypeCountString);
        */
        schools = new List<School>
        {
            new School()
            {
                name = "中立",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("中立"))
            },
            new School()
            {
                name = "野兽",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("野兽"))
            },
            new School()
            {
                name = "恶魔",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("恶魔"))
            },
            new School()
            {
                name = "机械",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("机械"))
            },
            new School()
            {
                name = "鱼人",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("鱼人"))
            },
            new School()
            {
                name = "巨龙",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("龙"))
            },
            new School()
            {
                name = "嘲讽",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("嘲讽"))
            },
            new School()
            {
                name = "圣盾",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("圣盾"))
            },
            new School()
            {
                name = "战吼",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("战吼"))
            },
            new School()
            {
                name = "亡语",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("亡语"))
            },
            new School()
            {
                name = "混合",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("混合") || card.tag.Contains("奇异") || card.tag.Contains("青玉"))
            },
            //schools.Add(new School()
            //{
            //    name = "奇异",
            //    cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("奇异"))
            //});
            //schools.Add(new School()
            //{
            //    name = "召唤",
            //    cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("召唤"))
            //});
            new School()
            {
                name = "手牌",
                cards = CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.tag.Contains("手牌"))
            }
        };
        //foreach (var item in schools)
        //{
        //    Debug.Log(item.name + "  " + item.cards.Count);
        //}

        for (int i = 1; i <= 6; i++)
        {
            for (int j = 0; j < Const.typeOfMinionsInCardPile[i - 1] - 3; j++)
            {
                starList.Add(i);
            }
        }
        starList.Shuffle();
        for (int i = 1;i<=3;i++)
        {
            List<int> tmpStarList = new List<int>();
            for (int j = 1; j <= 6; j++)
            {
                tmpStarList.Add(j);
            }
            tmpStarList.Shuffle();
            foreach(var item in tmpStarList)
            {
                starList.Add(item);
            }
        }
    }

    public static void Remove(Card card)
    {
        foreach (var item in schools)
        {
            item.cards.Remove(card);
        }
    }
}

public enum SchoolType
{
    [Description("中立")]
    General,
    [Description("野兽")]
    Beasts,
    [Description("恶魔")]
    Demons,
    [Description("机械")]
    Mechs,
    [Description("鱼人")]
    Murlocs,
    [Description("龙")]
    Dragons,
    [Description("嘲讽")]
    Taunt,
    [Description("圣盾")]
    DivineShield,
    [Description("战吼")]
    Battlecry,
    [Description("亡语")]
    Deathrattle,
    [Description("混合")]
    Blend,
    [Description("召唤")]
    Summon,
    [Description("手牌")]
    Hand,
    [Description("奇异")]
    Special,
}

public class School
{
    public string name;
    public List<Card> cards = new List<Card>();
}
