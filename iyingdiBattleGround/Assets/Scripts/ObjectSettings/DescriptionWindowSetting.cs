using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using UnityEngine;

public class DescriptionWindowSetting : MonoBehaviour
{
    [Autohook]
    public TapGesture Button;
    [Autohook]
    public InnerSetting inner;

    private Action<Tween> onClose = null;

    public Tween Show(Card card, Action<Tween> onClose)
    {
        this.onClose = onClose;
        inner.SetByCard(card);
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        return DOTween.Sequence().Append(transform.DOScale(Vector3.one, 0.2f));
    }


    public void OnEnable()
    {
        Button.Tapped += Button_Tapped;
    }
    private void OnDisable()
    {
        Button.Tapped -= Button_Tapped;
    }

    private void Button_Tapped(object sender, System.EventArgs e)
    {
        Close();
    }

    public void Close()
    {
        Tween tween = gameObject.transform.DOScale(Vector3.zero, 0.2f);
        onClose?.Invoke(tween);
    }
}
