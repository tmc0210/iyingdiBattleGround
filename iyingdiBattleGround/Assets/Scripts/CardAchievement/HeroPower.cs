using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class CardLongKeywordAchievement
{
    [CommonDescription("你每回合购买的第一个野兽铸币消耗减少(1)点")]
    public static bool BeastHeroPower(GameEvent gameEvent)
    {
        return true;
    }

    [CommonDescription("你每回合购买的第一个鱼人铸币消耗减少(1)点")]
    public static bool MurlocHeroPower(GameEvent gameEvent)
    {
        return true;
    }

    [CommonDescription("使你不具有种族的随从获得+1/+1")]
    public static bool AllyGeneralGain11(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally.IsMinionType(MinionType.General))
            {
                ally.effectsStay.Add(new BodyPlusEffect(1, 1));
            }
        }
        return true;
    }

    [CommonDescription("你的机械具有+{0}攻击力")]
    [SetCounter(1)]
    public static bool PlusAllyMech2Attack(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            if (ally == gameEvent.hostCard) continue;
            if (ally.IsMinionType(MinionType.Mechs))
            {
                ally.effects.Add(new BodyPlusEffect(gameEvent.thisEffect.Counter, 0));
            }
        }
        return true;
    }

    [CommonDescription("如果是恶魔,对一个随机敌方随从造成{0}点伤害")]
    [SetCounter(1)]
    public static bool Deal2DamageToRandomEnemyMinionWhenDemonDie(GameEvent gameEvent)
    {
        if (gameEvent.player != gameEvent.player.board.GetPlayer(gameEvent.targetCard)) return false;
        if (!gameEvent.targetCard.IsMinionType(MinionType.Demons)) return false;

        Card targetCard = gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
        if (targetCard != null)
        {
            gameEvent.player.board.DealDamageToMinion(new GameEvent()
            {
                hostCard = gameEvent.hostCard,
                targetCard = targetCard,
                player = gameEvent.player,
                number = gameEvent.thisEffect.Counter
            });
        }
        return true;
    }

    [CommonDescription("第一个随从始终具有嘲讽和+{0}攻击力")]
    [SetCounter(1)]
    public static bool GiveFirstMinionTaunt(GameEvent gameEvent)
    {
        if (gameEvent.player.GetAllAllyMinion().Count > 0)
        {
            Card targetCard = gameEvent.player.GetAllAllyMinion()[0];
            if (targetCard != null)
            {
                targetCard.effects.Add(new KeyWordEffect(Keyword.Taunt));
                targetCard.effects.Add(new BodyPlusEffect(gameEvent.thisEffect.Counter, 0));
            }
        }
        return true;
    }

    /// <summary>
    /// 馆长
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你的随从具有所有种族")]
    public static bool AllyGainAllMinionType(GameEvent gameEvent)
    {
        foreach (Card ally in gameEvent.player.GetAllAllyMinion())
        {
            ally.type = MinionType.Any;
        }
        foreach (Card ally in gameEvent.player.handPile)
        {
            if (ally.cardType == CardType.Minion)
            {
                ally.type = MinionType.Any;
            }
        }
        return true;
    }

    /// <summary>
    /// 艾德温·范克里夫
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("在本回合中，你每购买过一个随从，使一个随从获得+1/+1")]
    public static bool Give11ForEachMinionBoughtThisTurnToSelectedMinion(GameEvent gameEvent)
    {
        Card card = gameEvent.player.board.ChooseTarget
            (
                gameEvent.player.board.GetMinionTargetLambda(gameEvent.player, MinionType.Any, null)
            );

        if (card != null && card.cardType.Equals(CardType.Minion))
        {
            int value = gameEvent.player.board.numOfMinionsBought;
            card.effectsStay.Add(new BodyPlusEffect(value, value));
            return true;
        }

        return false;
    }

    [CommonDescription("造成{0}点伤害,随机分配给敌方随从（每当升级酒馆时升级!）")]
    [SetCounter(1)]
    public static bool RandomDealDamageByConter(GameEvent gameEvent)
    {
        for (int i = 0; i < gameEvent.thisEffect.Counter; i++)
        {
            Card targetCard = gameEvent.player.board.GetAnotherPlayer(gameEvent.player).RandomlyGetAliveMinion();
            if (targetCard != null)
            {
                gameEvent.player.board.DealDamageToMinion(new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = targetCard,
                    player = gameEvent.player,
                    number = 1
                });
            }
        }
        return true;
    }

    [HidePrompt]
    public static bool UpdateCounter0(GameEvent gameEvent)
    {
        gameEvent.thisEffect.Counter += 1;
        return true;
    }



    /// <summary>
    /// 贸易大王加里维克斯
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("你每有一个剩余的铸币,便将一个铸币置入你的手牌")]
    public static bool GainCoinsForEachLeftCois(GameEvent gameEvent)
    {
        Card targetCard = CardBuilder.SearchCardByName("铸币");
        for (int i = 0; i < gameEvent.player.leftCoins; i++)
        {
            gameEvent.player.AddMinionToHandPile(targetCard.NewCard());
        }
        gameEvent.player.leftCoins = 0;
        return true;
    }

    /// <summary>
    /// 雷诺杰克逊
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将一个非金色的友方随从变为金色,每局游戏只能使用一次")]
    public static bool TransformAnAllyToGold(GameEvent gameEvent)
    {
        return false;
    }

    /// <summary>
    /// 砰砰博士
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("升级你的酒馆!(每场对战限一次)")]
    public static bool UpdateYourInn(GameEvent gameEvent)
    {
        if (gameEvent.player.star < 6)
        {
            gameEvent.player.board.UpgradeFunc();
            gameEvent.player.hero.cost = -3;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 砰砰博士
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [HidePrompt]
    public static bool BanHeroPowerWhenUpdateto6(GameEvent gameEvent)
    {
        if (gameEvent.player.star == 6)
        {
            Debug.Log("!!!");
            gameEvent.player.hero.cost = -3;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 虚空之影瓦莉拉
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将一张暗影映像置入你的手牌")]
    public static bool AddAShadowToHand(GameEvent gameEvent)
    {
        var targetCard = CardBuilder.SearchCardByName("暗影映像");
        if (targetCard != null)
        {
            gameEvent.player.AddMinionToHandPile(targetCard.NewCard());
        }

        return true;
    }

    /// <summary>
    /// 暗影映像
    /// </summary>
    [CommonDescription("变成它的复制")]
    [GoldDescription("变成它的复制")]
    public static bool TransformToACopyOfMinion(GameEvent gameEvent)
    {
        Card tmp = CardBuilder.SearchCardByName("暗影映像", gameEvent.hostCard.isGold);
        gameEvent.hostCard.TransformToNewCardWithEffects(gameEvent.targetCard.NewCard());
        gameEvent.hostCard.effectsStay.Add(new ProxyEffect(ProxyEnum.AfterMinionPlayedInHand, tmp.GetProxys(ProxyEnum.AfterMinionPlayedInHand)));
        gameEvent.hostCard.effectsStay.Add(new KeyWordEffect(Keyword.Changing));
        return true;
    }

    /// <summary>
    /// 狗头人国王托瓦格尔
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("刷新你的酒馆")]
    public static bool FlushYourInn(GameEvent gameEvent)
    {
        gameEvent.player.board.FlushFunc();
        return true;
    }

    /// <summary>
    /// 尤格萨隆
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("将酒馆中的一个随机随从置入你的手牌,可以重复使用")]
    public static bool RandomlyAddMinionOnSaleToYouHand(GameEvent gameEvent)
    {
        BattlePile<Card> cards = gameEvent.player.board.players[1].battlePile;
        if (cards.Count != 0)
        {
            Card card = cards.GetOneRandomly();
            if (card != null && gameEvent.player.handPile.Count < Const.numOfHandPile)
            {
                gameEvent.player.board.AddToHandPile(card);
                gameEvent.player.board.players[1].RemoveMinionFromBattlePile(card);
            }
        }
        gameEvent.player.hero.cost = CardBuilder.GetCard(gameEvent.player.hero.id).cost;
        return true;
    }

    /// <summary>
    /// 火车王里诺艾
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("如果你的上一场战斗失败,将一张1/1的雏龙置入你的手牌")]
    public static bool Add11DragonToYouHandIfLost(GameEvent gameEvent)
    {
        var targetCard = CardBuilder.SearchCardByName("雏龙");
        if (gameEvent.player.board.ResultOfTheLastGame.Equals("Player1 Wins") && targetCard != null)
        {
            gameEvent.player.AddMinionToHandPile(targetCard.NewCard());
            return true;
        }
        return false;
    }

    /// <summary>
    /// 观星者露娜
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("如果你的随从星级各不相同，使他们获得+1/+1")]
    public static bool GiveAlly11IfDifferentStar(GameEvent gameEvent)
    {
        var stars = gameEvent.player.GetAllAllyMinion().Select(card => card.star);
        if (stars.Distinct().Count() == stars.Count())
        {
            foreach(Card card in gameEvent.player.GetAllAllyMinion())
            {
                card.effectsStay.Add(new BodyPlusEffect(1, 1));
            }
        }
        return false;
    }

    /// <summary>
    /// 探险家伊莉斯
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    [CommonDescription("发现一个不在牌池中的星级不大于酒馆等级的随从")]
    public static bool DiscoverAMinionNotInCardPile(GameEvent gameEvent)
    {
        gameEvent.player.board.DiscoverToHand(CardBuilder.AllCards.FilterValue(card => card.star > 0 && !card.isToken && !card.isGold && card.star <= gameEvent.player.star && !gameEvent.player.board.cardPile.cardPile.ContainsKey(card))).isToken = true;
        return false;
    }
}
