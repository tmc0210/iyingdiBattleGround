using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanel : UIPanel
{
    public void ShowExit()
    {
        UIManager.Instance.ShowPanel<ExitPanel>();
    }

}
