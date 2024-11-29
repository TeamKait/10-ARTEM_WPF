using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace WPF
{
    public static class UtilsWPF
    {
        public static Random random = new Random();
        public static string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../"));
        private static int logCount = Directory.GetFiles(path + "Logs").Length;
        /// <summary>
        /// Информация о программе
        /// </summary>
        public struct ProgramInfo
        {
            public static string author;
            public static string name;
            public static string description;
            public static string instruction;
            public static void Show()
            {
                Notify("Информация о программе",
                        $"Автор: {author}" +
                        $"\n\nНазвание: {name}" +
                        $"\n\nОписание: {description}" +
                        $"\n\nИнструкции:\n{instruction}");
            }
        }

        /// <summary>
        /// Открыть окно с уведомлением
        /// </summary>
        /// <param name="caption">Заголовок</param>
        /// <param name="text">Содержание</param>
        /// <param name="icon">Иконка (опционально)</param>
        /// <returns>MessageBoxResult</returns>
        public static MessageBoxResult Notify(string caption, string text, MessageBoxImage icon = MessageBoxImage.Information, MessageBoxButton button = MessageBoxButton.OK)
        {
            return MessageBox.Show(text, caption, button, icon);
        }

        /// <summary>
        /// Метод, иницилизирующий полезные функции UtilsWPF
        /// </summary>
        /// <param name="window">Основное окно (можно вызвать из него)</param>
        /// <param name="author">Автор</param>
        /// <param name="name">Название программы</param>
        /// <param name="description">Описание</param>
        /// <param name="instruction">Инструкции</param>
        public static void Init(this Window window, string author, string name, string description, string instruction)
        {
            ProgramInfo.author = author;
            ProgramInfo.name = name;
            ProgramInfo.description = description;
            ProgramInfo.instruction = instruction;

            window.Title = name;

            window.KeyDown += (object sender, KeyEventArgs e) =>
            {
                switch (e.Key)
                {
                    case Key.F1:
                        ProgramInfo.Show();
                        break;
                    case Key.F2:
                        try
                        {
                            NewWindow(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, $"../../Logs/LOG_{logCount - 1}.txt")));
                        }
                        catch
                        {
                            Notify("Логов нет", "Папка logs пуста");
                        }
                        break;
                }
            };
        }

        /// <summary>
        /// Асинхронно выполнить переданное действие
        /// </summary>
        /// <param name="callback">действие</param>
        /// <returns>Thread</returns>
        public static Thread StartCoroutine(Action callback)
        {
            Thread coroutine = new Thread(() => callback());
            coroutine.Start();
            return coroutine;
        }

        /// <summary>
        /// Рестарт приложения
        /// </summary>
        /// <param name="e">Exception (опционально)</param>
        /// <param name="errorMessage">Ошибка</param>
        /// <param name="description">Содержание ошибки</param>
        public static void RestartApp(this Exception e, string errorMessage, string description)
        {
            Log(e.Message, "ERROR");
            if (Notify("Ошибка", $"Содержание: {errorMessage}" +
                $"\nРекоммендации: {description}" +
                $"\n\nПерезапустить приложение?", MessageBoxImage.Error, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
            }
        }
        public static void RestartApp(string errorMessage, string description)
        {
            Log(errorMessage + " : " + description, "ERROR");
            if (Notify("Ошибка", $"Содержание: {errorMessage}" +
                $"\nРекоммендации: {description}" +
                $"\n\nПерезапустить приложение?", MessageBoxImage.Error, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
            }
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
        /// Записывает сообщение в файл в папке Logs
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>true, если удалось записать, иначе false</returns>
        public static bool Log(string message, string logType = "LOG")
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path + $"Logs/LOG_{logCount}.txt", true))
                {
                    writer.WriteLine($"[{logType}] <{DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss")}>: {message}");
                }
                return true;
            }
            catch { return false; }
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
}
