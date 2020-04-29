using OrderedJson.Core;
using OrderedJson.Definer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 控制流 语言相关, +-*/
/// </summary>
public static partial class CommonCommandDefiner
{


    public static void If(GameEvent gameEvent, bool condition, IOJMethod action)
    {
        if (condition)
        {
            action.Invoke(gameEvent);
        }
    }

    public static object IfElse(GameEvent gameEvent, bool condition, IOJMethod _if, IOJMethod _else)
    {
        if (condition)
        {
            return _if.Invoke(gameEvent);
        }
        else
        {
            return _else.Invoke(gameEvent);
        }
    }

    public static void ForeachCard(GameEvent gameEvent, List<Card> cards, IOJMethod action)
    {
        foreach (var card in cards)
        {
            gameEvent.Cursor = card;
            action.Invoke(gameEvent);
        }
    }
    public static Card Cur(GameEvent gameEvent)
    {
        return gameEvent.Cursor;
    }

    public static void Log(GameEvent gameEvent, object msg)
    {
        msg = msg.OJGetValue(gameEvent);
        Debug.Log(msg);
        $"[Log] {gameEvent.hostCard.name}: {msg}".LogToFile();
    }

    public static bool True(GameEvent gameEvent)
    {
        return true;
    }
    public static bool False(GameEvent gameEvent)
    {
        return false;
    }
    
    //public static bool Equals(GameEvent gameEvent, object obj1, object obj2)
    //{
    //    obj1 = obj1.OJGetValue(gameEvent);
    //    obj2 = obj2.OJGetValue(gameEvent);
    //    if (obj1 == obj2) return true;
    //    if (obj1.Equals(obj2)) return true;
    //    return false;
    //}

}
