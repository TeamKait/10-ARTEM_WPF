using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System.Linq.Expressions;
using System.IO;

/// <summary>
/// Величайшая библиотека утилит всех времён и народов
/// </summary>
public static class Utils
{
    /*
     *     ____ ___   __  .__.__          
     *    |    |   \_/  |_|__|  |   ______
     *    |    |   /\   __\  |  |  /  ___/
     *    |    |  /  |  | |  |  |__\___ \ 
     *    |______/   |__| |__|____/______>
     */
    private static Stopwatch executionTime = new Stopwatch();
    public static Random random = new Random();
    public static ProgramInfo info;
    public static bool exitedWithError = false;

    /// <summary>
    /// Информация о текущей программе
    /// </summary>
    public class ProgramInfo
    {
        public string author;
        public string name;
        public string description;
        public string instruction;

        public ProgramInfo(string author, string name, string description, string instruction)
        {
            this.author = author;
            this.name = name;
            this.description = description;
            this.instruction = instruction;
        }
    }

    /// <summary>
    /// Метод, иницилизирующий программу
    /// </summary>
    public static void PrintAuthor()
    {
        Console.Title = info.name;
        //Authors
        void PrintTitle()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("----------------------");
            Console.WriteLine($"Задание #{info.name.Split('.')[0]}");
            Console.WriteLine($"Выполнил {info.author}");
            Console.WriteLine("ИСП211       ");
            Console.WriteLine("----------------------");
            Console.ResetColor();
        }
        PrintTitle();

        //OTHER
        executionTime.Start();
        //on exit
        AppDomain.CurrentDomain.ProcessExit += delegate (object sender, EventArgs e)
        {
            executionTime.Stop();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n----------------------");
            Console.WriteLine($"Время: {Math.Round(executionTime.Elapsed.TotalMilliseconds)}мс | {Math.Round(executionTime.Elapsed.TotalSeconds, 2)}с");
            Console.WriteLine("\nНажмите любую клавишу для выхода . . .");
            Console.ForegroundColor = ConsoleColor.Black;
        };

        //HANDLE F1
        ColoredWriteLine("\n<fg=gray>Нажмите <fg=darkyellow>F1 <fg=gray>для вывода информации о программе\n<fg=darkyellow>Любой символ<fg=gray> для продолжения");
        Console.ForegroundColor = ConsoleColor.Black;
        if (Console.ReadKey().Key == ConsoleKey.F1)
        {
            PrintTitle();
            string accent = "<r><fg=magenta>";
            string normal = "<r><fg=cyan>";
            ColoredWriteLine($"\n{accent}Автор:{normal} " + info.author);
            ColoredWriteLine($"{accent}Название:{normal} " + info.name);
            ColoredWriteLine($"{accent}Описание:{normal} " + info.description);
            ColoredWriteLine($"{accent}Инструкция:{normal} " + info.instruction + "\n");
        }
        else
        {
            PrintTitle();
            Console.WriteLine();
        }
        Console.ResetColor();
    }

    /// <summary>
    /// Быстрый ввод переменной + парсинг в переданный тип
    /// </summary>
    /// <typeparam name="T">Тип переменной</typeparam>
    /// <param name="name">Имя переменной (опционально)</param>
    /// <returns>Ввод с консоли</returns>
    public static T Input<T>(string name = "?", bool additionalText = true)
    {
        if (additionalText) ColoredWrite($"\nВведите {name}<r>:\n");
        else ColoredWrite($"\n{name}<r>:\n");
        T result = default(T);
        try
        {
            result = (T)Convert.ChangeType(Console.ReadLine(), typeof(T));
        }
        catch (Exception _)
        {
            RestartApp("Неверные входные данные", "Проверьте правильность введёных данных и соответствие их типу переменной");
        }
        return result;
    }

    /// <summary>
    /// Быстрый вывод переменной
    /// </summary>
    /// <typeparam name="T">Тип переменной (опционально, можно вызвать с переменной)</typeparam>
    /// <param name="variable">Переменная</param>
    /// <param name="name">Имя (опционально)</param>
    public static void Output<T>(this T variable, string name = "")
    {
        ColoredWriteLine($"{name}<r> = {variable}");
    }
    public static void Output<T>(this T[] variable, string name = "")
    {
        ColoredWriteLine($"{name}<r> = {String.Join(", ", variable)}");
    }
    public static void ArrayOutput(this Array array, bool printIndices = true)
    {
        int maxLength = 0;
        array.Map((element) => {maxLength = Math.Max(maxLength, element.ToString().Length); });
        if (array.Rank == 2)
        {
            for (int y = 0; y < array.GetLength(0); y++)
            {
                for (int x = 0; x < array.GetLength(1); x++)
                {
                    if (printIndices)
                    {
                        ColoredWrite($"<fg=DarkGray>[{y},{x}]:<r>{array.GetValue(y, x)}");
                    }
                    else
                    {
                        Console.Write(array.GetValue(y, x));
                    }
                    for (int i = 0; i <= maxLength - array.GetValue(y, x).ToString().Length + 1; i++) Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
        else if (array.Rank == 1)
        {
            for (int x = 0; x < array.GetLength(0); x++)
            {
                if (printIndices)
                {
                    ColoredWrite($"<fg=DarkGray>[{x}]:<r>{array.GetValue(x)}");
                }
                else
                {
                    Console.Write(array.GetValue(x));
                }
                for (int i = 0; i <= maxLength - array.GetValue(x).ToString().Length + 1; i++) Console.Write(" ");
            }
        }
        else
        {
            array.Map(
                delegate (object element, object indices)
                {
                    ColoredWriteLine($"<fg=darkgray>[{string.Join(", ", (int[])indices)}]:<r>{element}");
                    return null;

                }
            );
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Перезапустить приложение
    /// </summary>
    /// <param name="errorMessage">Содержание ошибки (опционально)</param>
    public static void RestartApp(string errorMessage = null, string recommendations = null)
    {
        exitedWithError = true;
        ColoredWriteLine("<fg=darkred>Произошла ошибка");
        if (errorMessage != null) ColoredWriteLine("<fg=darkred>Содержание ошибки:<fg=red> " + errorMessage);
        if (recommendations != null) ColoredWriteLine("<fg=darkred>Рекоммендации:<fg=red> " + recommendations);
        ColoredWriteLine("\n<fg=darkred>Нажмите <fg=red>Q<fg=darkred> для выхода\nили <fg=red>любую клавишу<fg=darkred> для перезапуска");

        Console.ForegroundColor = ConsoleColor.Black;
        if (Console.ReadKey().Key != ConsoleKey.Q) System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
        Environment.Exit(0);
    }
    public static void RestartApp(Exception e = null)
    {
        exitedWithError = true;
        ColoredWriteLine("<fg=darkred>Произошла ошибка");
        if (e != null) ColoredWriteLine("<fg=darkred>Содержание ошибки:<fg=red> " + e);
        ColoredWriteLine("\n<fg=darkred>Нажмите <fg=red>Q<fg=darkred> для выхода\nили <fg=red>любую клавишу<fg=darkred> для перезапуска");

        Console.ForegroundColor = ConsoleColor.Black;
        if (Console.ReadKey().Key != ConsoleKey.Q) System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
        Environment.Exit(0);
    }

    /// <summary>
    /// Вывод красивой ошибки
    /// </summary>
    /// <param name="errorMessage">Содержание ошибки</param>
    public static void PrintError(string errorMessage)
    {
        //Console.BackgroundColor = ConsoleColor.DarkRed;
        //Console.WriteLine($"ошибка: {errorMessage}".ToUpper());
        //Console.ResetColor();
        ColoredWriteLine($"<bg=DarkRed>ошибка: {errorMessage}".ToUpper());
    }


    /// <summary>
    /// Выполнение действие над массивом любой размерности
    /// </summary>
    /// <param name="array">Массив (можно вызвать из него)</param>
    /// <param name="action">Само действие</param>
    /// <param name="dimension"></param>
    /// <param name="indices"></param>
    /// 
    public static void Map(this Array array, Func<object, object> action, int dimension = 0, int[] indices = null)
    {
        if (indices == null) indices = new int[array.Rank];

        for (int i = 0; i < array.GetLength(dimension); i++)
        {
            indices[dimension] = i;

            if (dimension == array.Rank - 1)
            {
                var newValue = action(array.GetValue(indices));
                array.SetValue(newValue, indices);
            }
            else
            {
                Map(array, action, dimension + 1, indices);
            }
        }
    }
    public static void Map(this Array array, Func<object, object, int[]> action, int dimension = 0, int[] indices = null)
    {
        if (indices == null) indices = new int[array.Rank];

        for (int i = 0; i < array.GetLength(dimension); i++)
        {
            indices[dimension] = i;

            if (dimension == array.Rank - 1)
            {
                var newValue = action(array.GetValue(indices), indices);
                array.SetValue(newValue, indices);
            }
            else
            {
                Map(array, action, dimension + 1, indices);
            }
        }
    }
    public static void Map(this Array array, Action<object> action, int dimension = 0, int[] indices = null)
    {
        if (indices == null) indices = new int[array.Rank];

        for (int i = 0; i < array.GetLength(dimension); i++)
        {
            indices[dimension] = i;

            if (dimension == array.Rank - 1)
            {
                action(array.GetValue(indices));
            }
            else
            {
                Map(array, action, dimension + 1, indices);
            }
        }
    }

    /// <summary>
    /// Возвращает случайный элемент из массива или строки
    /// </summary>
    /// <typeparam name="T">Тип переменной (можно вызвать из массива)</typeparam>
    /// <param name="array">Массив (можно вызвать из массива)</param>
    /// <returns>Случайный элемент из массива</returns>
    public static T PickRandom<T>(this Array array)
    {
        List<T> elements = new List<T>();
        array.Map(item => elements.Add((T)item));
        return elements[random.Next(elements.Count)];
    }
    public static string PickRandom(this string str)
    {
        return str[random.Next(str.Length)].ToString();
    }

    /// <summary>
    /// Возвращает случайный double в заданном диапозоне
    /// </summary>
    /// <param name="min">Минимум</param>
    /// <param name="max">Максимум</param>
    /// <returns></returns>
    public static double RandomDouble(double min, double max)
    {
        return random.NextDouble() * (max - min) + min;
    }
    public static double RandomDouble(double max)
    {
        return RandomDouble(0, max);
    }

    /// <summary>
    /// Массив [][] в Массив [,]
    /// </summary>
    /// <typeparam name="T">Тип массива (можно вызвать из массива)</typeparam>
    /// <param name="array">массив</param>
    /// <returns>Массив [,]</returns>
    public static T[,] ToArray<T>(this T[][] array)
    {
        int rows = array.Length;
        int cols = array[0].Length;
        T[,] rectArray = new T[rows, cols];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                rectArray[y, x] = array[y][x];
            }
        }

        return rectArray;
    }

    /// <summary>
    /// Вывод цветного текста. <fg=color> - цвет текста, <bg=color> - цвет фона, <r> - сброс цвета
    /// </summary>
    /// <param name="text">Текст</param>
    public static void ColoredWrite(string text, bool resetColor = true)
    {
        if (resetColor) text += "<r>";
        string pattern = @"(<[^>]+>)|([^<]+)";
        List<string> wordList = new List<string>();

        foreach (Match match in Regex.Matches(text, pattern))
        {
            if (match.Groups[1].Success)
            {
                wordList.Add(match.Groups[1].Value);
            }
            else if (match.Groups[2].Success)
            {
                wordList.Add(match.Groups[2].Value);
            }
        }
        Array words = wordList.ToArray();
        words.Map(item =>
        {
            var itemString = item.ToString();
            if (itemString[0] == '<' && itemString[itemString.Length - 1] == '>')
            {
                //tag
                itemString = string.Concat(itemString.Where(c => !char.IsWhiteSpace(c))).ToLower();
                if (itemString.Contains("="))
                {
                    var parts = itemString.Split('=');
                    ConsoleColor color = new ConsoleColor();
                    foreach (ConsoleColor c in (ConsoleColor[])Enum.GetValues(typeof(ConsoleColor)))
                    {
                        if (c.ToString().ToLower() + ">" == parts[1])
                        {
                            color = c;
                        }
                    }
                    if (parts[0] == "<bg")
                    {
                        Console.BackgroundColor = color;
                    }
                    else if (parts[0] == "<fg")
                    {
                        Console.ForegroundColor = color;
                    }
                    else
                    {
                        Console.Write(item);
                    }
                }
                else
                {
                    if (itemString == "<r>")
                    {
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(item);
                    }
                }
            }
            else
            {
                //word
                Console.Write(item);
            }
        });
    }
    public static void ColoredWriteLine(string text)
    {
        ColoredWrite(text + "\n");
    }

    /// <summary>
    /// Открыть файл в новом окне
    /// </summary>
    /// <param name="path">Путь к файлу</param>
    public static void NewWindow(string path)
    {
        Process process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.UseShellExecute = true;
        process.Start();
    }

    /// <summary>
    /// Конец приложения (для запуска без дебага)
    /// </summary>
    public static void Ending()
    {
        if (Debugger.IsAttached)
        {
            executionTime.Stop();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n----------------------");
            Console.WriteLine($"Время: {Math.Round(executionTime.Elapsed.TotalMilliseconds)}мс | {Math.Round(executionTime.Elapsed.TotalSeconds, 2)}с");
            Console.WriteLine("\nНажмите любую клавишу для выхода . . .");
            Console.ForegroundColor = ConsoleColor.Black;
            if (!exitedWithError) Console.ReadKey();
        }
    }

    /// <summary>
    /// struct с полезными константами
    /// </summary>
    public struct Constants
    {
        /// <summary>
        /// абвгдеёжзийклмнопрстуфхцчшщъыьэюя
        /// </summary>
        public static string russianAlphabet => "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        /// <summary>
        /// abcdefghijklmnopqrstuvwxyz
        /// </summary>
        public static string englishAlphabet => "abcdefghijklmnopqrstuvwxyz";
        /// <summary>
        /// 0123456789
        /// </summary>
        public static string numbers => "0123456789";
    }
}