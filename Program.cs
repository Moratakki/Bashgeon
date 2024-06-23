using System;
using System.Text;
using System.Threading;
using System.Media;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            SoundPlayer mainMenuAmbience = new SoundPlayer($@"{Environment.CurrentDirectory}\main_menu_bg_sound.wav");
            SoundPlayer wallDestroySound = new SoundPlayer($@"{Environment.CurrentDirectory}\destroy_wall.wav");
            SoundPlayer pickUpTreasure = new SoundPlayer($@"{Environment.CurrentDirectory}\pick_up.wav");
            SoundPlayer footstepSound = new SoundPlayer($@"{Environment.CurrentDirectory}\footstep.wav");
            SoundPlayer enemyHitSound = new SoundPlayer($@"{Environment.CurrentDirectory}\enemy_hit.wav");
            // Инициализируем данные, нужные для выбора сложности
            string[] difficultyOptions = { "Easy", "Medium", "Hard" };
            int currentOptionIndex = 0;
            bool difficultyIsChosen = false;
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
            while (!difficultyIsChosen)
            {
                RenderMainMenu(difficultyOptions, ref currentOptionIndex);
                HandleDifficultyInput(playerInfo, ref currentOptionIndex, ref difficultyIsChosen);
                Console.Clear();
            }
            mainMenuAmbience.Stop();


            // ===================================================================================================================================
            // ==========================================================ГЕНЕРАЦИЯ КАРТЫ==========================================================
            // ===================================================================================================================================
            //Инициализируем данные для генерации карты
            char[,] map = new char[16, 24];
            Random rand = new Random();
            bool playerSpawned = false;
            int treasuresCount = 0;
            // Генерация карты
            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if (y == 0 || y == map.GetLength(0) - 1 || x == 0 || x == map.GetLength(1) - 1) map[y, x] = '#';
                    else
                    {
                        switch (rand.Next(0, 15))
                        {
                            case 0:
                            case 1:
                            case 14:
                                map[y, x] = '#';
                                break;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                                if (!playerSpawned)
                                {
                                    playerInfo["playerY"] = y;
                                    playerInfo["playerX"] = x;
                                }
                                map[y, x] = ' ';
                                break;
                            case 7:
                                map[y, x] = '!';
                                break;
                            case 8:
                                map[y, x] = 'X';
                                treasuresCount++;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            playerInfo["playerMaxHealth"] = playerInfo["playerHealth"];
            playerInfo["playerMaxMana"] = playerInfo["playerMana"];
            char currentCell;

            while (true)
            {

                RenderMap(map, playerInfo);
                DrawHealthBar(playerInfo["playerHealth"], playerInfo["playerMaxHealth"]);
                DrawManaBar(playerInfo["playerMana"], playerInfo["playerMaxMana"]);
                RenderUI(playerInfo, treasuresCount);
                HandlePlayerInput(footstepSound, wallDestroySound, map, playerInfo);


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
                Console.Clear();
            }
        }


        static void RenderMainMenu(string[] difficultyOptions, ref int currentOptionIndex)
        {
            // Отрисовка главного меню с выбором сложности
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
                if (currentOptionIndex == 0 && i == 0) Console.BackgroundColor = ConsoleColor.Green;
                else if (currentOptionIndex == 1 && i == 1) Console.BackgroundColor = ConsoleColor.DarkYellow;
                else if (currentOptionIndex == 2 && i == 2) Console.BackgroundColor = ConsoleColor.Red;
                if (i == currentOptionIndex) Console.Write(difficultyOptions[i] + " <");
                else Console.Write(difficultyOptions[i]);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.WriteLine();
            }
        }

        static void RenderMap(char[,] map, Dictionary<string, int> playerInfo)
        {
            // Отрисовка игрового поля
            for (int y = 0; y < map.GetLength(0); y++)
            {
                Console.SetCursorPosition(40, y);
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if (y == playerInfo["playerY"] && x == playerInfo["playerX"])
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        if (playerInfo["playerHealth"] > 0) Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.Write("* ");
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

        static void RenderUI(Dictionary<string, int> playerInfo,int treasuresCount)
        {
            Console.SetCursorPosition(0, 15);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("\nНаши спонсоры: ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("https://pornhub.com/");
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
            Console.WriteLine($"Очки: {playerInfo["score"]}");
            Console.WriteLine($"\nВрагов побеждено: {playerInfo["kills"]}");
            Console.SetCursorPosition(47, 25);
            Console.WriteLine($"Ваши координаты: X: {playerInfo["playerX"]}; Y: {playerInfo["playerY"]}");
            Console.SetCursorPosition(100, 0);
            Console.WriteLine("Управление:");
            Console.SetCursorPosition(95, 2);
            Console.WriteLine("Передвижение: ← ↓ → ↑");
            Console.SetCursorPosition(95, 4);
            Console.WriteLine("Уничтожить стены: E");
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

        static void HandleDifficultyInput(Dictionary<string, int> playerInfo, ref int currentOptionIndex, ref bool difficultyIsChosen)
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
                        playerInfo["playerMaxHealth"] = 150;
                        playerInfo["playerMaxMana"] = 100;
                        playerInfo["playerHealth"] = 150;
                        playerInfo["playerMana"] = 100;
                        playerInfo["movesLeft"] = 20;
                        playerInfo["killPoints"] = 45;
                        playerInfo["treasurePickUpPoints"] = 125;

                    }
                    else if (currentOptionIndex == 1)
                    {
                        playerInfo["playerMaxHealth"] = 100;
                        playerInfo["playerMaxMana"] = 60;
                        playerInfo["playerHealth"] = 100;
                        playerInfo["playerMana"] = 60;
                        playerInfo["movesLeft"] = 15;
                        playerInfo["killPoints"] = 60;
                        playerInfo["treasurePickUpPoints"] = 150;
                    }
                    else
                    {
                        playerInfo["playerMaxHealth"] = 70;
                        playerInfo["playerMaxMana"] = 40;
                        playerInfo["playerHealth"] = 70;
                        playerInfo["playerMana"] = 40;
                        playerInfo["movesLeft"] = 10;
                        playerInfo["killPoints"] = 80;
                        playerInfo["treasurePickUpPoints"] = 180;
                    }
                    difficultyIsChosen = true;
                    break;
                default:
                    break;
            }
        }

        static void HandlePlayerInput(SoundPlayer footstepSound, SoundPlayer explosionSound, char[,] map, Dictionary<string, int> playerInfo)
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

            if (direction[0] + direction[1] != 0 && nextCell != '#' && playerInfo["playerHealth"] > 0 && playerInfo["movesLeft"] > 0)
            {
                playerInfo["playerY"] = playerNextY;
                playerInfo["playerX"] = playerNextX;
                footstepSound.Play();
                playerInfo["movesLeft"]--;
            }

            if (pressedKey.Key == ConsoleKey.E && playerInfo["playerMana"] > 0)
            {
                playerInfo["playerMana"] -= 10;
                explosionSound.Play();
                DestroyWalls(map, playerInfo["playerX"], playerInfo["playerY"]);
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
}
