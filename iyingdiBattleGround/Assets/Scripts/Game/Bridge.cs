using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// 静态桥接类，用于交换消息
/// </summary>
public class Bridge {

    private static UnityEvent GameStartEvent = new UnityEvent();
    private static UnityChangeMessageEvent GameSendMessageEvent = new UnityChangeMessageEvent();
    private static UnityChangeMessageEvent GameReceiveMessageEvent = new UnityChangeMessageEvent();

    private static Server serverInstance = null;
    private static Client clientInstance = null;

    public static Server GetServer()
    {
        if (serverInstance == null) serverInstance = new Server();
        return serverInstance;
    }
    public static Client GetClient()
    {
        if (clientInstance == null) clientInstance = new Client();
        return clientInstance;
    }

    

    // 私有内部类

    public class Server
    {
        public void OnGameStart(UnityAction GameStart)
        {
            GameStartEvent.AddListener(GameStart);
        }
        public void SendMessage(ChangeMessage changeMessage)
        {
            GameSendMessageEvent.Invoke(changeMessage);
        }
        public void OnReceiveMessage(UnityAction<ChangeMessage> ReceiveMessage)
        {
            GameReceiveMessageEvent.AddListener(ReceiveMessage);
        }

       
    }


    public class Client
    {
        public void Start()
        {
            GameStartEvent.Invoke();
        }
        public void SendMessage(ChangeMessage changeMessage)
        {
            GameReceiveMessageEvent.Invoke(changeMessage);
        }
        public void OnReceiveMessage(UnityAction<ChangeMessage> ReceiveMessage)
        {
            GameSendMessageEvent.AddListener(ReceiveMessage);
        }

      
    }
}





[Serializable]
public class UnityChangeMessageEvent : UnityEvent<ChangeMessage> { }



/// <summary>
/// 用作前后端交换信息
/// </summary>
[Serializable]
public class ChangeMessage: ICloneable
{
    public ChangeMessageGameData data = null;
    public ChangeMessageCode code = null;

    public object Clone()
    {
        ChangeMessage changeMessage = MemberwiseClone() as ChangeMessage;
        if (data != null)
            changeMessage.data = data.Clone() as ChangeMessageGameData;
        if (code != null)
            changeMessage.code = code.Clone() as ChangeMessageCode;
        return changeMessage;
    }
}

[Serializable]
public class ChangeMessageGameData : ICloneable
{
    public bool isBattle = false;                   // 战斗棋盘/酒馆棋盘

    public Card hero = null;                        // 我方英雄
    public IPile<Card> heroHandPile = null;         // 我方手牌
    public IPile<Card> heroBattlePile = null;       // 我方战场随从

    public Card opponent = null;                    // 敌方英雄(可以为空)
    public IPile<Card> opponentHandPile = null;     // 对面手牌(可以为空)
    public IPile<Card> opponentBattlePile = null;   // 对面战场随从(可以为空)

    public int upgradeCost = -1;                    // 升星花费 -1表示不能使用
    public int flushCost = -1;                      // 刷新花费
    public int freezeCost = 0;                      // 冻结花费
    public int star = 1;                            // 酒馆星级 1~6

    public int maxCoins = 4;                        // 当前最大铸币数
    public int leftCoins = 4;                       // 剩余铸币数量
    
    //int extendCost = -1;   // 升级人口花费

    public object Clone()
    {
        ChangeMessageGameData changeMessageGameData = MemberwiseClone() as ChangeMessageGameData;
        if (heroHandPile != null)
            changeMessageGameData.heroHandPile = heroHandPile.Copy() as IPile<Card>;
        if (heroBattlePile != null)
            changeMessageGameData.heroBattlePile = heroBattlePile.Copy() as IPile<Card>;
        if (opponentHandPile != null)
            changeMessageGameData.opponentHandPile = opponentHandPile.Copy() as IPile<Card>;
        if (opponentBattlePile != null)
            changeMessageGameData.opponentBattlePile = opponentBattlePile.Copy() as IPile<Card>;
        if (hero != null)
            changeMessageGameData.hero = hero.Clone() as Card;
        if (opponent != null)
            changeMessageGameData.opponent = opponent.Clone() as Card;
        return changeMessageGameData;
    }

}


/// 可能的情况:
///   客户端发送玩家操作
///     code:"buy";         pos1为购买的卡的位置
///     code:"sell";        pos1为出售的卡的位置
///     code:"swap";        pos1与pos2是自己场上的两个位置
///     code:"upgrade"      升星
///     code:"freeze"       冻结
///     code:"flush"        刷新
///     code:"play"         pos1为打出的手牌 pos2为打出位置 pos3为target目标位置(可为空)
///     code:"battleStart"  对战开始
///     code:"choose"       number为发现的卡(0~3)
///     
///   服务端发送对战信息
///     code:"attack"       card1为进攻者的 card2为被攻击的
///     code:"attackReady"  card1为进攻者的 card2为被攻击的
///     code:"trigger"  pos为目标卡的位置 trigger为该卡触发的效果（"亡语" "剧毒" "闪电"）
///     code:"create"   card为生成的卡 pos1为该卡的位置 
///     code:"die"      card为死掉的卡 pos1为该卡的位置 
///     code:"damage"   pos1为目标卡的位置 number为伤害值
///     code:"collectStar" 
///     code:"merge"    card1为生成的卡 card2~card4为进行合并的卡
///     code:"choose"   card1 card2 card3 card4 为进行发现的卡
///     code:selectTarget     List<Card> cardList


[Serializable]
public class ChangeMessageCode: ICloneable
{
    public string code = "";
    public CardPosition pos1 = null;
    public CardPosition pos2 = null;
    public CardPosition pos3 = null;
    public Card card1 = null;
    public Card card2 = null;
    public Card card3 = null;
    public Card card4 = null;
    public List<Card> cardList = null;
    public string trigger;
    public int number;
    public int number2;



    public object Clone()
    {
        ChangeMessageCode changeMessageCode = MemberwiseClone() as ChangeMessageCode;
        if (pos1 != null)
            changeMessageCode.pos1 = pos1.Clone() as CardPosition;
        if (pos2 != null)
            changeMessageCode.pos2 = pos2.Clone() as CardPosition;
        if (pos3 != null)
            changeMessageCode.pos3 = pos3.Clone() as CardPosition;
        if (card1 != null)
            changeMessageCode.card1 = card1.Clone() as Card;
        if (card2 != null)
            changeMessageCode.card2 = card2.Clone() as Card;
        if (card3 != null)
            changeMessageCode.card3 = card3.Clone() as Card;
        if (card4 != null)
            changeMessageCode.card4 = card4.Clone() as Card;
        if (cardList != null)
        {
            changeMessageCode.cardList = cardList.Map(card=>card.Clone() as Card);
        }
        return changeMessageCode;
    }
}

[Serializable]
public class CardPosition: ICloneable
{
    public PositionType type = PositionType.None;
    public int pos = -1;

    public int pileTotal = 10; // 记录手牌总数

    public CardPosition(PositionType type = PositionType.None, int pos=-1)
    {
        this.type = type;
        this.pos = pos;
    }

    public object Clone()
    {
        return Copy();
    }

    public CardPosition Copy()
    {
        CardPosition position = new CardPosition(type, pos);
        return position;
    }
    public override bool Equals(object obj)
    {
        if (obj is CardPosition position)
        {
            return type == position.type && pos == position.pos;
        }
        return false;
    }
    public bool AllEquals(object obj)
    {
        if (obj is CardPosition position)
        {
            return type == position.type && pos == position.pos && pileTotal == position.pileTotal;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return type.GetHashCode()+pos.GetHashCode();
    }
    public override string ToString()
    {
        return "("+ type + "," + pos + "," + pileTotal + ")";
    }
}
public enum PositionType { None, Hero, Opponent, HeroHandPile, OpponentHandPile, HeroBattlePile, OpponentBattlePile, Choose }