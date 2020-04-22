using OrderedJson.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonCommandDefiner
{
    public static void AfterMinionSummon(GameEvent gameEvent, IOJMethod method)
    {
        //Debug.Log("gameEvent = " + gameEvent);
        gameEvent.hostCard.AddProxy(ProxyEnum.AfterMinionSummon, method);
    }

    public static Card Target(GameEvent gameEvent)
    {
        return gameEvent.targetCard;
    }
    public static Card Host(GameEvent gameEvent)
    {
        return gameEvent.hostCard;
    }

    public static void If(GameEvent gameEvent, bool condition, IOJMethod action)
    {
        if (condition)
        {
            action.Invoke(gameEvent);
        }
    }

    public static bool IsType(GameEvent gameEvent, Card card, string type)
    {
        var minionType = BIF.BIFStaticTool.GetEnumDescriptionEnumSaved<MinionType>(type, MinionType.General);
        return card.IsMinionType(minionType);
    }
    public static void AddBuff(GameEvent gameEvent, Card card, string buff)
    {
        //Debug.Log("添加buff:" + buff);
        Card buffCard = CardBuilder.SearchBuffByName(buff);
        if (buffCard != null)
        {
            card.effectsStay.Add(buffCard);
        }
    }

    public static void Log(GameEvent gameEvent, object msg)
    {
        Debug.Log(msg);
    }

    public static bool True(GameEvent gameEvent)
    {
        return true;
    }
    public static bool False(GameEvent gameEvent)
    {
        return false;
    }

    public static void IFNotSelf(GameEvent gameEvent, IOJMethod method)
    {
        if (gameEvent.targetCard != gameEvent.hostCard)
        {
            method.Invoke(gameEvent);
        }
    }
}
