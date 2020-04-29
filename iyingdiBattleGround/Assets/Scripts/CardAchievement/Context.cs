

using OrderedJson.Core;
/// <summary>
/// 通用 gameEvent相关
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

    public static bool Own(GameEvent gameEvent, Card card)
    {
        if (gameEvent.player.battlePile.Contains(card)) return true;
        if (gameEvent.player.handPile.Contains(card)) return true;
        return false;
    }


    public static bool IsType(GameEvent gameEvent, Card card, string type)
    {
        var minionType = BIF.BIFStaticTool.GetEnumDescriptionEnumSaved(type, MinionType.General);
        return card.IsMinionType(minionType);
    }

    public static int Gold(GameEvent gameEvent)
    {
        if (gameEvent.hostCard.isGold)
        {
            return 1;
        }
        return 0;
    }

    public static bool IsGold(GameEvent gameEvent, Card card)
    {
        return card.isGold;
    }

    public static int ToInt(GameEvent gameEvent, object obj)
    {
        if (obj is IOJMethod method)
        {
            obj = method.Invoke(gameEvent);
        }

        if (obj is int intValue)
        {
            return intValue;
        }
        if (obj is bool boolValue)
        {
            if (boolValue) return 1;
            return 0;
        }
        return 0;
    }
    
}
