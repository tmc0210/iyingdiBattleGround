using BIF;
using System;
using System.Reflection;
using TouchScript;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour {


    Action<int> IntPrint;

    // Use this for initialization
    void Start() {
        //Card card = CardBuilder.NewCard(1);
        //print(CardBuilder.GetProxyDescription(card.GetProxys(ProxyEnum.WhenMinionSummon), true));
        //print(CardBuilder.GetCardDescription(card));
        //print("card "+card.proxys[ProxyEnum.WhenMinionSummon]);

    }

    class III {
        public III()
        {
            print("new III");
        }
    }


    Board board;

    [ContextMenu("test")]
    public void Test()
    {
        //board.SendGameMessage(new ChangeMessage());
        Action a = TestAction;
        Action b = a;
        b += TestAction;
        a?.Invoke();
    }

    public void TestAction()
    {
        print("in testAction");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnEnable()
    {
        GetComponent<LongPressGesture>().LongPressed += pointersPressedHandler;
        GetComponent<TransformGesture>().TransformStarted += TransformEventHandler;
        
    }
    
    private void OnDisable()
    {
        GetComponent<LongPressGesture>().LongPressed -= pointersPressedHandler;
        GetComponent<TransformGesture>().TransformStarted -= TransformEventHandler;
    }

    private void pointersPressedHandler(object sender, EventArgs e)
    {
        print("LongPressed");

    }
    private void TransformEventHandler(object sender, EventArgs e)
    {
        print("transfrom started");
    }

    void PrintInt(int i)
    {
        print(i);
    }
}
