using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : UIPanel
{
    public Text text;
    public Button btn_Confirm;
    public Button btn_Cancel;

    public override void ShowPanel(UIData data)
    {
        var uidata = data as ConfirmUIData;
        text.text = uidata.tip;
        btn_Confirm.onClick.RemoveAllListeners();
        btn_Confirm.onClick.AddListener(() => uidata.action());
    }

    public void Confirm()
    {
        Application.Quit();
    }

    public class ConfirmUIData : UIData
    {
        public string tip;
        public Action action;

        public ConfirmUIData(string tip, Action action)
        {
            this.tip = tip;
            this.action = action;
        }
    }

}
