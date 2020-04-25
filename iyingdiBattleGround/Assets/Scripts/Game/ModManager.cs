using OrderedJson.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class ModManger
{
    private static readonly string ModPath = Application.streamingAssetsPath + $"/Mods";
    private static readonly List<string> modsName = new List<string>();

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
        OJParser parser = new OJParser(typeof(CommonCommandDefiner));

        foreach (var modName in modNames)
        {
            // parse command
            var remapMethods = new Dictionary<string, string>();
            yield return mono.StartCoroutine(ParseAllCommandFromCsv(modName, remapMethods));
            parser.AddRemapMethod(remapMethods);

            // parse card
            BIF.Map<int, Card> cardMap = new BIF.Map<int, Card>();
            yield return mono.StartCoroutine(ParseAllCardFromCsv(modName, parser, cardMap));
            CardBuilder.AllCards.Update(cardMap);

        }

        // 执行卡牌的初始化函数
        GameEvent gameEvent = new GameEvent();
        foreach (var pair in CardBuilder.AllCards)
        {
            var card = pair.Value;
            gameEvent.hostCard = card;
            card.initMethod?.Invoke(gameEvent);
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
        Debug.Log($"load mod \"{modName}\" commands: {remapMethods.Count}");
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
        Debug.Log($"load mod \"{modName}\" cards: {cardMap.Count}");
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
}

