using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TouchScript.Gestures;
using UnityEngine;
using UnityEngine.Rendering;

public class HeroSetting : MonoBehaviour, ISortable
{
    #region public vars
    [Autohook]
    public SpriteRenderer face;
    [Autohook]
    public SortingGroup SortingGroup;

    [Autohook]
    public Transform attack;
    [Autohook]
    public Transform health;
    [Autohook]
    public TextMeshPro attackText;
    [Autohook]
    public TextMeshPro healthText;
    [Autohook]
    public SpriteRenderer Hit;
    [Autohook]
    public SpriteRenderer Border;

    [Autohook]
    public TextMeshPro hitText;

    [Autohook]
    public Animator 免疫;

    #endregion

    #region private var

    private LongPressGesture longPressGesture;
    private TapGesture TapGesture;
    private Action longPressed = null;
    private Action tapped = null;

    #endregion

    #region unity function

    private void OnEnable()
    {
        longPressGesture = GetComponent<LongPressGesture>();
        TapGesture = GetComponent<TapGesture>();


        longPressGesture.LongPressed += LongPressGesture_LongPressed;
        TapGesture.Tapped += TapGesture_Tapped;
    }
    private void OnDisable()
    {
        longPressGesture.LongPressed -= LongPressGesture_LongPressed;
        TapGesture.Tapped -= TapGesture_Tapped;
    }


    private void TapGesture_Tapped(object sender, EventArgs e)
    {
        tapped?.Invoke();
    }


    private void LongPressGesture_LongPressed(object sender, EventArgs e)
    {
        longPressed?.Invoke();
    }

    #endregion

    #region interactive

    public void EnableLongPress(bool isOk, Action longPressCallback = null)
    {
        if (isOk)
        {
            longPressed = longPressCallback;
        }
        else
        {
            longPressed = null;
        }
    }

    public void EnableTap(bool isOk, Action tapped = null)
    {
        if (isOk)
        {
            this.tapped = tapped;
        }
        else
        {
            this.tapped = null;
        }
    }

    #endregion

    #region set
    public void SetHeroBody(int attack, int health)
    {
        this.attack.gameObject.SetActive(attack > 0);
        attackText.text = attack.ToString();
        healthText.text = health.ToString();
    }

    public void SetHeroFace(Sprite face)
    {
        this.face.sprite = face;
    }

    public Card setedCard = null;

    public Transform Transform => transform;

    public void SetByCard(Card card)
    {
        if (card.cardType != CardType.Hero) return;
        setedCard = card;

        Vector2Int body = card.GetMinionBody();
        SetHeroBody(body.x, body.y);

        SetHeroFace(GameAnimationSetting.instance.Images.GetSpriteByString(card.image));

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

        if (card.HasKeyword(Keyword.Immune))
        {
            免疫.gameObject.SetActive(true);
        }
        else
        {
            免疫.gameObject.SetActive(false);
        }


    }

    private void SetHealthColor(Color color)
    {
        healthText.color = color;
    }

    public void SetSortingOrder(int order)
    {
        SortingGroup.sortingOrder = order;
    }
    public void SetSortingOrderBy(int dOrder)
    {
        SortingGroup.sortingOrder += dOrder;
    }

    #endregion

    #region show
    public Tween ShowHit(string number)
    {
        hitText.text = "-" + number;

        var go = Hit.gameObject;
        var scale = go.transform.localScale;
        go.transform.localScale = Vector3.zero;
        go.SetActive(true);
        SpriteRenderer spriteRenderer = Hit;


        return DOTween.Sequence().AppendCallback(() => {

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
                .AppendCallback(() => {
                    Hit.color = Color.white;
                    Hit.gameObject.SetActive(false);
                });
        });
    }
    
    public Tween ShowBorder(bool isShow = true)
    {
        return DOTween.Sequence().AppendCallback(()=> {
            Border.gameObject.SetActive(isShow);
        });
    }
    
    #endregion
}
