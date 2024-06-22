using System;
using System.Text;
using System.Media;
using System.IO;
using System.Reflection;

namespace Bashgeon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
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
            int playerHealth = 0, playerMana = 0, movesCount = 0;
            mainMenuAmbience.Play();

            // ===================================================================================================================================
            // ===================================================ГЛАВНОЕ МЕНЮ. ВЫБОР СЛОЖНОСТИ===================================================
            // ===================================================================================================================================
            while (!difficultyIsChosen)
            {
                RenderMainMenu(ref difficultyOptions, ref currentOptionIndex);
                ReadDifficultyMenuInput(ref currentOptionIndex, ref playerHealth, ref playerMana, ref movesCount, ref difficultyIsChosen);
                Console.Clear();
            }
            mainMenuAmbience.Stop();


            // ===================================================================================================================================
            // ==========================================================ГЕНЕРАЦИЯ КАРТЫ==========================================================
            // ===================================================================================================================================
            //Инициализируем данные для генерации карты
            int[] playerPosition = new int[2];
            char[,] map = new char[16, 24];
            Random rand = new Random();
            bool playerSpawned = false;
            // Генерация карты
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (i == 0 || i == map.GetLength(0) - 1 || j == 0 || j == map.GetLength(1) - 1)
                    {
                        map[i, j] = '#';
                    }
                    else
                    {
                        switch (rand.Next(0, 15))
                        {
                            case 0:
                            case 1:
                            case 14:
                                map[i, j] = '#';
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
                                    playerPosition[0] = i;
                                    playerPosition[1] = j;
                                }
                                map[i, j] = ' ';
                                break;
                            case 7:
                                map[i, j] = '!';
                                break;
                            case 8:
                                map[i, j] = 'X';
                                break;
                            default:
                                break;
                        }
                    }
                }
            }


            int treasuresCount = 0;
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] == 'X') treasuresCount++;
                }
            }

            int maxHP = playerHealth;
            int maxMP = playerMana;
            int enemiesEliminated = 0, treasuresFound = 0;
            bool isAlive = true;
            char[] bag = new char[1];
            Console.Clear();
            Console.SetWindowSize(116, 26);
            while (true)
            {
                if (playerHealth <= 0)
                {
                    isAlive = false;

                };

                RenderMap(ref map, playerPosition, isAlive);
                DrawHealthBar(playerHealth, maxHP);
                DrawManaBar(playerMana, maxMP);
                RenderUI(movesCount, treasuresFound, treasuresCount, enemiesEliminated, playerPosition, bag);

                ReadPlayerInput(footstepSound, wallDestroySound, isAlive, ref movesCount, ref playerPosition, ref playerMana, ref map);
                if (map[playerPosition[0], playerPosition[1]] == 'X')
                {
                    treasuresFound++;
                    pickUpTreasure.Play();
                    movesCount += 4;
                    map[playerPosition[0], playerPosition[1]] = ' ';
                    char[] tempBag = new char[bag.Length + 1];
                    for (int i = 0; i < bag.Length; i++)
                    {
                        tempBag[i] = 'X';
                    }
                    bag = tempBag;

                }
                else if (map[playerPosition[0], playerPosition[1]] == '!')
                {
                    enemyHitSound.Play();
                    map[playerPosition[0], playerPosition[1]] = ' ';
                    playerHealth -= 10;
                    movesCount += 2;
                    enemiesEliminated++;
                }
                Console.Clear();
            }
        }


        static void RenderMainMenu(ref string[] difficultyOptions, ref int currentOptionIndex)
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

        static void RenderMap(ref char[,] map, int[] playerPosition, bool isAlive)
        {
            // Отрисовка игрового поля
            for (int i = 0; i < map.GetLength(0); i++)
            {
                Console.SetCursorPosition(40, i);
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (i == playerPosition[0] && j == playerPosition[1])
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        if (isAlive) Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.Write("* ");
                    }
                    else if (map[i, j] == 'X')
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(map[i, j] + " ");
                    }
                    else if (map[i, j] == '!')
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(map[i, j] + " ");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(map[i, j] + " ");
                    }
                }
                Console.WriteLine();
            }
        }

        static void RenderUI(
            int movesCount, 
            int treasuresFound, 
            int treasuresCount, 
            int enemiesEliminated, 
            int[] playerPosition, 
            char[] bag)
        {
            Console.SetCursorPosition(0, 15);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("\nНаши спонсоры: ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("https://pornhub.com/");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Осталось шагов: {movesCount}");
            if (treasuresFound == treasuresCount)
            {
                Console.WriteLine("Все сокровища найдены!");
            }
            else
            {
                Console.WriteLine($"Сокровищ найдено: {treasuresFound}");
            }
            Console.Write("Содержимое сумки: ");
            for (int i = 0; i < bag.Length; i++)
            {
                Console.Write(bag[i] + " ");
            }
            Console.WriteLine($"\nВрагов побеждено: {enemiesEliminated}");
            Console.SetCursorPosition(47, 25);
            Console.WriteLine($"Ваши координаты: X: {playerPosition[1]}; Y: {playerPosition[0]}");
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

        static void ReadDifficultyMenuInput(
            ref int currentOptionIndex, 
            ref int playerHealth, 
            ref int playerMana, 
            ref int movesCount, 
            ref bool difficultyIsChosen)
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
                        playerHealth = 150;
                        playerMana = 100;
                        movesCount = 20;

                    }
                    else if (currentOptionIndex == 1)
                    {
                        playerHealth = 100;
                        playerMana = 60;
                        movesCount = 15;

                    }
                    else
                    {
                        playerHealth = 70;
                        playerMana = 40;
                        movesCount = 10;

                    }
                    difficultyIsChosen = true;
                    break;
                default:
                    break;
            }
        }

        static void ReadPlayerInput(
            SoundPlayer footstepSound, 
            SoundPlayer wallDestroySound, 
            bool isAlive, 
            ref int movesCount, 
            ref int[] playerPosition, 
            ref int playerMana, 
            ref char[,] map)
        {
            /*
             Регистрируем нажитие на клавишу.

            Если была нажата клавиша передвижения, то проверяем возможные коллизии, меняем координаты персонажа при возможности и отрисовываем корту снова.

            Если была нажата клавиша разрушения стен, то проверяем на достаточность маны,
            и при наличии стен в 4 сторонах от пользователя, меняем символ стены на пробел в двумерном массиве символов map и отрисовываем корту снова
             */
            ConsoleKeyInfo keyPressed = Console.ReadKey(true);
            switch (keyPressed.Key)
            {
                case ConsoleKey.UpArrow:
                    if (map[playerPosition[0] - 1, playerPosition[1]] != '#' && isAlive && movesCount > 0)
                    {
                        footstepSound.Play();
                        movesCount--;
                        playerPosition[0]--;
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (map[playerPosition[0], playerPosition[1] - 1] != '#' && isAlive && movesCount > 0)
                    {
                        footstepSound.Play();
                        movesCount--;
                        playerPosition[1]--;
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (map[playerPosition[0] + 1, playerPosition[1]] != '#' && isAlive && movesCount > 0)
                    {
                        footstepSound.Play();
                        movesCount--;
                        playerPosition[0]++;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (map[playerPosition[0], playerPosition[1] + 1] != '#' && isAlive && movesCount > 0)
                    {
                        footstepSound.Play();
                        movesCount--;
                        playerPosition[1]++;
                    }
                    break;
                case ConsoleKey.E:
                    if (playerMana <= 0) break;
                    playerMana -= 10;
                    wallDestroySound.Play();
                    if (map[playerPosition[0] - 1, playerPosition[1]] != 'X' && map[playerPosition[0] - 1, playerPosition[1]] != '!' && playerPosition[0] - 1 != 0)
                    {
                        map[playerPosition[0] - 1, playerPosition[1]] = ' ';
                    }
                    if (map[playerPosition[0] + 1, playerPosition[1]] != 'X' && map[playerPosition[0] + 1, playerPosition[1]] != '!' && playerPosition[0] + 1 != map.GetLength(0) - 1)
                    {
                        map[playerPosition[0] + 1, playerPosition[1]] = ' ';
                    }
                    if (map[playerPosition[0], playerPosition[1] - 1] != 'X' && map[playerPosition[0], playerPosition[1] - 1] != '!' && playerPosition[1] - 1 != 0)
                    {
                        map[playerPosition[0], playerPosition[1] - 1] = ' ';
                    }
                    if (map[playerPosition[0], playerPosition[1] + 1] != 'X' && map[playerPosition[0], playerPosition[1] + 1] != '!' && playerPosition[1] + 1 != map.GetLength(1) - 1)
                    {
                        map[playerPosition[0], playerPosition[1] + 1] = ' ';
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
