using System.Collections.Generic;
using System.ComponentModel;

public partial class CardLongKeywordAchievement
{
    [CommonDescription("发现一张1星随从")]
    public static bool Discover1StarMinion(GameEvent gameEvent)
    {
        gameEvent.player.board.DiscoverToHand(gameEvent.player.board.cardPile.cardPile.FilterKey(card => card.star == 1));
        return true;
    }

    [CommonDescription("发现一张2星随从")]
    public static bool Discover2StarMinion(GameEvent gameEvent)
    {
        gameEvent.player.board.DiscoverToHand(gameEvent.player.board.cardPile.cardPile.FilterKey(card => card.star == 2));
        return true;
    }

    [CommonDescription("发现一张3星随从")]
    public static bool Discover3StarMinion(GameEvent gameEvent)
    {
        gameEvent.player.board.DiscoverToHand(gameEvent.player.board.cardPile.cardPile.FilterKey(card => card.star == 3));
        return true;
    }

    [CommonDescription("发现一张4星随从")]
    public static bool Discover4StarMinion(GameEvent gameEvent)
    {
        gameEvent.player.board.DiscoverToHand(gameEvent.player.board.cardPile.cardPile.FilterKey(card => card.star == 4));
        return true;
    }

    [CommonDescription("发现一张5星随从")]
    public static bool Discover5StarMinion(GameEvent gameEvent)
    {
        gameEvent.player.board.DiscoverToHand(gameEvent.player.board.cardPile.cardPile.FilterKey(card => card.star == 5));
        return true;
    }

    [CommonDescription("发现一张6星随从")]
    public static bool Discover6StarMinion(GameEvent gameEvent)
    {
        gameEvent.player.board.DiscoverToHand(gameEvent.player.board.cardPile.cardPile.FilterKey(card => card.star == 6));
        return true;
    }

    /// <summary>
    /// 铸币
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("获得一枚铸币")]
    public static bool Gain1Coin(GameEvent gameEvent)
    {
        if (gameEvent.player.leftCoins < Const.MaxCoin)
        {
            gameEvent.player.leftCoins++;
            return true;
        }
        else
        {
            return false;
        }
    }

}
