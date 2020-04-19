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
            go.SetActive(false);
        }
    }

    private void Start()
    {
        ShowPanel<IntroPanel>();
    }

    public void ShowPanel<P>() where P : UIPanel
    {
        var panel = Panels.Find(p => p is P);
        panel.gameObject.SetActive(true);
        panel.ShowPanel();
    }

    public void HidePanel<P>() where P : UIPanel
    {
        var panel = Panels.Find(p => p is P);
        panel.gameObject.SetActive(true);
        panel.ShowPanel();
    }

}
