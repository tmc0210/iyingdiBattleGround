using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOESetting : MonoBehaviour
{
    [Autohook]
    public SpriteRenderer AOEDown;

    [Autohook]
    public SpriteRenderer AOEUp;

    private void OnEnable()
    {
        AOEDown.gameObject.SetActive(false);
        AOEUp.gameObject.SetActive(false);
    }

    public Tween AnimateAOE(AOEType type)
    {
        Sequence sequence = DOTween.Sequence();

        if (type == AOEType.Down || type == AOEType.All)
        {
            sequence.Insert(0, Animate(AOEDown, new Vector3(0, -3f)));
        }
        if (type == AOEType.Up || type == AOEType.All)
        {
            sequence.Insert(0, Animate(AOEUp, new Vector3(0, 3f)));
        }

        return sequence;
    }

    private Tween Animate(SpriteRenderer sprite, Vector3 dtrans)
    {
        return DOTween.Sequence().AppendCallback(() => {
            sprite.gameObject.SetActive(true);
            sprite.transform.localScale = Vector3.zero;
            sprite.transform.localPosition = Vector3.zero;
            sprite.color = Color.white;
        }).Append(sprite.transform.DOScale(Vector3.one, 0.02f))
        .Join(sprite.transform.DOLocalMove(dtrans, 0.5f).SetEase(Ease.InFlash))
        .Join(sprite.DOFade(0, 0.5f).SetEase(Ease.InCirc))
        .AppendCallback(()=> {
            sprite.gameObject.SetActive(false);
        });
    }
}

public enum AOEType {
    Up,
    Down,
    All,
    None
}
