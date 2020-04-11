using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SelectWindowCounterSetting : MonoBehaviour
{
    [Autohook]
    public TextMeshPro FamilyText;
    [Autohook]
    public TextMeshPro KeywordText;

    public void SetByCardPile(CardPile cardPile)
    {
        string minionTypeCountString = cardPile.cardPile
            .GroupBy(pair => pair.Key.type)
            .Select(x => (card: x.First().Key, count: x.Sum(pair => pair.Value)))
            .Select(x => (x.card.type, count: x.count / Const.numOfMinionsInCardPile[x.card.star - 1]))
            .Select(pair => (type: BIF.BIFStaticTool.GetEnumDescriptionSaved(pair.type), pair.count))
            .Where(pair => !string.IsNullOrEmpty(pair.type))
            .OrderBy(pair => pair.type)
            .Map(pair => pair.type + ":" + pair.count)
            .StringJoin(" ");

        FamilyText.text = minionTypeCountString;

        var keywordCount = cardPile.cardPile
            .Select(pair => (card: pair.Key, count: pair.Value / Const.numOfMinionsInCardPile[pair.Key.star - 1]))
            .Select(p => (p.card.GetAllKeywords(), p.count));

        var keywordstringList = keywordCount
            .SelectMany(p => p.Item1)
            .Distinct()
            .Select(x => (x, keywordCount.Where(p => p.Item1.Contains(x)).Sum(p => p.count)))
            .Select(pair => (name: BIF.BIFStaticTool.GetEnumDescriptionSaved(pair.x), count: pair.Item2))
            .ToList();

        var BattlecryCnt = cardPile.cardPile
            .Select(pair => (card: pair.Key, count: pair.Value / Const.numOfMinionsInCardPile[pair.Key.star - 1]))
            .Where(p => p.card.GetProxys(ProxyEnum.Battlecry) != null)
            .Sum(p => p.count);
        if (BattlecryCnt > 0)
        {
            keywordstringList.Add(("战吼", BattlecryCnt));
        }
        var DeathrattleCnt = cardPile.cardPile
            .Select(pair => (card: pair.Key, count: pair.Value / Const.numOfMinionsInCardPile[pair.Key.star - 1]))
            .Where(p => p.card.GetProxys(ProxyEnum.Deathrattle) != null)
            .Sum(p => p.count);
        if (DeathrattleCnt > 0)
        {
            keywordstringList.Add(("亡语", DeathrattleCnt));
        }

        var keywordstring = keywordstringList.Where(pair => !string.IsNullOrEmpty(pair.name))
            .OrderBy(pair => pair.name)
            .Map(pair => pair.name + ":" + pair.Item2)
            .StringJoin(" ");

        KeywordText.text = keywordstring;

    }
}
