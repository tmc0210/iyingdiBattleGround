using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 功能性函数
/// </summary>
public static partial class CommonCommandDefiner
{

    public static void Summon(GameEvent gameEvent, Card target, int number = 1)
    {
        if (target != null)
        {
            for (int i = 0; i < number; i++)
            {
                gameEvent.player.board.SummonMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = target.NewCard(),
                    player = gameEvent.player,
                });
            }
        }
    }

    public static Card SearchCard(GameEvent gameEvent, string name, bool isGold=false)
    {
        var card = CardBuilder.SearchCardByName(name, isGold);
        return card;
    }

    public static void DealDamage(GameEvent gameEvent, Card target, int number)
    {
        Card targetCard = gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
        if (target != null)
        {
            gameEvent.player.board.DealDamageToMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = target,
                player = gameEvent.player,
                number = number
            });
        }
    }

    public static void AddBuff(GameEvent gameEvent, Card card, string buff)
    {
        Card buffCard = CardBuilder.SearchBuffByName(buff);
        AddCardBuff(gameEvent, card, buffCard);
    }
    public static void AddBuffAura(GameEvent gameEvent, Card card, string buff)
    {
        Card buffCard = CardBuilder.SearchBuffByName(buff);
        AddCardBuffAura(gameEvent, card, buffCard);
    }


    public static void AddBodyBuff(GameEvent gameEvent, Card card, int attack, int health)
    {
        Card buffCard = CardBuilder.NewBodyBuffCard(attack, health);
        AddCardBuff(gameEvent, card, buffCard);
    }
    public static void AddBodyBuffAura(GameEvent gameEvent, Card card, int attack, int health)
    {
        Card buffCard = CardBuilder.NewBodyBuffCard(attack, health);
        AddCardBuffAura(gameEvent, card, buffCard);
    }

    public static void AddCardBuff(GameEvent gameEvent, Card card, Card buff)
    {
        if (card != null && buff != null)
        {
            card.effectsStay.Add(buff);
        }
    }
    public static void AddCardBuffAura(GameEvent gameEvent, Card card, Card buff)
    {
        if (card != null && buff != null)
        {
            card.effects.Add(buff);
        }
    }

    public static void AddKeywordBuff(GameEvent gameEvent, Card card, string keyword)
    {
        if (card != null)
        {
            Card buffCard = CardBuilder.NewEmptyBuffCard();
            var keywordType = BIF.BIFStaticTool.GetEnumDescriptionEnumSaved(keyword, Keyword.None);
            if (keywordType != Keyword.None)
            {
                buffCard.keyWords.Add(keywordType);
                //card.effectsStay.Add(buffCard);
                AddCardBuff(gameEvent, card, buffCard);
            }
        }
    }
    public static void AddKeywordBuffAura(GameEvent gameEvent, Card card, string keyword)
    {
        if (card != null)
        {
            Card buffCard = CardBuilder.NewEmptyBuffCard();
            var keywordType = BIF.BIFStaticTool.GetEnumDescriptionEnumSaved(keyword, Keyword.None);
            if (keywordType != Keyword.None)
            {
                buffCard.keyWords.Add(keywordType);
                AddCardBuffAura(gameEvent, card, buffCard);
            }
        }
    }

    public static void AddMoreMinionBuff(GameEvent gameEvent, Card card, int n)
    {
        if (card != null)
        {
            Card buffCard = CardBuilder.NewEmptyBuffCard();
            buffCard.SpecBuffMoreMinion = n;
            AddCardBuff(gameEvent, card, buffCard);
        }
    }
    public static void AddMoreMinionBuffAura(GameEvent gameEvent, Card card, int n)
    {
        if (card != null)
        {
            Card buffCard = CardBuilder.NewEmptyBuffCard();
            buffCard.SpecBuffMoreMinion = n;
            AddCardBuffAura(gameEvent, card, buffCard);
            //Log(gameEvent, "card's effect:" + card.effects.Count);
        }
    }

    public static void AddMoreBattlecryBuff(GameEvent gameEvent, Card card, int n)
    {
        if (card != null)
        {
            Card buffCard = CardBuilder.NewEmptyBuffCard();
            buffCard.SpecBuffBattlecry = n;
            AddCardBuff(gameEvent, card, buffCard);
        }
    }
    public static void AddMoreBattlecryBuffAura(GameEvent gameEvent, Card card, int n)
    {
        if (card != null)
        {
            Card buffCard = CardBuilder.NewEmptyBuffCard();
            buffCard.SpecBuffBattlecry = n;
            AddCardBuffAura(gameEvent, card, buffCard);
        }
    }

    public static void AddMoreDeathrattleBuff(GameEvent gameEvent, Card card, int n)
    {
        if (card != null)
        {
            Card buffCard = CardBuilder.NewEmptyBuffCard();
            buffCard.SpecBuffDeathrattle = n;
            AddCardBuff(gameEvent, card, buffCard);
        }
    }
    public static void AddMoreDeathrattleBuffAura(GameEvent gameEvent, Card card, int n)
    {
        if (card != null)
        {
            Card buffCard = CardBuilder.NewEmptyBuffCard();
            buffCard.SpecBuffDeathrattle = n;
            AddCardBuffAura(gameEvent, card, buffCard);
        }
    }
}
