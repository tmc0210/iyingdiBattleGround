using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILevel : MonoBehaviour
{
    public GameObject starPrefab;
    [HideInInspector]
    public GridLayout gridLayout;
    private List<GameObject> starGos = new List<GameObject>(6);

    private void Awake()
    {
        gridLayout = GetComponent<GridLayout>();
    }

    public void SetLevel(int level)
    {
        if (level<=0||level>6)
        {
            Debug.LogErrorFormat("星级设置{0}不符合游戏规则！", level);
        }
        foreach (var go in starGos)
        {
            //TODO
            Destroy(go);
        }
        starGos.Clear();
        for (int i = 0; i < level; i++)
        {
            //TODO
            starGos.Add(Instantiate(starPrefab, transform));
        }
    }
}
