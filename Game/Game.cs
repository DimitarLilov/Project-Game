using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.ConsoleKey;

namespace Game
{
    class Game
    {
        static SoundPlayer shotsSound = new SoundPlayer("../../Resource/shot.wav");
        static SoundPlayer killPlayer = new SoundPlayer("../../Resource/KillPlayer.wav");
        static SoundPlayer enemyDead = new SoundPlayer("../../Resource/enemyDead.wav");
        const int Height = 30;
        const int Width = 30;
        static int liveCount = 3;
        static int shotsCount;
        static int KillsCount;
        static string success;
        static string name;
        static int playerPosition;
        static List<List<int>> enemies = new List<List<int>>();
        static List<List<int>> bonus = new List<List<int>>();
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
            LoadeResult();
            int stepsEnemy = 0;
            int stepsBonus = 0;
            int enemiesPause = 6;
            int liveBonusPause = 500;
            if (name != string.Empty)
            {
                while (liveCount > 0)
                {

                    UpdateField();
                    if (stepsEnemy % enemiesPause == 0)
                    {
                        GenerateRandomEnemy();
                        UpdateEnemies();
                        HandleCollisionsEnemiesPlayer();
                        stepsEnemy = 0;
                    }
                    stepsEnemy++;
                    if (stepsBonus != 0 && stepsBonus % liveBonusPause == 0)
                    {
                        GenerateRandomBonus();
                        stepsBonus = 0;
                    }
                    stepsBonus++;
                    
                    Drow();
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                        while (Console.KeyAvailable)
                        {
                            Console.ReadKey(true);

                        }
                        if (pressedKey.Key == ConsoleKey.LeftArrow || pressedKey.Key == ConsoleKey.A)
                        {
                            if (playerPosition - 1 > 0)
                            {
                                playerPosition--;
                            }
                        }
                        else
                        if (pressedKey.Key == ConsoleKey.RightArrow || pressedKey.Key == ConsoleKey.D)
                        {
                            if (playerPosition + 1 < Width )
                            {
                                playerPosition++;
                            }
                        }
                        else
                        if (pressedKey.Key == ConsoleKey.Spacebar)
                        {
                            Shoot();
                            shotsCount++;
                            shotsSound.Play();
                        }

                    }

                    Thread.Sleep(200);
                    Console.Clear();
                }
            }
            Console.WriteLine("GAME OVER");
            Console.WriteLine("Kills: " + KillsCount);
            Console.WriteLine("Shots: " + shotsCount);
            Console.WriteLine("Success: " + success);
            SaveResults();
        }

        private static void SaveResults()
        {
            using (var source = new FileStream("../../Resource/Result.txt", FileMode.Append))
            {

                byte[] bytes = Encoding.UTF8.GetBytes(name + "|" + KillsCount + "" + Environment.NewLine);
                source.Write(bytes, 0, bytes.Length);
            }
        }

        private static void LoadeResult()
        {
            StreamReader reader = new StreamReader("../../Resource/Result.txt");
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
                        if (points != "NaN" && namePlayer != "")
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

        private static void DrowTopResults(int x, int y)
        {
            int i = 0;
            foreach (KeyValuePair<string, int> result in topResults.OrderByDescending(key => key.Value).Take(10))
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
            HandleCollisionsEnemiesShots();
            UpdateShots();
            HandleCollisionsBonusPlayer();
            UpdateLiveBonus();
        }

        private static void HandleCollisionsEnemiesPlayer()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i][0] == playerPosition && enemies[i][1] == Height - 1)
                {
                    liveCount--;
                    killPlayer.Play();
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
                        enemyDead.Play();
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

        private static void HandleCollisionsBonusPlayer()
        {
            for (int i = 0; i < bonus.Count; i++)
            {
                if (bonus[i][0] == playerPosition && bonus[i][1] == Height - 1)
                {
                    if (liveCount < 10)
                    {
                        liveCount++;
                    }
                }
            }
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
            DrowBonus();
            DrowShots();
            DrowPlayer();
            DrowInfo(35, 2, "Player Name: " + name, ConsoleColor.White);
            DrowInfo(35, 4, "Lives: " + new string(heart, 10), ConsoleColor.DarkRed);
            DrowInfo(35, 4, "Lives: " + new string(heart, liveCount), ConsoleColor.Red);
            DrowInfo(35, 6, "Shots: " + shotsCount, ConsoleColor.Blue);
            DrowInfo(35, 8, "Kills: " + KillsCount, ConsoleColor.Green);
            DrowInfo(35, 10, "Success: " + success + " %", ConsoleColor.Yellow);
            DrowInfo(35, 12, "TOP 10: ", ConsoleColor.Red);
            DrowTopResults(35, 13);
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

        private static void DrowBonus()
        {
            foreach (List<int> live in bonus)
            {
                ConsoleColor bonusColor = ConsoleColor.Red;
                DrowSymbolAtCoordinates(live, heart, bonusColor);
            }
        }

        private static void DrowPlayer()
        {
            List<int> PlayerCoordinates = new List<int>() { playerPosition, Height - 1 };
            ConsoleColor playerColor = ConsoleColor.White;
            char symbol = playerSymbol;
            DrowSymbolAtCoordinates(PlayerCoordinates, playerSymbol, playerColor);
        }

        private static void UpdateLiveBonus()
        {
            for (int i = 0; i < bonus.Count; i++)
            {
                bonus[i][1] = bonus[i][1] + 1;
            }
            int index = -1;
            for (int i = 0; i < bonus.Count; i++)
            {
                if (bonus[i][1] >= Height)
                {
                    index = i;
                    break;

                }
            }
            if (index != -1)
            {
                bonus.RemoveAt(index);
            }
        }

        private static void GenerateRandomBonus()
        {
            int randomBonusPosition = rend.Next(1, Width);
            List<int> randomBonusCordinates = new List<int>()
            {
                randomBonusPosition,
                1
            };
            bonus.Add(randomBonusCordinates);
        }
    }
}
