using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
[RequireComponent(typeof(TapGesture), typeof(PressGesture), typeof(ReleaseGesture))]
public class SpriteButton : MonoBehaviour
{
    [SerializeField]
    private bool isActive = true;
    public UnityEvent Tapped;

    SpriteRenderer SpriteRenderer;

    public bool IsActive {
        get => isActive;
        set
        {
            isActive = value;
            if (SpriteRenderer != null)
            {
                SpriteRenderer.color = Color.white * (isActive ? 1f : 0.5f);
            }
        }
    }


    private Vector3 oldPosition = Vector3.zero;
    private void OnEnable()
    {
        TapGesture TapGesture = GetComponent<TapGesture>();
        PressGesture pressGesture = GetComponent<PressGesture>();
        ReleaseGesture releaseGesture = GetComponent<ReleaseGesture>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        IsActive = isActive;

        TapGesture.Tapped -= TapGesture_Tapped;
        TapGesture.Tapped += TapGesture_Tapped;
        releaseGesture.Released -= ReleaseGesture_Released;
        releaseGesture.Released += ReleaseGesture_Released;
        pressGesture.Pressed -= PressGesture_Pressed;
        pressGesture.Pressed += PressGesture_Pressed;

        if (oldPosition == Vector3.zero)
        {
            oldPosition = gameObject.transform.position;
        }
        else
        {
            gameObject.transform.position = oldPosition;
        }
    }

    private void PressGesture_Pressed(object sender, System.EventArgs e)
    {
        if (IsActive)
        {
            SpriteRenderer?.transform.DOLocalMove(new Vector3(0, -0.05f, 0), 0.05f).SetRelative();
        }
    }

    private void ReleaseGesture_Released(object sender, System.EventArgs e)
    {
        if (IsActive)
        {
            SpriteRenderer?.transform.DOLocalMove(new Vector3(0, 0.05f, 0), 0.05f).SetRelative();
        }
    }

    private void TapGesture_Tapped(object sender, System.EventArgs e)
    {
        if (IsActive)
        {
            GameAnimationSetting.instance.PlayAudioClick();
            Tapped?.Invoke();
        }
    }
}
