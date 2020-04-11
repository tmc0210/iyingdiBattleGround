
using BIF;
using DG.Tweening;
using System;
using UnityEngine;

public class PrefabStayLong
{
    static readonly Map<string, Unit> animations = new Map<string, Unit>();

    public static Tween Show(string name, Vector3 position, Action<GameObject> setup = null, float time = -1f)
    {
        var unit = animations.GetByDefault(name, null);
        if (unit == null) return null;

        if (time < 0) time = unit.defaultTime;
        GameObject gameObject = null;

        return DOTween.Sequence().PrependCallback(()=> {
            DOTween.Sequence()
            .PrependCallback(() => {
                gameObject = unit.getter.Invoke();
                gameObject.transform.localPosition = position;
                setup?.Invoke(gameObject);
                unit.onStart?.Invoke(gameObject);
            })
            .AppendInterval(time)
            .AppendCallback(() => {
                unit.onComplete?.Invoke(gameObject);
                unit.remover?.Invoke(gameObject);
            });
        });
    }

    public static void SetAnimation(string name, 
        Func<GameObject> getter, 
        Action<GameObject> remover = null, 
        Action<GameObject> onStart = null, 
        Action<GameObject> onComplete = null,
        float defaultTime = 0.2f)
    {
        animations[name] = new Unit() {
            getter = getter,
            remover = remover,
            onStart = onStart,
            onComplete = onComplete,
            defaultTime = defaultTime,
        };
    }


    class Unit
    {
        public Func<GameObject> getter = null;
        public Action<GameObject> remover = null;
        public Action<GameObject> onStart = null;
        public Action<GameObject> onComplete = null;
        public float defaultTime = 0.2f;
    }
}
