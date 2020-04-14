using BIF;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using UnityEngine.Rendering;

public class CardSetting : MonoBehaviour, ISortable
{
    #region public var
    [Autohook]
    public TextMeshPro attackText;
    [Autohook]
    public TextMeshPro healthText;
    [Autohook]
    public Transform attack;
    [Autohook]
    public Transform health;

    [Autohook]
    public SortingGroup SortingGroup;

    [Autohook]
    public SpriteRenderer main;

    [Autohook]
    public Transform Content;
    [Autohook]
    public Transform AllContent;


    [Autohook]
    public StarSetting Star;
    [Autohook]
    public SpriteRenderer Hit;
    [Autohook]
    public TextMeshPro hitText;
    [Autohook]
    public SpriteRenderer Border;
    [Autohook]
    public Transform Cost;
    [Autohook]
    public TextMeshPro CostText;
    [Autohook]
    public SpriteMask CleaveMask;

    [Autohook]
    public SpriteRenderer 金色边框;

    [Autohook]
    public SpriteRenderer 嘲讽;
    public Sprite 普通嘲讽;
    public Sprite 金色嘲讽;
    [Autohook]
    public Transform 光环;
    [Autohook]
    public Transform 亡语;
    [Autohook]
    public Transform 剧毒;
    [Autohook]
    public Transform 闪电;
    [Autohook]
    public Transform 风怒;
    [Autohook]
    public Transform 圣盾;
    [Autohook]
    public Transform 冻结;
    [Autohook]
    public Transform 复生;
    [Autohook]
    public Transform 潜行;
    [Autohook]
    public Transform 变幻;

    [Autohook]
    public Animator 免疫;

    [Autohook]
    public Transform 恶魔;
    [Autohook]
    public Transform 机械;
    [Autohook]
    public Transform 野兽;
    [Autohook]
    public Transform 鱼人;
    [Autohook]
    public Transform 龙;
    [Autohook]
    public Transform 融合怪;


    [Autohook]
    public SpriteRenderer 三连提示;
    [Autohook]
    public SpriteRenderer LockBG;
    #endregion

    #region private var

    private TransformGesture transformGesture;
    private LongPressGesture longPressGesture;
    private TapGesture tapGesture;

    private Transform[] keywords;
    private Transform[] family;
    private Action dragStart = null, dragComplete = null, longpress = null, tapped = null;

    private bool isMergeable = false;
    private bool isMerging = false;

    #endregion

    #region set card
    [NonSerialized]
    public Card setedCard = null;

    public void SetByCard(Card card)
    {
        Init();
        LockThis(false);
        setedCard = card;
        foreach (var trans in keywords)
        {
            trans.gameObject.SetActive(false);
        }
        foreach (var keyword in card.GetAllKeywords())
        {
            string keywordName = BIFStaticTool.GetEnumDescriptionSaved(keyword);
            foreach (var trans in keywords)
            {
                if (keywordName == trans.name)
                {
                    trans.gameObject.SetActive(true);
                }
            }
        }

        var map = BIFStaticTool.GetEnumNameAndDescriptionSaved<ProxyEnum>();
        List<string> effects = new List<string>() { "闪电", "亡语", "光环" };
        foreach (var pair in map)
        {
            if (effects.Count == 0) break;
            string description = pair.Value.Value;
            if (effects.Contains(description) && card.GetProxys(pair.Key) != null)
            {
                effects.Remove(description);
                if (description == "闪电")
                {
                    闪电.gameObject.SetActive(true);
                }
                else if (description == "亡语")
                {
                    亡语.gameObject.SetActive(true);
                }
                else if (description == "光环")
                {
                    光环.gameObject.SetActive(true);
                }
            }
        }

        if (card.isGold)
        {
            嘲讽.sprite = 金色嘲讽;
        }
        else
        {
            嘲讽.sprite = 普通嘲讽;
        }
        金色边框.gameObject.SetActive(card.isGold);


        string name = BIFStaticTool.GetEnumDescriptionSaved(card.type);

        foreach (var spec in family)
        {
            if (name == spec.name)
            {
                spec.gameObject.SetActive(true);
            }
            else
            {
                spec.gameObject.SetActive(false);
            }
        }
        main.sprite = GameAnimationSetting.instance.Images.GetSpriteByString(card.image);

        #region set text and its color
        if (card.cardType != CardType.Spell)
        {


            Vector2Int body = card.GetMinionBody();
            body.x = Math.Max(0, body.x);
            attackText.text = body.x.ToString();
            healthText.text = body.y.ToString();

            Vector2Int extraBody = card.GetExtraBody();
            if (card.health < CardBuilder.GetCard(card.id).health)
            {
                SetHealthColor(Color.red);
            }
            else if (extraBody.y > 0)
            {
                SetHealthColor(Color.green);
            }
            else
            {
                SetHealthColor(Color.white);
            }

            if (extraBody.x > 0)
            {
                SetAttackColor(Color.green);
            }
            else
            {
                SetAttackColor(Color.white);
            }
        }

        if (card.cardType == CardType.Spell || card.cost > 0)
        {
            CostText.text = card.cost.ToString();
            Cost.gameObject.SetActive(true);
        }
        else
        {
            Cost.gameObject.SetActive(false);
        }

        if (card.cardType == CardType.Spell)
        {
            attack.gameObject.SetActive(false);
            health.gameObject.SetActive(false);
        }
        else
        {
            attack.gameObject.SetActive(true);
            health.gameObject.SetActive(true);
        }
        #endregion


        if (潜行.gameObject.activeSelf)
        {
            //BIFStaticTool.SetAlphaRecursively(Content.gameObject, 0.5f);
            //BIFStaticTool.SetColorRecursively(Content.gameObject, new Color(1f, 1f, 1f, 0.5f));
            BIFStaticTool.SetColorRecursively(Content.gameObject, new Color(0.5f, 0.5f, 0.5f, 1f));
        }
        else
        {
            //BIFStaticTool.SetAlphaRecursively(Content.gameObject, 1f);
            BIFStaticTool.SetColorRecursively(Content.gameObject, new Color(1f, 1f, 1f, 1f));
        }
    }

    public void SetStarBySetedCard()
    {
        Star.SetStar(setedCard.star);
    }
    public void HideStar()
    {
        Star.SetStar(-1);
    }

    public void SetSortingOrder(int order)
    {
        SortingGroup.sortingOrder = order;
    }
    public void SetSortingOrderBy(int dOrder)
    {
        SortingGroup.sortingOrder += dOrder;
    }
    public Tween Triger(string triger)
    {
        if (triger == "剧毒") return null;
        foreach (var trans in keywords)
        {
            if (trans.gameObject.activeSelf && triger == trans.name)
            {
                return trans.DOShakeScale(0.2f);
            }
        }
        return null;
    }
    #endregion

    public void SetHealthColor(Color color)
    {
        if (潜行.gameObject.activeSelf)
        {
            color = new Color(color.r / 2, color.g / 2, color.b / 2, color.a);
        }
        healthText.color = color;
    }

    public void SetAttackColor(Color color)
    {
        if (潜行.gameObject.activeSelf)
        {
            color = new Color(color.r / 2, color.g / 2, color.b / 2, color.a);
        }
        attackText.color = color;
    }


    #region interactive

    private Sequence sequence = null;

    public void EnableDrag(bool isDraggable, Action dragStart = null, Action dragComplete = null)
    {
        if (isDraggable)
        {
            //transformGesture.enabled = true;
            transformGesture.Type = TransformGesture.TransformType.Translation;
            this.dragStart = dragStart;
            this.dragComplete = dragComplete;

            if (sequence != null) sequence.Kill();
            sequence = DOTween.Sequence().AppendInterval(0.3f).Append(Border.DOFade(1, 0.15f));

            //Border.color = new Color(1, 1, 1, 0);
            //Border.DOFade(1, 0.3f);
        }
        else
        {
            transformGesture.Type = TransformGesture.TransformType.None;
            //transformGesture.enabled = false;
            //Border.DOKill();
            //;

            if (sequence != null) sequence.Kill();
            sequence = DOTween.Sequence().AppendInterval(0.8f).Append(Border.DOFade(0, 0.15f));
        }
    }
    public void EnableLongPress(bool isOk, Action longpress = null)
    {
        if (isOk)
        {
            this.longpress = longpress;
        }
        else
        {
            this.longpress -= this.longpress;
        }
    }

    public void EnableTap(bool isOk, Action tapped = null)
    {
        //print("in enable" + setedCard.name+tapped);
        if (isOk)
        {
            this.tapped = tapped;
        }
        else
        {
            this.tapped = null;
        }
    }

    private void LongPressed(object sender, EventArgs e)
    {
        longpress?.Invoke();
    }

    public void LockThis(bool isLock = true)
    {
        if (isLock)
        {
            LockBG.gameObject.SetActive(true);
            AllContent.gameObject.SetActive(false);
        }
        else
        {
            LockBG.gameObject.SetActive(false);
            AllContent.gameObject.SetActive(true);
        }
    }


    private void TransformStarted(object sender, EventArgs e)
    {
        if (dragStart != null)
        {
            transform.DOKill();
            SetSortingOrder(1000);
            transform.DOScale(1.2f * Vector3.one, 0.1f);
            dragStart?.Invoke();
            //print("drag Start" + setedCard.name + BattleBoardSetting.instance.GetCard(this).position);
        }
    }
    private void TransformCompleted(object sender, EventArgs e)
    {
        if (dragComplete != null)
        {
            transform.DOScale(1f * Vector3.one, 0.1f);
            dragComplete?.Invoke();
            //print("drag Start" + setedCard.name+BattleBoardSetting.instance.GetCard(this).position);
        }
        //SetSortingOrderBy(-1000);
        //transform.DOScale(Vector3.one, 0.5f);
    }


    #endregion


    #region unity method
    bool isInited = false;

    public Transform Transform => transform;

    private void Init()
    {

        if (isInited) return;
        isInited = true;
        keywords = new Transform[] { 嘲讽.transform, 光环, 亡语, 剧毒, 闪电, 风怒, 圣盾, 冻结, 复生, 免疫.transform, 潜行, 变幻};
        family = new Transform[] { 恶魔, 野兽, 鱼人, 机械, 龙, 融合怪 };
        transformGesture = GetComponent<TransformGesture>();
        longPressGesture = GetComponent<LongPressGesture>();
        tapGesture = GetComponent<TapGesture>();

    }

    private void OnEnable()
    {
        Init();
        transformGesture.TransformStarted += TransformStarted;
        transformGesture.TransformCompleted += TransformCompleted;
        longPressGesture.LongPressed += LongPressed;
        tapGesture.Tapped += TapGesture_Tapped;
        isMergeable = false;
        isMerging = false;
    }

    private void TapGesture_Tapped(object sender, EventArgs e)
    {
        //print("in setting tapped");
        tapped?.Invoke();
    }

    public void OnDisable()
    {
        transformGesture.TransformStarted -= TransformStarted;
        transformGesture.TransformCompleted -= TransformCompleted;
        longPressGesture.LongPressed -= LongPressed;
        tapGesture.Tapped -= TapGesture_Tapped;
    }

    private void FixedUpdate()
    {
        if (isMergeable && !isMerging)
        {
            isMerging = true;
            三连提示.transform.localPosition = new Vector3(0, -1f, 0);
            三连提示.color = Color.white;

            DOTween.Sequence()
                .Append(三连提示.transform.DOLocalMove(new Vector3(0, 1f, 0), 1.3f))
                .Insert(0.6f, 三连提示.DOFade(0, 0.6f))
                .AppendInterval(0.5f)
                .AppendCallback(() => {
                    isMerging = false;
                })
                ;
        }
    }

    #endregion

    #region show
    private Vector3 localScale = Vector3.zero;
    public Tween ShowHit(string number)
    {
        hitText.text = "-" + number;

        var go = Hit.gameObject;
        if (localScale == Vector3.zero) localScale = go.transform.localScale;
        var scale = localScale;

        return DOTween.Sequence().AppendCallback(() =>
        {

            go.transform.DOKill();
            go.transform.localScale = Vector3.zero;
            go.SetActive(true);
            SpriteRenderer spriteRenderer = Hit;

            if (免疫.gameObject.activeSelf)
            {
                免疫.SetFloat("play", 1f);
                DOTween.Sequence().AppendInterval(0.3f).AppendCallback(() =>
                {
                    免疫.SetFloat("play", 0f);
                });
            }

            DOTween.Sequence().Append(go.transform.DOScale(scale, 0.1f))
                .AppendInterval(0.3f)
                .Append(Hit.DOFade(0, 0.1f))
                .AppendCallback(() =>
                {
                    Hit.color = Color.white;
                    Hit.gameObject.SetActive(false);
                });
        });
    }

    public Tween ShowCleave()
    {
        return DOTween.Sequence().AppendCallback(() =>
        {
            DOTween.Sequence()
                .Append(CleaveMask.transform.DOLocalMove(new Vector3(-2, 0, 0), 0.12f))
                .AppendInterval(0.3f)
                .AppendCallback(() => {
                    CleaveMask.transform.localPosition = Vector3.zero;
                });
        });
    }


    public void ShowMerge(bool isShow = true)
    {
        isMergeable = isShow;
    }

    #endregion 
}


/// <summary>
/// card和hero的通用接口
/// </summary>
public interface ISortable
{
    void SetSortingOrderBy(int dOrder);
    Transform Transform { get; }
    void SetSortingOrder(int order);

    Tween ShowHit(string number);
}