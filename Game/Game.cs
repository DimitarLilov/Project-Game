﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.ConsoleKey;

namespace Game
{
    class Game
    {
        const int Height = 30;
        const int Width = 30;
        static int liveCount = 5;
        static int shotsCount = 0;
        static int KillsCount = 0;
        static string success;
        static string name;
        static int playerPosition;
        static List<List<int>> enemies = new List<List<int>>();
        static List<List<int>> shots = new List<List<int>>();
        static Dictionary<string, int> topResults = new Dictionary<string, int>();
        static char playerSymbol = (char)1;
        static char enemySymbol = (char)31;
        static char shotSymbol = '|';
        static char heart = (char)3;
        static Random rend = new Random();

        private static void Main()
        {
            Console.Title = "Game";
            Console.BufferHeight = Console.WindowHeight = Height;
            Console.BufferWidth = Console.WindowWidth = 90;
            Console.Write("Player Name: ");
            name = Console.ReadLine();
            Console.Title = name + " Game";
            playerPosition = Width / 2;
            LoadeResult(topResults);
            int steps = 0;
            int enemiesPause = 4;
            if (name != string.Empty)
            {
                while (liveCount > 0)
                {

                    UpdateField();
                    if (steps % enemiesPause == 0)
                    {
                        GenerateRandomEnemy();
                        UpdateEnemies();
                        HandleCollisionsEnemiesPlayer();
                        steps = 0;
                    }
                    steps++;
                    Drow();
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                        while (Console.KeyAvailable)
                        {
                            Console.ReadKey(true);

                        }
                        if (pressedKey.Key == ConsoleKey.LeftArrow)
                        {
                            if (playerPosition - 1 > 0)
                            {
                                playerPosition--;
                            }
                        }

                        if (pressedKey.Key == ConsoleKey.RightArrow)
                        {
                            if (playerPosition + 1 < Width)
                            {
                                playerPosition++;
                            }
                        }
                        if (pressedKey.Key == ConsoleKey.Spacebar)
                        {
                            Shoot();
                            shotsCount++;
                        }

                    }

                    Thread.Sleep(200);
                    Console.Clear();
                }
            }
            

            Console.Clear();
            Console.WriteLine("GAME OVER");
            Console.WriteLine("Kills: " + KillsCount);
            Console.WriteLine("Shots: " + shotsCount);
            Console.WriteLine("Success: " + success);
            SaveResults();



        }

        private static void SaveResults()
        {
            using (var source = new FileStream("../../Game.txt", FileMode.Append))
            {

                byte[] bytes = Encoding.UTF8.GetBytes(name + "|" + KillsCount + "" + Environment.NewLine);
                source.Write(bytes, 0, bytes.Length);
            }
        }

        private static void LoadeResult(Dictionary<string, int> topResults)
        {
            StreamReader reader = new StreamReader("../../Game.txt");
            using (reader)
            {
                string line = reader.ReadLine();

                while (true)
                {
                    if (line != null)
                    {
                        string[] playerInfo = line.Split('|');
                        string namePlayer = playerInfo[0];
                        string points = playerInfo[1];
                        if (points != "NaN" && namePlayer !="")
                        {
                            int point = int.Parse(points);
                            if (!topResults.ContainsKey(namePlayer))
                            {

                                topResults.Add(namePlayer, point);
                            }

                            topResults.Remove(namePlayer);
                            topResults.Add(namePlayer, point);
                        }

                       
                    }
                    else
                    {
                        break;
                    }

                    line = reader.ReadLine();
                }

            }

        }



        private static void DrowTopResults(int x, int y, Dictionary<string, int> topResult)
        {
            int i = 0;
            foreach (KeyValuePair<string, int> result in topResult.OrderByDescending(key => key.Value).Take(10))
            {
                Console.SetCursorPosition(x, y + i);
                Console.WriteLine("{0} - {1} Kills", result.Key, result.Value);
                i++;

            }




        }
        private static void DrowInfo(int x, int y, string str, ConsoleColor color)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.WriteLine(str);

        }

        private static void UpdateField()
        {
            UpdateShots();
            HandleCollisionsEnemiesShots();
        }

        private static void HandleCollisionsEnemiesPlayer()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i][0] == playerPosition && enemies[i][1] == Height - 1)
                {
                    liveCount--;
                }
            }
        }

        private static void HandleCollisionsEnemiesShots()
        {
            List<int> enemiesToRemove = new List<int>();
            List<int> shotsToRemove = new List<int>();
            for (int enemy = 0; enemy < enemies.Count; enemy++)
            {
                for (int shot = 0; shot < shots.Count; shot++)
                {
                    if (shots[shot][0] == enemies[enemy][0] && shots[shot][1] == enemies[enemy][1])
                    {
                        enemiesToRemove.Add(enemy);
                        shotsToRemove.Add(shot);
                        KillsCount++;
                    }
                }
            }
            List<List<int>> newEnemies = new List<List<int>>();
            List<List<int>> newShots = new List<List<int>>();
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemiesToRemove.Contains(i))
                {
                    newEnemies.Add(enemies[i]);

                }
            }
            for (int i = 0; i < shots.Count; i++)
            {
                if (!shotsToRemove.Contains(i))
                {
                    newShots.Add(shots[i]);
                }
            }
            enemies = newEnemies;
            shots = newShots;
        }

        private static void UpdateShots()
        {
            for (int i = 0; i < shots.Count; i++)
            {
                shots[i][1] = shots[i][1] - 1;
            }
            int index = -1;
            for (int i = 0; i < shots.Count; i++)
            {
                if (shots[i][1] <= 1)
                {
                    index = i;
                    break;

                }
            }
            if (index != -1)
            {
                shots.RemoveAt(index);
            }
        }

        private static void UpdateEnemies()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i][1] = enemies[i][1] + 1;
            }
            int index = -1;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i][1] >= Height)
                {
                    index = i;
                    break;

                }
            }
            if (index != -1)
            {
                enemies.RemoveAt(index);
            }
        }

        private static void GenerateRandomEnemy()
        {
            int randomEnemyPosition = rend.Next(1, Width);
            List<int> randomEnemyCordinates = new List<int>()
            {
                randomEnemyPosition,
                1
            };
            enemies.Add(randomEnemyCordinates);
        }

        private static void Shoot()
        {
            shots.Add(new List<int>() { playerPosition, Height - 3 });
        }

        private static void Drow()
        {

            success = ((KillsCount * 100.0) / shotsCount).ToString("F2");
            DrowEnemies();
            DrowShots();
            DrowPlayer();
            DrowInfo(35, 2, "Player Name: " + name, ConsoleColor.White);
            DrowInfo(35, 4, "Lives: " + new string(heart, liveCount), ConsoleColor.Red);
            DrowInfo(35, 6, "Shots: " + shotsCount, ConsoleColor.Blue);
            DrowInfo(35, 8, "Kills: " + KillsCount, ConsoleColor.Green);
            DrowInfo(35, 10, "Success: " + success + " %", ConsoleColor.Yellow);
            DrowInfo(35, 12, "TOP 10: ", ConsoleColor.Red);
            DrowTopResults(35, 13, topResults);
        }
        private static void DrowSymbolAtCoordinates(List<int> coordinates, char symbol, ConsoleColor color)
        {
            Console.SetCursorPosition(coordinates[0], coordinates[1]);
            Console.ForegroundColor = color;
            Console.WriteLine(symbol);

        }
        private static void DrowShots()
        {
            foreach (List<int> shot in shots)
            {
                ConsoleColor shotColor = ConsoleColor.Blue;
                DrowSymbolAtCoordinates(shot, shotSymbol, shotColor);
            }
        }

        private static void DrowEnemies()
        {
            foreach (List<int> enemy in enemies)
            {
                ConsoleColor enemyColor = ConsoleColor.Green;
                DrowSymbolAtCoordinates(enemy, enemySymbol, enemyColor);
            }
        }

        private static void DrowPlayer()
        {
            List<int> PlayerCoordinates = new List<int>() { playerPosition, Height - 1 };
            ConsoleColor playerColor = ConsoleColor.White;
            char symbol = playerSymbol;
            DrowSymbolAtCoordinates(PlayerCoordinates, playerSymbol, playerColor);
        }
    }
}
