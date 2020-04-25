using OrderedJson.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static bool Equals(GameEvent gameEvent, object obj1, object obj2)
    {
        if (obj1 is IOJMethod method1)
        {
            obj1 = method1.Invoke(gameEvent);
        }
        if (obj2 is IOJMethod method2)
        {
            obj2 = method2.Invoke(gameEvent);
        }
        if (obj1 == obj2) return true;
        if (obj1.Equals(obj2)) return true;
        return false;
    }

    public static bool Not(GameEvent gameEvent, bool value)
    {
        return !value;
    }

    public static void IFNotSelf(GameEvent gameEvent, IOJMethod method)
    {
        if (gameEvent.targetCard != gameEvent.hostCard)
        {
            method.Invoke(gameEvent);
        }
    }
}
