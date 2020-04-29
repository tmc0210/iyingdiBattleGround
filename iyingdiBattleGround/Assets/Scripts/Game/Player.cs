using OrderedJson.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : ICloneable
{
    public Board board;

    public UnfixedPile<Card> handPile = new UnfixedPile<Card>();
    public BattlePile<Card> battlePile = new BattlePile<Card>(Const.numOfBattlePile);
    public Card hero = CardBuilder.SearchCardByName("英雄").NewCard();

    public List<Card> treasures = new List<Card>(); //宝藏,一小局游戏开始时触发他们的效果

    public int upgradeCost = Const.InitialUpgradeCost;
    public int flushCost = Const.InitialFlushCost;
    public int freezeCost = Const.InitialFreezeCost;
    public int star = Const.InitialStar;
    public int maxCoins = Const.InitialMaxCoins;
    public int leftCoins = Const.InitialLeftCoins;
    
    /// <summary>
    /// 记录战斗中死亡的随从，战斗开始时清空
    /// </summary>
    public List<int> deathMinionCollection = new List<int>();

    /// <summary>
    /// 青玉魔像计数器
    /// </summary>
    public int JadeGloem = 0;

    public Player(Board board)
    {
        this.board = board;
    }
    
    public Player(Board board, Card hero)
    {
        this.board = board;
        this.hero = hero;
    }
    public Player(Card hero)
    {
        this.hero = hero;
    }

    public bool IsContainTreasure(Card card)
    {
        bool flag = false;
        if (card.tag.Contains("宝藏"))
        {
            foreach(var item in treasures)
            {
                if (item.id == card.id)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
        return false;
    }

    public List<Card> GetAllAllyMinionExceptDead()
    {       
        List<Card> cards = new List<Card>();
        foreach (Card card in battlePile.ToList())
        {
            if (!card.isDead)
            {
                cards.Add(card);
            }
        }
        return cards;
    }

    public void GetCoin(int num)
    {
        leftCoins += num;
        leftCoins = Const.MaxCoin < leftCoins ? Const.MaxCoin : leftCoins;
    }

    public List<Card> GetAllAllyMinion()
    {
        return battlePile.ToList();
    }
    
    public List<Card> GetAllAllyMinionWithHealthabove0()
    {
        List<Card> cards = new List<Card>();
        foreach (Card card in battlePile.ToList())
        {
            if (!card.isDead && card.GetMinionBody().y > 0)
            {
                cards.Add(card);
            }
        }
        return cards;
    }
    
    public int GetNumOfMinion()
    {
        return GetAllAllyMinion().Count;
    }
    
    public Card GetMinionByIndex(int index)
    {
        if (index < battlePile.fixedNumber && index >= 0)
            return battlePile[index];
    
        return null;
    }
    
    public bool ContainsMinion(Card card)
    {
        if (GetMinionIndex(card) != -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public int GetMinionIndex(Card card)
    {
        for (int i = 0; i < battlePile.fixedNumber; i++)
        {
            if (battlePile[i] == card)
            {
                return i;
            }
        }
        if (board.players[0] == this && board.mergeCardPosition.ContainsKey(card))
        {
            return board.mergeCardPosition[card];
        }
        return -1;
    }
    
    private System.Random random = new System.Random();
    public Card RandomlyGetAliveMinion()
    {
        List<Card> aliveMinion = new List<Card>();
        for (int i = 0; i < battlePile.fixedNumber; i++)
        {
            if (battlePile[i] == null) continue;
            if (battlePile[i].isDead == true) continue;
            if (battlePile[i].GetMinionBody().y <= 0) continue;
            aliveMinion.Add(battlePile[i]);
        }
        if (aliveMinion.Count != 0)
        {
            return aliveMinion[random.Next(aliveMinion.Count)];
        }
        else
        {            
            return null;
        }
    }
    
    public Card RandomlyGetMinionToAttack() //考虑嘲讽（说不定以后要考虑潜行）
    {
        List<Card> aliveTauntMinion = new List<Card>();
        for (int i = 0; i < battlePile.fixedNumber; i++)
        {
            if (battlePile[i] == null) continue;
            if (battlePile[i].isDead == true) continue;
            if (battlePile[i].GetMinionBody().y <= 0) continue;
            if (battlePile[i].HasKeyword(Keyword.Taunt))
            {
                aliveTauntMinion.Add(battlePile[i]);
            }
        }
        if (aliveTauntMinion.Count != 0)
        {
            return aliveTauntMinion[new System.Random().Next(aliveTauntMinion.Count)];
        }
        else
        {
            return RandomlyGetAliveMinion();
        }
    }
    
    public Card RandomlyGetMinionOfLowestAttack() //扎普 斯里维克 TODO
    {
        //TODO
        return null;
    }
    
    public bool AddMinionToBattlePile(Card card, int position)
    {
        if (card != null)
        {
            return battlePile.AddMinion(card, position);
        }
        else
        {
            return false;
        }
    }
    
    public void RemoveMinionFromBattlePile(Card card)
    {
        for(int i = 0;i < battlePile.fixedNumber; i++)
        {
            if (battlePile[i] == card)
            {
                battlePile[i] = null;
            }
        }
    }
    
    public void AddMinionToHandPile(Card card)
    {
        if (handPile.Count < Const.numOfHandPile && card != null)
        {
            handPile.Add(card);
        }
    }
    
    public void RemoveMinionFromHandPile(Card card)
    {
        handPile.Remove(card);
    }
    
    public object Clone()
    {
        Player player = MemberwiseClone() as Player;
        player.handPile = (UnfixedPile<Card>)this.handPile.Clone();
        player.battlePile = battlePile.Clone() as BattlePile<Card>;
        return player;
    }

    public Player Copy()
    {
        Player player = Clone() as Player;
        player.hero = player.hero.Clone() as Card;
        return player;
    }
}


/// <summary>
/// 用作触发事件时消息的传递
/// </summary>
public class GameEvent: OJContext
{
    public Player player = null;
    /// <summary>
    /// 正在处理事件的卡（例如:招潮者）
    /// </summary>
    public Card hostCard = null;
    /// <summary>
    /// 目标卡/事件发生中心的卡（例如：其他的鱼人）
    /// </summary>
    public Card targetCard = null;
    public int number = 0; //伤害的数值

    #region for context
    /// <summary>
    /// 当前正在执行委托函数的卡
    /// </summary>
    public Card thisEffect = null;
    /// <summary>
    /// foreachCard中指示当前卡牌的变量
    /// </summary>
    public Card Cursor = null;
    /// <summary>
    /// 指示委托的返回值
    /// </summary>
    public bool Trigger = false;
    #endregion
}


/// <summary>
/// 牌堆接口
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPile<T> : ICloneable, ICollection<T>, IEnumerable<T>
{

    List<T> ToList();
    
    IPile<T> Shuffle();
    T this[int i] { get; set; }


    bool Empty();
    
    void Swap(int i, int j);
    
    IPile<T> Copy();

}

/// <summary>
/// 不固定张数的牌堆
/// </summary>
/// <typeparam name="T"></typeparam>
public class UnfixedPile<T> : List<T>, IPile<T>
{
    public List<T> ToList()
    {
        return this;
    }

    public IPile<T> Shuffle()
    {
        int lenght = Count;
        for (int i = 0; i < lenght; i++)
        {
            Swap(i, new System.Random().Next(lenght));
        }
        return this;
    }
    
    public void Swap(int i, int j)
    {
        T tmp = this[i];
        this[i] = this[j];
        this[j] = tmp;
    }
    
    public bool Empty()
    {
        return Count == 0;
    }
    
    public IPile<T> Copy()
    {
        IPile<T> newPile = new UnfixedPile<T>();
        foreach (var item in this)
        {
            if (item is ICloneable card)
            {
                newPile.Add((T)card.Clone());
            }
            else
            {
                newPile.Add(item);
            }
        }
        return newPile;
    }
    
    public object Clone()
    {
        return Copy();
    }
}

/// <summary>
/// 固定张数的牌堆
/// </summary>
/// <typeparam name="T"></typeparam>
public class FixedPile<T> : IPile<T>
{
    public int fixedNumber;
    public T[] values;
    public FixedPile(int n = 7)
    {
        fixedNumber = n;
        values = new T[fixedNumber];
        for (int i = 0; i < fixedNumber; i++)
        {
            this[i] = default;
        }
    }

    public T this[int i] { get { return values[i]; } set { values[i] = value; } }
    
    public int Count
    {
        get
        {
            return ToList().Count;
        }
    }
    
    public bool IsReadOnly => throw new NotImplementedException();
    
    public void Add(T t)
    {
        int pos = GetEmptyPos();
        if (pos < 0)
        {
            throw new Exception("FixedPile[i] 超出界限");
        }
        else
        {
            this[pos] = t;
        }
    }
    
    public void Clear()
    {
        for (int i = 0; i < Count; i++)
        {
            this[i] = default;
        }
    }
    
    public object Clone()
    {
        return Copy();
    }
    
    public bool Contains(T item)
    {
        foreach (T obj in this)
        {
            if (obj.Equals(item)) return true;
        }
        return false;
    }
    
    public IPile<T> Copy()
    {
        FixedPile<T> newPile = new FixedPile<T>(fixedNumber);
        for(int i=0; i<fixedNumber; i++)
        {
            if (this[i] is ICloneable card)
            {
                newPile[i] = (T)card.Clone();
            } else
            {
                newPile[i] = this[i];
            }
        }
        return newPile;
    }

 


    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }


    public bool Empty()
    {
        return ToList().Count == 0;
    }
    
    public int GetEmptyPos()
    {
        for (int i = 0; i < fixedNumber; i++)
        {
            if (this[i] == null) return i;
        }
        return -1;
    }


    public IEnumerator<T> GetEnumerator()
    {
        return ToList().GetEnumerator();
    }
    
    public bool Remove(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (this[i].Equals(item))
            {
                this[i] = default;
                return true;
            }
        }
        return false;
    }

    System.Random random = new System.Random();
    public IPile<T> Shuffle()
    {
        for (int i = 0; i < fixedNumber; i++)
        {
            Swap(i, random.Next(fixedNumber));
        }
        return this;
    }
    public void Swap(int i, int j)
    {
        T tmp = this[i];
        this[i] = this[j];
        this[j] = tmp;
    }
    
    public List<T> ToList()
    {
        List<T> ts = new List<T>();
        for (int i = 0; i < fixedNumber; i++)
        {
            if (this[i] == null) continue;
            ts.Add(this[i]);
        }
        return ts;
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ToList().GetEnumerator();
    }
}

/// <summary>
/// 用于记录战场随从
/// </summary>
/// <typeparam name="T"></typeparam>

public class BattlePile<T> : FixedPile<T>
{
    public new object Clone()
    {
        BattlePile<T> newPile = new BattlePile<T>(fixedNumber);
        for (int i = 0; i < fixedNumber; i++)
        {
            if (this[i] is ICloneable card)
            {
                newPile[i] = ((T)card.Clone());
            }
            else
            {
                newPile[i] = this[i];
            }
        }
        return newPile;

    }
    
    public BattlePile(int n = 7)
    {
        fixedNumber = n;
        values = new T[fixedNumber];
        for (int i = 0; i < fixedNumber; i++)
        {
            this[i] = default;
        }
    }
    
    public bool AddMinion(T t, int position)
    {
        if (position < 0 || position >= fixedNumber)
        {
            throw new Exception("超出界限:" + position);            
        }
        else
        {
            if (this[position] == null)
            {
                this[position] = t;
                return true;
            }
            else if (position + 1 < fixedNumber && this[position + 1] == null)
            {
                this[position + 1] = t;
                return true;
            }
            else if (PushRight(position + 1))
            {
                this[position + 1] = t;
                return true;
            }
            else if (PushLeft(position))
            {
                this[position] = t;
                return true;
            }            
            else
            {
                return false;
            }
        }
    }
    
    public bool PushLeft(int position)
    {
        if (position <= 0 || position >= fixedNumber)
        {
            return false;
        }
        else
        {
            int i;
            for (i = position; i > 0; i--)
            {
                if (MoveLeft(i))
                {
                    break;
                }
            }
            for (int j = i; j <= position; j++)
            {
                MoveLeft(j);
            }
            if (this[position] == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    
    public bool PushRight(int position)
    {
        if (position < 0 || position >= fixedNumber - 1)
        {
            return false;
        }
        else
        {
            int i;
            for (i = position; i < fixedNumber; i++)
            {
                if (MoveRight(i))
                {
                    break;
                }
            }
            for (int j = i; j >= position; j--)
            {
                MoveRight(j);
            }
            if (this[position] == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    
    private bool MoveLeft(int position)
    {
        if (position <= 0)
        {
            return false;
        }
        if (this[position - 1] == null)
        {
            Swap(position, position - 1);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    private bool MoveRight(int position)
    {
        if (position >= fixedNumber - 1)
        {
            return false;
        }
        if (this[position + 1] == null)
        {
            Swap(position, position + 1);
            return true;
        }
        else
        {
            return false;
        }
    }
}