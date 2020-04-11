using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TouchScript.Behaviors;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class SelectBoardSetting : MonoBehaviour
{
    #region public var
    [Autohook]
    public SelectBoardTitleSetting Title;
    [Autohook]
    public SelectBoardButtonSetting Buttons;
    [Autohook]
    public Transform StartSence;
    [Autohook]
    public SelectBoardDescriptionWindowSetting DescriptionWindow;
    [Autohook]
    public DescriptionWindowSetting DescriptionWindowSingle;
    [Autohook]
    public SummaryWindowSetting SummaryWindow;
    [Autohook]
    public Transform HeroDescWindow;
    [Autohook]
    public HeroSelectCommponentSetting HeroDesc;
    [Autohook]
    public SelectWindowCounterSetting Counter;
    [Autohook]
    public Transform TreasureContent;
    [Autohook]
    public TextMeshPro TipTitle;


    [Autohook]
    public Transform GameObjects;

    [Autohook]
    public Transform ScrollView; 
    [Autohook]
    public Transform HideArea;

     [Autohook]
    public HeroSelectCommponentSetting First;
    [Autohook]
    public HeroSelectCommponentSetting Second;

    [Autohook]
    public GameObject HeroSelectCommponentPrefab;
    [Autohook]
    public GameObject CardPileSelectCommponentPrefab;
    [Autohook]
    public GameObject TraseureSelectCommponentPrefab;
    [Autohook]
    public GameObject Card2Prefab;
    [Autohook]
    public TextMeshPro Tip;


    [Autohook]
    public SpriteButton Btn确定;
    [Autohook]
    public SpriteButton Btn返回;
    [Autohook]
    public CardSetting CardL0T0;
    [Autohook]
    public CardSetting CardL1T0;
    [Autohook]
    public CardSetting CardL0T1;
    [Autohook]
    public SpriteButton Collection_Treasure;
    [Autohook]
    public SpriteButton Collection_Keyword;
    [Autohook]
    public SpriteButton Collection_Card;

    private Action<Vector3> MoveScrollView = null;

    #endregion

    #region private var
    private float ScrollTop = 0;
    private float ScrollBottom = 0;
    private HeroSelectCommponentSetting selectedHero = null;
    public Action<Card> selectCardAction = null;
    internal Action<int> selectPileAction = null;
    public Action clickAction = null;           // 确认按钮的回调函数
    internal Action<int> startSenceAction = null;
    #endregion



    public void ShowStartSence()
    {
        ClearScrollView();
        Title.Hide();
        Buttons.Hide();
        StartSence.gameObject.SetActive(true);
        SetTip("欢迎来到营地战棋");
        MoveScrollView = null;
        DescriptionWindowSingle.Close();
    }

    public void SelectHero(List<Card> heros, List<Card> lockHeros)
    {
        Title.SetSelectHero();
        Buttons.Show();
        ClearScrollView();
        StartSence.gameObject.SetActive(false);

        List<Card> allCards = new List<Card>();
        allCards.AddRange(heros);
        allCards.AddRange(lockHeros);

        Vector3 first = First.transform.position;
        Vector3 second = Second.transform.position;
        Vector3 dPosition = second - first;

        int i = 0;
        List<HeroSelectCommponentSetting> settings = heros.Map(card =>
        {
            HeroSelectCommponentSetting setting = GetHeroSelectCommponent();
            setting.Lock(lockHeros.Contains(card));
            setting.SetByCard(card);
            setting.Glow(false);
            setting.transform.position = first + dPosition * i++;
            return setting;
        });
        lockHeros.Map(card =>
        {
            HeroSelectCommponentSetting setting = GetHeroSelectCommponent();
            setting.Lock(lockHeros.Contains(card));
            setting.SetByCard(card);
            setting.Glow(false);
            setting.transform.position = first + dPosition * i++;
            return setting;
        });

        foreach (var setting in settings)
        {
            setting.EnableTap(true, () =>
            {
                if (setting == selectedHero)
                {
                    selectedHero = null;
                    OnSelectHero(null);
                    setting.Glow(false);
                }
                else
                {
                    settings.Map(st => st.Glow(false));
                    setting.Glow(true);
                    selectedHero = setting;
                    OnSelectHero(setting.Hero.setedCard);
                }
            });
        }
        selectedHero = null;
        OnSelectHero(null);
        ScrollTop = -(first + dPosition * (allCards.Count - 2)).y;
        HookAllScrollViewObjects();
    }

    public void ShowSummary(List<Card> enemys, List<Card> treasures, CardPile cardPile)
    {
        ClearScrollView();
        Title.SetSummary();
        SetTip("这是你的战绩");

        SummaryWindow.gameObject.SetActive(true);
        SummaryWindow.SetHero(enemys);
        SummaryWindow.SetTreasure(treasures);
        Counter.gameObject.SetActive(true);
        Counter.SetByCardPile(cardPile);

        Btn确定.IsActive = true;
        clickAction = () => {
            SummaryWindow.gameObject.SetActive(false);
            Counter.gameObject.SetActive(false);
            selectPileAction?.Invoke(0);
        };
    }

    public void IntroEnemy(Player player, int level)
    {
        Title.Hide();
        ClearScrollView();
        HeroDescWindow.gameObject.SetActive(true);
        Counter.gameObject.SetActive(false);
        HeroDesc.SetByCard(player.hero);
        Btn确定.IsActive = true;
        TipTitle.text = "挑战 "+level+"/4";
        clickAction = () =>
        {
            HeroDescWindow.gameObject.SetActive(false);
            selectCardAction?.Invoke(null);
        };
        SetTip("即将面对的挑战");
    }

    public void ShowCollection()
    {
        void ReturnBack()
        {
            selectCardAction?.Invoke(null);
            Buttons.Hide();
            TreasureContent.gameObject.SetActive(false);
        }
        clickAction = ReturnBack;
        Btn返回.Tapped.RemoveAllListeners();
        Btn返回.Tapped.AddListener(ReturnBack);
        Btn返回.IsActive = true;
        Btn确定.IsActive = true;
        Buttons.Show();
        StartSence.gameObject.SetActive(false);

        TreasureContent.gameObject.SetActive(true);
        SetTip("我的收藏");
        Title.Hide();
        showCC = -1;
        ShowCardCollection();
    }

    public void SelectTreasure(List<Card> list)
    {
        Title.SetSelectTreasure();
        Buttons.Show();
        ClearScrollView();
        StartSence.gameObject.SetActive(false);

        Vector3 first = First.transform.position;
        Vector3 second = Second.transform.position;
        Vector3 dPosition = second - first;
        Vector3[] positions = new Vector3[] { first, second, second+dPosition};

        List<TrasureSelectCommponentSetting> settings = list.Map(card=> GetTrasureSelectCommponentSetting());

        for(int i=0; i< list.Count; i++)
        {
            var setting = settings[i];
            Card card = list[i];
            //Debug.Log(card.name);
            setting.SetByCard(card);
            setting.transform.position = positions[i];
            setting.Glow(false);
            setting.EnableTap(true, () => {
                OnSelectTreasure(card);
                settings.Map(st => st.Glow(false));
                setting.Glow(true);
            });
        }
        OnSelectTreasure(null);
    }


    public void SelectPile(List<Card> firstPile, List<Card> secondPile, CardPile cardPile, string name1 = "", string name2 = "")
    {
        Title.SetSelectPile();
        Buttons.Show();
        ClearScrollView();
        Counter.gameObject.SetActive(true);
        Counter.SetByCardPile(cardPile);
        StartSence.gameObject.SetActive(false);


        Vector3 first = First.transform.position;
        Vector3 second = Second.transform.position;

        List<Card>[] piles = new List<Card>[] { firstPile , secondPile};
        Vector3[] positions = new Vector3[] { first, second};
        string[] names = new string[] { name1, name2 };

        CardPileSelectCommponentSetting[] settings = new CardPileSelectCommponentSetting[2] {
            GetCardPileSelectCommponent(),
            GetCardPileSelectCommponent()
        };

        for (int j = 0; j < 2; j++) 
        {
            var i = j;  // 闭包
            CardPileSelectCommponentSetting setting = settings[i];
            setting.SetByCardPile(piles[i]);
            setting.ShowStarOrMoon(i == 0);
            setting.gameObject.transform.position = positions[i];
            setting.ShowGlow(false);
            setting.EnableTap(true, () => {
                OnSelectPile(i, piles[i], names[i]);
                setting.ShowGlow(true);
                settings[1 - i].ShowGlow(false);
            });
        }
        OnSelectPile(-1, null, name1);
    }

    #region tool function for show collcetion


    private int showCC = -1;
    public void ShowCardCollection()
    {
        if (showCC == 0) return;
        List<Card> cards = GameController.GetAllCard();
        showCC = 0;
        Title.SetShowCollection();
        SetTip("这是你的卡牌收藏");
        ClearScrollView();
        DescriptionWindowSingle.Close();
        Vector3 LT = CardL0T0.transform.position;
        Vector3 L1T0 = CardL1T0.transform.position;
        Vector3 L0T1 = CardL0T1.transform.position;
        Vector3 dx = L1T0 - LT;
        Vector3 dy = L0T1 - LT;

        int groupNum = cards.Count / 12 + (cards.Count % 12 == 0?0:1);
        List<List<Card>> groups = new List<List<Card>>();
        for (int i = 0; i < groupNum; i++)
        {
            groups.Add(cards.Skip(12 * i).Take(12).ToList());
        }

        // 预先初始化
        if (cards.Count > 12 * 3)
        {
            List<GameObject> save = new List<GameObject>();
            for (int i = 0; i < Math.Min(6, groupNum) * 12; i++)
            {
                save.Add(GetCardSetting(cards[0]).gameObject);
            }
            save.Map(RemoveCardSetting);
        }

        List<CardSetting> settings = cards.Take(12*3).Map(GetCardSetting);

        settings.Map((setting, index) => {
            int fx = index % 3;
            int fy = index / 3;
            setting.transform.localPosition = LT + fx * dx + fy * dy;
            if (!setting.setedCard.Lock)
            {
                setting.EnableTap(true, () => {
                    DescriptionWindowSingle.Show(setting.setedCard, tween => { });
                });
            }
        });

        ScrollTop = -(LT + dy * (groupNum*4 - 1)).y;
        HookAllScrollViewObjects(true);
        
        MoveScrollView = delta => {
            var next = ScrollView.transform.position;
            var last = ScrollView.transform.position - delta;
            var nextGroup = GetGroup(next, dy.y);
            var lastGroup = GetGroup(last, dy.y);
            if (nextGroup != lastGroup)
            {
                int nextnextGroup = nextGroup + 2 * Math.Sign(delta.y);
                int lastlastGroup = nextGroup + -3 * Math.Sign(delta.y);
                AddGroup(nextnextGroup, groups, LT, dx, dy);
                RemoveGroup(lastlastGroup, groups);
            }
        };
    }

    public void ShowTreasureCollection()
    {
        if (showCC == 1) return;
        showCC = 1;
        ClearScrollView();
        DescriptionWindowSingle.Close();
        MoveScrollView = null;
        List<Card> cards = GameController.GetAllTreasureCard();
        Vector3 LT = CardL0T0.transform.position;
        Vector3 L1T0 = CardL1T0.transform.position;
        Vector3 L0T1 = CardL0T1.transform.position;
        Vector3 dx = L1T0 - LT;
        Vector3 dy = L0T1 - LT;

        List<CardSetting> settings = cards.Map(GetCardSetting);

        settings.Map((setting, index) => {
            int fx = index % 3;
            int fy = index / 3;
            setting.HideStar();
            setting.transform.localPosition = LT + fx * dx + fy * dy;
            if (!setting.setedCard.Lock)
            {
                setting.EnableTap(true, () => {
                    DescriptionWindowSingle.Show(setting.setedCard, tween => { });
                });
            }
        });

        ScrollTop = -(LT + dy * (cards.Count/3 - 1)).y;
        HookAllScrollViewObjects();
    }


    #endregion

    #region tool function for scroll dyn load
    int GetGroup(Vector3 pos, float dy)
    {
        return (int)(pos.y / (4 * -dy));
    }
    
    void AddGroup(int group, List<List<Card>> groups, Vector3 LT, Vector3 dx, Vector3 dy)
    {
        if (group < 0 || group >= groups.Count) return;
        //print("add " + group + groups[group].Map(card=>card.name).StringJoin(","));

        groups[group].Map((card, index) => {

            int fx = index % 3;
            int fy = index / 3 + group * 4;

            CardSetting setting = GetCardSetting(card);
            setting.transform.localPosition = LT + fx*dx + fy*dy;
            if (!setting.setedCard.Lock)
            {
                setting.EnableTap(true, () => {
                    DescriptionWindowSingle.Show(setting.setedCard, tween => { });
                });
            }
        });

        HookAllScrollViewObjects();
    }
    void RemoveGroup(int group, List<List<Card>> groups)
    {
        if (group < 0 || group >= groups.Count) return;
        //print("remove "+ group + groups[group].Map(card => card.name).StringJoin(","));
        List<GameObject> coll = new List<GameObject>();
        foreach (var trans in ScrollView.transform)
        {
            if (trans is Transform transform)
            {
                CardSetting cardSetting = transform.GetComponent<CardSetting>();
                if (cardSetting == null) continue;
                if (groups[group].Contains(cardSetting.setedCard))
                {
                    coll.Add(cardSetting.gameObject);
                }
            }
        }
        coll.Map(go=>RemoveCardSetting(go));
    }

    #endregion

    #region tool function


    private void ClearScrollView()
    {
        List<Transform> transforms = new List<Transform>();
        foreach (Transform transform in ScrollView.transform)
        {
            transforms.Add(transform);
        }
        transforms.Map(transform => {
            if (transform.GetComponent<CardSetting>() != null)
            {
                RemoveCardSetting(transform.gameObject);
            }
            else
            {
                Destroy(transform.gameObject);
            }
        });
        ScrollView.gameObject.transform.localPosition = Vector3.zero;
    }

    public void OnClickSelect()
    {
        //Debug.Log("selected!");
        clickAction?.Invoke();
    }

    public void SetTip(string tip)
    {
        Tip.text = tip;
    }

    public void OnSelectHero(Card hero)
    {
        if (hero == null)
        {
            SetTip("请选择英雄");
            Btn确定.IsActive = false;
            clickAction = null;
        }
        else
        {
            SetTip("已选择\"" + hero.name + "\"");
            Btn确定.IsActive = true;
            clickAction = ()=> {
                selectCardAction?.Invoke(hero);
                Btn返回.IsActive = false;
            };
        }
        Btn返回.IsActive = true;
        Btn返回.Tapped.RemoveAllListeners();
        Btn返回.Tapped.AddListener(() => {
            selectCardAction?.Invoke(null);
        });
    }

    private int lastSelectedPile = -1;
    public void OnSelectPile(int n, List<Card> cards, string name = "")
    {
        if (n != 1 && n != 0)
        {
            SetTip("请选择卡牌");
            Btn确定.IsActive = false;
            clickAction = null;
        }
        else
        {
            SetTip("已选择卡牌");
            Btn确定.IsActive = true;
            clickAction = () => {
                Counter.gameObject.SetActive(false);
                selectPileAction?.Invoke(n);
            };
            if (lastSelectedPile == n)
            {
                DescriptionWindow.Show(cards, name);
                DescriptionWindow.confirm = () =>
                {
                    Counter.gameObject.SetActive(false);
                    selectPileAction?.Invoke(n);
                };
            }
        }
        lastSelectedPile = n;
    }
    private void OnSelectTreasure(Card card)
    {
        if (card == null)
        {
            SetTip("请选择宝藏");
            Btn确定.IsActive = false;
            clickAction = null;
        }
        else
        {
            SetTip("已选择\""+card.name+"\"");
            Btn确定.IsActive = true;
            clickAction = () => {
                selectCardAction?.Invoke(card);
            };
        }
    }

    private void HookAllScrollViewObjects(bool isHook = true)
    {
        foreach (var trans in ScrollView.transform)
        {
            if (trans is Transform transform)
            {
                var transformGesture = transform.GetComponent<TransformGesture>();
                if (transformGesture == null) continue;
                if (transformGesture.OnTransform != null)
                {
                    transformGesture.Transformed -= TransformGesture_Transformed;
                }

                if (isHook)
                {
                    transformGesture.Transformed += TransformGesture_Transformed;
                }
            }
        }
    }


    private void TransformGesture_Transformed(object sender, System.EventArgs e)
    {
        if (sender is TransformGesture transformGesture)
        {
            Vector3 deltaPosition = transformGesture.DeltaPosition;
            Vector3 newPos = ScrollView.transform.position + new Vector3(0, deltaPosition.y, 0);
            if (newPos.y <= ScrollTop &&  newPos.y > ScrollBottom)
            {
                ScrollView.transform.position = newPos;
                MoveScrollView?.Invoke(transformGesture.DeltaPosition);
            }
        }
        //print("in");
    }

    public void OnStartButtonTapped()
    {
        startSenceAction?.Invoke(0);
    }
    public void OnCollectionButtonTapped()
    {
        startSenceAction?.Invoke(1);
    }

    #endregion

    #region prefab
    private void Awake()
    {
        InitPrefab();
    }
    private void InitPrefab()
    {
        BIF.BIFStaticTool.PrefabPool("HeroSelectCommponent", HeroSelectCommponentPrefab, HideArea);
        BIF.BIFStaticTool.PrefabPool("CardPileSelectCommponent", CardPileSelectCommponentPrefab, HideArea);
        BIF.BIFStaticTool.PrefabPool("TrasureSelectCommponent", TraseureSelectCommponentPrefab, HideArea);
        BIF.BIFStaticTool.PrefabPool("CardCommponent", Card2Prefab, HideArea);
    }
    private HeroSelectCommponentSetting GetHeroSelectCommponent()
    {
        return BIF.BIFStaticTool.GetPrefabInPool("HeroSelectCommponent", ScrollView).GetComponent<HeroSelectCommponentSetting>();
    }
    private void RemoveHeroSelectCommponent(HeroSelectCommponentSetting setting)
    {
        BIF.BIFStaticTool.PrefabPool("HeroSelectCommponent", setting.gameObject);
    }
    private void RemoveHeroSelectCommponent(GameObject go)
    {
        BIF.BIFStaticTool.PrefabPool("HeroSelectCommponent", go);
    }

    private CardPileSelectCommponentSetting GetCardPileSelectCommponent()
    {
        return BIF.BIFStaticTool.GetPrefabInPool("CardPileSelectCommponent", ScrollView).GetComponent<CardPileSelectCommponentSetting>();
    }
    private void RemoveCardPileSelectCommponent(GameObject go)
    {
        BIF.BIFStaticTool.PrefabPool("CardPileSelectCommponent", go);
    }

    private TrasureSelectCommponentSetting GetTrasureSelectCommponentSetting()
    {
        GameObject go = BIF.BIFStaticTool.GetPrefabInPool("TrasureSelectCommponent", ScrollView);
        return go.GetComponent<TrasureSelectCommponentSetting>();
    }
    private CardSetting GetCardSetting(Card card)
    {
        GameObject go = BIF.BIFStaticTool.GetPrefabInPool("CardCommponent", ScrollView);
        CardSetting setting = go.GetComponent<CardSetting>();
        Transformer transformer = go.GetComponent<Transformer>();
        transformer.enabled = false;
        setting.SetByCard(card);
        //setting.EnableDrag(false);
        setting.EnableTap(false);
        setting.EnableLongPress(false);
        setting.ShowMerge(false);
        if (card.Lock)
        {
            setting.LockThis();
        }
        else
        {
            setting.LockThis(false);
        }
        setting.SetStarBySetedCard();
        return setting;
    }

    private void RemoveCardSetting(GameObject go)
    {
        BIF.BIFStaticTool.PrefabPool("CardCommponent", go);
    }

    #endregion
}
