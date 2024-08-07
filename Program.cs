﻿using System;
using System.Collections.Generic;
using System.Media;
using System.Text;
using System.Threading;

namespace Bashgeon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(120, 30);
            Console.Title = "Bashgeon";
            // Устанавливаем кодировку Unicode, чтобы все символы отображались, как нужно
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            Console.CursorVisible = false; // Убираем мигающий курсор, так как он не нужен в игре
                                           // Подгружаем все использующиеся в игре звуковые эффекты
            SoundPlayer mainMenuAmbience = new SoundPlayer($@"{Environment.CurrentDirectory}\assets\main_menu_bg_sound.wav");
            SoundPlayer wallDestroySound = new SoundPlayer($@"{Environment.CurrentDirectory}\assets\destroy_wall.wav");
            SoundPlayer pickUpTreasure = new SoundPlayer($@"{Environment.CurrentDirectory}\assets\pick_up.wav");
            SoundPlayer footstepSound = new SoundPlayer($@"{Environment.CurrentDirectory}\assets\footstep.wav");
            SoundPlayer enemyHitSound = new SoundPlayer($@"{Environment.CurrentDirectory}\assets\enemy_hit.wav");
            SoundPlayer portalWarp = new SoundPlayer($@"{Environment.CurrentDirectory}\assets\portal.wav");
            SoundPlayer selfHurt = new SoundPlayer($@"{Environment.CurrentDirectory}\assets\self_hurt.wav");
            // Инициализируем данные, нужные для выбора сложности
            string[] difficultyOptions = { "Easy", "Medium", "Hard" };
            int currentOptionIndex = 0;
            bool isDifficultyChosen = false;
            Dictionary<string, int> mapAttributes = new Dictionary<string, int>()
            {
                {"xSize", 0 },
                {"ySize", 0 },
                {"enemiesCount", 0 },
                {"treasuresCount", 0 }
            };
            Dictionary<string, int> playerInfo = new Dictionary<string, int>()
            {
                {"playerX", 0},
                {"playerY", 0},
                {"playerMaxHealth", 0},
                {"playerMaxMana", 0},
                {"playerHealth", 0},
                {"playerMana", 0},
                {"movesLeft", 0},
                {"killPoints", 0 },
                {"treasurePickUpPoints", 0},
                {"kills", 0 },
                {"treasures", 0},
                {"score",  0}
            };
            mainMenuAmbience.Play();

            // ===================================================================================================================================
            // ===================================================ГЛАВНОЕ МЕНЮ. ВЫБОР СЛОЖНОСТИ===================================================
            // ===================================================================================================================================
            while (!isDifficultyChosen)
            {
                DrawrMainMenu(difficultyOptions, ref currentOptionIndex);
                HandleDifficultyInput(playerInfo, mapAttributes, ref currentOptionIndex, ref isDifficultyChosen);
            }
            mainMenuAmbience.Stop();


            // ===================================================================================================================================
            // ==========================================================ГЕНЕРАЦИЯ КАРТЫ==========================================================
            // ===================================================================================================================================
            //Инициализируем данные для генерации карты
            char[,] map = new char[mapAttributes["ySize"], mapAttributes["xSize"]];
            Random entity = new Random();
            bool isPlayerSpawned = false;
            bool isFirstCycle = true;
            int treasuresSpawned = 0, enemiesSpawned = 0;
            // Генерация карты
            while (treasuresSpawned != mapAttributes["treasuresCount"] || enemiesSpawned != mapAttributes["enemiesCount"])
            {
                for (int y = 0; y < map.GetLength(0); y++)
                {
                    for (int x = 0; x < map.GetLength(1); x++)
                    {
                        if (y == 0 || y == map.GetLength(0) - 1 || x == 0 || x == map.GetLength(1) - 1) // стены по краям карты
                        {
                            map[y, x] = '#';
                            continue;
                        }
                        else if (x == 3 && y == 3 || x == map.GetLength(1) - 4 && y == map.GetLength(0) - 4)
                        {
                            map[y, x] = 'O';
                            continue;
                        }

                        switch (entity.Next(0, 25))
                        {
                            case 0:
                            case 1:
                            case 2:
                                if (isFirstCycle)
                                    map[y, x] = '#';
                                break;
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                            case 22:
                                if (!isPlayerSpawned)
                                {
                                    playerInfo["playerY"] = y;
                                    playerInfo["playerX"] = x;
                                    isPlayerSpawned = true;

                                }
                                if (isFirstCycle) map[y, x] = ' ';
                                break;
                            case 23:
                                if (!isFirstCycle && enemiesSpawned < mapAttributes["enemiesCount"] && map[y, x] == ' ' && (x != playerInfo["playerX"] || y != playerInfo["playerY"]))
                                {
                                    enemiesSpawned++;
                                    map[y, x] = '!';
                                }
                                else if (isFirstCycle) map[y, x] = ' ';
                                break;
                            case 24:
                                if (!isFirstCycle && treasuresSpawned < mapAttributes["treasuresCount"] && map[y, x] == ' ' && (x != playerInfo["playerX"] || y != playerInfo["playerY"]))
                                {
                                    treasuresSpawned++;
                                    map[y, x] = 'X';
                                }
                                else if (isFirstCycle) map[y, x] = ' ';
                                break;
                            default:
                                break;

                        }
                    }
                }
                isFirstCycle = false;
            }

            char currentCell;

            while (playerInfo["playerHealth"] > 0 && playerInfo["treasures"] != mapAttributes["treasuresCount"] && playerInfo["movesLeft"] > 0)
            {
                Console.Clear();
                DrawrMap(map, playerInfo);
                DrawHealthBar(playerInfo["playerHealth"], playerInfo["playerMaxHealth"]);
                DrawManaBar(playerInfo["playerMana"], playerInfo["playerMaxMana"]);
                DrawrUI(playerInfo, mapAttributes["treasuresCount"]);
                HandlePlayerInput(footstepSound, selfHurt, wallDestroySound, map, playerInfo);


                currentCell = map[playerInfo["playerY"], playerInfo["playerX"]];
                if (currentCell == 'X')
                {
                    map[playerInfo["playerY"], playerInfo["playerX"]] = ' ';
                    playerInfo["treasures"]++;
                    playerInfo["score"] += playerInfo["treasurePickUpPoints"];
                    playerInfo["movesLeft"] += 4;
                    pickUpTreasure.Play();
                }
                else if (currentCell == '!')
                {
                    map[playerInfo["playerY"], playerInfo["playerX"]] = ' ';
                    playerInfo["kills"]++;
                    playerInfo["score"] += playerInfo["killPoints"];
                    playerInfo["playerHealth"] -= 10;
                    playerInfo["movesLeft"] += 2;
                    enemyHitSound.Play();
                }
                else if (currentCell == 'O')
                {
                    portalWarp.Play();
                    if (playerInfo["playerX"] == 3)
                    {
                        playerInfo["playerX"] = map.GetLength(1) - 4;
                        playerInfo["playerY"] = map.GetLength(0) - 4;
                    }
                    else
                    {
                        playerInfo["playerX"] = 3;
                        playerInfo["playerY"] = 3;
                    }
                }
                Console.Clear();
            }
            Thread.Sleep(300);
            playerInfo["score"] += playerInfo["movesLeft"] * 35;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(@"
			 ██████╗  █████╗ ███╗   ███╗███████╗     ██████╗ ██╗   ██╗███████╗██████╗ 
			██╔════╝ ██╔══██╗████╗ ████║██╔════╝    ██╔═══██╗██║   ██║██╔════╝██╔══██╗
			██║  ███╗███████║██╔████╔██║█████╗      ██║   ██║██║   ██║█████╗  ██████╔╝
			██║   ██║██╔══██║██║╚██╔╝██║██╔══╝      ██║   ██║╚██╗ ██╔╝██╔══╝  ██╔══██╗
			╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗    ╚██████╔╝ ╚████╔╝ ███████╗██║  ██║
			 ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝     ╚═════╝   ╚═══╝  ╚══════╝╚═╝  ╚═╝");
            Console.SetCursorPosition(50, 11);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"| Врагов убито: {playerInfo["kills"]} |");
            Console.SetCursorPosition(48, 13);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"| Сокровищ собрано: {playerInfo["treasures"]} |");
            Console.SetCursorPosition(53, 15);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"| Очки: {playerInfo["score"]} |");
            Console.SetCursorPosition(42, 25);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static void DrawrMainMenu(string[] difficultyOptions, ref int currentOptionIndex)
        {
            // Отрисовка главного меню с выбором сложности
            Console.Clear();
            Console.WriteLine(@"
░▒▓███████▓▒░   ░▒▓██████▓▒░   ░▒▓███████▓▒░ ░▒▓█▓▒░░▒▓█▓▒░  ░▒▓██████▓▒░  ░▒▓████████▓▒░  ░▒▓██████▓▒░  ░▒▓███████▓▒░  
░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░        ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░        ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░        ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░        ░▒▓█▓▒░        ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓███████▓▒░  ░▒▓████████▓▒░  ░▒▓██████▓▒░  ░▒▓████████▓▒░ ░▒▓█▓▒▒▓███▓▒░ ░▒▓██████▓▒░   ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░        ░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░        ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░        ░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░        ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓███████▓▒░  ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓███████▓▒░  ░▒▓█▓▒░░▒▓█▓▒░  ░▒▓██████▓▒░  ░▒▓████████▓▒░  ░▒▓██████▓▒░  ░▒▓█▓▒░░▒▓█▓▒░ 
																														
																														
");
            Console.SetCursorPosition(50, 9);
            Console.WriteLine("Выберите сложность(↓↑): ");
            Console.SetCursorPosition(45, 25);
            Console.WriteLine("Нажмите Enter для подтверждения.");
            for (int i = 0; i < difficultyOptions.Length; i++)
            {

                Console.SetCursorPosition(55, 12 + i);
                if (currentOptionIndex == 0) Console.BackgroundColor = ConsoleColor.Green;
                else if (currentOptionIndex == 1) Console.BackgroundColor = ConsoleColor.DarkYellow;
                else if (currentOptionIndex == 2) Console.BackgroundColor = ConsoleColor.Red;
                if (i == currentOptionIndex) Console.WriteLine(difficultyOptions[i] + " <");
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(difficultyOptions[i]);
                }
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        static void DrawrMap(char[,] map, Dictionary<string, int> playerInfo)
        {
            // Отрисовка игрового поля
            for (int y = 0; y < map.GetLength(0); y++)
            {
                Console.SetCursorPosition(40, y);
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if (y == playerInfo["playerY"] && x == playerInfo["playerX"])
                    {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.Write("ጿ ");
                    }
                    else if (map[y, x] == 'O')
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(map[y, x] + " ");
                    }
                    else if (map[y, x] == 'X')
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(map[y, x] + " ");
                    }
                    else if (map[y, x] == '!')
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(map[y, x] + " ");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(map[y, x] + " ");
                    }
                }
                Console.WriteLine();
            }
        }

        static void DrawrUI(Dictionary<string, int> playerInfo, int treasuresCount)
        {
            Console.SetCursorPosition(0, 5);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Осталось шагов: {playerInfo["movesLeft"]}");
            if (playerInfo["treasures"] == treasuresCount)
            {
                Console.WriteLine("Все сокровища найдены!");
            }
            else
            {
                Console.WriteLine($"Сокровищ найдено: {playerInfo["treasures"]}");
            }
            Console.WriteLine($"Врагов побеждено: {playerInfo["kills"]}");
            Console.WriteLine($"Очки: {playerInfo["score"]}");
            Console.SetCursorPosition(47, 25);
            Console.WriteLine($"Ваши координаты: X: {playerInfo["playerX"]}; Y: {playerInfo["playerY"]}");
            Console.SetCursorPosition(100, 0);
            Console.WriteLine("Управление:");
            Console.SetCursorPosition(97, 2);
            Console.WriteLine("Передвижение: ← ↓ → ↑");
            Console.SetCursorPosition(97, 4);
            Console.WriteLine("Уничтожить стены: E");
            Console.SetCursorPosition(97, 6);
            Console.WriteLine("Адреналин: F");
        }

        static void DrawHealthBar(int hp, int maxHP)
        {
            Console.SetCursorPosition(0, 0);
            ConsoleColor hpColor = ConsoleColor.DarkGreen;
            if (hp <= maxHP / 5) hpColor = ConsoleColor.Red;
            else if (hp <= maxHP / 5 * 2) hpColor = ConsoleColor.DarkYellow;
            else if (hp <= maxHP / 5 * 3) hpColor = ConsoleColor.Yellow;
            else if (hp <= maxHP / 5 * 4) hpColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(0, 0);
            Console.Write("[");
            for (int i = 0; i < maxHP / 10; i++)
            {
                if (hp / 10 > i) Console.BackgroundColor = hpColor;
                else Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write(" ");
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("]");
        }

        static void DrawManaBar(int mana, int maxMP)
        {
            Console.SetCursorPosition(0, 2);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[");

            for (int i = 0; i < maxMP / 10; i++)
            {
                if (mana / 10 > i) Console.BackgroundColor = ConsoleColor.Blue;
                else Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write(" ");
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("]");
        }

        // ДЕКОМПОЗИРОВАТЬ
        static void HandleDifficultyInput(Dictionary<string, int> playerInfo, Dictionary<string, int> mapAttributes, ref int currentOptionIndex, ref bool isDifficultyChosen)
        {
            // регистрируем нажатие на клавишу и далее имитируем сдвиг указателя на сложность
            ConsoleKey difficultyChangeKey = Console.ReadKey().Key;
            switch (difficultyChangeKey)
            {

                case ConsoleKey.DownArrow:
                    if (currentOptionIndex < 2)
                    {
                        Console.Beep(900, 50);
                        currentOptionIndex++;
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (currentOptionIndex > 0)
                    {
                        Console.Beep(900, 50);
                        currentOptionIndex--;
                    }
                    break;
                // При нажатии Enter, определяется выбранная сложность и задаются соответсвенно начальные параметры для игры
                case ConsoleKey.Enter:
                    if (currentOptionIndex == 0)
                    {
                        InitializeGameInfo(playerInfo, 150, 100, 10, 45, 125, mapAttributes, 20, 12, 15, 15);
                    }
                    else if (currentOptionIndex == 1)
                    {
                        InitializeGameInfo(playerInfo, 100, 60, 12, 80, 200, mapAttributes, 24, 16, 20, 15);
                    }
                    else
                    {
                        InitializeGameInfo(playerInfo, 70, 40, 15, 125, 300, mapAttributes, 28, 20, 35, 12);
                    }
                    isDifficultyChosen = true;
                    break;
                default:
                    break;
            }
        }

        static void InitializeGameInfo(Dictionary<string, int> player, int HP, int MP, int moves, int killPts, int treasurePickUpPts,
            Dictionary<string, int> mapAttr, int x, int y, int enemies, int treasures)
        {
            player["playerHealth"] = HP;
            player["playerMana"] = MP;
            player["playerMaxHealth"] = HP;
            player["playerMaxMana"] = MP;
            player["movesLeft"] = moves;
            player["killPoints"] = killPts;
            player["treasurePickUpPoints"] = treasurePickUpPts;
            mapAttr["xSize"] = x;
            mapAttr["ySize"] = y;
            mapAttr["enemiesCount"] = enemies;
            mapAttr["treasuresCount"] = treasures;
        }

        static void HandlePlayerInput(SoundPlayer footstepSound, SoundPlayer selfHurt, SoundPlayer explosionSound, char[,] map, Dictionary<string, int> playerInfo)
        {
            /*
             Регистрируем нажитие на клавишу.

            Если была нажата клавиша передвижения, то проверяем возможные коллизии, меняем координаты персонажа при возможности и отрисовываем корту снова.

            Если была нажата клавиша разрушения стен, то проверяем на достаточность маны,
            и при наличии стен в 4 сторонах от пользователя, меняем символ стены на пробел в двумерном массиве символов map и отрисовываем корту снова
             */
            ConsoleKeyInfo pressedKey = Console.ReadKey(true);

            int[] direction = GetDirection(pressedKey);
            int playerNextY = playerInfo["playerY"] + direction[0];
            int playerNextX = playerInfo["playerX"] + direction[1];

            char nextCell = map[playerNextY, playerNextX];
            if (pressedKey.Key == ConsoleKey.E && playerInfo["playerMana"] > 0)
            {
                playerInfo["playerMana"] -= 10;
                explosionSound.Play();
                DestroyWalls(map, playerInfo["playerX"], playerInfo["playerY"]);
            }
            else if (pressedKey.Key == ConsoleKey.F && playerInfo["playerHealth"] > 20)
            {
                selfHurt.Play();
                playerInfo["playerHealth"] -= 20;
                playerInfo["movesLeft"] += 5;
            }
            else if (direction[0] + direction[1] != 0 && nextCell != '#' && playerInfo["playerHealth"] > 0 && playerInfo["movesLeft"] > 0)
            {
                playerInfo["playerY"] = playerNextY;
                playerInfo["playerX"] = playerNextX;
                footstepSound.Play();
                playerInfo["movesLeft"]--;
            }

        }

        static void DestroyWalls(char[,] map, int playerX, int playerY)
        {
            if (map[playerY - 1, playerX] == '#' && playerY - 1 != 0)
            {
                map[playerY - 1, playerX] = ' ';
            }
            if (map[playerY + 1, playerX] == '#' && playerY + 1 != map.GetLength(0) - 1)
            {
                map[playerY + 1, playerX] = ' ';
            }
            if (map[playerY, playerX - 1] == '#' && playerX - 1 != 0)
            {
                map[playerY, playerX - 1] = ' ';
            }
            if (map[playerY, playerX + 1] == '#' && playerX + 1 != map.GetLength(1) - 1)
            {
                map[playerY, playerX + 1] = ' ';
            }
        }

        static int[] GetDirection(ConsoleKeyInfo pressedKey)
        {
            int[] direction = { 0, 0 };

            if (pressedKey.Key == ConsoleKey.UpArrow) direction[0] = -1;
            else if (pressedKey.Key == ConsoleKey.DownArrow) direction[0] = 1;
            else if (pressedKey.Key == ConsoleKey.LeftArrow) direction[1] = -1;
            else if (pressedKey.Key == ConsoleKey.RightArrow) direction[1] = 1;

            return direction;
        }
    }

    class Enemy
    {
        public int HP;
        public int Damage;

        public Enemy(int hp, int damage)
        {
            HP = hp;
            Damage = damage;
        }

        public int Collision(int playerDamage)
        {
            HP -= playerDamage;
            return Damage;
        }

    }

    class EasyEnemy : Enemy
    {
        public ConsoleColor Color = ConsoleColor.Green;
        public char Appearance = '⋌';

        public EasyEnemy(int hp, int damage) : base(hp, damage) { }

    }

    class MidEnemy : Enemy
    {
        public float Scaling = 1.3f;
        public ConsoleColor Сolor = ConsoleColor.Yellow;
        public char Appearance = '‼';

        public MidEnemy(int hp, int damage) : base(hp, damage)
        {
            HP = Convert.ToInt32(hp * Scaling);
        }

    }

    class HardEnemy : Enemy
    {
        public float Scaling = 1.6f;
        public ConsoleColor Color = ConsoleColor.DarkRed;
        public char Appearance = '†';

        public HardEnemy(int hp, int damage) : base(hp, damage)
        {
            HP = Convert.ToInt32(hp * Scaling);
        }

    }
}