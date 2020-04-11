// 宝藏

using System.Collections.Generic;
using System.Linq;

public partial class CardLongKeywordAchievement
{

    /// <summary>
    /// 夺旗
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你的随从获得+1/+1")]
    public static bool AllyGain11(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            {
                ally.effects.Add(new BodyPlusEffect(1, 1));
            }
        }
        return true;
    }

    /// <summary>
    /// 活化水晶
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("铸币上限增加1点")]
    public static bool AddMainCoin(GameEvent gameEvent)
    {
        gameEvent.player.maxCoins++;
        gameEvent.player.leftCoins = gameEvent.player.maxCoins;
        return true;
    }

    /// <summary>
    /// 活力药水
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("生命值上限翻倍")]
    public static bool DoubleYourHeroHealth(GameEvent gameEvent)
    {
        gameEvent.player.hero.effectsStay.Add(new BodyPlusEffect(0,gameEvent.player.hero.GetMinionBody().y));
        return true;
    }

    /// <summary>
    /// 狂野生长
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你的所有随从随机进化")]
    public static bool EvolveAllAlly(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            gameEvent.player.board.Evolve(new GameEvent()
            {
                targetCard = ally,
                player = gameEvent.player
            });
        }
        return true;
    }

    /// <summary>
    /// 凯尔萨斯的宝珠
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("每当你在一回合中购买三个随从,其中的第三个不消耗铸币")]
    public static bool EveryThirdMinionYouBoughtCost0(GameEvent gameEvent)
    {
        if (gameEvent.player.board.numOfMinionsBought%3 == 2)
        {
            Const.coinCostToBuyMinion = 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 高级调酒师
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你下一次升级酒馆的铸币消耗减少(2)点")]
    public static bool Reduce2CostOfUpgrade(GameEvent gameEvent)
    {
        if (gameEvent.player.star <= 5)
        {
            gameEvent.player.upgradeCost -= 2;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 高级酒馆
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你的酒馆之后每次出售七个随从")]
    public static bool Always7MinionsOnSale(GameEvent gameEvent)
    {
        Const.numOfMinionsOnSale = new int[6] { 7, 7, 7, 7, 7, 7 };
        return true;
    }

    /// <summary>
    /// 馆长的收藏品
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("召唤两个1/1的融合怪")]
    public static bool Summon2FusionMoster(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("融合怪");
        if (targetCard != null)
        {
            gameEvent.player.AddMinionToBattlePile(targetCard.NewCard(), 0);
            gameEvent.player.AddMinionToBattlePile(targetCard.NewCard(), 0);
        }
        return true;
    }

    /// <summary>
    /// 魔术表演
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使酒馆中的所有随从获得+2/+2")]
    public static bool OtherAllyOnSaleGain11Or22(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.board.GetAnotherPlayer(gameEvent.player).GetAllAllyMinion())
        {
            ally.effectsStay.Add(new BodyPlusEffect(2, 2));
        }
        return true;
    }

    /// <summary>
    /// 人才招募计划
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将一张星级不大于你的酒馆等级的随从牌置入你的手牌")]
    public static bool CreateRandomMinion(GameEvent gameEvent)
    {
        var targetCard = gameEvent.player.board.cardPile.RandomlyGetCardByFilterAndReduceIt(Card => Card.star <= gameEvent.player.star);
        if (targetCard != null)
        {
            gameEvent.player.AddMinionToHandPile(targetCard.NewCard());
        }

        return true;
    }

    /// <summary>
    /// 超级加倍
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("如果升级至3或5等级,发现一张宝藏(已废弃)")]
    public static bool DiscoverAnotherTreasure(GameEvent gameEvent)
    {
        if (gameEvent.player.star == 3 || gameEvent.player.star == 5)
        {
            //gameEvent.player.board.DiscoverTreasure();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 皮糙肉厚
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("酒馆中,你的英雄具有免疫")]    
    public static bool AllyHeroGainImmuneInRecruit(GameEvent gameEvent)
    {
        if (!gameEvent.player.board.isBattleField)
        {
            gameEvent.player.hero.effects.Add(new KeyWordEffect(Keyword.Immune));
        }
        return true;
    }

    /// <summary>
    /// 集结号角
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("在酒馆中添加一个同种族的随从并使其+1/+1")]
    public static bool AddNewMinionAndGain11(GameEvent gameEvent)
    {
        BattlePile<Card> battlePile = gameEvent.player.board.players[1].battlePile;
        if (battlePile.Count < Const.numOfBattlePile)
        {
            Card newCard =
            (Card)gameEvent.player.board.cardPile.RandomlyGetCardByFilterAndReduceIt(card => card.star <= gameEvent.player.star &&
            card.IsMinionType(gameEvent.targetCard.type)).NewCard();
            newCard.effectsStay.Add(new BodyPlusEffect(1, 1));
            battlePile[battlePile.GetEmptyPos()] = newCard;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 一袋铸币
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将5枚铸币置入你的手牌")]
    public static bool FullYourHandWithCoins(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("铸币");
        for (int i = 0; i < 5; i++)
        {
            gameEvent.player.AddMinionToHandPile(targetCard.NewCard());
        }
        return true;
    }

    /// <summary>
    /// 商店兑换劵
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将酒馆中所有的随从置入你的手牌")]
    public static bool AddAllMinionOnSaleToYouHand(GameEvent gameEvent)
    {
        BattlePile<Card> cards = gameEvent.player.board.players[1].battlePile;
        if (cards.Count != 0)
        {
            foreach (Card card in cards)
            {
                if (card != null && gameEvent.player.handPile.Count < Const.numOfHandPile)
                {
                    gameEvent.player.board.AddToHandPile(card);
                    gameEvent.player.board.players[1].RemoveMinionFromBattlePile(card);
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 荆棘外壳
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使你的英雄获得+3攻击力")]
    public static bool YourHeroGain3Attack(GameEvent gameEvent)
    {
        gameEvent.player.hero.effectsStay.Add(new BodyPlusEffect(3, 0));
        return true;
    }

    /// <summary>
    /// 阴
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("本回合中你购买下一个随从的铸币消耗减少(1)点,至多减少(1)点")]
    public static bool YourNextCostOfBuyMinionReduce1(GameEvent gameEvent)
    {
        if (Const.coinCostToBuyMinion > 0)
        {
            Const.coinCostToBuyMinion = Const.InitialCoinCostToBuyMinion - 1;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 阳
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("本回合中你出售下一个随从的获得的铸币增加(1)点,至多增加(1)点")]
    public static bool YourNextCoinGainOfSellMinionAdd1(GameEvent gameEvent)
    {
        Const.coinGetBySellMinion = Const.InitialCoinGetBySellMinion + 1;
        return true;
    }

    /// <summary>
    /// 起源图腾
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("使一个随机友方随从获得+1/+1")]
    public static bool GiveRandomAlly11(GameEvent gameEvent)
    {
        if (gameEvent.player.GetAllAllyMinion().Count > 0)
        {
            gameEvent.player.battlePile.GetOneRandomly().effectsStay.Add(new BodyPlusEffect(1, 1));
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 三个愿望
    /// </summary>
    /// <returns></returns>
    [CommonDescription("实现你三个愿望（当时机成熟时）")]
    [HidePrompt]
    public static bool GetAWish(GameEvent gameEvent)
    {
        if (gameEvent.player.star >= 4)
        {
            gameEvent.player.AddMinionToHandPile(CardBuilder.SearchCardByName("一个愿望").NewCard());
        }
        return true;
    }


    /// <summary>
    /// 一个愿望
    /// </summary>
    /// <returns></returns>
    [CommonDescription("发现一张你想要的卡")]
    public static bool GetAWonderfulCard(GameEvent gameEvent)
    {
        List<Card> cards = new List<Card>();

        // 即将三连的牌
        cards.AddRange(GetPlayerCardsReadyForMerge(gameEvent.player));
        // boss 针对牌


        // 套路核心牌
        // 功能牌



        // 打工牌
        cards.Add(GetTheBigestCardInCardPile(gameEvent.player.board.cardPile, gameEvent.player.star));
        cards.Add(CardBuilder.SearchCardByName("月亮巨人"));
        cards.Add(CardBuilder.SearchCardByName("月亮巨人"));
        cards.Add(CardBuilder.SearchCardByName("月亮巨人"));

        gameEvent.player.board.DiscoverToHand(cards.Shuffle().Take(3).Select(card=>card.NewCard()).ToList());

        return true;
    }

    public static List<Card> GetPlayerCardsReadyForMerge(Player player)
    {
        return player.battlePile.ToList().Concat(player.handPile.ToList())
            .GroupBy(card => card.id)
            .Where(g => g.Count() == 2)
            .Select(g => g.FirstOrDefault().id)
            .Select(CardBuilder.GetCard)
            .ToList();
    }

    public static Card GetTheBigestCardInCardPile(CardPile cardPile, int star)
    {
        var big = cardPile.cardPile
            .FilterKey(card => card.star == 3)
            .OrderByDescending(card => (card.attack + card.health))
            .FirstOrDefault();

        if (big == null)
        {
            big = CardBuilder.SearchCardByName("月亮巨人");
        }
        return big;
    }

}
