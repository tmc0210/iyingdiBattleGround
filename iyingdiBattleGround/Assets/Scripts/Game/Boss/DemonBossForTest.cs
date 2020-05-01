using System.Collections.Generic;
using UnityEngine;

public class DemonBossForTest : Enemy
{
    private bool haveImmune = false;

    public DemonBossForTest() : base()
    {
        player = new Player(CardBuilder.SearchCardByName("恶魔boss").NewCard());
    }

    public override void StrenthenHeroPower()
    {
        foreach (var item in player.hero.GetProxysByEffect(ProxyEnum.Aura))
        {
            item.Counter += 1;
        }
    }

    public override void InitCardPile(CardPile cardPile)
    {
        this.cardPile = cardPile;
    }

    public override void InitTag()
    {
    }

    public override void InitUpgrade()
    {
        turnOfUpgrade = new int[5] { 4, 6, 8, 12, 20 };
    }

    public override void Strenthen()
    {
        Debug.Log("第" + turnNum + "回合开始," + player.hero.name + "酒馆等级为" + player.star + " ,剩余费用为" + player.leftCoins);        
        Upgrade();
        SpecialAction();
        GetBuff(BuyMinions());
        SetPosition();
    }

    public void MergeCheck()
    {
        CardPile tmpCardPile = new CardPile();
        foreach (Card card in player.battlePile)
        {
            if (!card.HasKeyword(Keyword.Changing))
            {
                tmpCardPile.AddCard(CardBuilder.GetCard(card.id), 1);
            }
        }
        foreach (Card card in player.handPile)
        {
            if (card.cardType.Equals(CardType.Minion))
            {
                if (!card.HasKeyword(Keyword.Changing))
                {
                    tmpCardPile.AddCard(CardBuilder.GetCard(card.id), 1);
                }
            }
        }

        List<Card> cards = new List<Card>();

        foreach (var item in tmpCardPile.cardPile)
        {
            if (item.Value >= 3 && !item.Key.isGold)
            {

                foreach (Card card in player.battlePile)
                {
                    if (cards.Count == 3)
                    {
                        break;
                    }
                    if (card.id == item.Key.id)
                    {
                        cards.Add(card);
                    }
                }
                foreach (Card card in player.handPile)
                {
                    if (cards.Count == 3)
                    {
                        break;
                    }

                    if (card.id == item.Key.id)
                    {
                        cards.Add(card);
                    }
                }
            }
            if (cards.Count == 3)
            {
                break;
            }
        }

        if (cards.Count == 3)
        {

            player.RemoveMinionFromBattlePile(cards[0]);
            player.RemoveMinionFromBattlePile(cards[1]);
            player.RemoveMinionFromBattlePile(cards[2]);
            player.RemoveMinionFromHandPile(cards[0]);
            player.RemoveMinionFromHandPile(cards[1]);
            player.RemoveMinionFromHandPile(cards[2]);

            Card card = CardBuilder.NewCard(cards[0].goldVersion);

            foreach (var item in cards)
            {
                foreach (var effectStay in item.effectsStay)
                {
                    card.effectsStay.Add(effectStay);
                }
            }

            Debug.Log(player.hero.name + "将" + card.name + "三连升级了!");
            player.AddMinionToBattlePile(card, 0);

            MergeCheck();
        }
    }

    private int BuyMinions()
    {
        int numOfDemons = 0;
        if (turnNum == 1 || turnNum == 2)
        {
            player.AddMinionToBattlePile(CardBuilder.SearchCardByName("愤怒编织者").NewCard(), 0);
            Debug.Log(player.hero.name + "购买了愤怒编织者");
            player.leftCoins -= 3;
        }
        if (turnNum == 6)
        {
            player.AddMinionToBattlePile(CardBuilder.SearchCardByName("愤怒编织者").NewCard(), 0);
            Debug.Log(player.hero.name + "购买了愤怒编织者");
            MergeCheck();
            player.AddMinionToBattlePile(CardBuilder.SearchCardByName("漂浮观察者").NewCard(), 0);
            player.leftCoins -= 3;
            numOfDemons++;
        }
        if (turnNum >= 10 && !haveImmune)
        {
            player.AddMinionToBattlePile(CardBuilder.SearchCardByName("漂浮观察者").NewCard(), 0);
            Debug.Log(player.hero.name + "购买了漂浮观察者");

            MergeCheck();
            player.leftCoins -= 3;
            numOfDemons++;
        }
        int i = player.leftCoins / 2;
        numOfDemons += i;
        for (int j = 0; j < i; j++)
        {
            player.hero.InvokeProxy(ProxyEnum.HeroPower, new GameEvent()
            {
                player = player
            });
            Debug.Log(player.hero.name + "购买并出售了恶魔");
        }
        return numOfDemons;
    }

    private void GetBuff(int numOfDemons)
    {
        int numOfHurt = 0;
        
        if (haveImmune)
        {
            foreach (Card card in player.battlePile)
            {
                if (card.name == "愤怒编织者")
                {
                    card.effectsStay.Add(new BodyPlusEffect(card.isGold ? 4 : 2, card.isGold ? 4 : 2));                    
                }
            }
        }
        else
        {
            foreach (Card card in player.battlePile)
            {
                if (card.name == "愤怒编织者")
                {
                    card.effectsStay.Add(new BodyPlusEffect(numOfDemons*(card.isGold ? 4 : 2), numOfDemons * (card.isGold ? 4 : 2)));
                    player.hero.health -= numOfDemons;
                    numOfHurt++;
                }
            }
            foreach (Card card in player.battlePile)
            {
                if (card.name == "漂浮观察者")
                {
                    int value = numOfHurt * (card.isGold ? 4 : 2);
                    card.effectsStay.Add(new BodyPlusEffect(value, value));
                }
            }
        }
    }    

    public override void SpecialAction()
    {
        if (player.star > 4 && player.hero.GetMinionBody().y <= 7)
        {
            if (!haveImmune)
            {
                player.AddMinionToBattlePile(CardBuilder.SearchCardByName("玛尔加尼斯").NewCard(), 0);
                haveImmune = true;
            }
            else
            {

            }
        }
    }

    /* 
     *  1/3/5  粉 
     *  2/4/4  粉 粉
     *  3/5/3  粉 粉
     *  4/6/2  粉 粉
     *  5/7/5  粉 粉
     *  6/8/4  金粉 绿 
     *  7/9/6  金粉 绿
     *  8/10/5 金粉 绿
     *  9/10/4 金粉 绿
     *  10/10/7 金粉 绿 绿
     *  11/10/6 金粉 金绿 绿
     *  12/10/5 金粉 金绿 绿 绿
     *  13/10/4 金粉 金绿 金绿 绿
     *  13/10/4 金粉 金绿 金绿 绿 绿
     */
}
