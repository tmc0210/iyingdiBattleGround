using OrderedJson.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Threading;

public static class ModManger
{
    private static readonly string ModPath = Application.streamingAssetsPath + $"/Mods";
    private static readonly List<string> modsName = new List<string>();
    public static OJParser parser = null;

    public static List<string> GetModsName()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(ModPath);
        foreach (var modPath in directoryInfo.GetDirectories())
        {
            Debug.Log("find mod: "+ modPath.Name);
            if (!modsName.Contains(modPath.Name))
            {
                modsName.Add(modPath.Name);
            }
        }
        return modsName;
    }

    public static IEnumerator LoadModsByName(List<string> modNames, MonoBehaviour mono)
    {
        yield return mono.StartCoroutine(InitAllCards(modNames, mono));
        yield return mono.StartCoroutine(LoadImages(modNames, mono));
        StartReadCmdFile();
    }

    private static IEnumerator LoadImages(List<string> modNames, MonoBehaviour mono)
    {
        foreach(string modName in modNames)
        {
            var imageDirPath = GetImageDirPath(modName);
            DirectoryInfo directoryInfo = new DirectoryInfo(imageDirPath);
            if (!directoryInfo.Exists) continue;
            foreach (var imageName in directoryInfo.GetFiles())
            {
                if (imageName.Extension != ".jpg" && imageName.Extension != ".png") continue;
                //Debug.Log("Got: " + imageName.Name);

                var unityWebRequest = UnityWebRequest.Get(imageName.FullName);
                DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
                unityWebRequest.downloadHandler = texDl;
                yield return unityWebRequest.SendWebRequest();

                while (!texDl.isDone) //判断是否接受数据
                {
                    yield return texDl;
                }
                var sprite = Sprite.Create(texDl.texture, new Rect(0, 0, texDl.texture.width, texDl.texture.height), new Vector2(0.5f, 0.5f));
                sprite.name = imageName.Name.Substring(0, imageName.Name.Length-4);
                ImageCollection.instance.spritesIngame.Add(sprite);

            }
        }
        yield return null;
    }

    public static IEnumerator InitAllCards(List<string> modNames, MonoBehaviour mono)
    {
        CardBuilder.AllCards = new BIF.Map<int, Card>();
        parser = new OJParser(typeof(CommonCommandDefiner));
        foreach (var pair in ProxyEventDefiner.GetProxyEvents())
        {
            parser.AddMethod(pair.Key, pair.Value);
        }

        foreach (var modName in modNames)
        {
            // parse command
            var remapMethods = new Dictionary<string, string>();
            yield return mono.StartCoroutine(ParseAllCommandFromCsv(modName, remapMethods));

            try
            {
                parser.AddRemapMethod(remapMethods);
            }
            catch (OJException e)
            {
                $"[Error] {e.Message}".LogToFile();
                Debug.LogWarning(e);
            }

            // parse card
            BIF.Map<int, Card> cardMap = new BIF.Map<int, Card>();
            yield return mono.StartCoroutine(ParseAllCardFromCsv(modName, parser, cardMap));
            CardBuilder.AllCards.Update(cardMap);

        }

        try
        {
            // 执行卡牌的初始化函数
            GameEvent gameEvent = new GameEvent();
            foreach (var pair in CardBuilder.AllCards)
            {
                var card = pair.Value;
                gameEvent.hostCard = card;
                card.initMethod?.Invoke(gameEvent);
            }
        }
        catch (OJException e)
        {
            $"[Error] {e.Message}".LogToFile();
            Debug.LogWarning(e);
        }
    }


    private static IEnumerator ParseAllCommandFromCsv(string modName, Dictionary<string, string> remapMethods)
    {
        var commandPath = GetCommandCSVPath(modName);
        var commandRequest = UnityWebRequest.Get(commandPath);
        yield return commandRequest.SendWebRequest();

        var commandtext = commandRequest.downloadHandler.text;
        commandRequest.downloadHandler.Dispose();
        if (!string.IsNullOrEmpty(commandtext))
        {
            var commands = CsvFileReader.Parse(commandtext);
            foreach (var line in commands)
            {
                if (line.Count < 2) break;
                if (line[0].StartsWith("//")) continue;
                if (!string.IsNullOrEmpty(line[0]) && !string.IsNullOrEmpty(line[1]))
                {
                    remapMethods.Add(line[0], line[1]);
                }
            }
        }
        //Debug.Log($"load mod \"{modName}\" commands: {remapMethods.Count}");
    }

    private static IEnumerator ParseAllCardFromCsv(string modName, OJParser parser, BIF.Map<int, Card> cardMap)
    {
        var path = GetCardCSVPath(modName);
        var request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        var text = request.downloadHandler.text;
        request.downloadHandler.Dispose();
        if (!string.IsNullOrEmpty(text))
        {
            var data = CsvFileReader.Parse(text);


            cardMap.Update(CardBuilder.ReadCSV(data, parser));
           
        }
        //Debug.Log($"load mod \"{modName}\" cards: {cardMap.Count}");
    }


    private static string GetCardCSVPath(string modName)
    {
        return Application.streamingAssetsPath + $"/Mods/{modName}/Card/card.csv";
    }
    private static string GetCommandCSVPath(string modName)
    {
        return Application.streamingAssetsPath + $"/Mods/{modName}/Card/command.csv";
    }
    private static string GetImageDirPath(string modName)
    {
        return Application.streamingAssetsPath + $"/Mods/{modName}/Art";
    }



    private static readonly string LogFilePath = Application.streamingAssetsPath+"/ModHelper/Data/logging.log";
    private static readonly string CommandFilePath = Application.streamingAssetsPath+"/ModHelper/Data/command";


    public static void LogToFile(this string log)
    {
#if UNITY_STANDALONE
        using (FileStream fs = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
        {
            var buffer = Encoding.Default.GetBytes(log + "\n");
            fs.Write(buffer, 0, buffer.Length);
        }
#endif
    }

    public static void StopReadCmdFile()
    {
        //isCanRead = false;
        readCmdFileThread?.Abort();
    }
    public static Thread readCmdFileThread = null;
    public static void StartReadCmdFile()
    {
        $"游戏开始接受卡牌创建指令...".LogToFile();
        Debug.Log("开始接受卡牌创建指令...");
#if UNITY_STANDALONE
        if (!File.Exists(CommandFilePath))
        {
            File.Create(CommandFilePath).Dispose();
        }

        //Task.Run((Func<Task>)(() =>
        //{
        //    ReadCmdFile();
        //}));

        readCmdFileThread = new Thread(ReadCmdFile);
        readCmdFileThread.Start();
    }

    private static void ReadCmdFile()
    {
        byte[] buffer = new byte[1024 * 10];
        using (FileStream fs = new FileStream(CommandFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            fs.Seek(0, SeekOrigin.End);
            while (true)
            {
                //Debug.Log("get cmd...");
                int length = fs.Read(buffer, 0, 1024 * 10);
                string cmdText = Encoding.UTF8.GetString(buffer, 0, length).Trim();
                if (!string.IsNullOrEmpty(cmdText))
                {
                    Debug.Log($"get cmd...{cmdText}");
                    var csvDate = CsvFileReader.Parse(cmdText);
                    foreach (var line in csvDate)
                    {
                        //Debug.Log($"get cmd...{line.Count}");
                        Card nCard = CardBuilder.NewCardByCsvData(line);
                        try
                        {
                            GameEvent gameEvent = new GameEvent
                            {
                                hostCard = nCard
                            };
                            nCard.initMethod?.Invoke(gameEvent);
                            var oldCard = CardBuilder.SearchCardByName(nCard.name, false);
                            if (oldCard != null)
                            {
                                nCard.id = oldCard.id;
                                nCard.goldVersion = oldCard.goldVersion;
                                CardBuilder.AllCards[nCard.id] = nCard;
                            }
                            else
                            {
                                nCard.id = CardBuilder.idCnt++;
                                var goldenCard = CardBuilder._GetNewGoldenCard(nCard);
                                nCard.goldVersion = goldenCard.id;
                                CardBuilder.AllCards[nCard.id] = nCard;
                                CardBuilder.AllCards[goldenCard.id] = goldenCard;
                            }
                            GameAnimationSetting.instance.board?.CreateCard(nCard);
                        }
                        catch (OJException e)
                        {
                            $"[Error] {e.Message}".LogToFile();
                        }

                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
#endif
}

