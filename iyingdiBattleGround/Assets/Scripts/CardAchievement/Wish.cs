using BIF;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wish
{
    #region wish object
    Player player;

    private Wish() { }


    private Map<string, int> tags = new Map<string, int>();
    private Map<Keyword, int> keywords = new Map<Keyword, int>();
    private Map<MinionType, int> minionTypes = new Map<MinionType, int>();
    private Map<ProxyEnum, int> proxys = new Map<ProxyEnum, int>();

    private void CalculateTags(List<Card> cardList)
    {
        tags = new Map<string, int>();
        keywords = new Map<Keyword, int>();
        minionTypes = new Map<MinionType, int>();
        proxys = new Map<ProxyEnum, int>();
        foreach (Card card in cardList)
        {
            if (card.IsMinionType(MinionType.Mechs) || card.tag.Contains("机械流"))
            {
                AddToMap(tags, "机械流", 1);
                AddToMap(minionTypes, MinionType.Mechs, 1);
            }
            if (card.IsMinionType(MinionType.Murlocs) || card.tag.Contains("鱼人流"))
            {
                AddToMap(tags, "鱼人流", 1);
                AddToMap(minionTypes, MinionType.Murlocs, 1);
            }
            if (card.IsMinionType(MinionType.Demons) || card.tag.Contains("恶魔流"))
            {
                AddToMap(tags, "恶魔流", 1);
                AddToMap(minionTypes, MinionType.Demons, 1);
            }
            if (card.IsMinionType(MinionType.Beasts) || card.tag.Contains("野兽流"))
            {
                AddToMap(tags, "野兽流", 1);
                AddToMap(minionTypes, MinionType.Beasts, 1);
            }
            if (card.HasKeyword(Keyword.DivineShield) || card.tag.Contains("圣盾流"))
            {
                AddToMap(tags, "圣盾流", 1);
                AddToMap(keywords, Keyword.DivineShield, 1);
            }
            if (CardBuilder.GetCard(card.id).GetProxys(ProxyEnum.Deathrattle) != null || card.tag.Contains("亡语流"))
            {
                AddToMap(tags, "亡语流", 1);
                AddToMap(proxys, ProxyEnum.Deathrattle, 1);
            }
            if (card.IsMinionType(MinionType.Dragons))
            {
                AddToMap(minionTypes, MinionType.Dragons, 1);
            }
            if (card.tag.Contains("手牌"))
            {
                AddToMap(tags, "手牌", 1);
            }
        }
    }

    private void AddToMap<T>(Map<T, int> map, T t, int num)
    {
        if (map.ContainsKey(t))
        {
            map[t] += num;
        }
        else
        {
            map.Add(t, num);
        }
    }

    private int CalculateFit(Card card)
    {
        int fit = 0;
        foreach (string item1 in card.tag)
        {
            foreach (var item2 in tags)
            {
                if (item1.Equals(item2.Key))
                {
                    fit += item2.Value;
                }
            }
        }
        foreach (var item in minionTypes)
        {
            if (card.IsMinionType(item.Key))
            {
                fit += item.Value;
            }
        }
        foreach (var item in keywords)
        {
            if (card.HasKeyword(item.Key))
            {
                fit += item.Value;
            }
        }
        foreach (var item in proxys)
        {
            if (card.GetProxys(item.Key) != null)
            {
                fit += item.Value;
            }
        }
        fit += card.star / 3;
        return fit;
    }

    #endregion

    #region public method

    public static Card GetFitedCard(Player player)
    {
        Wish wish = new Wish();
        wish.player = player;
        wish.CalculateTags(player.battlePile.ToList().Concat(player.handPile.ToList()).ToList());

        var cards = player.board.cardPile.cardPile.Keys
            .Where(card => card.star <= player.star && card.type != MinionType.Any)
            .Select(card=>(card, wish.CalculateFit(card)))
            .OrderByDescending(pair=>pair.Item2)
            .Take(3)
            .ToList();
        Debug.Log("最合适的三张牌: " + cards.Select(c => c.card.name+" "+c.Item2).StringJoin());

        return cards.GroupBy(pair => pair.Item2).OrderByDescending(x=>x.Key).First().ToList().GetOneRandomly().card;
    }

    public static List<Card> GetWishCardToDiscover(Player player)
    {
        List<Card> cards = new List<Card>();
        // 即将三连的第一张牌
        IOrderedEnumerable<Card> cardToMerge = GetPlayerCardsReadyForMerge(player).OrderByDescending(card => card.star);
        cards.AddRange(cardToMerge.Take(1));

        // 套路核心牌
        List<Card> fitestedCards = GetTheFitestCardInCardPile(player);
        cards.AddRange(fitestedCards);
        Card fitedCard = GetTheFitedCardInCardPile(player);
        if (fitedCard != null)
        {
            cards.Add(fitedCard);
        }

        // 剩余即将三连的牌
        cards.AddRange(cardToMerge.Skip(1));

        // 打工牌
        cards.AddRange(GetTheBigestCardInCardPile(player.board.cardPile, player.star));


        // 功能牌
        // 圣盾boss针对 时空龙或者食尸鬼
        // 喷子针对 时空龙 烈马 boss
        //player.board.enemy.player.battlePile

        List<Card> list = cards.Distinct().Take(3).ToList();
        return list;
    }

    #endregion

    #region private method


    private static Card GetTheFitedCardInCardPile(Player player)
    {
        return Wish.GetFitedCard(player);
    }
    private static List<Card> GetTheFitestCardInCardPile(Player player)
    {
        List<Card> cards = new List<Card>();
        var allCards = player.board.cardPile.cardPile.Keys
            .Where(c => c.star <= player.star);

        //获取流派名字
        var names = allCards
            .SelectMany(card => card.tag.Where(str => str.EndsWith("流")))
            .Distinct()
            .Select(str => str.Substring(0, str.Length - 1))
            .ToList();

        Debug.Log("所有流派:" + names.StringJoin());

        var has = player.battlePile.Concat(player.handPile);

        // 已经持有的流派
        var nameFited = names
            .Select(name => (name, count: has.Where(card => card.tag.Contains(name)).Count()))
            .Where(pair => pair.count > 2)
            .Select(pair => pair.name);

        Debug.Log("已持有的流派:" + nameFited.StringJoin());

        foreach (var name in nameFited)
        {
            Card card = null;
            // 获得最高级别的卡

            // 获得name流*2
            var nameEH = name + "流";
            if (card == null)
            {
                card = allCards
                    .Where(c => c.tag.Count(str => str == nameEH) >= 2)
                    .FirstOrDefault();
            }
            // 获得name流
            if (card == null)
            {
                card = allCards
                    .Where(c => c.tag.Contains(nameEH))
                    .FirstOrDefault();

                //Debug.Log("nameEHs = " + allCards
                //    .Where(c => c.tag.Contains(nameEH)).Select(c=>c.name).StringJoin());
            }
            // 获得name
            if (card == null)
            {
                card = allCards
                    .Where(c => c.tag.Contains(name))
                    .ToList()
                    .GetOneRandomly();

                //Debug.Log("names = " + allCards
                //    .Where(c => c.tag.Contains(name)).Select(c => c.name).StringJoin());
            }

            if (card != null)
            {
                cards.Add(card);
            }
        }

        Debug.Log("推荐的卡:" + cards.Select(card => card.name).StringJoin());

        return cards;
    }

    private static List<Card> GetPlayerCardsReadyForMerge(Player player)
    {
        return player.battlePile
            .Concat(player.handPile.ToList())
            .Concat(player.board.GetAnotherPlayer(player).battlePile)
            .GroupBy(card => card.id)
            .Where(g => g.Count() == 2)
            .Select(g => g.FirstOrDefault().id)
            .Select(CardBuilder.GetCard)
            .ToList();
    }

    private static List<Card> GetTheBigestCardInCardPile(CardPile cardPile, int star)
    {
        return cardPile.cardPile
            .FilterKey(card => card.star <= star)
            .OrderByDescending(card => (card.attack + card.health + 3 * card.GetAllKeywords().Count))
            .Take(3)
            .ToList();
    }
    #endregion
}
