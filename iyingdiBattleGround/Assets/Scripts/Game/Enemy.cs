using BIF;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy
{
    public Player player;
    public int[] turnOfUpgrade = new int[5];
    public CardPile cardPile = new CardPile();
    public int turnNum = 0;
    public List<KeyValuePair<Card, int>> setPositionList = new List<KeyValuePair<Card, int>>();
    private System.Random random = new System.Random();

    public Map<string, int> tags = new Map<string, int>();
    public Map<Keyword, int> keywords = new Map<Keyword, int>();
    public Map<MinionType, int> minionTypes = new Map<MinionType, int>();
    public Map<ProxyEnum, int> proxys = new Map<ProxyEnum, int>();

    public Map<Card, int> fitMap = new Map<Card, int>();
    
    public void Init(CardPile cardPile)
    {        
        InitCardPile(cardPile);
        InitUpgrade();
        InitTag();
        CalculateFitMap();
    }

    public virtual void SetLevel(int level)
    {
        //Debug.LogWarning("add 10 health");
        CommonCommandDefiner.AddBodyBuff(null, player.hero, 0, 10 * (level - 1));
        //player.hero.effectsStay.Add(new BodyPlusEffect(0, 10 * (level - 1)));
        //if (level >= 2)
        //{
        //    if (player.maxCoins < Const.MaxCoin)
        //    {
        //        player.maxCoins++;
        //    }
        //    player.leftCoins = player.maxCoins;
        //}
        if (level >= 3)
        {
            StrenthenHeroPower();
        }
    }

    public virtual void StrenthenHeroPower() { }

    public abstract void InitCardPile(CardPile cardPile);

    public abstract void InitTag();

    public abstract void InitUpgrade();

    public void CalculateTags() //当前思路识别,导向对应抓牌思路
    {
        tags = new Map<string, int>();
        keywords = new Map<Keyword, int>();
        minionTypes = new Map<MinionType, int>();
        proxys = new Map<ProxyEnum, int>();
        foreach (Card card in player.battlePile)
        {
            if (card.IsMinionType(MinionType.Mechs) || card.tag.Contains("机械流"))
            {
                AddToMap(tags, "机械流", 2);
                AddToMap(minionTypes, MinionType.Mechs, 1);
            }
            if (card.IsMinionType(MinionType.Murlocs) || card.tag.Contains("鱼人流"))
            {
                AddToMap(tags, "鱼人流", 2);
                AddToMap(minionTypes, MinionType.Murlocs, 1);
            }
            if (card.IsMinionType(MinionType.Demons) || card.tag.Contains("恶魔流"))
            {
                AddToMap(tags, "恶魔流", 2);
                AddToMap(minionTypes, MinionType.Demons, 1);
            }
            if (card.IsMinionType(MinionType.Beasts) || card.tag.Contains("野兽流"))
            {
                AddToMap(tags, "野兽流", 2);
                AddToMap(minionTypes, MinionType.Beasts, 1);
            }
            if (card.HasKeyword(Keyword.DivineShield) || card.tag.Contains("圣盾流"))
            {
                AddToMap(tags, "圣盾流", 3);
                AddToMap(keywords, Keyword.DivineShield, 1);
            }
            if (CardBuilder.GetCard(card.id).GetProxys(ProxyEnum.Deathrattle) != null || card.tag.Contains("亡语流"))
            {
                AddToMap(tags, "亡语流", 3);
                AddToMap(proxys, ProxyEnum.Deathrattle, 1);
            }
        }
        InitTag();
    }

    public void AddToMap<T>(Map<T, int> map, T t, int num)
    {
        if (map.ContainsKey(t))
        {
            map[t] += num;
        }
        else
        {
            map.Add(t, num);
        }
    }

    public void UseHeroPower()
    {
        if (player.hero.GetProxys(ProxyEnum.HeroPower) != null)
        {
            if (player.leftCoins >= player.hero.cost)
            {
                player.leftCoins -= player.hero.cost;
                player.hero.InvokeProxy(ProxyEnum.HeroPower, new GameEvent()
                {
                    player = player
                });
            }
        }
    }

    public void CalculateFitMap()
    {
        fitMap = new Map<Card, int>();
        foreach (var item in cardPile.cardPile)
        {
            if (!IsRecruitFieldEffect(item.Key))
            {
                fitMap.Add(item.Key, CalculateFit(item.Key));
            }
        }
        foreach (var item in player.battlePile)
        {
            if (!fitMap.ContainsKey(CardBuilder.GetCard(item.id)))
            {
                fitMap.Add(CardBuilder.GetCard(item.id), CalculateFit(CardBuilder.GetCard(item.id)));
            }
        }
    }

    public virtual void Strenthen()
    {
        Debug.Log("第" + turnNum + "回合开始," + player.hero.name + "酒馆等级为" + player.star + " ,剩余费用为" + player.leftCoins);
        Upgrade();
        SpecialAction();
        CalculateTags();
        CalculateFitMap();
        RandomlyBuyMinions();
        TransformMinionsToBetter();
        UseHeroPower();
        if (player.star >= 4)
        {
            RandomlyTransformMinionsToGold();
        }
        UpgradeCheck();
        SpecialCardCheck();
        if (player.star >= 4)
        {
            RandomlyGiveMinionBuff();
        }
        SetPosition();
    }

    public virtual void SpecialAction(){ }

    public void SpecialCardCheck()
    {
        foreach (Card card in player.battlePile)
        {
            if (card.name.Equals("钴制卫士"))
            {
                //card.effectsStay.Add(new KeyWordEffect(Keyword.DivineShield));
                Debug.LogWarning("add 圣盾！");
            }
        }
    }

    public void TransformMinionsToBetter()
    {
        if (player.leftCoins >= Const.coinCostToBuyMinion)
        {
            List<KeyValuePair<Card, Card>> cards = new List<KeyValuePair<Card, Card>>();
            foreach (Card item in player.battlePile)
            {
                Card targetCard = cardPile.RandomlyGetBestCard(card => (
                card.star <= player.star
                && CheckDuplicate(card)
                && CheckTransform(item.GetPositionTag(), card.GetPositionTag())) ? GetFit(card): -1);
                if (targetCard != null && GetFit(targetCard) > GetFit(item))
                {
                    cards.Add(new KeyValuePair<Card, Card>(item, targetCard));
                }
            }
            if (cards.Count != 0)
            {
                KeyValuePair<Card, Card> cardPair = cards.GetOneRandomly();
                int tmpID = cardPair.Key.id;
                cardPair.Key.TransformToNewCardWithEffectsForBoss(cardPair.Value.NewCard());               
                cardPile.ReduceCard(CardBuilder.GetCard(cardPair.Value.id), 1);
                player.leftCoins -= Const.coinCostToBuyMinion;
                if (cardPair.Key.GetPositionTag() >= 3)
                {
                    cardPair.Key.RemoveKeyWord(Keyword.Taunt);
                }
                Debug.Log(player.hero.name + "购买了权值为" + GetFit(cardPair.Key) + "的" + cardPair.Key.name + "替换了权值为" + GetFit(CardBuilder.GetCard(tmpID)) + "的" + CardBuilder.GetCard(tmpID).name);
            }
        }
    }

    public void RandomlyTransformMinionsToGold()
    {
        if (player.leftCoins >= Const.coinCostToBuyMinion * 2 - 1 && random.Next(10) > 5)
        {           
            List<Card> cards = new List<Card>();
            foreach (Card item in player.battlePile)
            {
               if (!item.isGold)
               {
                    cards.Add(item);
               }
            }
            if (cards.Count != 0)
            {
                Card cardToTransform = cards.GetOneRandomly();
                Debug.Log(player.hero.name + "将" + cardToTransform.name + "三连升级了!");
                cardToTransform.TransformToNewCardWithEffectsForBoss(CardBuilder.GetCard(cardToTransform.goldVersion).NewCard());
                player.leftCoins -= Const.coinCostToBuyMinion * 2 - 1;
            }
        }
    }

    public void RandomlyGiveMinionBuff()
    {
        player.battlePile.Shuffle();
        AllAllyStrenthen();
        foreach (var card in player.battlePile)
        {
            if (card.GetPositionTag() == 0 && !card.HasKeyword(Keyword.Taunt)) //为1号送死随从使用阿古斯添加嘲讽
            {
                if (random.Next(10) > 4)
                {
                    if (player.leftCoins >= Const.coinCostToBuyMinion - 1)
                    {
                        //card.effectsStay.Add(new KeyWordEffect(Keyword.Taunt));
                        //card.effectsStay.Add(new BodyPlusEffect(1, 1));
                        player.leftCoins -= Const.coinCostToBuyMinion - 1;
                        Debug.Log(player.hero.name + "使" + card.name + "获得了嘲讽");
                    }
                }
            }
            else if (card.GetPositionTag() == 1) //对于一号进攻位随从进行优先强化,若为机械,添加31磁力
            {
                if (random.Next(10) > 4)
                {
                    if (card.IsMinionType(MinionType.Mechs))
                    {
                        if (player.leftCoins >= Const.coinCostToBuyMinion - 1)
                        {
                            //card.effectsStay.Add(new ProxyEffect(ProxyEnum.Deathrattle, CardBuilder.SearchCardByName("量产型恐吓机").GetProxys(ProxyEnum.Deathrattle)));
                            //card.effectsStay.Add(new BodyPlusEffect(3, 1));
                            player.leftCoins -= Const.coinCostToBuyMinion - 1;
                            Debug.Log(player.hero.name + "将量产型恐吓机磁力了" + card.name);
                        }
                    }
                    if (player.leftCoins >= Const.coinCostToBuyMinion - 1)
                    {
                        //card.effectsStay.Add(new BodyPlusEffect(2, 2));
                        player.leftCoins -= Const.coinCostToBuyMinion - 1;
                        Debug.Log(player.hero.name + "优先强化了进攻随从" + card.name);
                    }
                }
            }
            else if (card.GetPositionTag() == 2) //对于二号常规位机械磁力吵吵模组,如有圣盾,优先强化
            {
                if (random.Next(10) > 3)
                {
                    if (card.IsMinionType(MinionType.Mechs))
                    {
                        if (player.leftCoins >= Const.coinCostToBuyMinion - 1 && !card.HasKeyword(Keyword.DivineShield))
                        {
                            //card.effectsStay.Add(new KeyWordEffect(Keyword.DivineShield));
                            //card.effectsStay.Add(new KeyWordEffect(Keyword.Taunt));
                            //card.effectsStay.Add(new BodyPlusEffect(2, 4));
                            player.leftCoins -= Const.coinCostToBuyMinion - 1;
                            Debug.Log(player.hero.name + "将吵吵模组磁力了" + card.name);
                        }
                    }
                    else if (card.HasKeyword(Keyword.DivineShield))
                    {
                        if (player.leftCoins >= Const.coinCostToBuyMinion - 1)
                        {
                            //card.effectsStay.Add(new BodyPlusEffect(2, 2));
                            player.leftCoins -= Const.coinCostToBuyMinion - 1;
                            Debug.Log(player.hero.name + "优先强化了圣盾随从" + card.name);
                        }
                    }
                }
            }
            if (card.IsMinionType(MinionType.Murlocs)) //对于鱼人,添加剧毒
            {
                if (random.Next(15) == 5)
                {
                    if (player.leftCoins >= Const.coinCostToBuyMinion - 1 && !card.HasKeyword(Keyword.Poisonous))
                    {
                        //card.effectsStay.Add(new KeyWordEffect(Keyword.Poisonous));
                        player.leftCoins -= Const.coinCostToBuyMinion - 1;
                        Debug.Log(player.hero.name + "使" + card.name + "获得了剧毒");
                    }
                }
            }
        }
        do
        {
            if (random.Next(10) > 5) //使用剩余费用随机刷新并购买普通的+2/+2战吼随从并卖出
            {
                if (player.leftCoins >= Const.coinCostToBuyMinion - 1)
                {
                    Card card = player.battlePile.GetOneRandomly();
                    //card.effectsStay.Add(new BodyPlusEffect(2, 2));
                    player.leftCoins -= Const.coinCostToBuyMinion - 1;
                    Debug.Log(player.hero.name + "使用剩余费用强化了" + card.name);
                }
            }
            else
            {
                player.leftCoins -= Const.InitialFlushCost;
            }
        }
        while (player.leftCoins > 0);
    }

    public void AllAllyStrenthen()//为体现流派集群特色,为群体buff特殊编码
    {
        if (player.star > 5 && ((minionTypes.ContainsKey(MinionType.Murlocs) && minionTypes[MinionType.Murlocs] > 0)))
        {
            if (random.Next(10) > 7)
            {
                if (player.leftCoins >= Const.coinCostToBuyMinion - 1)
                {
                    foreach (Card ally in player.GetAllAllyMinion())
                    {
                        if (ally.IsMinionType(MinionType.Murlocs))
                        {
                            player.board.EvolveFunc(ally);
                        }
                    }
                    player.leftCoins -= Const.coinCostToBuyMinion - 1;
                    Debug.Log(player.hero.name + "使用巨壳龙群体进化了鱼人");
                }
            }
        }

        if (player.star > 2 && (minionTypes.ContainsKey(MinionType.Murlocs) && minionTypes[MinionType.Murlocs] > 2))
        {
            if (random.Next(10) > 7)
            {
                if (player.leftCoins >= Const.coinCostToBuyMinion - 1)
                {
                    foreach (Card ally in player.GetAllAllyMinion())
                    {
                        if (ally.IsMinionType(MinionType.Murlocs))
                        {
                            //ally.effectsStay.Add(new BodyPlusEffect(2, 2));
                        }
                    }
                    player.leftCoins -= Const.coinCostToBuyMinion - 1;
                    Debug.Log(player.hero.name + "群体强化鱼人随从");
                }
            }
        }
        
        if ((minionTypes.ContainsKey(MinionType.Demons) && minionTypes[MinionType.Demons] > 2))
        {
            if (random.Next(10) > 6)
            {
                if (player.leftCoins >= Const.coinCostToBuyMinion - 1)
                {
                    Card targetCard = CardBuilder.SearchCardByName("小鬼囚徒");
                    foreach (Card ally in player.GetAllAllyMinion())
                    {
                        if (ally.IsMinionType(MinionType.Demons))
                        {
                            //ally.effectsStay.Add(new ProxyEffect(ProxyEnum.Deathrattle, targetCard.GetProxys(ProxyEnum.Deathrattle)));
                        }
                    }
                    player.leftCoins -= Const.coinCostToBuyMinion - 1;
                    Debug.Log(player.hero.name + "使用锈誓信徒群体强化了恶魔");
                }
            }
        }

        if ((minionTypes.ContainsKey(MinionType.Mechs) && minionTypes[MinionType.Mechs] > 1))
        {
            if (random.Next(10) > 6)
            {
                if (player.leftCoins >= Const.coinCostToBuyMinion - 1)
                {
                    foreach (Card ally in player.GetAllAllyMinion())
                    {
                        if (ally.IsMinionType(MinionType.Mechs))
                        {
                            //ally.effectsStay.Add(new BodyPlusEffect(2, 0));
                        }
                    }
                    player.leftCoins -= Const.coinCostToBuyMinion - 1;
                    Debug.Log(player.hero.name + "使用金刚刃牙兽群体强化机械随从");
                }
            }
        }
    }

    public void SetPosition()
    {
        BattlePile<Card> newCards = new BattlePile<Card>();
        List<Card>[] cardLists = new List<Card>[5] { new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>() };
        player.battlePile.Shuffle();
        foreach (Card card in player.battlePile)
        {
            for (int i = 0; i < 5; i++)
            {
                if (card != null)
                {
                    if (card.GetPositionTag().Equals(i))
                    {
                        cardLists[i].Add(card);
                    }
                }
            }
        }
        foreach (var cardList in cardLists)
        {
            if (cardList != null && cardList.Count > 0)
            {
                foreach (var card in cardList)
                {
                    newCards.Add(card);
                }
            }
        }

        player.battlePile = newCards;
    }

    public void RandomlyBuyMinions()
    {
        if (player.GetNumOfMinion() < Const.numOfBattlePile)
        {
            if (player.leftCoins >= Const.coinCostToBuyMinion)
            {
                Card card = BuyCard();
                if (card != null)
                {
                    player.AddMinionToBattlePile(card, 0);
                    cardPile.ReduceCard(card, 1);
                    player.leftCoins -= Const.coinCostToBuyMinion;
                    Debug.Log(player.hero.name + "购买了权值为" + GetFit(card) + "的" + card.name);
                    RandomlyBuyMinions();
                }
            }
        }
    }

    public Card BuyCard()
    {
        if (cardPile.cardPile.FilterKey(card => card.star <= player.star).Count > 0)
        {
            Card tmpCard = cardPile.RandomlyGetBestCard(card => (
                card.star <= player.star
                && !IsRecruitFieldEffect(card)
                && CheckDuplicate(card)
                && CheckTransform(-1, card.GetPositionTag())) ? GetFit(card) : -1);
            if (tmpCard != null)
            {
                return tmpCard.NewCard();
            }
            else
            {
                return cardPile.RandomlyGetCardByFilterAndReduceIt(card => (
                card.star <= player.star
                && !IsRecruitFieldEffect(card)
                && CheckDuplicate(card)
                && CheckTransform(-1, card.GetPositionTag()))).NewCard();
            }
        }
        else
        {
            return null;
        }
    }

    public void Upgrade()
    {

        if (player.star < 6 && turnNum == turnOfUpgrade[player.star - 1])
        {
            player.star += 1;
            player.leftCoins -= player.upgradeCost;
            if (player.star == 6)
            {
                player.upgradeCost = -1;
            }
            else
            {
                player.upgradeCost = Const.upgradeCosts[player.star - 1];
            }
            Debug.Log(player.hero.name + "升级了酒馆");
        }
    }

    public void UpgradeCheck()
    {
        if (player.star < 6 && player.leftCoins >= player.upgradeCost && random.Next(10) > 5)
        {
            player.star += 1;
            player.leftCoins -= player.upgradeCost;
            if (player.star == 6)
            {
                player.upgradeCost = -1;
            }
            else
            {
                player.upgradeCost = Const.upgradeCosts[player.star - 1];
            }
            Debug.Log(player.hero.name + "升级了酒馆");
        }
    }

    public void NewTurn()
    {
        turnNum++;
        if (player.upgradeCost > 0)
        {
            player.upgradeCost -= 1;
        }
        if (player.maxCoins < Const.MaxCoin)
        {
            player.maxCoins++;
        }
        player.leftCoins = player.maxCoins;
        Strenthen();
    }

    public Player GetPlayerForBattle()
    {
        NewTurn();
        return (Player)player.Clone();
    }

    public Map<int,int>  GetPositionMap()
    {
        Map<int, int> positionMap = new Map<int, int>() {};
        foreach(Card card in player.battlePile)
        {
            if (positionMap.ContainsKey(card.GetPositionTag()))
            {
                positionMap[card.GetPositionTag()]++;
            }
            else
            {
                positionMap.Add(card.GetPositionTag(), 1);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            if (!positionMap.ContainsKey(i))
            {
                positionMap.Add(i, 0);
            }
        }        
        return positionMap;
    }

    public bool CheckDuplicate(Card card)
    {
        int tmp = 0;
        foreach(Card item in player.battlePile)
        {
            if (item.name == card.name)
            {
                tmp++;
            }
        }
        if (tmp >= 2 || card.tag.Contains("唯一") && tmp >= 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool CheckTransform(int origin, int target)
    {
        Map<int, int> map = GetPositionMap();
        if (map.ContainsKey(origin))
        {
            map[origin]--;
        }
        if (map.ContainsKey(target))
        {
            map[target]++;
        }
        else
        {
            map.Add(target, 1);
        }
        if (CheckMap(map))
        {
            return true;
        }
        return false;
    }

    public bool CheckMap(Map<int, int> map)
    {
        if (map[0] + map[1] <= 3)
        {
            if (map[3] + map[4] <= 3)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckFit(Card target, Card origin)
    {
        return GetFit(origin) < GetFit(target);               
    }

    public int GetFit(Card card)
    {
        Card tmpCard = CardBuilder.GetCard(card.id);
        if (fitMap.ContainsKey(tmpCard))
        {
            return fitMap[tmpCard];
        }
        else
        {
            return -10;
        }
    }

    public int CalculateFit(Card card)
    {
        int fit = 0;
        foreach (string item1 in card.tag)
        {
            foreach (var item2 in tags)
            {
                if (item1.Equals(item2.Key))
                {
                    fit += item2.Value;
                }
            }
        }
        foreach (var item in minionTypes)
        {
            if (card.IsMinionType(item.Key))
            {
                fit += item.Value;
            }
        }
        foreach (var item in keywords)
        {
            if (card.HasKeyword(item.Key))
            {
                fit += item.Value;
            }
        }
        foreach (var item in proxys)
        {
            if (card.GetProxys(item.Key)!= null)
            {
                fit += item.Value;
            }
        }
        if (card.HasKeyword(Keyword.Magnetic) && card.star >= 4)
        {
            fit--;
        }
        if (card.isGold)
        {
            fit++;
        }
        fit += card.star/2;
        return fit;
    }

    public bool IsRecruitFieldEffect(Card card)
    {
        if (!card.tag.Contains("战吼") && !card.tag.Contains("手牌") && !card.tag.Contains("特殊"))
        {            
            if (card.GetProxys(ProxyEnum.Battlecry) == null)
            {
                if (card.GetProxys(ProxyEnum.AfterBoughtMinion) == null)
                {
                    if (card.GetProxys(ProxyEnum.AfterHeroHurt) == null)
                    {
                        if (card.GetProxys(ProxyEnum.AfterMinionPlayed) == null)
                        {
                            if (card.GetProxys(ProxyEnum.AfterMinionSold) == null)
                            {
                                if (card.GetProxys(ProxyEnum.AfterUpgrade) == null)
                                {
                                    if (card.GetProxys(ProxyEnum.TurnEnd) == null)
                                    {
                                        if (card.GetProxys(ProxyEnum.TurnEndInHand) == null)
                                        {
                                            if (card.GetProxys(ProxyEnum.TurnStart) == null)
                                            {
                                                if (card.GetProxys(ProxyEnum.WhenMinionPlayed) == null)
                                                {
                                                    return false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return true;
    }
}
