using OrderedJson.Core;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 与gameEvent相关的提供数据的函数
/// context相关函数
/// </summary>
public static partial class CommonCommandDefiner
{

    public static Card Target(GameEvent gameEvent)
    {
        return gameEvent.targetCard;
    }
    public static Card Host(GameEvent gameEvent)
    {
        return gameEvent.hostCard;
    }

    public static Card Hero(GameEvent gameEvent)
    {
        return gameEvent.player.hero;
    }
    public static Card Opponent(GameEvent gameEvent)
    {
        return gameEvent.player.board.GetAnotherPlayer(gameEvent.player).hero;
    }

    /// <summary>
    /// 酒馆星级
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    public static int Star(GameEvent gameEvent)
    {
        return gameEvent.player.star;
    }

    public static int Attack(GameEvent gameEvent, Card card)
    {
        return card.GetMinionBody().x;
    }
    public static int Health(GameEvent gameEvent, Card card)
    {
        return card.GetMinionBody().y;
    }
    public static Card Creator(GameEvent gameEvent, Card card)
    {
        return card.creator;
    }

    public static void Trigger(GameEvent gameEvent)
    {
        gameEvent.Trigger = true;
    }

    public static List<Card> GetAdjacentMinions(GameEvent gameEvent, Card card)
    {
        var am = gameEvent.player.board.GetAdjacentMinion(card);
        List<Card> cards = new List<Card>();
        if (am.Item1 != null) cards.Add(am.Item1);
        if (am.Item2 != null) cards.Add(am.Item2);
        return cards;
    }



    public static int Gold(GameEvent gameEvent)
    {
        if (gameEvent.hostCard.isGold)
        {
            return 1;
        }
        return 0;
    }

    
    public static bool Own(GameEvent gameEvent, Card card)
    {
        if (gameEvent.player == gameEvent.player.board.GetPlayer(card)) return true;
        //if (gameEvent.player.battlePile.Contains(card)) return true;
        //if (gameEvent.player.handPile.Contains(card)) return true;
        return false;
    }


    public static List<Card> AllAllyMinions(GameEvent gameEvent)
    {
        return gameEvent.player.GetAllAllyMinion();
    }
    public static List<Card> AllAliveAllyMinions(GameEvent gameEvent)
    {
        return gameEvent.player.GetAllAllyMinionWithHealthabove0();
    }

    public static List<Card> AllOpponentMinions(GameEvent gameEvent)
    {
        return gameEvent.player.board.GetAnotherPlayer(gameEvent.player).GetAllAllyMinion();
    }
    public static List<Card> AllAliveOpponentMinions(GameEvent gameEvent)
    {
        return gameEvent.player.board.GetAnotherPlayer(gameEvent.player).GetAllAllyMinionWithHealthabove0();
    }

    public static List<Card> AllMinionsInPool(GameEvent gameEvent)
    {
        return gameEvent.player.board.cardPile.cardPile.Keys.ToList();
    }
}
