using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 功能性函数
/// </summary>
public static partial class CommonCommandDefiner
{
    public static void AddBuff(GameEvent gameEvent, Card card, string buff)
    {
        //Debug.Log("添加buff:" + buff);
        Card buffCard = CardBuilder.SearchBuffByName(buff);
        if (buffCard != null)
        {
            card.effectsStay.Add(buffCard);
        }
    }

    public static void AddBodyBuff(GameEvent gameEvent, Card card, int attack, int health)
    {
        Card buffCard = CardBuilder.NewBodyBuffCard(attack, health);
        if (buffCard != null)
        {
            card.effectsStay.Add(buffCard);
        }
    }
    public static void AddBodyBuffArua(GameEvent gameEvent, Card card, int attack, int health)
    {
        Card buffCard = CardBuilder.NewBodyBuffCard(attack, health);
        if (buffCard != null)
        {
            card.effects.Add(buffCard);
        }
    }
}
