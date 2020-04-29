using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static partial class CommonCommandDefiner
{
    public static List<Card> AllAllyMinions(GameEvent gameEvent)
    {
        return gameEvent.player.GetAllAllyMinion();
    }
    public static List<Card> AllOpponentMinions(GameEvent gameEvent)
    {
        return gameEvent.player.board.GetAnotherPlayer(gameEvent.player).GetAllAllyMinionWithHealthabove0();
    }

    //public static Card OpponentRandomCard(GameEvent gameEvent)
    //{
    //    return gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
    //}
    public static Card GetRandomCard(GameEvent gameEvent, List<Card> cards)
    {
        return cards.GetOneRandomly();
    }
}
