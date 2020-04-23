using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
            }
            return instance;
        }
    }


    public Dictionary<string, UIPanel> PanelsDict = new Dictionary<string, UIPanel>();
    public List<UIPanel> Panels = new List<UIPanel>();
    public List<GameObject> PanelGos;

    private void Awake()
    {
        foreach (var go in PanelGos)
        {
            Panels.Add(go.GetComponent<UIPanel>());
            //go.SetActive(false);
        }
    }

    private void Start()
    {
        //ShowPanel<IntroPanel>();
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="P">想要显示的面板</typeparam>
    public void ShowPanel<P>() where P : UIPanel
    {
        var panel = Panels.Find(p => p is P);
        panel.gameObject.SetActive(true);
        panel.ShowPanel();
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="P">想要显示的面板</typeparam>
    /// <typeparam name="Data">想要传输的UI数据类型</typeparam>
    /// <param name="data">想要传输的UI数据类型</param>
    public void ShowPanel<P, Data>(UIData data) where P : UIPanel
    {
        var panel = Panels.Find(p => p is P);
        panel.gameObject.SetActive(true);
        panel.ShowPanel(data);
    }

    public void HidePanel<P>() where P : UIPanel
    {
        var panel = Panels.Find(p => p is P);
        panel.gameObject.SetActive(true);
        panel.ShowPanel();
    }

    public P GetPanel<P>() where P : UIPanel
    {
        return (P)Panels.Find(p => p is P);
    }

}
