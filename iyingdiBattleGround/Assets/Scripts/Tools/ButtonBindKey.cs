using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBindKey : MonoBehaviour {


    public KeyCode bindKey = KeyCode.Return;

    private Button button;

    // Use this for initialization
    void Start () {
        button = GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
        if (button != null)
        {
            if (Input.GetKeyUp(bindKey))
            {
                button.onClick.Invoke();
            }
        }
	}
}
