using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class GameAnimationSetting : MonoBehaviour
{
    #region public var

    [Autohook]
    public BattleBoardSetting BattleBoard;

    [Autohook]
    public SelectBoardSetting SelectBoard;
    [Autohook]
    public ExitWindowSetting ExitWindow;
    [Autohook]
    public ImageCollection Images;

    [Autohook]
    public AudioSource AudioMain;
    [Autohook]
    public AudioSource AudioCollection;
    [Autohook]
    public AudioSource AudioBattle;
    [Autohook]
    public AudioSource AudioHit;
    [Autohook]
    public AudioSource AudioPass;
    [Autohook]
    public AudioSource AudioClick;
    [Autohook]
    public AudioSource AudioBuy;
    [Autohook]
    public AudioSource AudioSell;
    [Autohook]
    public AudioSource AudioUpgrade;

    [Autohook]
    public TextMeshPro VerText;
    #endregion

    #region private var

    public Board board;
    public GameController gameController;
    Thread boardThread = null;

    public Action<Card> selectCardAction = null;
    public Action<int> selectPileAction = null;
    public Action<int> startSenceAction = null;
    public Action<bool> gameEndAction = null;

    #endregion

    #region get singleton
    public static GameAnimationSetting instance = null;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    #region game start from this

    
    private void Start()
    {
        GameStartSetting.instance.BeforeGameStart();

        VerText.text = Application.version;

        StartGame();
    }
    private void Update()
    {
        DoLoopAnimation();
    }

    private void StartGame()
    {
        InitTalkingData();
        CardBuilder.InitAllCards();
        //gameController = gameObject.AddComponent<GameController>();
        //UnlockAllHero(true);

        gameController = new GameController(this);
        StartCoroutine(gameController.EGameStart());
    }

    private void UnlockAllHero(bool lockCard)
    {
        CardBuilder.AllCards.FilterValue(card => card.cardType == CardType.Hero)
            .Map(card => card.Lock = !lockCard);

        GameController.SaveLockCard();
        GameController.LoadLockCard();
    }

    private void InitTalkingData()
    {
        DataCollection.Init();
    }

    #endregion

    #region main sences

    public void StartNewGame(BoardInitArgs boardInitArgs)
    {
        BattleBoard.gameObject.SetActive(true);
        BattleBoard.Init();
        SelectBoard.gameObject.SetActive(false);
        NewBoardThread(boardInitArgs);
        gameEndAction = win => {
            selectPileAction?.Invoke(win ? 0 : 1);
            BattleBoard.gameObject.SetActive(false);
            SelectBoard.gameObject.SetActive(true);
        };
    }

    public void StartSence()
    {
        BattleBoard.gameObject.SetActive(false);
        SelectBoard.gameObject.SetActive(true);
        SelectBoard.ShowStartSence();
        SelectBoard.startSenceAction = n =>
        {
            startSenceAction?.Invoke(n);
        };
    }

    public void SelectHero(List<Card> heros, List<Card> lockHeros = null)
    {
        SelectBoard.selectCardAction = hero => {
            Debug.Log("select " + hero?.name);
            selectCardAction?.Invoke(hero);
        };
        SelectBoard.SelectHero(heros, lockHeros);
    }

    public void ShowSummary(List<Card> heros, List<Card> treasures, CardPile cardPile)
    {
        SelectBoard.selectPileAction = n => {
            selectPileAction?.Invoke(0);
        };
        SelectBoard.ShowSummary(heros, treasures, cardPile);
    }

    public void SelectCardForCardPile(List<Card> firstPile, List<Card> secondPile, CardPile cardPile, string name1 = "", string name2 = "")
    {
        SelectBoard.selectPileAction = n => {
            //Debug.Log("select Pile " + n);
            selectPileAction?.Invoke(n);
        };
        GameAnimationSetting.instance.PlayAudioPass();
        SelectBoard.SelectPile(firstPile, secondPile, cardPile, name1, name2);
    }


    public void SelectTreasure(List<Card> list)
    {
        SelectBoard.selectCardAction = card => {
            //Debug.Log("select " + card.name);
            selectCardAction?.Invoke(card);
        };
        SelectBoard.SelectTreasure(list);
    }

    public void ShowCollection()
    {
        SelectBoard.selectCardAction = card => {
            selectCardAction?.Invoke(null);
        };
        SelectBoard.ShowCollection();
    }

    public void IntroEnemy(Player player, int level)
    {
        SelectBoard.selectCardAction = card => {
            selectCardAction?.Invoke(null);
        };
        SelectBoard.IntroEnemy(player, level);
    }

    #endregion

    #region audio

    public void PlayAudioMain()
    {
        if (!AudioMain.isPlaying)
        {
            AudioMain.Play();
        }
        AudioBattle.Stop();
        AudioCollection.Stop();

    }
    public void PlayAudioHit()
    {
        AudioHit.Play();
    }
    public void PlayAudioPass()
    {
        AudioPass.Play();
    }
    public void PlayAudioClick()
    {
        AudioClick.Play();
    }
    public void PlayAudioBuy()
    {
        AudioBuy.Play();
    }
    public void PlayAudioSell()
    {
        AudioSell.Play();
    }
    public void PlayAudioUpgrade()
    {
        AudioUpgrade.Play();
    }

    public void PlayAudioCollection()
    {
        if (!AudioCollection.isPlaying)
        {
            AudioCollection.Play();
        }
        AudioBattle.Stop();
        AudioMain.Stop();
    }
    public void PlayAudioBattle()
    {
        if (!AudioBattle.isPlaying)
        {
            AudioBattle.Play();
        }
        AudioMain.Stop();
        AudioCollection.Stop();
    }

    #endregion

    #region board thread


    public void NewBoardThread(BoardInitArgs boardInitArgs)
    {

        if (boardThread != null)
        {
            boardThread.Abort();
        }
        //boardThread = new Thread(new ThreadStart(BoardThread));
        boardThread = new Thread(new ParameterizedThreadStart(BoardThread));
        boardThread.Start(boardInitArgs);
    }

    public readonly BlockingCollection<ChangeMessage> changeMessagesToServer = new BlockingCollection<ChangeMessage>();
    public readonly BlockingCollection<ChangeMessage> changeMessagesToClient = new BlockingCollection<ChangeMessage>();


    public void BoardThread(object boardInitArgs)
    {
        board = new Board();
        board.changeMessagesToServer = changeMessagesToServer;
        board.SendGameMessageAction = changeMessage => {
            changeMessagesToClient.Add(changeMessage.Clone() as ChangeMessage);
        };
        board.InitGame((BoardInitArgs)boardInitArgs);

        while (true)
        {
            //Thread.Sleep(1000 / 60);
            board.OnMessage(changeMessagesToServer.Take());
        }
    }


    private void DoLoopAnimation()
    {
        if (changeMessagesToClient.TryTake(out ChangeMessage changeMessage))
        {
            BattleBoardSetting.instance.OnGameMessage(changeMessage);
        }
    }




    #endregion
}