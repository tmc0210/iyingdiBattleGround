using BIF;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using System.Threading;
using System.Collections.Concurrent;

public class BattleBoardSetting : MonoBehaviour
{
    #region public fileds
    [Autohook]
    public Transform Points;
    [Autohook]
    public SpriteRenderer BG;
    [Autohook]
    public Transform ShopPoints;
    [Autohook]
    public Transform GameObjects;

    [Autohook]
    public SpriteRenderer Mask;
    [Autohook]
    public DescriptionMaskSetting DescriptionMask;
    [Autohook]
    public GameEndSetting GameEndWindow;



    [Autohook]
    public Transform HidingArea;
    [Autohook]
    public GameObject CardPrefab;
    [Autohook]
    public GameObject HeroPrefab;
    [Autohook]
    public GameObject DeathrattlePrefab;
    [Autohook]
    public GameObject HitPrefab;
    [Autohook]
    public GameObject RebornPrefab;
    [Autohook]
    public StarSetting Stars;
    [Autohook]
    public DecorateSetting Decorate;
    [Autohook]
    public ChooseWindowSetting ChooseWindow;
    [Autohook]
    public DescriptionWindowSetting DescriptionWindow;
    [Autohook]
    public AOESetting AOE;

    public Sprite BattleBoardBGSprite;
    public Sprite ShopBoardBGSprite;
    #endregion

    #region  make this singleton

    public static BattleBoardSetting instance = null;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    #region private var
    private bool isBattle = true;
    private bool isBattleChanged = true;
    [NonSerialized]
    public ChangeMessage curChangeMessage = null;
    private HashSet<int> minionNotUsed = new HashSet<int>();
    private bool isFreeze = false;
    private bool isBegin = true;
    private bool isGameover = false;

    public void Init()
    {
        isGameover = false;
        isAnimating = false;
        messageSequences.Clear();
        isPause = false;
        isBegin = true;
        GameEndWindow.gameObject.SetActive(false);
        //cardMaps.GetValues().Map(st=>RemoveCard(st.setting));
        //heroMaps.GetValues().Map(st=>RemoveHero(st));
    }

    private float cardWidth = 1.4f;
    private float cardHeight = 1.8f;

    private bool predictToStop = false;

    // for test
    public Board board;
    Player player;

    #endregion

    #region unity functions

    private void Start()
    {
        InitPointTransfroms();
        DOTween.SetTweensCapacity(500, 2000);

        #region set prefab
        BIFStaticTool.PrefabPool("Card", CardPrefab, HidingArea);
        BIFStaticTool.PrefabPool("Hero", HeroPrefab, HidingArea);
        BIFStaticTool.PrefabPool("Deathrattle", DeathrattlePrefab, HidingArea);
        BIFStaticTool.PrefabPool("Reborn", RebornPrefab, HidingArea);

        PrefabStayLong.SetAnimation("亡语触发", () =>
        {
            return BIFStaticTool.GetPrefabInPool("Deathrattle", GameObjects);
        }, (GameObject go) =>
        {
            BIFStaticTool.PrefabPool("Deathrattle", go);
        }, (GameObject go) =>
        {
            var scale = go.transform.localScale;
            go.transform.localScale = Vector3.zero;
            SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(1, 1, 1, 0);
            DOTween.Sequence().Append(go.transform.DOScale(1.3f * scale, 0.7f).SetEase(Ease.OutExpo))
                .Insert(0f, spriteRenderer.DOColor(Color.white, 0.2f))
                .Insert(0.2f, spriteRenderer.DOFade(0, 0.5f))
                .AppendCallback(() =>
                {
                    spriteRenderer.color = Color.white;
                    go.transform.localScale = scale;
                });
        }, null, 0.7f);



        PrefabStayLong.SetAnimation("Reborn", () =>
        {
            return BIFStaticTool.GetPrefabInPool("Reborn", GameObjects);
        }, (GameObject go) =>
        {
            BIFStaticTool.PrefabPool("Reborn", go);
        }, (GameObject go) =>
        {
            var scale = go.transform.localScale;
            go.transform.localScale = Vector3.zero;
            SpriteRenderer spriteRenderer = go.GetComponentInChildren<SpriteRenderer>();
            DOTween.Sequence().Append(go.transform.DOScale(scale, 0.2f))
                .AppendInterval(0.2f)
                .Append(go.transform.DOLocalMove(new Vector3(0, 1, 0), 0.5f).SetRelative().SetEase(Ease.InExpo))
                .Join(spriteRenderer.DOFade(0, 0.5f).SetEase(Ease.InExpo))
                .AppendCallback(() => { spriteRenderer.color = Color.white; });
        }, null, 0.9f);
        #endregion

        // 绑定Bridge事件
        //var client = Bridge.GetClient();
        ////client.OnReceiveMessage(OnGameMessage);




        //board = new Board();
        //Bridge.GetClient().Start();



    }

    // 消息队列
    private Queue<ChangeMessage> messageSequences = new Queue<ChangeMessage>();
    private bool isAnimating = false;
    private bool isPause = false;


    private void FixedUpdate()
    {
        if (!isAnimating && messageSequences.Count != 0 && !isPause)
        {
            isAnimating = true;

            DealGameMessage(messageSequences.Dequeue())
                .OnComplete(() => { isAnimating = false; })
                .Play();


        }
    }
    #endregion

    #region deal with message
    public void OnGameMessage(ChangeMessage changeMessage)
    {
        //ChangeMessage changeMessage = changeMessageOri.Clone() as ChangeMessage;
        //Debug.Log("client got message");
        messageSequences.Enqueue(changeMessage);
    }


    /// <summary>
    /// not use
    /// </summary>
    /// <param name="changeMessage"></param>
    /// <returns></returns>
    HashSet<Card> CollectCardFronChangeMessageData(ChangeMessage changeMessage)
    {
        List<Card> cards = new List<Card>();
        if (changeMessage.data != null)
        {
            cards.Add(changeMessage.data.hero);
            cards.AddRange(changeMessage.data.heroBattlePile);
            cards.AddRange(changeMessage.data.heroHandPile);
            cards.Add(changeMessage.data.opponent);
            cards.AddRange(changeMessage.data.opponentBattlePile);
            cards.AddRange(changeMessage.data.opponentHandPile);
        }
        return new HashSet<Card>(cards);
    }

    HashSet<int> heroNotUsed = new HashSet<int>();

    ChangeMessage lastChangeMessage = null;
    public Tween DealGameMessage(ChangeMessage changeMessage)
    {
        //Debug.Log("client got message:" + JsonUtility.ToJson(changeMessage));
        if (isGameover)
        {
            return null;
        }

        // 保存所有的随从
        // 之后删去用过的随从， 剩下的随从就是要移除的
        minionNotUsed.Clear();
        minionNotUsed.UnionWith(cardMaps.Map(pair => pair.Key));
        heroNotUsed.Clear();
        heroNotUsed.UnionWith(heroMaps.Map(pair => pair.Key));
        //print("before "+heroMaps.Count + ":" + heroNotUsed.Count);

        curChangeMessage = changeMessage;

        #region set isBattleChanged and isBattle
        isBattleChanged = false;
        if (changeMessage.data != null)
        {
            isBattleChanged = isBattle != changeMessage.data.isBattle;
            isBattle = changeMessage.data.isBattle;
        }
        if (isBegin)
        {
            isBattleChanged = true;
        }
        #endregion

        Sequence sequence = DOTween.Sequence();

        #region 清除星级标志和三连动画
        cardMaps.Values.Map(st =>
        {
            st.setting.HideStar();
            st.setting.ShowMerge(false);
        });
        #endregion

        // 预测系统
        if (NextChangeMessage == null)
        {
            predictToStop = false;
        }

        //if (lastChangeMessage?.data?.opponent != null)
        //{
        //    print("check " + lastChangeMessage.data.opponent.uniqueId);
        //}

        #region 正常流程
        if (!isBattleChanged)
        {
            sequence.AppendCallback(() =>
            {
                // 移除没有用过的随从
                minionNotUsed.Map(RemoveCard);
                heroNotUsed.Map(RemoveHero);
                //print(heroMaps.Count + ":" + heroNotUsed.Count);
                //minionNotUsed.Map(id=>print("remove uid:"+id));
            });
            sequence.Append(DecodeGameMessageBofore(changeMessage));
            sequence.Append(UpdateHeroMinions(changeMessage));
            sequence.Join(UpdateHero(changeMessage));
            if (isBattle) sequence.Join(UpdateOpponentHero(changeMessage));
            sequence.Join(UpdateOpponentMinions(changeMessage));
            sequence.Join(UpdateHeroHands(changeMessage));

            if (!isBattle) // 商店中
            {
                EnableInteractive(false);

                if (!predictToStop)
                {
                    sequence.AppendCallback(() =>
                    {
                        EnableInteractive(true);
                    });
                }

                CheckMerge();
            }
            else // 战斗中
            {
                EnableInteractive(false);
                EnableClickForDescription();
            }

            if (!isBattle)
                sequence.Append(SwitchBoardBG(changeMessage));
            sequence.Append(DecodeGameMessageAfter(changeMessage));

            //sequence.Append();
            //if (!isBattle && changeMessage.code == null && NextChangeMessage == null)
            //{
            //    sequence.AppendCallback(() =>
            //    {
            //        EnableInteractive(true);
            //    });
            //}

        }
        #endregion

        #region 转入战场
        else if (isBattle)
        {
            EnableInteractive(false);

            sequence.PrependCallback(() =>
            {
                minionNotUsed.Map(RemoveCard);
            });
            sequence.AppendInterval(1f);
            sequence.AppendCallback(() =>
            {
                SwitchBoardBG(changeMessage);
                Decorate.ShowChangeBoard(isBattle);
            });
            sequence.Append(UpdateHeroMinions(changeMessage));
            sequence.Join(UpdateHeroHands(changeMessage));
            sequence.Join(UpdateHero(changeMessage));
            sequence.Join(UpdateOpponentHero(changeMessage));
            sequence.Append(UpdateOpponentMinions(changeMessage));
            sequence.AppendInterval(1f);
            sequence.Append(DecodeGameMessageAfter(changeMessage));
            //sequence.Append(Decorate.ShowChangeBoard(isBattle));
        }
        #endregion

        #region 转入商店
        else
        {
            //EnableInteractive(false);



            if (!isBegin)
            {
                sequence.PrependInterval(2f);
            }
            sequence.AppendCallback(() =>
            {


                SwitchBoardBG(changeMessage);
                minionNotUsed.Map(RemoveCard);
                heroNotUsed.Map(RemoveHero);

                //EnableInteractive(true);
                Decorate.ShowChangeBoard(isBattle);
            });

            #region 清除cardMaps内的当前战场
            if (changeMessage != null && changeMessage.data != null)
            {
                HashSet<Card> cardCollect = new HashSet<Card>();
                cardCollect.UnionWith(changeMessage.data.heroBattlePile);
                cardCollect.UnionWith(changeMessage.data.opponentBattlePile);
                cardCollect.UnionWith(changeMessage.data.heroHandPile);
                List<CardStruct> cardStructCollect = cardCollect.Map(card => cardMaps.RemoveItem(card.uniqueId));

                sequence.AppendCallback(() =>
                {
                    cardStructCollect.Map(st =>
                    {
                        if (st != null)
                            RemoveCard(st.setting);
                    });
                });
            }
            #endregion

            sequence.Append(UpdateHeroMinions(changeMessage));
            sequence.Join(UpdateHero(changeMessage));
            sequence.Join(UpdateHeroHands(changeMessage));
            sequence.Append(UpdateOpponentMinions(changeMessage));
            sequence.Append(DecodeGameMessageAfter(changeMessage));
            //sequence.Append(Decorate.ShowChangeBoard(isBattle));


            // 冻结判定代码
            if (isFreeze)
                sequence.AppendInterval(1f);
            isFreeze = false;
        }
        #endregion




        if (isBegin) isBegin = false;
        lastChangeMessage = changeMessage;
        return sequence;
    }

    private ChangeMessage NextChangeMessage
    {
        get
        {

            if (messageSequences.Count > 0)
                return messageSequences.Peek();
            return null;
        }
    }

    private Tween DecodeGameMessageBofore(ChangeMessage changeMessage)
    {

        ChangeMessageCode code = changeMessage.code;
        if (code == null) return null;
        if (code.code == "damage")
        {
            if (code.pos1 != null)
            {
                var number = code.number.ToString();
                Sequence sequence = DOTween.Sequence();

                sequence.Append(GetCardBase(code.pos1)?.Sortable.ShowHit(number));


                return sequence;
            }
        }
        else if (code.code == "AOE")
        {
            if (code.trigger == "up")
            {
                return AOE.AnimateAOE(AOEType.Up);
            }
            else if (code.trigger == "down")
            {
                return AOE.AnimateAOE(AOEType.Down);
            }
            else
            {
                return AOE.AnimateAOE(AOEType.All);
            }
        }
        return null;
    }
    private Tween DecodeGameMessageAfter(ChangeMessage changeMessage)
    {
        ChangeMessageCode code = changeMessage.code;
        if (code == null) return null;
        if (code.code == "attack")
        {
            if (code.card1 != null && code.card2 != null)
            {
                return Attack(code.card1, code.card2);
            }
        }
        else if (code.code == "attackReady")
        {
            if (code.card1 != null && code.card2 != null)
            {
                return AttackReady(code.card1, code.card2);
            }
        }
        else if (code.code == "create")
        {
            if (code.card1 != null && code.pos1 != null)
            {
                Sequence sequence = DOTween.Sequence();
                CardStruct cardStruct = GetCard(code.card1);
                SetCardToSpcePos(cardStruct, code.pos1);


                Vector3 oldScale = Vector3.one;
                cardStruct.setting.transform.localScale = 0.4f * oldScale;

                cardStruct.setting.transform.localEulerAngles = new Vector3(8f, -90f, 8f);
                Transform cardTrans = cardStruct.setting.transform;
                cardTrans.gameObject.SetActive(false);
                DOTween.Sequence()
                    //.Append(cardStruct.setting.transform.DOScale(oldScale, 0.3f))
                    .PrependCallback(() =>
                    {
                        cardTrans.gameObject.SetActive(true);
                        cardTrans.gameObject.transform.localScale = Vector3.one;
                    })
                    .Append(cardTrans.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.OutQuad))
                    .Join(cardTrans.DOScale(1.14f * oldScale, 0.2f))
                    .Insert(0.5f, cardTrans.DOScale(oldScale, 0.2f).SetEase(Ease.OutBack));
                return DOTween.Sequence().AppendInterval(0.12f);
            }
        }
        else if (code.code == "trigger")
        {

            if (code.pos1 != null && !string.IsNullOrEmpty(code.trigger))
            {
                if (code.trigger == "亡语")
                {
                    return DOTween.Sequence()
                        .Append(PrefabStayLong.Show("亡语触发", GetChessPosition(code.pos1)))
                        .AppendInterval(0.7f);
                }
                else if (GetCard(code.pos1) != null)
                {
                    return GetCard(code.pos1).setting.Triger(code.trigger);
                }
            }
        }

        else if (code.code == "play")
        {
            if (code.card1 != null)
            {
                CardStruct cardStruct = GetCard(code.card1);
                Vector3 aimPos = GetChessPosition(cardStruct.position);
                Vector3 aimPosHigh = new Vector3(aimPos.x, aimPos.y, aimPos.z - 1f);
                return DOTween.Sequence()
                    //.AppendInterval(1f)
                    .Append(cardStruct.setting.transform.DOLocalMove(aimPos, 0.3f).SetEase(Ease.InSine));
                //.Join(cardStruct.setting.transform.DOScale(Vector3.one, 0.2f));


            }
        }
        else if (code.code == "die")
        {
            if (code.card1 != null)
            {
                CardSetting setting = GetCard(code.card1).setting;
                cardMaps.Remove(code.card1.uniqueId);
                return DOTween.Sequence()
                    //.Append(setting.transform.DOLocalRotate(new Vector3(8f, -60f, 8f), 0.3f).SetEase(Ease.OutExpo))
                    .AppendInterval(0.3f)
                    .Append(setting.transform.DOShakePosition(0.3f, 0.1f, 30))
                    .AppendCallback(() =>
                    {
                        //setting.transform.localEulerAngles = Vector3.zero;
                        RemoveCard(setting);
                    });
            }
        }
        else if (code.code == "choose")
        {
            return ChooseCard(code.card1, code.card2, code.card3, code.card4);
        }
        else if (code.code == "selectTarget")
        {
            return SelectTarget(code.cardList);
        }
        else if (code.code == "merge")
        {
            return MergeCard(code.card1, code.card2, code.card3, code.card4);
        }
        else if (code.code == "reborn")
        {
            if (code.card1 != null && code.pos1 != null)
            {
                Sequence sequence = DOTween.Sequence();
                CardStruct cardStruct = GetCard(code.card1);
                SetCardToSpcePos(cardStruct, code.pos1);


                Vector3 oldScale = cardStruct.setting.transform.localScale;
                cardStruct.setting.transform.localScale = Vector3.zero;

                Transform cardTrans = cardStruct.setting.transform;
                cardTrans.gameObject.SetActive(false);
                return DOTween.Sequence()
                    .Append(cardTrans.DOScale(oldScale, 0.3f))
                    .Join(PrefabStayLong.Show("Reborn", GetChessPosition(code.pos1) + new Vector3(0, 0, -0.0001f)))
                    .AppendInterval(0.9f);
            }
        }
        else if (code.code == "cleave")
        {
            return GetCard(code.card1).setting.ShowCleave();
        }
        else if (code.code == "win")
        {
            GameEndWindow.Win();
            isGameover = true;
        }
        else if (code.code == "lose")
        {
            GameEndWindow.Lose();
            isGameover = true;
        }
        return null;
    }

    IPile<Card> handPileSaves = null; // 用于保存手牌

    private Tween UpdateOpponentMinions(ChangeMessage changeMessage)
    {
        if (changeMessage.data == null) return null;
        IPile<Card> opponentBattlePile = changeMessage.data.opponentBattlePile;
        Sequence sequenceAfter = DOTween.Sequence();

        // opponent battlePile
        for (int i = 0; i < 7; i++)
        {
            if (opponentBattlePile == null) break;
            if (opponentBattlePile[i] == null) continue;
            Card card = opponentBattlePile[i];
            if (!isBattle && isFreeze) card.keyWords.Add(Keyword.Freeze);  // 冻结判定代码
            CardStruct cardStruct = GetCard(card);

            // 设置星级
            if (!isBattle) cardStruct.setting.SetStarBySetedCard();

            sequenceAfter.Insert(0, SetCard(cardStruct.setting, card));
            if (cardStruct.position.type == PositionType.None)
            {
                cardStruct.setting.gameObject.SetActive(false);
                sequenceAfter.Insert(0, MoveCardToSpcePosFromSide(cardStruct,
                    new CardPosition(PositionType.OpponentBattlePile, i)));
            }
            else if (isBattleChanged || !cardStruct.position.Equals(new CardPosition(PositionType.OpponentBattlePile, i)))
            {
                sequenceAfter.Insert(0, MoveCardToSpcePos(cardStruct, new CardPosition(PositionType.OpponentBattlePile, i)));
            }
        }
        return sequenceAfter;
    }

    private void CheckMerge()
    {
        var changeMessage = curChangeMessage;

        if (changeMessage == null) return;
        if (changeMessage.data == null) return;

        List<Card> cards = new List<Card>();
        cards.AddRange(changeMessage.data.heroHandPile.ToList());
        cards.AddRange(changeMessage.data.heroBattlePile.ToList());
        cards.AddRange(changeMessage.data.opponentBattlePile.ToList());

        cards = cards.Where(card => !card.isGold && card.cardType == CardType.Minion).ToList();
        var mergeableCards = cards.GroupBy(card => card.id).Where(x => x.Count() >= 3).SelectMany(x => x).ToList();
        mergeableCards.Where(changeMessage.data.opponentBattlePile.Contains)
            .Map(card => GetCard(card).setting.ShowMerge(true));
    }


    private Tween UpdateHeroHands(ChangeMessage changeMessage)
    {
        if (changeMessage.data == null) return null;
        IPile<Card> heroHandPile = changeMessage.data.heroHandPile;
        Sequence sequenceBefore = DOTween.Sequence();
        // hero hand
        if (heroHandPile != null)
        {
            handPileSaves = heroHandPile;
            for (int i = 0; i < heroHandPile.Count; i++)
            {
                Card item = heroHandPile[i];
                sequenceBefore.Insert(0, SetCard(GetCard(item).setting, item));
            }
            sequenceBefore.Insert(0, FlushHandPile());
        }
        return sequenceBefore;
    }

    private Tween UpdateHero(ChangeMessage changeMessage)
    {
        if (changeMessage.data == null) return null;
        //print("changeMessage" + changeMessage.data.hero.uniqueId);

        Card hero = changeMessage.data.hero;
        Sequence sequence = DOTween.Sequence();

        HeroStruct heroStruct = GetHero(hero);
        sequence.Append(SetHero(heroStruct.herosetting, hero));

        if (heroStruct.position.type == PositionType.None)
        {
            sequence.Append(MoveCardToSpcePosFromSide(heroStruct, new CardPosition(PositionType.Hero)));
        }
        else if (isBattleChanged)
        {
            sequence.Append(MoveCardToSpcePosFromSide(heroStruct, new CardPosition(PositionType.Hero)));
        }
        return sequence;
    }

    private Tween UpdateOpponentHero(ChangeMessage changeMessage)
    {
        if (changeMessage.data == null) return null;
        //print("changeMessage" + changeMessage.data.hero.uniqueId);

        if (!isBattle) return null;

        Card hero = changeMessage.data.opponent;
        Sequence sequence = DOTween.Sequence();

        HeroStruct heroStruct = GetHero(hero);
        //print("update opponent");
        sequence.Append(SetHero(heroStruct.herosetting, hero));

        if (heroStruct.position.type == PositionType.None)
        {
            sequence.Append(MoveCardToSpcePosFromSide(heroStruct, new CardPosition(PositionType.Opponent)));
        }
        else if (isBattleChanged)
        {
            sequence.Append(MoveCardToSpcePosFromSide(heroStruct, new CardPosition(PositionType.Opponent)));
        }
        return sequence;
    }


    private Tween UpdateHeroMinions(ChangeMessage changeMessage)
    {
        if (changeMessage.data == null) return null;
        IPile<Card> heroBattlePile = changeMessage.data.heroBattlePile;

        Sequence sequenceBefore = DOTween.Sequence();


        // hero battlePile
        for (int i = 0; i < 7; i++)
        {
            if (heroBattlePile[i] == null) continue;
            Card card = heroBattlePile[i];
            CardStruct cardStruct = GetCard(card);
            sequenceBefore.Insert(0, SetCard(cardStruct.setting, card));

            // 设置星级
            if (!isBattle) cardStruct.setting.HideStar();

            if (cardStruct.position.type == PositionType.None)
            {
                if (isBattleChanged)
                {
                    cardStruct.setting.gameObject.SetActive(false);
                    sequenceBefore.Insert(0, MoveCardToSpcePosFromSide(cardStruct,
                        new CardPosition(PositionType.HeroBattlePile, i)));
                }
                else
                    sequenceBefore.Insert(0, SetCardToSpcePos(cardStruct,
                        new CardPosition(PositionType.HeroBattlePile, i)));

            }
            else if (isBattleChanged || !cardStruct.position.Equals(new CardPosition(PositionType.HeroBattlePile, i)))
            {
                sequenceBefore.Insert(0, MoveCardToSpcePos(cardStruct, new CardPosition(PositionType.HeroBattlePile, i)));
            }
        }


        //sequence.AppendInterval(0.0000001f); // in case of sequence is empty
        return sequenceBefore;
    }


    Tween FlushHandPile()
    {
        if (handPileSaves == null || handPileSaves.Count == 0) return null;
        //if (isBattle) // && isBattleChanged)
        //{
        //    List<Card> cloneHandPile = new List<Card>();
        //    cloneHandPile.AddRange(handPileSaves);
        //    for (int i = 0; i < cloneHandPile.Count; i++)
        //    {
        //        CardStruct cardStruct = GetCard(cloneHandPile[i]);
        //        //cardStruct.setting.gameObject.SetActive(false);
        //        HideObject(cardStruct.setting.transform);
        //    }
        //}
        if (true)//!isBattle)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < handPileSaves.Count; i++)
            {
                CardStruct cardStruct = GetCard(handPileSaves[i]);
                //cardStruct.setting.gameObject.SetActive(true);
                //cardStruct.setting.gameObject.SetActive(false);

                var newPos = new CardPosition(PositionType.HeroHandPile, i)
                {
                    pileTotal = handPileSaves.Count
                };


                if (newPos.AllEquals(cardStruct.position) && !isBattleChanged)
                {
                    sequence.AppendCallback(() =>
                    {
                        cardStruct.setting.gameObject.SetActive(true);
                    });
                }
                else
                {
                    int dlayer = 0;
                    if (cardStruct.setting.transform.localPosition.y > heroPointTransfroms[0].position.y)
                    {
                        dlayer = 100;
                    }

                    if (isBattleChanged)
                    {
                        cardStruct.setting.gameObject.SetActive(false);
                        sequence.Insert(0, MoveCardToSpcePosFromSide(cardStruct, newPos, 0.3f, dlayer));
                    }
                    else
                    {
                        sequence.Insert(0, MoveCardToSpcePos(cardStruct, newPos, 0.3f, dlayer));
                    }

                    if (isBattle)
                    {
                        sequence.Insert(0, cardStruct.setting.transform.DOScale(Vector3.one * 0.8f, 0.3f));
                    }
                }
            }
            return sequence;
        }
    }



    public void SendGameMessage(ChangeMessage changeMessage)
    {
        EnableInteractive(false);
        //Debug.Log("client send:" + JsonUtility.ToJson(changeMessage));
        //Bridge.GetClient().SendMessage(changeMessage);
        GameAnimationSetting.instance.changeMessagesToServer.Add(changeMessage);

        // 预测
        if (changeMessage.code != null)
        {
            if (changeMessage.code.code == "play")
            {
                Card card = GetCard(changeMessage.code.pos1).setting.setedCard;
                if (card.HasKeyword(Keyword.Magnetic) || card.GetProxys(ProxyEnum.Battlecry) != null)
                {
                    predictToStop = true;
                }
            }
        }
    }

    public Tween SwitchBoardBG(ChangeMessage changeMessage)
    {
        if (changeMessage.data == null) return null;
        //if (isBattle == changeMessage.data.isBattle) return;

        if (isBattle && isBattleChanged)
        {
            BG.sprite = BattleBoardBGSprite;
            Stars.SetStar(-1);
            Decorate.SetFlushCost(-1);
            Decorate.SetFreezeCost(-1);
            Decorate.SetUpgrateCost(-1);
            Decorate.SetCoinNumber(-1);
            Decorate.SetHeroPower(-3);
            Decorate.SetBattle(false);
        }
        else if (!isBattle)
        {
            BG.sprite = ShopBoardBGSprite;
            Stars.SetStar(changeMessage.data.star);
            Decorate.SetCoinNumber(changeMessage.data.leftCoins);
            //EnableBoardDecorate(true);
        }

        return null;
    }

    void EnableBoardDecorate(bool isOk)
    {
        //print("decorate " + isOk + " " + Time.time);
        var changeMessage = curChangeMessage;


        if (isBattle)
        {
            return;
        }

        if (isOk)
        {
            Decorate.SetBattle(true, () =>
            {
                EnableInteractive(false);
                SendGameMessage(new ChangeMessage()
                {
                    code = new ChangeMessageCode()
                    {
                        code = "startCombat"
                    },
                    data = null
                });
            });
            Decorate.SetFlushCost(changeMessage.data.flushCost, () =>
            {
                if (changeMessage.data.flushCost >= 0
                    && changeMessage.data.leftCoins >= changeMessage.data.flushCost)
                {
                    EnableInteractive(false);
                    SendGameMessage(new ChangeMessage()
                    {
                        code = new ChangeMessageCode()
                        {
                            code = "flush"
                        },
                        data = null
                    });
                }
            });
            Decorate.SetFreezeCost(changeMessage.data.freezeCost, () =>
            {
                if (changeMessage.data.freezeCost >= 0
                    && changeMessage.data.leftCoins >= changeMessage.data.freezeCost)
                {
                    isFreeze = !isFreeze; // 冻结判定代码
                    EnableInteractive(false);
                    SendGameMessage(new ChangeMessage()
                    {
                        code = new ChangeMessageCode()
                        {
                            code = "freeze"
                        }
                    });
                }
            });
            Decorate.SetUpgrateCost(changeMessage.data.upgradeCost, () =>
            {
                if (changeMessage.data.upgradeCost >= 0
                    && changeMessage.data.leftCoins >= changeMessage.data.upgradeCost)
                {
                    EnableInteractive(false);
                    SendGameMessage(new ChangeMessage()
                    {
                        code = new ChangeMessageCode()
                        {
                            code = "upgrade"
                        },
                        data = null
                    });
                }
            });

            if (changeMessage.data.hero != null)
            {
                Decorate.SetHeroPower(changeMessage.data.hero.cost, () =>
                {
                    if (changeMessage.data.leftCoins >= changeMessage.data.hero.cost)
                    {
                        EnableInteractive(false);
                        SendGameMessage(new ChangeMessage()
                        {
                            code = new ChangeMessageCode()
                            {
                                code = "heroPower"
                            }
                        });
                    }
                }, () =>
                {
                    DescriptionWindow.Show(changeMessage.data.hero, tween =>
                    {
                        //EnableInteractive(true);
                    });
                });
            }
            else
            {
                Decorate.SetHeroPower(-3);
            }
        }
        else
        {

            Decorate.SetBattle(true);
            Decorate.SetFlushCost(changeMessage.data.flushCost);
            Decorate.SetFreezeCost(changeMessage.data.freezeCost);
            Decorate.SetUpgrateCost(changeMessage.data.upgradeCost);

            if (changeMessage.data.hero != null)
            {
                Decorate.SetHeroPower(changeMessage.data.hero.cost);
            }
            else
            {
                Decorate.SetHeroPower(-3);
            }
        }
    }



    #endregion

    #region test

    [ContextMenu("Animation InitTest")]
    public void Test()
    {
        board.AddToHandPile(CardBuilder.NewCard(5));
        board.AddToHandPile(CardBuilder.NewCard(5));
        board.AddToHandPile(CardBuilder.NewCard(73));
        board.AddToHandPile(CardBuilder.NewCard(75));
        board.AddToHandPile(CardBuilder.NewCard(79));
        board.AuraCheck();
        board.SendGameMessage(new ChangeMessage()
        {
            data = board.GetDataForSend()
        });
    }

    [ContextMenu("Test2")]
    public void Test2()
    {
        Bridge.GetServer().SendMessage(new ChangeMessage()
        {
            code = new ChangeMessageCode()
            {
                code = "choose",
                card1 = CardBuilder.NewCard(1),
                card2 = CardBuilder.NewCard(6),
                card3 = CardBuilder.NewCard(9),
                card4 = CardBuilder.NewCard(9),
            }
        });
    }

    [ContextMenu("Test3")]
    public void Test3()
    {
        player.battlePile[0].attack = 13;
        player.battlePile[2].keyWords.Remove(Keyword.DivineShield);
        Bridge.GetServer().SendMessage(new ChangeMessage()
        {
            data = new ChangeMessageGameData()
            {
                isBattle = true,
                heroBattlePile = player.battlePile,
                heroHandPile = player.handPile,
                flushCost = -1,
                upgradeCost = -1,
                freezeCost = -1
            },
            code = new ChangeMessageCode()
            {
                code = "trigger",
                trigger = "亡语",
                pos1 = new CardPosition(PositionType.HeroBattlePile, 3),
            }
        });

        Bridge.GetServer().SendMessage(new ChangeMessage()
        {
            code = new ChangeMessageCode()
            {
                code = "damage",
                number = 4,
                pos1 = new CardPosition(PositionType.HeroBattlePile, 1),
            },
        });
        Bridge.GetServer().SendMessage(new ChangeMessage()
        {
            code = new ChangeMessageCode()
            {
                code = "damage",
                number = 4,
                pos1 = new CardPosition(PositionType.HeroBattlePile, 0),
            },
        });
    }

    [ContextMenu("InitTest")]
    public void InitTest()
    {
        board.InitTest();
    }

    [ContextMenu("AttackTest")]
    public void AttackTest()
    {
        board.Combat();
    }
    #endregion

    #region tool fuction for decode
    Tween SelectTarget(List<Card> cardList)
    {
        int len = cardList.Count;
        int cnt = 0;
        Sequence sequence = DOTween.Sequence();
        EnableInteractive(false);
        //Debug.Log("flase outside");
        sequence.PrependCallback(() =>
        {
            EnableInteractive(false);
            //Debug.Log("flase inside");
            cardList.Map(GetCard).Map(st =>
            {
                var index = cnt++;
                //print("deal card:" + st.setting.setedCard.name + st.setting.setedCard.uniqueId);
                //MoveCardToSpcePos(st, new CardPosition(PositionType.Choose, index) { pileTotal = len }, 0.3f, 100);
                st.setting.SetSortingOrder(200);
                st.setting.transform.DOLocalMove(new Vector3(0, 0, -0.5f), 0.1f).SetRelative();
                Mask.gameObject.SetActive(true);
                st.setting.EnableTap(true, () =>
                {
                    Mask.gameObject.SetActive(false);
                    cardList.Map(GetCard).Map(stru => MoveCardToSpcePos(stru, stru.position));
                    EnableInteractive(false);
                    DOTween.Sequence().AppendInterval(0.3f)
                        .AppendCallback(() =>
                        {
                            SendGameMessage(new ChangeMessage()
                            {
                                code = new ChangeMessageCode()
                                {
                                    code = "selectTarget",
                                    number = index
                                }
                            });
                        });
                });
            });
        });
        return sequence;
    }

    Tween ChooseCard(params Card[] cards)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            EnableInteractive(false);
        });
        //print("close");
        sequence.Append(ChooseWindow.Show((Tween tween, int index) =>
        {
            //print("out show" + index);
            DOTween.Sequence().Append(tween).AppendCallback(() =>
            {
                SendGameMessage(new ChangeMessage()
                {
                    code = new ChangeMessageCode()
                    {
                        code = "choose",
                        number = index
                    }
                });
            });
        }, cards));

        //print("in choose");
        //List<Card> cardList = cards.Filter(card => card != null);
        //int len = cardList.Count;
        //int cnt = 0;
        //EnableInteractive(false);
        //cardList.Map(GetCard).Map(st=> {
        //    var index = cnt++;
        //    print("deal card:" + st.setting.setedCard.name + st.setting.setedCard.uniqueId);
        //    MoveCardToSpcePos(st, new CardPosition(PositionType.Choose, index) { pileTotal = len}, 0.3f, 100);
        //    Mask.gameObject.SetActive(true);
        //    st.setting.EnableTap(true, () => {
        //        Mask.gameObject.SetActive(false);
        //    });
        //});
        return sequence;
    }

    Tween MergeCard(params Card[] cards)
    {
        Vector3 centerPos = GetCenterPos();
        Card newCard = cards[0];
        CardStruct cardStruct = GetCard(newCard);
        cardStruct.setting.gameObject.SetActive(false);
        cardStruct.setting.transform.position = centerPos;

        Sequence sequence = DOTween.Sequence();

        cards.Filter(card => card != null && card != newCard)
            .Map(GetCard)
            .Map(st => st.setting.transform.DOLocalMove(centerPos, 0.3f).OnStart(() => { st.setting.SetSortingOrder(10); }))
            .Map(move => sequence.Insert(0, move));

        return sequence.AppendInterval(1f);
    }
    #endregion

    #region Tool function

    private Vector3 GetCenterPos()
    {
        return choosePointTransforms[1].transform.position;
    }

    private readonly Map<int, CardStruct> cardMaps = new Map<int, CardStruct>();  // 卡牌数据与卡牌实例的对应关系

    void GetCardSideEffect(int uniqueId) // 为了移除不存在的随从
    {
        minionNotUsed.Remove(uniqueId);
    }

    public CardStruct NewCard(Card card)
    {
        GameController.UnlockCard(card.id);
        CardStruct cardStruct = new CardStruct()
        {
            setting = BIFStaticTool.GetPrefabInPool("Card", GameObjects).GetComponent<CardSetting>()
        };
        cardStruct.setting.setedCard = null;
        cardStruct.setting.transform.position = Vector3.zero;
        cardStruct.setting.SetByCard(card);
        cardStruct.setting.SetSortingOrder(0);
        cardStruct.setting.transform.localScale = Vector3.one;
        cardStruct.setting.EnableDrag(false);
        cardStruct.setting.EnableLongPress(false);
        return cardStruct;
    }

    public class HeroStruct : CardStructBase
    {
        public HeroSetting herosetting = null;

        public override Transform Transform => herosetting.transform;

        public override ISortable Sortable => herosetting;
    }

    public HeroStruct NewHero(Card card)
    {
        HeroStruct heroStruct = new HeroStruct()
        {
            herosetting = BIFStaticTool.GetPrefabInPool("Hero", GameObjects).GetComponent<HeroSetting>(),
        };
        heroStruct.herosetting.SetByCard(card);
        return heroStruct;
    }
    public void RemoveHero(HeroStruct heroStruct)
    {
        if (heroStruct.herosetting != null)
        {
            BIFStaticTool.PrefabPool("Hero", heroStruct.herosetting.gameObject);
        }
    }
    public void RemoveHero(Card card)
    {
        if (card.cardType != CardType.Hero) return;
        RemoveHero(card.uniqueId);
    }
    public void RemoveHero(int uniqueId)
    {
        if (heroMaps.ContainsKey(uniqueId))
        {
            RemoveHero(heroMaps[uniqueId]);
            heroMaps.Remove(uniqueId);
        }
    }



    private Map<int, HeroStruct> heroMaps = new Map<int, HeroStruct>();
    public HeroStruct GetHero(Card card)
    {
        heroNotUsed.Remove(card.uniqueId);
        if (heroMaps.ContainsKey(card.uniqueId))
        {
            return heroMaps[card.uniqueId];
        }
        //print("Uid:" + card.uniqueId);
        HeroStruct heroStruct = NewHero(card);
        heroMaps[card.uniqueId] = heroStruct;
        //print("addHero"+card.uniqueId);
        return heroStruct;
    }



    /// <summary>
    /// 获取卡牌（如果没有则创建）
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public CardStruct GetCard(Card card)
    {
        GetCardSideEffect(card.uniqueId);
        if (cardMaps.ContainsKey(card.uniqueId))
        {
            return cardMaps[card.uniqueId];
        }

        cardMaps[card.uniqueId] = NewCard(card);
        return cardMaps[card.uniqueId];
    }
    public CardStruct GetCard(CardPosition position)
    {
        foreach (var st in cardMaps)
        {
            if (st.Value.position.Equals(position))
            {
                GetCardSideEffect(st.Key);
                return st.Value;
            }
        }
        return null;
    }
    public CardStruct GetCard(CardSetting cardSetting)
    {
        foreach (var st in cardMaps)
        {
            if (st.Value.setting == cardSetting)
            {
                GetCardSideEffect(st.Key);
                return st.Value;
            }
        }
        return null;
    }

    private CardStructBase GetCardBase(CardPosition position)
    {
        CardStruct cardStruct = GetCard(position);
        if (cardStruct != null)
        {
            return cardStruct;
        }
        foreach (var st in heroMaps)
        {
            if (st.Value.position.Equals(position))
            {
                GetCardSideEffect(st.Key);
                return st.Value;
            }
        }
        return null;
    }
    private CardStructBase GetCardBase(Card card)
    {
        if (card.cardType == CardType.Hero)
        {
            return GetHero(card);
        }
        else
        {
            return GetCard(card);
        }
    }


    private Vector3 oldScale = Vector3.zero;
    public Tween SetCard(CardSetting setting, Card card)
    {


        if (!setting.gameObject.activeSelf)
        {
            //print("正在设置隐藏的card"+card.name);
            return null;
        }
        CardStruct cardStruct = GetCard(card);
        CardSetting cardSetting = cardStruct.setting;
        Vector2Int body = card.GetMinionBody();
        bool isAttackChanged = setting.attackText.text != body.x.ToString();
        bool isHealthChanged = setting.healthText.text != body.y.ToString();
        bool isBreakDivineShield = setting.圣盾.gameObject.activeSelf
            && !card.HasKeyword(Keyword.DivineShield);
        bool isGainDivineShield = !setting.圣盾.gameObject.activeSelf
            && card.HasKeyword(Keyword.DivineShield);

        setting.SetByCard(card);

        Sequence sequence = DOTween.Sequence();

        if (oldScale == Vector3.zero) oldScale = setting.healthText.transform.localScale;

        if (isAttackChanged && card.cardType == CardType.Minion)
        {
            DOTween.Sequence().Append(setting.attackText.transform.DOScale(1.2f * oldScale, 0.1f))
                .Append(setting.attackText.transform.DOScale(oldScale, 0.1f));
        }
        if (isHealthChanged && card.cardType == CardType.Minion)
        {
            //sequence.Insert(0, setting.healthText.transform.DOPunchScale(0.24f * Vector3.one, 0.2f).SetRelative());
            //setting.healthText.transform.DOKill();
            //setting.healthText.transform.DOPunchScale(0.1f * Vector3.one, 0.2f).SetRelative();
            //setting.healthText.transform.DOPunchScale(0.2f * Vector3.one, 0.2f).OnComplete(() => {
            //    setting.healthText.transform.localScale = oldScale;
            //}).OnStart(()=> {
            //    setting.healthText.transform.localScale = oldScale;
            //});
            DOTween.Sequence().Append(setting.healthText.transform.DOScale(1.2f * oldScale, 0.1f))
                .Append(setting.healthText.transform.DOScale(oldScale, 0.1f));
        }
        if (isBreakDivineShield)
        {
            var render = setting.圣盾.GetComponent<SpriteRenderer>();
            Vector3 scale = render.transform.localScale;
            render.gameObject.SetActive(true);

            sequence.Insert(0, setting.圣盾.transform.DOScale(0.5f * Vector3.one, 0.3f).SetRelative());
            sequence.Insert(0.15f, render.DOFade(0, 0.3f).OnComplete(() =>
            {
                render.color = Color.white;
                render.transform.localScale = scale;
                render.gameObject.SetActive(false);
            }));
        }
        else if (isGainDivineShield)
        {
            Vector3 scale = setting.圣盾.transform.localScale;
            setting.圣盾.transform.localScale = Vector3.zero;
            sequence.Insert(0, setting.圣盾.transform.DOScale(scale, 0.3f));
        }

        return sequence;
    }

    private Vector3 heroOldScale = Vector3.zero;
    public Tween SetHero(HeroSetting setting, Card card)
    {
        var body = card.GetMinionBody();
        bool isAttackChanged = setting.attackText.text != body.x.ToString();
        bool isHealthChanged = setting.healthText.text != body.y.ToString();
        setting.SetByCard(card);

        Sequence sequence = DOTween.Sequence();

        if (heroOldScale == Vector3.zero) heroOldScale = setting.healthText.transform.localScale;

        if (isAttackChanged)
        {
            setting.attackText.transform.DOPunchScale(0.2f * Vector3.one, 0.2f).OnComplete(() =>
            {
                setting.attackText.transform.localScale = heroOldScale;
            });
        }
        if (isHealthChanged)
        {
            setting.healthText.transform.DOPunchScale(0.2f * Vector3.one, 0.2f).OnComplete(() =>
            {
                setting.healthText.transform.localScale = heroOldScale;
            });
        }
        return sequence;
    }

    /// <summary>
    /// 回收卡牌
    /// </summary>
    /// <param name="card"></param>
    void RemoveCard(Card card)
    {
        RemoveCard(card.uniqueId);
    }
    void RemoveCard(int uniqueId)
    {
        CardStruct cardStruct = cardMaps.GetByDefault(uniqueId, null);
        if (cardStruct != null)
        {
            RemoveCard(cardStruct.setting);
        }
        cardMaps.Remove(uniqueId);
    }
    void RemoveCard(CardSetting setting)
    {
        if (setting != null)
        {
            setting.transform.DOScale(Vector3.zero, 0.08f).OnComplete(() =>
            {
                setting.transform.localScale = Vector3.one;
                setting.transform.position = Vector3.zero;
                BIFStaticTool.PrefabPool("Card", setting.gameObject);
            });
        }
    }

    Tween HideObject(Transform obj)
    {
        Vector3 oldScale = obj.localScale;
        return obj.DOScale(Vector3.zero, 0.08f).OnComplete(() =>
        {
            obj.localScale = oldScale;
            obj.gameObject.SetActive(false);
        });
    }



    public abstract class CardStructBase
    {
        public CardPosition position = new CardPosition();

        public virtual Transform Transform { get; }

        public virtual ISortable Sortable { get; }
    }

    public class CardStruct : CardStructBase
    {
        public CardSetting setting = null;

        public override Transform Transform { get => setting.transform; }

        public override ISortable Sortable => setting;
    }


    public Tween Attack(Card oriCard1, Card oriCard2)
    {
        CardStructBase cardStructBase1 = GetCardBase(oriCard1);
        CardStructBase cardStructBase2 = GetCardBase(oriCard2);
        Transform card1 = cardStructBase1.Transform;
        Transform card2 = cardStructBase2.Transform;
        if (card1 != null && card2 != null)
        {
            Sequence sequence = DOTween.Sequence();
            Vector3 fromPos = GetChessPosition(cardStructBase1.position);
            Vector3 fromPosHigh = fromPos + new Vector3(0, 0, -0.6f);
            Vector3 toPos = card2.transform.position;
            Vector3 aimPos = Vector3.Lerp(fromPosHigh, toPos, 0.76f);
            Vector3 backPos = Vector3.LerpUnclamped(fromPosHigh, toPos, -0.06f);
            Vector3 heroReturnPos = fromPos + new Vector3(0, 0, -0.6f);
            if (oriCard1.cardType != CardType.Hero)
            {
                sequence
                    .AppendInterval(0.1f)
                    .Append(card1.transform.DOLocalMove(aimPos, 0.16f).SetEase(Ease.InFlash))
                    .InsertCallback(0.14f, () => { GameAnimationSetting.instance.PlayAudioHit(); })
                    .AppendCallback(() =>
                    {
                        
                        card1.transform.DOLocalMove(fromPos, 0.2f).SetEase(Ease.OutFlash).OnComplete(() =>
                        {
                            //card1.SetSortingOrderBy(-100);
                            cardStructBase1.Sortable.SetSortingOrder(0);
                        });
                    });
            }
            else
            {
                sequence
                    .AppendInterval(0.1f)
                    .Append(card1.transform.DOLocalMove(aimPos, 0.2f).SetEase(Ease.InFlash))
                    .InsertCallback(0.18f, () => { GameAnimationSetting.instance.PlayAudioHit(); })
                    .AppendCallback(() =>
                    {
                        card1.transform.DOLocalMove(heroReturnPos, 0.32f).SetEase(Ease.OutFlash).OnComplete(() =>
                        {
                            card1.transform.DOLocalMove(fromPos, 0.2f);
                            cardStructBase1.Sortable.SetSortingOrder(0);
                        });
                    });
            }

            return sequence;
        }

        return null;
    }
    public Tween AttackReady(Card oriCard1, Card oriCard2)
    {
        CardStructBase cardStructBase1 = GetCardBase(oriCard1);
        CardStructBase cardStructBase2 = GetCardBase(oriCard2);
        Transform card1 = cardStructBase1.Transform;
        Transform card2 = cardStructBase2.Transform;
        if (card1 != null && card2 != null)
        {
            Sequence sequence = DOTween.Sequence();
            Vector3 fromPos = GetChessPosition(cardStructBase1.position);
            Vector3 fromPosHigh = fromPos + new Vector3(0, 0, -0.6f);
            Vector3 toPos = card2.transform.position;
            Vector3 aimPos = Vector3.Lerp(fromPosHigh, toPos, 0.76f);
            Vector3 backPos = Vector3.LerpUnclamped(fromPosHigh, toPos, -0.06f);
            sequence
                .PrependCallback(() => { cardStructBase1.Sortable.SetSortingOrderBy(100); })
                .AppendInterval(0.1f)
                .Append(card1.DOLocalMove(backPos, 0.24f).SetEase(Ease.OutSine));

            if (oriCard1.cardType == CardType.Hero)
            {
                sequence.AppendInterval(0.3f);
            }
            return sequence;
        }

        return null;
    }



    private Tween MoveCardToSpcePos(CardStructBase cardStruct, CardPosition pos, float duration = 0.3f, int dlayer = 0)
    {
        cardStruct.position = new CardPosition(pos.type, pos.pos)
        {
            pileTotal = pos.pileTotal
        };
        int revertLayer = -dlayer;
        if (pos.type == PositionType.HeroHandPile)
        {
            revertLayer = pos.pos - 15;
            //print("pos "+pos.pos+" "+revertLayer);
        }
        else if (pos.type == PositionType.Choose)
        {
            revertLayer = 2001;
        }
        else
        {
            revertLayer = 0;
        }

        return DOTween.Sequence().AppendInterval(duration).PrependCallback(() =>
        {
            cardStruct.Transform.transform.DOLocalMove(GetChessPosition(pos), duration).OnStart(() =>
            {
                cardStruct.Transform.gameObject.SetActive(true);
                cardStruct.Sortable.SetSortingOrderBy(dlayer);
                //print("move Start");
            }).OnComplete(() =>
            {
                cardStruct.Sortable.SetSortingOrder(revertLayer);
                //print("move Complete");
            });
        });
    }

    private Tween MoveCardToSpcePosFromSide(CardStructBase cardStruct, CardPosition pos, float duration = 0.3f, int dlayer = 0)
    {
        if (pos.type == PositionType.HeroBattlePile || pos.type == PositionType.OpponentBattlePile)
        {
            if (pos.pos <= 3)
            {
                cardStruct.Transform.transform.position = GetChessPosition(new CardPosition(pos.type, 3));
            }
            else
            {
                cardStruct.Transform.transform.position = GetChessPosition(new CardPosition(pos.type, 6));
            }
        }
        else if (pos.type == PositionType.HeroHandPile)
        {
            cardStruct.Transform.transform.position = heroHandPileTransfroms[1 + (isBattle ? 2 : 0)].position;
        }
        else if (pos.type == PositionType.Hero || pos.type == PositionType.Opponent)
        {
            //cardStruct.Transform.transform.position = GetChessPosition(pos) + new Vector3(0, 0, -1);

            if (pos.type == PositionType.Opponent)
                cardStruct.Transform.gameObject.SetActive(false);


            cardStruct.position = pos;
            return DOTween.Sequence()
                .AppendCallback(() =>
                {
                    cardStruct.Transform.gameObject.SetActive(true);
                })
                .Append(cardStruct.Transform.DOLocalMove(GetChessPosition(pos) + new Vector3(0, 0, -0.3f), 0.5f).SetEase(Ease.OutExpo))
                .Append(cardStruct.Transform.DOLocalMove(GetChessPosition(pos), 0.2f));
        }

        return MoveCardToSpcePos(cardStruct, pos, duration);
    }




    private Tween SetCardToSpcePos(CardStruct cardStruct, CardPosition pos)
    {
        cardStruct.position = new CardPosition(pos.type, pos.pos);
        Sequence sequence = DOTween.Sequence();
        sequence.PrependCallback(() =>
        {
            cardStruct.setting.gameObject.SetActive(true);
            cardStruct.setting.transform.position = GetChessPosition(pos);
        });
        return sequence;
    }


    /// <summary>
    /// 0~1 放Points 2~3放ShopPoints
    /// </summary>
    private readonly Transform[,] pointTransfroms = new Transform[4, 7];
    /// <summary>
    /// 0战斗我方英雄 1战斗敌方英雄 2商店我方英雄
    /// </summary>
    private readonly Transform[] heroPointTransfroms = new Transform[3];
    /// <summary>
    /// 0手牌左侧 1手牌右侧 2战斗手牌左侧 3战斗手牌右侧
    /// </summary>
    private readonly Transform[] heroHandPileTransfroms = new Transform[4];

    /// <summary>
    ///  发现
    /// </summary>
    private readonly Transform[] choosePointTransforms = new Transform[3];


    private void InitPointTransfroms()
    {
        for (int who = 0; who < 2; who++)
        {
            for (int position = 0; position < 7; position++)
            {
                pointTransfroms[who, position] = Points.transform.Find("Card" + who + (position + 1));
            }
        }

        for (int who = 0; who < 2; who++)
        {
            for (int position = 0; position < 7; position++)
            {
                pointTransfroms[who + 2, position] = ShopPoints.transform.Find("Card" + who + (position + 1));
            }
        }
        heroPointTransfroms[0] = Points.transform.Find("Hero");
        heroPointTransfroms[1] = Points.transform.Find("Opponent");
        heroPointTransfroms[2] = ShopPoints.transform.Find("Hero");
        heroHandPileTransfroms[0] = ShopPoints.transform.Find("HandPileLeft");
        heroHandPileTransfroms[1] = ShopPoints.transform.Find("HandPileRight");
        heroHandPileTransfroms[2] = Points.transform.Find("HandPileLeft");
        heroHandPileTransfroms[3] = Points.transform.Find("HandPileRight");
        for (int i = 0; i < 3; i++)
        {
            choosePointTransforms[i] = ShopPoints.transform.Find("Choose" + i);
        }
    }

    /// <summary>
    /// 交换两张卡的战场位置，第一张卡在上
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    public Tween SwapCard(CardPosition pos1, CardPosition pos2)
    {
        Sequence sequence = DOTween.Sequence();
        SendGameMessage(new ChangeMessage()
        {
            code = new ChangeMessageCode()
            {
                code = "swap",
                pos1 = pos1,
                pos2 = pos2
            },
        });

        CardStruct card1 = GetCard(pos1);
        CardStruct card2 = GetCard(pos2);
        if (card1 != null)
        {
            sequence.Insert(0, MoveCardToSpcePos(card1, pos2, 0.1f, 10));
        }
        if (card2 != null)
        {
            sequence.Insert(0, MoveCardToSpcePos(card2, pos1, 0.2f, 5));
        }


        return sequence;
        //return null;
    }
    void EnableClickForDescription()
    {
        var changeMessage = curChangeMessage;
        List<Card> cards = new List<Card>();
        cards.AddRange(changeMessage?.data?.heroBattlePile?.ToList());
        cards.AddRange(changeMessage?.data?.heroHandPile?.ToList());
        cards.AddRange(changeMessage?.data?.opponentBattlePile?.ToList());
        cards.Add(changeMessage?.data?.hero);
        cards.Add(changeMessage?.data?.opponent);
        foreach (var card in cards)
        {
            if (card == null) continue;
            if (card.cardType == CardType.Hero)
            {
                GetHero(card).herosetting.EnableTap(true, () =>
                {
                    isPause = true;
                    //Mask.gameObject.SetActive(true);
                    DescriptionWindow.Show(card, tween =>
                    {
                        isPause = false;
                        //Mask.gameObject.SetActive(false);
                    });
                });
            }
            else
            {
                GetCard(card).setting.EnableTap(true, () =>
                {
                    isPause = true;
                    //Mask.gameObject.SetActive(true);
                    DescriptionWindow.Show(card, tween =>
                    {
                        isPause = false;
                        //Mask.gameObject.SetActive(false);
                    });
                });
            }
        }
    }


    /// <summary>
    /// 商店中 我方战场可以自由调换位置
    /// 可打出手牌
    /// 可购买随从
    /// </summary>
    /// <param name="isOk"></param>
    public void EnableInteractive(bool isOk = true)
    {

        foreach (var st in cardMaps)
        {
            st.Value.setting.EnableDrag(false);
            st.Value.setting.EnableTap(false);
            st.Value.setting.EnableLongPress(false);
        }

        foreach (var st in heroMaps)
        {
            st.Value.herosetting.EnableTap(false);
        }
        if (curChangeMessage == null) return;
        if (curChangeMessage.data == null) return;

        #region selectTarget的特殊设置
        var test = NextChangeMessage?.code?.code.Equals("selectTarget");
        if (test.HasValue && test.Value)
        {
            isOk = false;
        }
        #endregion

        if (isOk)
        {
            EnableBoardDecorate(true);
            //print("in ok");
            curChangeMessage.data.heroBattlePile?.Map(card =>
            {
                GetCard(card).setting.EnableDrag(true, () => { }, () => BattleDragEndCallback(card));
            });
            curChangeMessage.data.heroHandPile?.Map(card =>
            {
                //Debug.Log("bind handDrag:"+ card.name);
                CardStruct cardStruct = GetCard(card);
                cardStruct.setting.EnableDrag(true, () => { }, () => HandDragEndCallback(card));
            });

            if (curChangeMessage.data.leftCoins >= Const.coinCostToBuyMinion && curChangeMessage.data.heroHandPile?.Count < 10)
            {
                curChangeMessage.data.opponentBattlePile?.Map(card =>
                {
                    GetCard(card).setting.EnableDrag(true, () => { }, () => BuyCallback(card));
                });
            }
            curChangeMessage.data.opponentBattlePile?.Map(card =>
            {
                GetCard(card).setting.EnableLongPress(true, () =>
                {
                    //DescriptionMask.gameObject.SetActive(true);
                    //DescriptionMask.Text.text = card.name + "\n\n" + CardBuilder.GetCardDescription(card);
                    //DescriptionMask.EnableTap(true, () =>{
                    //    DescriptionMask.gameObject.SetActive(false);
                    //});
                    EnableInteractive(false);
                    DescriptionWindow.Show(card, tween =>
                    {
                        EnableInteractive(true);
                    });
                });
            });
            curChangeMessage.data.heroBattlePile?.Map(card =>
            {
                GetCard(card).setting.EnableLongPress(true, () =>
                {
                    //DescriptionMask.gameObject.SetActive(true);
                    //DescriptionMask.Text.text = card.name + "\n\n" + CardBuilder.GetCardDescription(card);
                    //DescriptionMask.EnableTap(true, () => {
                    //    DescriptionMask.gameObject.SetActive(false);
                    //});

                    EnableInteractive(false);
                    DescriptionWindow.Show(card, tween =>
                    {
                        EnableInteractive(true);
                    });
                });
            });
            curChangeMessage.data.heroHandPile?.Map(card =>
            {
                GetCard(card).setting.EnableLongPress(true, () =>
                {
                    //DescriptionMask.gameObject.SetActive(true);
                    //DescriptionMask.Text.text = card.name + "\n\n" + CardBuilder.GetCardDescription(card);
                    //DescriptionMask.EnableTap(true, () => {
                    //    DescriptionMask.gameObject.SetActive(false);
                    //});

                    EnableInteractive(false);
                    DescriptionWindow.Show(card, tween =>
                    {
                        EnableInteractive(true);
                    });
                });
            });
            var hero = GetHero(curChangeMessage.data.hero);
            hero?.herosetting.EnableLongPress(true, () =>
            {
                DescriptionWindow.Show(hero.herosetting.setedCard, tween =>
                {
                    EnableInteractive(true);
                });
            });
        }
        else
        {
            EnableBoardDecorate(false);
            var hero = GetHero(curChangeMessage.data.hero);
            hero?.herosetting.EnableLongPress(false);
        }
    }

    #region drag callback
    void BattleDragEndCallback(Card card)
    {
        CardStruct cardStruct = GetCard(card);
        CardPosition position = cardStruct.position;
        var posAc = GetChessPosByPositionRound(cardStruct.setting.transform.localPosition);
        if (IsInSellArea(cardStruct.setting.transform.localPosition))
        {
            SendGameMessage(new ChangeMessage()
            {
                code = new ChangeMessageCode()
                {
                    code = "sell",
                    pos1 = position
                }
            });
        }
        else if (posAc != null && posAc.type == PositionType.HeroBattlePile)
        {
            SwapCard(position, posAc);
        }
        else
        {
            MoveCardToSpcePos(cardStruct, position, 0.3f, 5);
        }
    }
    void HandDragEndCallback(Card card)
    {
        CardStruct cardStruct = GetCard(card);
        CardSetting setting = cardStruct.setting;
        CardPosition position = cardStruct.position;
        var posAc = GetChessPosByPositionRound(setting.transform.localPosition);
        if (card.cardType == CardType.Minion && IsInSellArea(setting.transform.localPosition) && card.cost <= 0)
        {
            SendGameMessage(new ChangeMessage()
            {
                code = new ChangeMessageCode()
                {
                    code = "sell",
                    pos1 = position
                }
            });
            return;
        }
        if (card.cardType == CardType.Spell && IsInPlayArea(setting.transform.localPosition) && curChangeMessage.data.leftCoins >= card.cost)
        {
            SendGameMessage(new ChangeMessage()
            {
                code = new ChangeMessageCode()
                {
                    code = "play",
                    pos1 = position
                }
            });
            return;
        }
        if (card.cardType == CardType.Minion && posAc != null && posAc.type == PositionType.HeroBattlePile && GetCard(posAc) == null)
        {
            //SetCardToSpcePos(cardStruct, posAc);
            setting.EnableDrag(false);
            //handPileSaves.Remove(setting.setedCard);
            SendGameMessage(new ChangeMessage()
            {
                code = new ChangeMessageCode()
                {
                    code = "play",
                    pos1 = position,
                    pos2 = posAc,
                    pos3 = null,
                }
            });
            return;
        }
        MoveCardToSpcePos(cardStruct, position, 0.3f, 5);
    }
    void BuyCallback(Card card)
    {
        CardStruct cardStruct = GetCard(card);
        CardSetting setting = cardStruct.setting;
        CardPosition position = cardStruct.position;
        var posAc = GetHeroPosByPositionRound(setting.transform.localPosition);
        if (posAc != null && posAc.type == PositionType.Hero)
        {
            SendGameMessage(new ChangeMessage()
            {
                code = new ChangeMessageCode()
                {
                    code = "buy",
                    pos1 = position
                }
            });
        }
        else
        {
            MoveCardToSpcePos(cardStruct, position, 0.3f, 5);
        }
    }
    #endregion


    public Vector3 GetChessPosition(CardPosition pos)
    {
        switch (pos.type)
        {
            case PositionType.None:
                break;
            case PositionType.Hero:
                if (isBattle) return heroPointTransfroms[0].position;
                else return heroPointTransfroms[2].position;
            case PositionType.Opponent:
                if (isBattle) return heroPointTransfroms[1].localPosition;
                break;
            case PositionType.HeroHandPile:
                if (true)
                {
                    float left = heroHandPileTransfroms[0 + (isBattle ? 2 : 0)].position.x;
                    float right = heroHandPileTransfroms[1 + (isBattle ? 2 : 0)].position.x;
                    float center = (left + right) / 2;
                    Vector3 centerPos = (heroHandPileTransfroms[0].position + heroHandPileTransfroms[1].position) / 2;
                    float gep = 0.3f;
                    float d = pos.pos - (pos.pileTotal - 1) / 2f;
                    float width = 1.46f;
                    if ((width + gep) * pos.pileTotal - gep >= right - left)
                    {
                        gep = (right - left - width * pos.pileTotal) / (pos.pileTotal + 1);
                    }
                    return new Vector3(center + d * (width + gep), centerPos.y, centerPos.z - 0.00001f * pos.pos);
                }
            case PositionType.OpponentHandPile:
                break;
            case PositionType.HeroBattlePile:
                return pointTransfroms[(isBattle ? 0 : 2) + 0, pos.pos].position;
            case PositionType.OpponentBattlePile:
                //Debug.Log("input pos:" + pos.type+pos.pos);
                return pointTransfroms[(isBattle ? 0 : 2) + 1, pos.pos].position;

            case PositionType.Choose:
                if (true)
                {
                    float left = choosePointTransforms[0].position.x;
                    float right = choosePointTransforms[2].position.x;
                    float center = (left + right) / 2;
                    Vector3 centerPos = (choosePointTransforms[0].position + choosePointTransforms[2].position) / 2;
                    float gep = 0.6f;
                    float d = pos.pos - (pos.pileTotal - 1) / 2f;
                    float width = 1.46f;
                    if ((width + gep) * pos.pileTotal - gep >= right - left)
                    {
                        gep = (right - left - width * pos.pileTotal) / (pos.pileTotal + 1);
                    }
                    return new Vector3(center + d * (width + gep), centerPos.y, centerPos.z - 1);
                }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 只在商店中使用
    /// </summary>
    /// <param name="ori"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public CardPosition GetHeroPosByPositionRound(Vector3 ori, float distance = 1f)
    {
        if (Vector3.Distance(heroPointTransfroms[2].position, ori) < distance)
        {
            return new CardPosition(PositionType.Hero);
        }
        return null;
    }
    public bool IsInSellArea(Vector3 ori)
    {
        return pointTransfroms[3, 0].position.y <= ori.y;
    }
    public bool IsInPlayArea(Vector3 ori)
    {
        return pointTransfroms[2, 6].position.y <= ori.y;
    }

    public CardPosition GetChessPosByPositionRound(Vector3 ori, float distance = 1f)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < pointTransfroms.GetLength(1); j++)
            {
                Transform trans = pointTransfroms[i + (isBattle ? 0 : 2), j];
                if (trans != null)
                {
                    if (Vector3.Distance(trans.position, ori) < distance)
                    {
                        return new CardPosition()
                        {
                            type = i == 0 ? PositionType.HeroBattlePile : PositionType.OpponentBattlePile,
                            pos = j,
                        };
                    }
                }
            }
        }
        return null;
    }

    #endregion


}