using BIF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardPile
{
    public Map<Card, int> cardPile = new Map<Card, int>();

    public HashSet<Card> treasure = new HashSet<Card>();    //此处为宝藏池而非玩家持有的宝藏
    
    public static Map<Card, int>[] baseCardPileOfDifferentLevel = new Map<Card, int>[6];
    
    public static List<Card> DiscoverHigherStarSpell = new List<Card>();
    
    static System.Random random = new System.Random();


    /// <summary>
    /// 初始化一个包含所有牌的牌池
    /// </summary>
    public void InitCardPileFully() 
    {
        RemoveAll();
        CardBuilder.GetCard(0);
        for (int i = 0; i <= 5; i++)
        {
            baseCardPileOfDifferentLevel[i] = new Map<Card, int>();
            foreach (var item in CardBuilder.AllCards.FilterValue(card => !card.isToken && !card.isGold).Filter(card => card.star == i + 1))
            {
                baseCardPileOfDifferentLevel[i].Add(item, Const.numOfMinionsInCardPile[i]);
            }
            AddMap(baseCardPileOfDifferentLevel[i]);
        }
        InitSpecialCards();
    }


    public void InitCardPile(CardPile cardPile)
    {
        RemoveAll();
        CardBuilder.GetCard(0);
        foreach (var item in cardPile.cardPile)
        {
            this.cardPile.Add(item.Key, item.Value);
        }
        InitSpecialCards();
    }
    
    public static CardPile GetEmptyCardPile()
    {
        CardPile cardPile = new CardPile();
        cardPile.InitSpecialCards();
        return cardPile;
    }
    
    internal static CardPile GetFullCardPile()
    {
        CardPile cardPile = new CardPile();
        cardPile.InitCardPileFully();
        return cardPile;
    }
    
    public void InitSpecialCards()
    {
        DiscoverHigherStarSpell.Add(CardBuilder.SearchCardByName("三连奖励1星", false));
        DiscoverHigherStarSpell.Add(CardBuilder.SearchCardByName("三连奖励2星", false));
        DiscoverHigherStarSpell.Add(CardBuilder.SearchCardByName("三连奖励3星", false));
        DiscoverHigherStarSpell.Add(CardBuilder.SearchCardByName("三连奖励4星", false));
        DiscoverHigherStarSpell.Add(CardBuilder.SearchCardByName("三连奖励5星", false));
        DiscoverHigherStarSpell.Add(CardBuilder.SearchCardByName("三连奖励6星", false));
        treasure.UnionWith(CardBuilder.AllCards.FilterValue(card => card.tag.Contains("宝藏")));
    }
    
    //工具方法
    public void AddCard(Card card, int num = 1)
    {
        if (cardPile.ContainsKey(CardBuilder.GetCard(card.id)))
        {
            cardPile[CardBuilder.GetCard(card.id)] += num;
        }
        else
        {
            cardPile.Add(CardBuilder.GetCard(card.id), num);
        }
        //foreach (var item in cardPile)
        //{
        //    Debug.Log(item.Key.name + " " + item.Value);
        //}
    }
    
    public int Sum()
    {
        int sum = 0;
        foreach(var item in cardPile)
        {
            sum += item.Value;
        }
        return sum;
    }
    
    public void AddMap(Map<Card,int> map)
    {
        foreach(var item in map)
        {
            AddCard(item.Key, item.Value);
        }
    }
    
    public void ReduceCard(Card card,int num)
    {
        if (cardPile.ContainsKey(CardBuilder.GetCard(card.id)))
        {
            cardPile[CardBuilder.GetCard(card.id)] -= num;
            if(cardPile[CardBuilder.GetCard(card.id)] <= 0)
            {
                cardPile.Remove(CardBuilder.GetCard(card.id));
            }
        }
    }
    
    public void RemoveAll()
    {
        cardPile = new Map<Card, int>();
        treasure = new HashSet<Card>();
        baseCardPileOfDifferentLevel = new Map<Card, int>[6];
        DiscoverHigherStarSpell = new List<Card>();
    }
    
    public Card RandomlyGetCardAndReduceIt()
    {
        Card card;
        List<Card> cards = new List<Card>();
        int index = random.Next(Sum());
        foreach(var item in cardPile)
        {
            for(int i = 0;i < item.Value;i++)
            {
                cards.Add(item.Key);
            }
        }
        card = cards[index];
        ReduceCard(card, 1);
        return card;
    }
    
    public Card RandomlyGetCardByFilterAndReduceIt(Func<Card, bool> func)
    {
        Card card;
        List<Card> cards = new List<Card>();
        foreach (var item in cardPile)
        {
            for (int i = 0; i < item.Value; i++)
            {
                if (func(item.Key))
                {
                    cards.Add(item.Key);
                }
            }
        }
        int index = random.Next(cards.Count);
        if (cards.Count > 0)
        {
            card = cards[index];
            ReduceCard(card, 1);
            return card;
        }
        else
        {
            Debug.Log("牌池中没有符合条件的牌");
            return null;
        }
    }
    
    public Card RandomlyGetBestCard(Func<Card, int> func)
    {
        int value = 2;
        List<Card> cards = new List<Card>();
        foreach (var item in cardPile)
        {
            if (func(item.Key) > value)
            {
                value = func(item.Key);
                for (int i = cards.Count - 1; i >= 0; i--)
                {
                    if (func(cards[i]) < value - 1)
                    {
                        cards.Remove(cards[i]);
                    }
                }
                cards.Add(item.Key);
            }
            else if (func(item.Key) >= value - 1)
            {
                cards.Add(item.Key);
            }
        }
        if (cards.Count > 0)
        {
            Card card = cards.GetOneRandomly();
            return card;
        }
        else
        {
            Debug.Log("牌池中没有符合条件的牌");
            return null;
        }
    }

}
