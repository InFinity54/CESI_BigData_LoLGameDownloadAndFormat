using ChoETL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        readonly static string saveDir = Environment.CurrentDirectory + @"\Games\JSON_Original\";
        readonly static string saveFormattedDir = Environment.CurrentDirectory + @"\Games\JSON_Formatted\";
        readonly static string saveCsvDir = Environment.CurrentDirectory + @"\Games\CSV\";
        readonly static string puuid = "<puuid>";
        readonly static string riotApiKey = "<key-here>";
        static List<string> gamesList = new();
        static List<string> savedGamesList = new();

        static void Main(string[] args)
        {
            Console.WriteLine("Récupération de la liste des parties...");

            getGameList();

            Console.WriteLine("Récupération des parties...");

            for (int i = 0; i < gamesList.Count; i++)
            {
                if (!File.Exists(saveDir + gamesList[i] + ".json"))
                {
                    getGame(gamesList[i]);
                    Thread.Sleep(100);
                }
                else
                {
                    Console.WriteLine("La partie " + gamesList[i] + " existe déjà. Passage à la partie suivante.");
                }
            }

            Console.WriteLine("Mise à jour de la liste des parties...");

            for (int i = 0; i < gamesList.Count; i++)
            {
                if (File.Exists(saveDir + gamesList[i] + ".json"))
                {
                    savedGamesList.Add(gamesList[i]);
                }
            }

            Console.WriteLine("Formattage des parties...");

            for (int i = 0; i < savedGamesList.Count; i++)
            {
                formatGame(savedGamesList[i]);
            }

            Console.WriteLine("Terminé.");
            Console.WriteLine("Appuyez sur une touche pour quitter...");
            Console.ReadLine();
        }

        static void getGameList(int start = 0, bool hasMoreGames = true)
        {
            using (WebClient wc = new WebClient())
            {
                if (hasMoreGames)
                {
                    try
                    {
                        string json = wc.DownloadString("https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/" + puuid + "/ids?start=" + start.ToString() + "&count=100&api_key=" + riotApiKey);
                        JArray jsonArray = JArray.Parse(json);

                        if (jsonArray.Count > 0)
                        {
                            for (int i = 0; i < jsonArray.Count; i++)
                            {
                                gamesList.Add(jsonArray[i].ToString());
                            }

                            Console.WriteLine("Parties " + start.ToString() + " à " + (start + 100).ToString() + " récupérées.");
                            Thread.Sleep(100);
                            getGameList(start + 100, hasMoreGames);
                        }
                    }
                    catch (WebException err)
                    {
                        if (err.Message.Contains("429"))
                        {
                            Console.WriteLine("API surchargée. Reprise dans 60 secondes...");
                            Thread.Sleep(60000);
                            getGameList(start, hasMoreGames);
                        }
                        else
                        {
                            Console.WriteLine("Erreur durant la récupération des parties : " + err.Message);
                        }
                    }
                }
            }
        }

        static void getGame(string gameId)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    string json = wc.DownloadString("https://europe.api.riotgames.com/lol/match/v5/matches/" + gameId + "?api_key=" + riotApiKey);
                    File.WriteAllText(saveDir + gameId + ".json", json);
                    Console.WriteLine("Partie " + gameId + " récupérée.");
                }
                catch (WebException err)
                {
                    if (err.Message.Contains("404"))
                    {
                        Console.WriteLine("La partie " + gameId + " est introuvable sur les serveurs de Riot.");
                    }
                    else if (err.Message.Contains("429"))
                    {
                        Console.WriteLine("API surchargée. Reprise dans 60 secondes...");
                        Thread.Sleep(60000);
                        getGame(gameId);
                    }
                    else
                    {
                        Console.WriteLine("Erreur pour la partie " + gameId + " : " + err.Message);
                    }
                }
            }
        }

        static void formatGame(string gameId)
        {
            JObject jsonArray = JObject.Parse(File.ReadAllText(saveDir + gameId + ".json"));

            for (int i = 0; i < ((JArray)jsonArray["info"]["participants"]).Count; i++)
            {
                JObject gamePlayer = new JObject();

                gamePlayer.Add("dataVersion", jsonArray["metadata"]["dataVersion"].ToString());
                gamePlayer.Add("matchId", jsonArray["metadata"]["matchId"].ToString());
                gamePlayer.Add("gameCreation", jsonArray["info"]["gameCreation"].ToString());
                gamePlayer.Add("gameDuration", jsonArray["info"]["gameDuration"].ToString());
                gamePlayer.Add("gameId", jsonArray["info"]["gameId"].ToString());
                gamePlayer.Add("gameMode", jsonArray["info"]["gameMode"].ToString());
                gamePlayer.Add("gameName", jsonArray["info"]["gameName"].ToString());
                gamePlayer.Add("gameStartTimestamp", jsonArray["info"]["gameStartTimestamp"].ToString());
                gamePlayer.Add("timestamp", jsonArray["info"]["gameStartTimestamp"].ToString());
                gamePlayer.Add("gameType", jsonArray["info"]["gameType"].ToString());
                gamePlayer.Add("gameVersion", jsonArray["info"]["gameVersion"].ToString());
                gamePlayer.Add("mapId", jsonArray["info"]["mapId"].ToString());
                gamePlayer.Add("platformId", jsonArray["info"]["platformId"].ToString());
                gamePlayer.Add("queueId", jsonArray["info"]["queueId"].ToString());

                foreach (JToken item in jsonArray["info"]["participants"][i])
                {
                    gamePlayer.Add(item);
                }

                gamePlayer.Property("perks").Remove();

                File.WriteAllText(saveFormattedDir + gameId + "_" + jsonArray["info"]["participants"][i]["summonerName"] + ".json", gamePlayer.ToString());

                FileStream csvFile = File.OpenWrite(saveCsvDir + gameId + "_" + jsonArray["info"]["participants"][i]["summonerName"] + ".csv");
                List<string> headerElements = new List<string>();
                List<string> gameElements = new List<string>();

                foreach (JProperty item in gamePlayer.Properties())
                {
                    headerElements.Add(item.Name);
                    gameElements.Add(item.Value.ToString());
                }

                csvFile.Write(String.Join(";", headerElements.ToArray()));
                csvFile.Write(Environment.NewLine);
                csvFile.Write(String.Join(";", gameElements.ToArray()));

                csvFile.Close();
            }
        }
    }
}
