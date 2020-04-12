using BIF;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

/// <summary>
/// <![CDATA[通过创建Board对象，开始一局游戏]]>
/// 通过多线程的形式进行数据交换：
///     接受到消息时，触发<see cref="OnMessage(ChangeMessage)"/>；
///     如果需要卡住线性，等待消息，请使用<see cref="GetMessage"/>
/// </summary>
public class Board
{

    /// 传递信息原则为 player始终为hostCard的player,hostcard始终为函数的主语.

    #region public fileds

    public Map<int, int> CardPlayedCounter = new Map<int, int>();
    public bool isBattleField = true;                              // 对战中还是酒馆中
    public CardPile cardPile = new CardPile();

    public Player player;  // 玩家

    public Player[] players = new Player[2] { null, null }; //对战中双方，分别由玩家深拷贝和自动生成敌人而来,招揽时players [1]为酒馆

    public Player[] playersByOffensive = new Player[2]; //进入对战时赋值，0为先手方

    public int AttackingPlayer = 0; //按照先后手顺序排序

    public Card[] AttackingMinion = new Card[2] { null, null }; //按照先后手顺序排序

    public List<Card>[] tmpCards = new List<Card>[2] { new List<Card>(), new List<Card>() }; //用于对战中的复杂亡语和攻击顺序结算 按照先后手排序

    public BattlePile<Card> freezeCards = new BattlePile<Card>();

    public Enemy enemy;

    public bool isFreeze = false;

    public bool isDiscoverEnd = false;

    public int whichOneChoosed = -1;

    public int numOfMinionsBought = 0;

    public bool GameStartTrigger = false;

    public int level = 1;   //1,2,3为随机关卡(暂时固定),4为固定boss关,暂为馆长

    static System.Random random = new System.Random(unchecked((int)DateTime.Now.Ticks));

    public string ResultOfTheLastGame = "";

    #endregion

    #region thread
    public BlockingCollection<ChangeMessage> changeMessagesToServer = null; // 用于接受来自多线
    public ChangeMessage GetMessage()
    {
        return changeMessagesToServer.Take();
    }
    #endregion

    public Board()
    {
        Debug.Log("new Board");
    }

    public void InitGame()
    {
        Debug.Log("此方法即将被弃用,请修改参数");
        player = new Player(this, CardBuilder.SearchCardByName("雷诺·杰克逊").NewCard());
        //暂时随机生成为第几关
        level = random.Next(4) + 1;
        player.hero.effectsStay.Add(new BodyPlusEffect(0, 10 * (level - 1)));
        players[0] = player;
        players[1] = EnemyManager.GetEnemy();
        cardPile.InitCardPileFully();
        StartRecruit();
    }

    public void InitGame(Player player, Enemy enemy, CardPile cardPile, int level)
    {
        Debug.Log("server init game");
        this.player = player;
        this.level = level;
        players[0] = this.player;
        this.cardPile.InitCardPile(cardPile);
        this.enemy = enemy;
        enemy.player.board = this;
        StartRecruit();
    }
    public void InitGame(BoardInitArgs boardInitArgs)
    {
        //Debug.Log("in " + boardInitArgs.player);
        boardInitArgs.player.board = this;
        boardInitArgs.enemy.player.board = this;
        InitGame(boardInitArgs.player, boardInitArgs.enemy, boardInitArgs.cardPile, boardInitArgs.level);
    }

    public void OnMessage(ChangeMessage changeMessage)
    {
        if (changeMessage.code.code.Equals("buy"))
        {
            Card card = players[1].battlePile[changeMessage.code.pos1.pos];
            Buy(card);
        }
        else if (changeMessage.code.code.Equals("sell"))
        {
            Card card = null;
            if (changeMessage.code.pos1.type == PositionType.HeroBattlePile)
            {
                card = players[0].battlePile[changeMessage.code.pos1.pos];
            }
            else if (changeMessage.code.pos1.type == PositionType.HeroHandPile)
            {
                card = players[0].handPile[changeMessage.code.pos1.pos];
            }
            Sell(card);
        }
        else if (changeMessage.code.code.Equals("swap"))
        {
            Swap(changeMessage.code.pos1, changeMessage.code.pos2);
        }
        else if (changeMessage.code.code.Equals("upgrade"))
        {
            Upgrade();
        }
        else if (changeMessage.code.code.Equals("freeze"))
        {
            Freeze();
        }
        else if (changeMessage.code.code.Equals("flush"))
        {
            Flush();
        }
        else if (changeMessage.code.code.Equals("play"))
        {
            PlayCard(changeMessage.code.pos1, changeMessage.code.pos2);
        }
        else if (changeMessage.code.code.Equals("startCombat"))
        {
            StartCombat();
        }
        else if (changeMessage.code.code.Equals("heroPower"))
        {
            HeroPower();
        }
        else
        {
            throw new Exception(" there is not code: " + changeMessage.code.code);
        }
        AuraCheck();
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
    }

    public void SendGameMessage(ChangeMessage changeMessage)
    {
        SendGameMessageAction.Invoke(changeMessage);
    }

    public Action<ChangeMessage> SendGameMessageAction = null;

    #region combat

    public void RandomlyAttack()
    {
        int AttackNum = 1;
        if (GetAttackingMinion().HasKeyword(Keyword.Windfury))
        {
            AttackNum = 2;
        }
        if (GetAttackingMinion().HasKeyword(Keyword.MegaWindfury))
        {
            AttackNum = 4;
        }
        for (int i = 0; i < AttackNum; i++)
        {
            Card targetCard = GetAnotherPlayer(playersByOffensive[AttackingPlayer]).RandomlyGetMinionToAttack();
            if (GetAttackingMinion() != null && targetCard != null && GetAttackingMinion().GetMinionBody().y > 0 && GetAttackingMinion().GetMinionBody().x > 0)
            {
                Attack(new GameEvent()
                {
                    hostCard = GetAttackingMinion(),
                    targetCard = targetCard,
                    player = playersByOffensive[AttackingPlayer],
                });
            }
        }
    }

    public void DeathPhase()
    {
        AuraCheck();

        List<Card> deadMinions = new List<Card>();
        List<GameEvent> gameEvents = new List<GameEvent>();

        foreach (Card card in playersByOffensive[0].GetAllAllyMinion())
        {
            if (card.GetMinionBody().y <= 0)
            {
                card.isDead = true;
            }
        }
        foreach (Card card in playersByOffensive[1].GetAllAllyMinion())
        {
            if (card.GetMinionBody().y <= 0)
            {
                card.isDead = true;
            }
        }

        foreach (Card card in playersByOffensive[0].GetAllAllyMinion())
        {
            if (card.isDead)
            {
                deadMinions.Add(card);
                gameEvents.Add(new GameEvent()
                {
                    hostCard = card,
                    targetCard = null,
                    player = GetPlayer(card),
                });
            }
        }
        foreach (Card card in playersByOffensive[1].GetAllAllyMinion())
        {
            if (card.isDead)
            {
                deadMinions.Add(card);
                gameEvents.Add(new GameEvent()
                {
                    hostCard = card,
                    targetCard = null,
                    player = GetPlayer(card),
                });
            }
        }

        GameOverJudge();

        //Remove DeadMinion
        foreach (Card card in deadMinions)
        {
            CardPosition cardPosition = GetCardPosition(card);
            if (GetPlayer(card) == playersByOffensive[0])
            {
                tmpCards[0].Insert(tmpCards[0].IndexOf(card) + 1, CardBuilder.NewCard(0));
            }
            else
            {
                tmpCards[1].Insert(tmpCards[1].IndexOf(card) + 1, CardBuilder.NewCard(0));
            }
            GetPlayer(card).RemoveMinionFromBattlePile(card);
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "die",
                    card1 = card,
                    pos1 = cardPosition
                },
            });
            if (card.GetProxys(ProxyEnum.Deathrattle) != null)
            {
                SendGameMessage(new ChangeMessage()
                {
                    data = GetDataForSend(),
                    code = new ChangeMessageCode()
                    {
                        code = "trigger",
                        trigger = "亡语",
                        pos1 = cardPosition
                    },
                });
            }
        }

        // 记录死掉的随从
        foreach (var card in deadMinions)
        {
            GetPlayer(card).deathMinionCollection.Add(card.id);
        }

        //循环
        if (deadMinions.ToArray().Length != 0)
        {
            #region reborn

            List<GameEvent> gameEventsReborn = new List<GameEvent>();

            foreach (var card in deadMinions)
            {
                if (card.HasKeyword(Keyword.Reborn))
                {
                    Card rebornCard = CardBuilder.NewCard(card.id);
                    rebornCard.health = 1;
                    rebornCard.RemoveKeyWord(Keyword.Reborn);
                    SummonMinionByReborn(new GameEvent()
                    {
                        hostCard = card,
                        targetCard = rebornCard,
                        player = GetPlayer(card),
                    });
                    gameEventsReborn.Add(new GameEvent()
                    {
                        hostCard = rebornCard,
                        player = GetPlayer(card),
                    });
                }
            }
            foreach (var gameEvent in gameEventsReborn)
            {
                AfterReborn(gameEvent);
            }

            #endregion

            foreach (GameEvent gameEvent in gameEvents)
            {
                AfterMinionDeath(gameEvent);
            }

            deadMinions = new List<Card>();
            DeathPhase();
        }
    }

    public void NextRun()
    {
        GameOverJudge();
        AttackingPlayer = (AttackingPlayer + 1) % 2;
        int i = 0;
        if (AttackingMinion[AttackingPlayer] == null)
        {
            AttackingMinion[AttackingPlayer] = GetFirstAttackMinion(AttackingPlayer);
        }
        else
        {
            int index = tmpCards[AttackingPlayer].IndexOf(AttackingMinion[AttackingPlayer]);
            do
            {
                i++;
                index = (index + 1) % tmpCards[AttackingPlayer].Count;
                AttackingMinion[AttackingPlayer] = tmpCards[AttackingPlayer][index];
            }
            while ((AttackingMinion[AttackingPlayer] == null | AttackingMinion[AttackingPlayer].id == 0 | AttackingMinion[AttackingPlayer].isDead | AttackingMinion[AttackingPlayer].GetMinionBody().x <= 0) && i < 20);
        }
    }

    public Card GetAttackingMinion()
    {
        return AttackingMinion[AttackingPlayer];
    }

    public void Attack(GameEvent gameEvent)
    {
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend(),
            code = new ChangeMessageCode()
            {
                code = "attackReady",
                card1 = gameEvent.hostCard,
                card2 = gameEvent.targetCard,
            },
        });

        if (gameEvent.hostCard.HasKeyword(Keyword.Stealth))
        {
            gameEvent.hostCard.HasKeyword(Keyword.Stealth);
            //Maybe some messages
        }

        WhenMinionAttack(gameEvent);

        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend(),
            code = new ChangeMessageCode()
            {
                code = "attack",
                card1 = gameEvent.hostCard,
                card2 = gameEvent.targetCard,
            },
        });

        //Debug.Log(gameEvent.hostCard.name + "attack" + gameEvent.targetCard.name);

        GameEvent HostHurtTargetGameEvent = new GameEvent()
        {
            hostCard = gameEvent.hostCard,
            targetCard = gameEvent.targetCard,
            player = gameEvent.player,
            number = gameEvent.hostCard.GetMinionBody().x
        };

        GameEvent TargetHurtHostGameEvent = new GameEvent()
        {
            hostCard = gameEvent.targetCard,
            targetCard = gameEvent.hostCard,
            player = GetPlayer(gameEvent.targetCard),
            number = gameEvent.targetCard.GetMinionBody().x
        };
        List<GameEvent> gameEventList = new List<GameEvent>();

        if (HurtMinion(HostHurtTargetGameEvent))
        {
            gameEventList.Add(new GameEvent()
            {
                hostCard = HostHurtTargetGameEvent.targetCard,
                targetCard = HostHurtTargetGameEvent.hostCard,
                player = GetPlayer(HostHurtTargetGameEvent.targetCard),
            });
        };

        if (gameEvent.hostCard.HasKeyword(Keyword.Cleave))
        {
            Tuple<Card, Card> tuple = GetAdjacentMinion(gameEvent.targetCard);
            Card left = tuple.Item1;
            Card right = tuple.Item2;
            if (left != null)
            {
                SendGameMessage(new ChangeMessage()
                {
                    data = GetDataForSend(),
                    code = new ChangeMessageCode()
                    {
                        code = "cleave",
                        card1 = left,
                    }
                });
                GameEvent HurtLeftGameEvent = new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = tuple.Item1,
                    player = gameEvent.player,
                    number = gameEvent.hostCard.GetMinionBody().x
                };

                if (HurtMinion(HurtLeftGameEvent))
                {
                    gameEventList.Add(new GameEvent()
                    {
                        hostCard = HurtLeftGameEvent.targetCard,
                        targetCard = HurtLeftGameEvent.hostCard,
                        player = GetPlayer(HurtLeftGameEvent.targetCard),
                    });
                };
            }
            if (right != null)
            {
                SendGameMessage(new ChangeMessage()
                {
                    data = GetDataForSend(),
                    code = new ChangeMessageCode()
                    {
                        code = "cleave",
                        card1 = right,
                    }
                });
                GameEvent HurtRightGameEvent = new GameEvent()
                {
                    hostCard = gameEvent.hostCard,
                    targetCard = tuple.Item2,
                    player = GetPlayer(gameEvent.targetCard),
                    number = gameEvent.hostCard.GetMinionBody().x
                };

                if (HurtMinion(HurtRightGameEvent))
                {
                    gameEventList.Add(new GameEvent()
                    {
                        hostCard = HurtRightGameEvent.targetCard,
                        targetCard = HurtRightGameEvent.hostCard,
                        player = GetPlayer(HurtRightGameEvent.targetCard),
                    });
                };
            }
        }

        if (HurtMinion(TargetHurtHostGameEvent))
        {
            gameEventList.Add(new GameEvent()
            {
                hostCard = TargetHurtHostGameEvent.targetCard,
                targetCard = TargetHurtHostGameEvent.hostCard,
                player = GetPlayer(TargetHurtHostGameEvent.targetCard),
            });
        };

        AfterMinionAttack(gameEvent);

        foreach (GameEvent hurtGameEvent in gameEventList)
        {
            AfterMinionHurt(hurtGameEvent);
        }
    }

    public void AOE(List<GameEvent> gameEvents, AOEType type = AOEType.All)
    {

        #region send AOE message
        if (type == AOEType.All)
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "AOE",
                    trigger = "all" // or up,down
                }
            });
        }
        else if (type == AOEType.Up)
        {

            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "AOE",
                    trigger = "up" // or up,down
                }
            });
        }
        else if (type == AOEType.Down)
        {

            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "AOE",
                    trigger = "down" // or up,down
                }
            });
        }
        #endregion

        List<GameEvent> gameEventList = new List<GameEvent>();

        foreach (GameEvent item in gameEvents)
        {
            if (HurtMinion(item))
            {
                gameEventList.Add(new GameEvent()
                {
                    hostCard = item.targetCard,
                    targetCard = item.hostCard,
                    player = GetPlayer(item.targetCard),
                });
            };
        }

        foreach (GameEvent hurtGameEvent in gameEventList)
        {
            AfterMinionHurt(hurtGameEvent);
        }
    }

    /// <summary>
    /// hostCard对targetCard造成伤害
    /// </summary>
    /// <param name="gameEvent"></param>
    public bool HurtMinion(GameEvent gameEvent)
    {
        PreHurt(gameEvent);
        if (gameEvent.number == 0)
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "damage",
                    pos1 = GetCardPosition(gameEvent.targetCard),
                    number = 0
                },
            });
            return false;
        }
        else if (gameEvent.targetCard.HasKeyword(Keyword.DivineShield))
        {
            gameEvent.targetCard.RemoveKeyWord(Keyword.DivineShield);
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "damage",
                    pos1 = GetCardPosition(gameEvent.targetCard),
                    number = 0
                },
            });
            DivineShieldBroken(new GameEvent()
            {
                hostCard = gameEvent.targetCard,
                targetCard = null,
                player = GetPlayer(gameEvent.targetCard),
            });
            return false;
        }
        else
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "damage",
                    pos1 = GetCardPosition(gameEvent.targetCard),
                    number = gameEvent.number
                },
            });

            gameEvent.targetCard.health -= gameEvent.number;

            if (gameEvent.hostCard.HasKeyword(Keyword.Poisonous))
            {
                SendGameMessage(new ChangeMessage()
                {
                    data = GetDataForSend(),
                    code = new ChangeMessageCode()
                    {
                        code = "trigger",
                        trigger = "剧毒",
                        pos1 = GetCardPosition(gameEvent.hostCard)
                    },
                });
                gameEvent.targetCard.isDead = true;
            }

            if (gameEvent.hostCard.GetProxys(ProxyEnum.OverKill) != null)
            {
                if (gameEvent.targetCard.GetMinionBody().y < 0)
                {
                    if (gameEvent.hostCard == AttackingMinion[AttackingPlayer])
                    {
                        gameEvent.hostCard.InvokeProxy(ProxyEnum.OverKill, new GameEvent()
                        {
                            hostCard = gameEvent.hostCard,
                            targetCard = gameEvent.targetCard,
                            player = GetPlayer(gameEvent.hostCard),
                        });
                    }
                }
            }

            return true;
        }
    }

    /// <summary>
    /// hostCard对targetCard造成伤害并立即结算伤害后续事件
    /// </summary>
    /// <param name="gameEvent"></param>
    /// <returns></returns>
    public void DealDamageToMinion(GameEvent gameEvent)
    {
        if (HurtMinion(gameEvent))
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
            });
            AfterMinionHurt(new GameEvent()
            {
                hostCard = gameEvent.targetCard,
                targetCard = gameEvent.hostCard,
                player = GetPlayer(gameEvent.targetCard),
            });
        }
    }

    public void DivineShieldBroken(GameEvent gameEvent)
    {
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend(),
            code = new ChangeMessageCode()
            {
                code = "breakDivineShield",
                card1 = gameEvent.hostCard
            },
        });
        AfterMinionDivineShieldBroken(new GameEvent()
        {
            hostCard = gameEvent.hostCard,
            targetCard = null,
            player = gameEvent.player,
        });
    }

    /// <summary>
    /// hostCard summon targetCard
    /// </summary>
    /// <param name="gameEvent"></param>
    public void SummonMinion(GameEvent gameEvent)
    {
        SummonMinionByMinion(gameEvent, gameEvent.hostCard);
    }
    public void SummonMinionByReborn(GameEvent gameEvent)
    {
        SummonMinionByMinion(gameEvent, gameEvent.hostCard, summonCode: "reborn");
    }

    /// <summary>
    /// hostCard summon targetCard
    /// </summary>
    /// <param name="gameEvent"></param>
    public void SummonMinionByMinion(GameEvent gameEvent, Card byCard, string summonCode = "create")
    {
        MergeCheck(); // 这句一定要放在 WhenMinionSummon之前， 不然卡德加生成的新随从会挤掉
        if (gameEvent.player.GetNumOfMinion() < gameEvent.player.battlePile.fixedNumber)
        {
            SummonMinionByFunc(gameEvent, byCard, summonCode);
            AuraCheck();

            WhenMinionSummon(new GameEvent()
            {
                hostCard = gameEvent.targetCard,
                targetCard = null,
                player = GetPlayer(gameEvent.targetCard),
            });


            AfterMinionSummon(new GameEvent()
            {
                hostCard = gameEvent.targetCard,
                targetCard = null,
                player = GetPlayer(gameEvent.targetCard),
            });

            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
            });
        }
        else
        {
            //Debug.Log("随从位已满");
        }
    }

    public void SummonMinionByFunc(GameEvent gameEvent, Card byCard, string summonCode = "create")
    {
        // set creator
        gameEvent.targetCard.creator = gameEvent.hostCard;
        if (gameEvent.player.GetNumOfMinion() < gameEvent.player.battlePile.fixedNumber)
        {
            if (isBattleField)
            {
                if (gameEvent.player == playersByOffensive[0])
                {
                    int targetIndex = tmpCards[0].IndexOf(byCard) + 1;
                    tmpCards[0].Insert(targetIndex, gameEvent.targetCard);
                    for (int i = targetIndex; i < tmpCards[0].Count; i++)
                    {
                        if (tmpCards[0][i].id == 0)
                        {
                            tmpCards[0].RemoveAt(i);
                            break;
                        }
                    }
                }
                else
                {
                    int targetIndex = tmpCards[1].IndexOf(byCard) + 1;
                    tmpCards[1].Insert(targetIndex, gameEvent.targetCard);
                    for (int i = targetIndex; i < tmpCards[1].Count; i++)
                    {
                        if (tmpCards[1][i].id == 0)
                        {
                            tmpCards[1].RemoveAt(i);
                            break;
                        }
                    }
                }
                players[0].battlePile = new BattlePile<Card>();
                players[1].battlePile = new BattlePile<Card>();
                TempCardsToBattlePile();
                TempCardsCheck();
            }
            else
            {
                gameEvent.player.AddMinionToBattlePile(gameEvent.targetCard, gameEvent.player.GetMinionIndex(byCard));
            }

            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = summonCode,
                    card1 = gameEvent.targetCard,
                    pos1 = GetCardPosition(gameEvent.targetCard)

                },
            });
        }
        else
        {
            Debug.Log("随从位已满");
        }
    }

    public void DealDamageToHero(GameEvent gameEvent)
    {
        if (!gameEvent.player.hero.HasKeyword(Keyword.Immune))
        {
            gameEvent.player.hero.health -= gameEvent.number;
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "damage",
                    pos1 = GetCardPosition(gameEvent.targetCard),
                    number = gameEvent.number
                },
            });
            if (!isBattleField)
            {
                AfterHeroHurt(gameEvent);
                SendGameMessage(new ChangeMessage()
                {
                    data = GetDataForSend(),
                });
            }
        }
        GameOverJudge();
    }

    public void Evolve(GameEvent gameEvent)
    {
        if (EvolveFunc(gameEvent.targetCard))
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
            });
        }
    }

    public bool EvolveFunc(Card card)
    {
        List<Keyword> keywords = new List<Keyword>();
        foreach (var item in Const.EvolveKeyWords)
        {
            if (!card.HasKeyword(item))
            {
                keywords.Add(item);
            }
        }
        if (keywords != null)
        {
            Keyword keyword = keywords.GetOneRandomly();
            card.effectsStay.Add(new KeyWordEffect(keyword));
            return true;
        }
        else
        {
            card.effectsStay.Add(new BodyPlusEffect(1, 1));
            return true;
        }
    }

    public void GameOverJudge()
    {
        if (players[1].hero.GetMinionBody().y <= 0)
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "win",
                },
            });
            Debug.Log("player win");
        }
        else if (player.hero.GetMinionBody().y <= 0)
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "lose",
                },
            });
            Debug.Log("player lose");
        }
    }

    #endregion

    #region triggers

    public void WhenMinionAttack(GameEvent gameEvent)
    {
        List<Card> triggers = new List<Card>();
        triggers.AddRange(playersByOffensive[0].GetAllAllyMinion());
        triggers.AddRange(playersByOffensive[1].GetAllAllyMinion());

        if (gameEvent.hostCard.InvokeProxy(ProxyEnum.WhenAttack, gameEvent))
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "trigger",
                    trigger = "闪电",
                    pos1 = GetCardPosition(gameEvent.hostCard)
                },
            });
        }

        foreach (Card card in triggers)
        {
            if (card.InvokeProxy(ProxyEnum.WhenMinionAttack, new GameEvent()
            {
                hostCard = card,
                targetCard = gameEvent.hostCard,
                player = GetPlayer(card)
            }))
            {
                SendGameMessage(new ChangeMessage()
                {
                    data = GetDataForSend(),
                    code = new ChangeMessageCode()
                    {
                        code = "trigger",
                        trigger = "闪电",
                        pos1 = GetCardPosition(card)
                    },
                });
            }
        }
    }

    public void AfterMinionAttack(GameEvent gameEvent)
    {
        List<Card> triggers = new List<Card>();
        triggers.AddRange(playersByOffensive[0].GetAllAllyMinion());
        triggers.AddRange(playersByOffensive[1].GetAllAllyMinion());

        if (gameEvent.hostCard.InvokeProxy(ProxyEnum.AfterAttack, gameEvent))
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "trigger",
                    trigger = "闪电",
                    pos1 = GetCardPosition(gameEvent.hostCard)
                },
            });
        }
        foreach (Card card in triggers)
        {
            if (card.InvokeProxy(ProxyEnum.AfterMinionAttack, new GameEvent()
            {
                hostCard = card,
                targetCard = gameEvent.hostCard,
                player = GetPlayer(card)
            }))
            {
                SendGameMessage(new ChangeMessage()
                {
                    data = GetDataForSend(),
                    code = new ChangeMessageCode()
                    {
                        code = "trigger",
                        trigger = "闪电",
                        pos1 = GetCardPosition(card)
                    },
                });
            }
        }
    }

    public void AfterMinionDeath(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.AfterMinionDeath);

        Dealthrattle(gameEvent);

        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);
    }

    public void AfterReborn(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.AfterMinionReborn);
        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);
    }

    public void PreHurt(GameEvent gameEvent) //TODO
    {

    }

    public void AfterHeroHurt(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.AfterHeroHurt);
        if (gameEvent.hostCard.InvokeProxy(ProxyEnum.AfterHeroHurt, gameEvent))
            SendGameMessageCode闪电(gameEvent.hostCard);
        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);

    }

    public void AfterMinionHurt(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.AfterMinionHurt);
        if (gameEvent.hostCard.InvokeProxy(ProxyEnum.AfterHurt, gameEvent))
            SendGameMessageCode闪电(gameEvent.hostCard);
        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);
    }

    public void AfterMinionDivineShieldBroken(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.AfterMinionDivineShieldBroken);
        if (gameEvent.hostCard.InvokeProxy(ProxyEnum.AfterDivineShieldBroken, gameEvent))
            SendGameMessageCode闪电(gameEvent.hostCard);
        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);
    }

    public void WhenMinionSummon(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.WhenMinionSummon);
        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);
    }

    public void AfterMinionSummon(GameEvent gameEvent)
    {

        var triggers = CollectTriggers(ProxyEnum.AfterMinionSummon);
        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);

    }

    public void AuraCheck()
    {
        List<Card> triggers = new List<Card>();

        HashSet<Card> cardCollect = new HashSet<Card>();
        HashSet<Card> alivesMinion = new HashSet<Card>();
        cardCollect.UnionWith(playersByOffensive[0].GetAllAllyMinion());
        cardCollect.UnionWith(playersByOffensive[1].GetAllAllyMinion());
        cardCollect.Add(playersByOffensive[0].hero);
        cardCollect.Add(playersByOffensive[1].hero);

        foreach (Card card in cardCollect)
        {
            if (card.GetMinionBody().y > 0) alivesMinion.Add(card);
            card.RemoveAuraEffect();
            if (card.GetProxys(ProxyEnum.Aura) != null)
            {
                triggers.Add(card);
            }
        }
        foreach (Card card in triggers)
        {
            card.InvokeProxy(ProxyEnum.Aura, new GameEvent()
            {
                hostCard = card,
                player = GetPlayer(card)
            });
        }
        foreach (Card card in alivesMinion)
        {
            int health = card.GetMinionBody().y;
            if (health < 0)
            {
                card.health -= health;
                card.health += 1;
            }
        }
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
    }

    public void WhenMinionPlayed(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.WhenMinionPlayed);

        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);
    }

    public void Battlecry(GameEvent gameEvent)
    {
        int BattlecryNum = 1;
        foreach (var item in gameEvent.player.hero.effects)
        {
            if (item is TripleBattlecryEffect)
            {
                BattlecryNum = 3;
            }
        }
        foreach (var item in gameEvent.player.hero.effectsStay)
        {
            if (item is TripleBattlecryEffect)
            {
                BattlecryNum = 3;
            }
        }
        if (BattlecryNum < 3)
        {
            foreach (var item in gameEvent.player.hero.effects)
            {
                if (item is DouleBattlecryEffect)
                {
                    BattlecryNum = 2;
                }
            }
            foreach (var item in gameEvent.player.hero.effectsStay)
            {
                if (item is DouleBattlecryEffect)
                {
                    BattlecryNum = 2;
                }
            }
        }
        for (int i = 0; i < BattlecryNum; i++)
        {
            if (gameEvent.hostCard.InvokeProxy(ProxyEnum.Battlecry, gameEvent))
            {

            }
        }
    }

    public void Dealthrattle(GameEvent gameEvent)
    {
        int DealthrattleNum = 1;
        foreach (var item in gameEvent.player.hero.effects)
        {
            if (item is TripleDeathrattleEffect)
            {
                DealthrattleNum = 3;
            }
        }
        foreach (var item in gameEvent.player.hero.effectsStay)
        {
            if (item is TripleDeathrattleEffect)
            {
                DealthrattleNum = 3;
            }
        }
        if (DealthrattleNum < 3)
        {
            foreach (var item in gameEvent.player.hero.effects)
            {
                if (item is DouleDeathrattleEffect)
                {
                    DealthrattleNum = 2;
                }
            }
            foreach (var item in gameEvent.player.hero.effectsStay)
            {
                if (item is DouleDeathrattleEffect)
                {
                    DealthrattleNum = 2;
                }
            }
        }
        for (int i = 0; i < DealthrattleNum; i++)
        {
            if (gameEvent.hostCard.InvokeProxy(ProxyEnum.Deathrattle, gameEvent))
            {

            }
        }
    }

    public void SpellEffect(GameEvent gameEvent)
    {
        if (gameEvent.hostCard.InvokeProxy(ProxyEnum.SpellEffect, gameEvent))
        {

        }
    }

    public void AfterMinionPlayed(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.AfterMinionPlayed);

        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);
    }

    public void AfterFlush()
    {
        var triggers = CollectTriggers(ProxyEnum.AfterFlush);

        TriggerTriggers(triggers, null, SendGameMessageCode闪电);
    }

    public void AfterMinionPlayedInHand(GameEvent gameEvent)
    {
        foreach (Card card in player.handPile)
        {
            if (card.GetProxys(ProxyEnum.AfterMinionPlayedInHand) != null)
            {
                card.InvokeProxy(ProxyEnum.AfterMinionPlayedInHand, new GameEvent()
                {
                    hostCard = card,
                    targetCard = gameEvent.hostCard,
                    player = player
                });
            }
        }

        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
    }

    public void AfterMinionSold(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.AfterMinionSold);
        if (gameEvent.hostCard.InvokeProxy(ProxyEnum.AfterSold, gameEvent))
            SendGameMessageCode闪电(gameEvent.hostCard);
        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);
    }

    public void TurnStart()
    {
        var triggers = CollectTriggers(ProxyEnum.TurnStart);

        TriggerTriggers(triggers, null, SendGameMessageCode闪电);
    }

    public void AfterUpgrade()
    {
        var triggers = CollectTriggers(ProxyEnum.AfterUpgrade);

        TriggerTriggers(triggers, null, SendGameMessageCode闪电);
    }

    public void AfterBoughtMinion(GameEvent gameEvent)
    {
        var triggers = CollectTriggers(ProxyEnum.AfterBoughtMinion);

        TriggerTriggers(triggers, gameEvent.hostCard, SendGameMessageCode闪电);
    }

    public void TurnEnd()
    {
        var triggers = CollectTriggers(ProxyEnum.TurnEnd);

        TriggerTriggers(triggers, null, SendGameMessageCode闪电);

    }

    public void TurnEndInHand()
    {
        foreach (Card card in player.handPile)
        {
            if (card.GetProxys(ProxyEnum.TurnEndInHand) != null)
            {
                card.InvokeProxy(ProxyEnum.TurnEndInHand, new GameEvent()
                {
                    hostCard = card,
                    targetCard = card,
                    player = player
                });
            }
        }

        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
    }

    public void TurnStartInHand()
    {
        foreach (Card card in player.handPile)
        {
            if (card.GetProxys(ProxyEnum.TurnStartInHand) != null)
            {
                card.InvokeProxy(ProxyEnum.TurnStartInHand, new GameEvent()
                {
                    hostCard = card,
                    targetCard = card,
                    player = player
                });
            }
        }

        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
    }

    public void CombatStart()
    {
        var triggers = CollectTriggers(ProxyEnum.CombatStart);

        TriggerTriggers(triggers, null, SendGameMessageCode闪电);

        DeathPhase();
    }

    public void GameStart()
    {
        var triggers = CollectTriggers(ProxyEnum.GameStart);

        TriggerTriggers(triggers, null, SendGameMessageCode闪电);
    }

    public bool Magnetic(GameEvent gameEvent)
    {
        if (gameEvent.hostCard.HasKeyword(Keyword.Magnetic))
        {
            Card target = ChooseTarget(GetMinionTargetLambda(gameEvent.player, MinionType.Mechs, null));
            if (target != gameEvent.hostCard)
            {
                gameEvent.player.RemoveMinionFromBattlePile(gameEvent.hostCard);
                target.effectsStay.Add(new BodyPlusEffect(gameEvent.hostCard.GetMinionBody().x, gameEvent.hostCard.GetMinionBody().y));
                foreach (Keyword keyword in Enum.GetValues(typeof(Keyword)))
                {
                    if (!keyword.Equals(Keyword.Magnetic) && gameEvent.hostCard.HasKeyword(keyword))
                        target.effectsStay.Add(new KeyWordEffect(keyword));
                }
                foreach (ProxyEnum proxy in Enum.GetValues(typeof(ProxyEnum)))
                {
                    if (gameEvent.hostCard.GetProxys(proxy) != null)
                    {
                        target.effectsStay.Add(new ProxyEffect(proxy, gameEvent.hostCard.GetProxys(proxy)));
                    }
                }
                //Maybe Some Messages
                return true;
            }
        }
        return false;
    }

    #endregion

    #region recruit

    public void Buy(Card card)
    {
        //cardPile.ReduceCard(card, 1);
        AddToHandPile(card);
        players[1].RemoveMinionFromBattlePile(card);
        player.leftCoins -= Const.coinCostToBuyMinion;
        numOfMinionsBought++;
        AfterBoughtMinion(new GameEvent()
        {
            hostCard = card,
            player = GetPlayer(card)
        });
        Const.coinCostToBuyMinion = Const.InitialCoinCostToBuyMinion;
    }

    public void Sell(Card card)
    {
        ChangingRemove(card);
        if (!card.isToken)
        {
            cardPile.AddCard(CardBuilder.GetCard(card.id), 1);
        }
        else if (card.isGold && !CardBuilder.GetCard(card.goldVersion).isToken)
        {
            cardPile.AddCard(CardBuilder.GetCard(card.goldVersion), 3);
        }
        if (player.leftCoins < Const.MaxCoin)
        {
            player.GetCoin(Const.coinGetBySellMinion);
        }
        AfterMinionSold(new GameEvent()
        {
            hostCard = card,
            player = player
        });
        AuraCheck();
        player.RemoveMinionFromBattlePile(card);
        player.RemoveMinionFromHandPile(card);
        Const.coinGetBySellMinion = Const.InitialCoinGetBySellMinion;
        //SpecialAuraCheck();
    }

    public void Swap(CardPosition pos1, CardPosition pos2)
    {
        if (pos1.type == PositionType.HeroBattlePile && pos2.type == PositionType.HeroBattlePile)
        {
            player.battlePile.Swap(pos1.pos, pos2.pos);
        }
    }

    public void Upgrade()
    {
        player.leftCoins -= player.upgradeCost;
        UpgradeFunc();
    }

    public void Freeze()
    {
        isFreeze = !isFreeze;
    }

    public void Flush()
    {
        isFreeze = false;
        player.leftCoins -= Const.InitialFlushCost;
        FlushFunc();
    }

    public void PlayCard(CardPosition card, CardPosition PlayPosition)
    {
        Card handCard = player.handPile[card.pos];
        player.leftCoins -= handCard.cost;
        ChangingRemove(handCard);
        if (handCard.cardType == CardType.Minion)
        {
            Card minion = handCard;
            player.battlePile[PlayPosition.pos] = handCard;
            player.RemoveMinionFromHandPile(handCard);

            #region send play code
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "play",
                    card1 = minion,
                },
            });
            #endregion

            if (minion.isGold)
            {
                GetBonusOfStar(player.star);
            }

            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend()
            });
            AuraCheck();

            WhenMinionPlayed(new GameEvent()
            {
                hostCard = minion,
                targetCard = null,
                player = GetPlayer(minion),
            });

            WhenMinionSummon(new GameEvent()
            {
                hostCard = minion,
                targetCard = null,
                player = GetPlayer(minion),
            });

            Battlecry(new GameEvent()
            {
                hostCard = minion,
                targetCard = null,
                player = GetPlayer(minion)
            });

            if (minion.HasKeyword(Keyword.Magnetic) && Magnetic(new GameEvent()
            {
                hostCard = minion,
                targetCard = null,
                player = GetPlayer(minion)
            }))
            {

            }
            else
            {
                AfterMinionSummon(new GameEvent()
                {
                    hostCard = minion,
                    targetCard = null,
                    player = GetPlayer(minion),
                });

                AfterMinionPlayed(new GameEvent()
                {
                    hostCard = minion,
                    targetCard = null,
                    player = GetPlayer(minion),
                });

                AfterMinionPlayedInHand(new GameEvent()
                {
                    hostCard = minion,
                    targetCard = null,
                    player = GetPlayer(minion),
                });
            }
            MergeCheck();
        }
        else if (handCard.cardType == CardType.Spell)
        {
            Card spell = handCard;
            player.RemoveMinionFromHandPile(handCard);
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend()
            });
            SpellEffect(new GameEvent()
            {
                hostCard = spell,
                targetCard = null,
                player = player
            });
            MergeCheck();
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend()
            });
        }
        else
        {
            throw new Exception("Error MinionType");
        }
    }

    public void HeroPower()
    {
        player.leftCoins -= player.hero.cost;
        if (player.hero.GetProxys(ProxyEnum.HeroPower) != null)
        {
            if (player.hero.cost != -3)
            {
                player.hero.cost = -2;
            }
            player.hero.InvokeProxy(ProxyEnum.HeroPower, new GameEvent()
            {
                player = player,
                hostCard = player.hero
            });
        }
    }

    public void FlushFunc()
    {
        // 旧的加回牌池，从场上移除
        for (int i = 0; i < players[1].battlePile.fixedNumber; i++)
        {
            if (players[1].battlePile[i] == null) continue;
            if (!players[1].battlePile[i].isToken)
            {
                cardPile.AddCard(players[1].battlePile[i], 1);
            }
            players[1].battlePile[i] = null;
        }

        // 取出新的若干个
        for (int i = 0; i < Const.numOfMinionsOnSale[player.star - 1]; i++)
        {
            players[1].battlePile[i] = (Card)cardPile.RandomlyGetCardByFilterAndReduceIt(card => card.star <= player.star).NewCard();
        }

        AfterFlush();
    }

    public void UpgradeFunc()
    {
        if (player.star <= 6)
        {
            player.star += 1;
            if (player.star == 6)
            {
                player.upgradeCost = -1;
            }
            else
            {
                player.upgradeCost = Const.upgradeCosts[player.star - 1];
            }
            AfterUpgrade();
        }
    }

    public void ChangingRemove(Card card)
    {
        if (card.HasKeyword(Keyword.Changing))
        {
            Card tmp1 = CardBuilder.SearchCardByName("软泥教授弗洛普");
            Card tmp2 = CardBuilder.SearchCardByName("百变泽鲁斯");
            Card tmp3 = CardBuilder.SearchCardByName("暗影映像");
            card.RemoveKeyWord(Keyword.Changing);
            if (card.GetProxysByEffect(ProxyEnum.AfterMinionPlayedInHand) != null)
            {
                foreach (var item in card.GetProxysByEffect(ProxyEnum.AfterMinionPlayedInHand))
                {
                    item.cardProxyDelegate -= tmp1.GetProxys(ProxyEnum.AfterMinionPlayedInHand);
                    item.cardProxyDelegate -= tmp3.GetProxys(ProxyEnum.AfterMinionPlayedInHand);
                }
            }
            if (card.GetProxysByEffect(ProxyEnum.TurnStartInHand) != null)
            {
                foreach (var item in card.GetProxysByEffect(ProxyEnum.TurnStartInHand))
                {
                    item.cardProxyDelegate -= tmp2.GetProxys(ProxyEnum.TurnStartInHand);
                }
            }
        }
    }

    public readonly Map<Card, int> mergeCardPosition = new Map<Card, int>();
    public void MergeCheck()
    {
        CardPile tmpCardPile = new CardPile();
        if (!isBattleField)
        {
            foreach (Card card in player.battlePile)
            {
                // 排除变化卡
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
                        // 排除变化卡
                        if (card.HasKeyword(Keyword.Changing)) continue;

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
                #region 保存进行合并过的卡
                //mergeCardPosition.Clear();
                for (int i = 0; i < 3; i++)
                {
                    int index = player.GetMinionIndex(cards[i]);
                    if (index < 0) index = player.handPile.IndexOf(cards[i]);
                    if (index >= 0)
                    {
                        mergeCardPosition[cards[i]] = index;
                    }
                }
                #endregion

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


                SendGameMessage(new ChangeMessage()
                {
                    data = GetDataForSend(),
                    code = new ChangeMessageCode()
                    {
                        code = "merge",
                        card1 = card,
                        card2 = cards[0],
                        card3 = cards[1],
                        card4 = cards[2]
                    },
                });

                AddToHandPile(card); // 先不发card

                SendGameMessage(new ChangeMessage()
                {
                    data = GetDataForSend(),
                });

                MergeCheck();
            }
        }
    }

    public void AddToHandPile(Card card)
    {
        player.AddMinionToHandPile(card);
        MergeCheck();
    }

    public Card DiscoverToHand(List<Card> cardList)
    {
        Card card = Discover(cardList);
        AddToHandPile(card);
        cardPile.ReduceCard(card, 1);
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend(),
        });
        return card;
    }

    public void TriggerTreasure()
    {
        foreach (var treasure in player.treasures)
        {
            if (treasure.HasKeyword(Keyword.Passive))
            {
                foreach (ProxyEnum proxy in Enum.GetValues(typeof(ProxyEnum)))
                {
                    if (treasure.GetProxys(proxy) != null && !proxy.Equals(ProxyEnum.Selected) && !proxy.Equals(ProxyEnum.SpellEffect))
                    {
                        player.hero.effectsStay.Add(new ProxyEffect(proxy, treasure.GetProxys(proxy)));
                        if (treasure.name != "能量之泉" && player.treasures.Any(card => card.name == "能量之泉"))
                        {
                            player.hero.effectsStay.Add(new ProxyEffect(proxy, treasure.GetProxys(proxy)));
                        }
                    }
                }
            }
            if (treasure.HasKeyword(Keyword.Active))
            {
                player.AddMinionToHandPile(treasure);
            }
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
            });
        }
    }

    public Card Discover(List<Card> cardList)
    {
        SortedList<int, Card> discoverCards = new SortedList<int, Card>();
        List<Card> discoverCardPile = cardList.Filter(card => !card.isToken);
        if (discoverCardPile.Count >= 3)
        {
            do
            {
                Card tmpCard = discoverCardPile.GetOneRandomly();
                if (!discoverCards.ContainsValue(tmpCard))
                {
                    discoverCards.Add(discoverCards.Count, tmpCard);
                }
            }
            while (discoverCards.Count < 3);
        }
        else
        {
            foreach (var item in discoverCardPile)
            {
                discoverCards.Add(discoverCards.Count, item);
            }
        }
        SortedList<int, Card> discoverNewCards = new SortedList<int, Card>();
        foreach (var item in discoverCards)
        {
            discoverNewCards.Add(discoverNewCards.Count, item.Value.NewCard());
        }
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend(),
            code = new ChangeMessageCode()
            {
                code = "choose",
                card1 = discoverNewCards.Count > 0 ? discoverNewCards[0] : null,
                card2 = discoverNewCards.Count > 1 ? discoverNewCards[1] : null,
                card3 = discoverNewCards.Count > 2 ? discoverNewCards[2] : null,
                card4 = discoverNewCards.Count > 3 ? discoverNewCards[3] : null,
            },
        });
        ChangeMessage changeMessage = GetMessage();
        if (changeMessage.code.code != "choose")
        {
            throw new Exception("server: not match code:choose");
        }
        whichOneChoosed = changeMessage.code.number;
        Card cardDiscovered = discoverNewCards[whichOneChoosed];
        isDiscoverEnd = false;
        whichOneChoosed = -1;
        return cardDiscovered;
    }

    public Card ChooseTarget(Func<Card, bool> function)
    {
        List<Card> cardList = players[0].GetAllAllyMinion().Filter(function);
        cardList.AddRange(players[1].GetAllAllyMinion().Filter(function));
        //Debug.Log("cardList:" + cardList.Count);
        if (cardList.Count != 0)
        {
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
                code = new ChangeMessageCode()
                {
                    code = "selectTarget",
                    cardList = cardList
                },
            });
            ChangeMessage changeMessage = null;
            while (true)
            {
                changeMessage = GetMessage();
                if (changeMessage.code.code == "selectTarget")
                {
                    break;
                }
                else
                {
                    Debug.LogWarning("server: not match code:selectTarget");
                }
            }
            SendGameMessage(new ChangeMessage()
            {
                data = GetDataForSend(),
            });
            return cardList[changeMessage.code.number];
        }
        else
        {
            return null;
        }
    }

    #endregion

    #region tools

    List<(Card, ProxyEffects)> CollectTriggers(ProxyEnum proxyEnum)
    {

        List<(Card, ProxyEffects)> triggers = new List<(Card, ProxyEffects)>();

        var heroCardProxyDelegate0 = playersByOffensive[0].hero.GetProxysByEffect(proxyEnum);
        if (heroCardProxyDelegate0 != null) triggers.Add((playersByOffensive[0].hero, heroCardProxyDelegate0));

        foreach (Card card in playersByOffensive[0].GetAllAllyMinion())
        {
            var cardProxyDelegate = card.GetProxysByEffect(proxyEnum);
            if (cardProxyDelegate != null) triggers.Add((card, cardProxyDelegate));
        }

        var heroCardProxyDelegate1 = playersByOffensive[1].hero.GetProxysByEffect(proxyEnum);
        if (heroCardProxyDelegate1 != null) triggers.Add((playersByOffensive[1].hero, heroCardProxyDelegate1));

        foreach (Card card in playersByOffensive[1].GetAllAllyMinion())
        {
            var cardProxyDelegate = card.GetProxysByEffect(proxyEnum);
            if (cardProxyDelegate != null) triggers.Add((card, cardProxyDelegate));
        }

        return triggers;
    }

    void TriggerTriggers(List<(Card, ProxyEffects)> triggers, Card targetCard = null, Action<Card> triggerDone = null)
    {
        triggers.Map(pair =>
        {
            Card card = pair.Item1;
            var cardProxyDelegate = pair.Item2;
            if (cardProxyDelegate == null) return;
            if (cardProxyDelegate.Invoke(new GameEvent()
            {
                hostCard = card,
                player = GetPlayer(card),
                targetCard = targetCard
            }))
                triggerDone?.Invoke(card);
        });
    }

    void SendGameMessageCode闪电(Card card)
    {
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend(),
            code = new ChangeMessageCode()
            {
                code = "trigger",
                trigger = "闪电",
                pos1 = GetCardPosition(card)
            },
        });
    }

    /// <summary>
    /// 快捷获取player一方特定种族的随从，hostCrad置空表示可以选择自己
    /// </summary>
    public Func<Card, bool> GetMinionTargetLambda(Player player, MinionType? minionType, Card hostCard = null)
    {
        bool func(Card card) => card.IsMinionType(minionType ?? card.type) && GetPlayer(card) == player && card != hostCard;
        return func;
    }

    public ChangeMessageGameData GetDataForSend()
    {
        return new ChangeMessageGameData()
        {
            isBattle = isBattleField,
            hero = this.players[0].hero,
            heroBattlePile = players[0].battlePile,
            heroHandPile = players[0].handPile,

            opponent = this.players[1].hero,
            opponentBattlePile = players[1].battlePile,
            opponentHandPile = players[1].handPile,

            flushCost = players[0].flushCost,
            upgradeCost = players[0].upgradeCost,
            freezeCost = players[0].freezeCost,
            star = players[0].star,

            leftCoins = players[0].leftCoins,
            maxCoins = players[0].maxCoins,
        };

    }

    public CardPosition GetCardPosition(Card card)
    {
        return TransToCardPosition(GetPlayer(card), card);
    }

    public CardPosition TransToCardPosition(Player player, Card card)
    {
        if (card.cardType.Equals(CardType.Minion))
        {
            if (player == players[0])
            {
                return new CardPosition(PositionType.HeroBattlePile, players[0].GetMinionIndex(card));
            }
            else if (player == players[1])
            {
                return new CardPosition(PositionType.OpponentBattlePile, players[1].GetMinionIndex(card));

            }
            else
            {
                throw new Exception("Only two players");
            }
        }
        else if (card.cardType.Equals(CardType.Hero))
        {
            if (player == players[0])
            {
                return new CardPosition(PositionType.Hero);
            }
            else if (player == players[1])
            {
                return new CardPosition(PositionType.Opponent);

            }
            else
            {
                throw new Exception("Only two players");
            }
        }
        else
        {
            throw new Exception("We have not made spells!");
        }
    }

    public Player GetPlayer(Card card)
    {
        if (players[0].ContainsMinion(card) || players[0].hero == card || players[0].handPile.Contains(card))
        {
            return players[0];
        }
        else if (players[1].ContainsMinion(card) || players[1].hero == card || players[1].handPile.Contains(card))
        {
            return players[1];
        }
        else
        {
            if (tmpCards[0].Contains(card))
            {
                return playersByOffensive[0];
            }
            else if (tmpCards[1].Contains(card))
            {
                return playersByOffensive[1];
            }
            else if (mergeCardPosition.ContainsKey(card))
            {
                return players[0];
            }
            else
            {
                throw new Exception("This Minion Is Not In The Board: " + card.name);
            }
        }
    }

    public Player GetAnotherPlayer(Player player)
    {
        if (player != null)
        {
            if (player == players[0])
            {
                return players[1];
            }
            else if (player == players[1])
            {
                return players[0];
            }
            else
            {
                throw new Exception("ThisNotInTheBattle");
            }
        }
        else
        {
            throw new Exception("NullPointer");
        }
    }

    public Tuple<Card, Card> GetAdjacentMinion(Card card)
    {
        BattlePile<Card> cards = GetPlayer(card).battlePile;
        int position = GetCardPosition(card).pos;
        if (position == 0 || position == 4)
        {
            return new Tuple<Card, Card>(null, cards[position + 1]);
        }
        else if (position == 3 || position == 6)
        {
            return new Tuple<Card, Card>(cards[position - 1], null);
        }
        else
        {
            return new Tuple<Card, Card>(cards[position - 1], cards[position + 1]);
        }
    }

    public bool IsOutCast(Card card)
    {
        int pos = GetCardPosition(card).pos;
        if (pos == 0) return true;
        if (pos == 3) return true;
        if (pos == 4) return true;
        if (pos == 6) return true;
        return false;
    }

    /// <summary>
    /// 获取需要的种族数目
    /// </summary>
    /// <param name="allCards"></param>
    /// <param name="tuples">每个种族存在数目</param>
    /// <returns></returns>
    public static List<Card> FilterCardByMinionType(List<Card> allCards, params (MinionType, int)[] tuples)
    {
        var wants = tuples.ToDictionary(x => x.Item1, x => x.Item2);

        var have = allCards.GroupBy(card => card.type).Where(x => wants.ContainsKey(x.Key))
            .ToDictionary(x => x.Key, x => x.ToList().Shuffle().Take(wants[x.Key]).ToList());


        var haveCards = have.Select(x => x.Value).SelectMany(x => x).ToList();
        var demand = wants.Sum(x => x.Value) - haveCards.Count;
        var anyTypeCard = allCards.Where(card => card.type == MinionType.Any).ToList();

        var removeNum = anyTypeCard.Count - demand;

        var SavedCards = new List<Card>();

        foreach (var card in anyTypeCard)
        {
            if (demand-- > 0)
            {
                SavedCards.Add(card);
                continue;
            }
            var rnd = new List<int>() { 1, 2 }.GetOneRandomly();
            if (rnd == 1)
            {
                haveCards.Remove(haveCards.GetOneRandomly());
                SavedCards.Add(card);
            }
        }


        SavedCards.AddRange(haveCards);

        return SavedCards;
    }

    #endregion

    #region tools of combat

    #region tmpPile

    public void BattlePileToTempCards() //将战场现有随从和空格（使用占位0号随从）加入空的临时pile中
    {
        for (int i = 0; i < playersByOffensive[0].battlePile.fixedNumber; i++)
        {
            if (playersByOffensive[0].battlePile[i] != null)
            {
                tmpCards[0].Add(playersByOffensive[0].battlePile[i]);
            }
            else
            {
                tmpCards[0].Add(CardBuilder.NewCard(0));
            }
        }
        for (int i = 0; i < playersByOffensive[1].battlePile.fixedNumber; i++)
        {
            if (playersByOffensive[1].battlePile[i] != null)
            {
                tmpCards[1].Add(playersByOffensive[1].battlePile[i]);
            }
            else
            {
                tmpCards[1].Add(CardBuilder.NewCard(0));
            }
        }
    }

    public void TempCardsToBattlePile() //将临时pile中的随从和空格（使用占位0号随从）置入战场
    {
        int i = 0;
        foreach (Card card in tmpCards[0])
        {
            if (card != null && card.id != 0 && !card.isDead)
            {
                playersByOffensive[0].AddMinionToBattlePile(card, (i < playersByOffensive[0].battlePile.fixedNumber - 1 ? i : (playersByOffensive[0].battlePile.fixedNumber - 1)));
                i++;
            }
            else if (card.id == 0)
            {
                i++;
            }

        }
        i = 0;
        foreach (Card card in tmpCards[1])
        {
            if (card != null && card.id != 0 && !card.isDead)
            {
                playersByOffensive[1].AddMinionToBattlePile(card, (i < playersByOffensive[1].battlePile.fixedNumber - 1 ? i : (playersByOffensive[1].battlePile.fixedNumber - 1)));
                i++;
            }
            else if (card.id == 0)
            {
                i++;
            }
        }
    }

    public void TempCardsCheck() //消除临时pile中的不正确空格（使用占位0号随从）
    {
        int check = 0;
        for (int i = 0; i < playersByOffensive[0].battlePile.fixedNumber; i++)
        {
            do
            {
                if (tmpCards[0].Count <= check)
                {
                    break;
                }
                if (tmpCards[0][check] == playersByOffensive[0].battlePile[i])
                {
                    check++;
                    break;
                }
                else if (tmpCards[0][check].id == 0)
                {
                    if (playersByOffensive[0].battlePile[i] == null)
                    {
                        check++;
                        break;
                    }
                    else
                    {
                        tmpCards[0].RemoveAt(check);
                    }
                }
                else if (tmpCards[0][check].isDead)
                {
                    check++;
                }
            } while (true);
        }

        check = 0;
        for (int i = 0; i < playersByOffensive[1].battlePile.fixedNumber; i++)
        {
            do
            {
                if (tmpCards[1].Count <= check)
                {
                    break;
                }
                if (tmpCards[1][check] == playersByOffensive[1].battlePile[i])
                {
                    check++;
                    break;
                }
                else if (tmpCards[1][check].id == 0)
                {
                    if (playersByOffensive[1].battlePile[i] == null)
                    {
                        check++;
                        break;
                    }
                    else
                    {
                        tmpCards[1].RemoveAt(check);
                    }
                }
                else if (tmpCards[1][check].isDead)
                {
                    check++;
                }
            } while (true);
        }
    }

    public void TempCardsRemoveAll() //清空临时pile
    {
        tmpCards[0] = new List<Card>();
        tmpCards[1] = new List<Card>();
    }

    #endregion

    public void InitCombat()
    {
        TurnEnd();
        TurnEndInHand();
        if (isFreeze)
        {
            freezeCards = players[1].battlePile;
        }
        else
        {
            freezeCards = null;
        }
        //当前回合计数重置
        numOfMinionsBought = 0;
        if (player.hero.cost != -3)
        {
            player.hero.cost = CardBuilder.GetCard(player.hero.id).cost;
        }
        Const.coinCostToBuyMinion = Const.InitialCoinCostToBuyMinion;
        Const.coinGetBySellMinion = Const.InitialCoinGetBySellMinion;

        isBattleField = true;
        players[0] = (Player)player.Clone();
        players[1] = enemy.GetPlayerForBattle();
        players[0].upgradeCost = -1;
        players[0].flushCost = -1;
        players[0].freezeCost = -1;
        players[0].star = -1;
        AttackingPlayer = 0;
        players[0].deathMinionCollection.Clear();
        players[1].deathMinionCollection.Clear();
        JudgeOffensive();
        TempCardsRemoveAll();
        AuraCheck();
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
        TempCardsRemoveAll();
        BattlePileToTempCards();
    }

    public void StartCombat() //TODO
    {
        InitCombat();
        CombatStart();
        if (IsGameOver() == null)
        {
            AttackingMinion[0] = GetFirstAttackMinion(0);
            while (IsGameOver() == null)
            {
                RandomlyAttack();
                DeathPhase();
                if (IsGameOver() == null)
                {
                    NextRun();
                }
            }
        }
        EndBattle();
    }

    public Card GetFirstAttackMinion(int attackingPlayer)
    {
        Card card;
        int index = Const.numOfBattlePile - 1;
        int i = 0;
        do
        {
            i++;
            index = (index + 1) % tmpCards[attackingPlayer].Count;
            card = tmpCards[attackingPlayer][index];
        }
        while ((card == null | card.id == 0 | card.isDead | card.GetMinionBody().x <= 0) && i < 20);
        return card;
    }

    public void JudgeOffensive()
    {
        if (players[0].GetNumOfMinion() > players[1].GetNumOfMinion())
        {
            playersByOffensive[0] = players[0];
            playersByOffensive[1] = players[1];
        }
        else if ((players[0].GetNumOfMinion() < players[1].GetNumOfMinion()))
        {
            playersByOffensive[0] = players[1];
            playersByOffensive[1] = players[0];
        }
        else
        {
            if (new System.Random().Next(2) == 1)
            {
                playersByOffensive[0] = players[0];
                playersByOffensive[1] = players[1];
            }
            else
            {
                playersByOffensive[0] = players[1];
                playersByOffensive[1] = players[0];
            }
        }
    }

    public string IsGameOver()
    {
        bool drawBecauseOfEggs = true;
        foreach (Card card in players[0].battlePile)
        {
            if (card.attack > 0)
            {
                drawBecauseOfEggs = false;
                break;
            }
        }
        foreach (Card card in players[1].battlePile)
        {
            if (card.attack > 0)
            {
                drawBecauseOfEggs = false;
                break;
            }
        }
        if (players[0].GetNumOfMinion() == 0)
        {
            if (players[1].GetNumOfMinion() == 0)
            {
                return "Draw";
            }
            else
            {
                return "Player1 Wins";
            }
        }
        else if (players[1].GetNumOfMinion() == 0)
        {
            return "Player0 Wins";
        }
        else
        {
            if (drawBecauseOfEggs == true)
            {
                return "Draw";
            }
            else
            {
                return null;
            }
        }
    }

    public void EndBattle()
    {
        // 这里要多发一次消息 进行最后的校验
        SendGameMessage(new ChangeMessage() { data = GetDataForSend() });

        ResultOfTheLastGame = IsGameOver();
        Debug.Log(ResultOfTheLastGame);
        if (ResultOfTheLastGame.Equals("Player1 Wins"))
        {
            players[1].hero.attack = GetSumOfStar(players[1]);
            Attack(new GameEvent()
            {
                hostCard = players[1].hero,
                targetCard = players[0].hero,
                player = players[1]
            });
        }
        else if (ResultOfTheLastGame.Equals("Player0 Wins"))
        {
            players[0].hero.attack = GetSumOfStar(players[0]);
            Attack(new GameEvent()
            {
                hostCard = players[0].hero,
                targetCard = players[1].hero,
                player = players[0]
            });
        }
        else
        {

        }
        foreach (var item in playersByOffensive)
        {
            item.hero.attack = 0;
        }
        GameOverJudge();
        StartRecruit();
    }

    public int GetSumOfStar(Player playerToCalculate)
    {
        int sum = 0;
        if (playerToCalculate == players[0])
        {
            sum = player.star;
        }
        else
        {
            sum = playerToCalculate.star;
        }
        foreach (var minion in playerToCalculate.battlePile)
        {
            sum += minion.star;
        }
        return sum;
    }

    #endregion

    #region tools of recruit

    public void InitRecruit()
    {
        players[0] = player;
        players[1] = new Player(this, CardBuilder.SearchCardByName("英雄").NewCard());

        playersByOffensive[0] = players[0];
        playersByOffensive[1] = new Player(this, CardBuilder.SearchCardByName("英雄").NewCard());
        isBattleField = false;
        if (player.upgradeCost > 0)
        {
            player.upgradeCost -= 1;
        }
        if (player.maxCoins < Const.MaxCoin)
        {
            player.maxCoins++;
        }
        player.leftCoins = player.maxCoins;
    }

    public void StartRecruit()
    {
        InitRecruit();
        AuraCheck();
        MergeCheck();
        if (isFreeze)
        {
            players[1].battlePile = freezeCards;
            //补齐随从
            int tmp = Const.numOfMinionsOnSale[player.star - 1] - players[1].GetNumOfMinion();
            for (int i = 0; i < tmp; i++)
            {
                players[1].battlePile[players[1].battlePile.GetEmptyPos()] = (Card)cardPile.RandomlyGetCardByFilterAndReduceIt(card => card.star <= player.star).NewCard();
            }
            isFreeze = false;
        }
        else
        {
            FlushFunc();
        }
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
        if (!GameStartTrigger)
        {
            TriggerTreasure();
            GameStart();
            GameStartTrigger = true;
        }
        TurnStartInHand();
        TurnStart();
        MergeCheck();
        AuraCheck();
    }

    public void GetBonusOfStar(int n)
    {
        AddToHandPile(CardPile.DiscoverHigherStarSpell[n == 6 ? 5 : n].NewCard());
    }

    #endregion

    #region test

    public void InitTest()
    {
        //TriggerTreasure();
        AuraCheck();
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
    }

    public void CreateCard(Card card)
    {
        AddToHandPile(card.NewCard());
        AuraCheck();
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
    }
    public void AddCoin(int n = 10)
    {
        players[0].leftCoins += n;
        AuraCheck();
        SendGameMessage(new ChangeMessage()
        {
            data = GetDataForSend()
        });
    }

    public void Combat()
    {
        StartCombat();
    }

    public void CheckMinion()
    {
        string str = "";
        for (int i = 0; i < 7; i++)
        {
            str += (player.battlePile[i] ?? CardBuilder.NewCard(0)).name;
        }
        Debug.Log(str);
        str = "";
        for (int i = 0; i < 7; i++)
        {
            str += (players[0].battlePile[i] ?? CardBuilder.NewCard(0)).name;
        }
        Debug.Log(str);
        str = "";
        for (int i = 0; i < 7; i++)
        {
            str += (players[1].battlePile[i] ?? CardBuilder.NewCard(0)).name;
        }
        Debug.Log(str);
    }

    #endregion
}

