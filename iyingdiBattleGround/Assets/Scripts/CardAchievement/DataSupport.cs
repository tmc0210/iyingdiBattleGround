using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 与gameEvent无关的提供数据的函数
/// </summary>
public static partial class CommonCommandDefiner
{
    public static bool IsType(GameEvent gameEvent, Card card, string type)
    {
        var minionType = BIF.BIFStaticTool.GetEnumDescriptionEnumSaved(type, MinionType.General);
        return card.IsMinionType(minionType);
    }

    public static bool IsGold(GameEvent gameEvent, Card card)
    {
        return card.isGold;
    }


    

    //public static Card OpponentRandomCard(GameEvent gameEvent)
    //{
    //    return gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
    //}
    public static Card GetRandomCard(GameEvent gameEvent, List<Card> cards)
    {
        return cards.GetOneRandomly();
    }

    public static List<Card> FilterDeathrattle(GameEvent gameEvent, List<Card> cards)
    {
        List<Card> list = cards.Where(card => card.GetProxys(ProxyEnum.Deathrattle) != null).ToList();

        return list;
    }
    public static List<Card> FilterStar(GameEvent gameEvent, List<Card> cards, int star)
    {
        return cards.Where(card => card.star == star).ToList();
    }
    public static List<Card> ExceptCard(GameEvent gameEvent, List<Card> cards, Card except)
    {
        return cards.Where(card => card.id != except.id).ToList();
    }
}
