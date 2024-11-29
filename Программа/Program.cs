using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.CodeDom;
using System.IO;

namespace _10
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //BEGINNING
            Utils.info = new Utils.ProgramInfo(
                author: "Лющенко Артём",
                name: "10. Массивы",
                description: "Работа с двумерным массивом",
                instruction: "Программа выполняет действия над массивами." +
                "\n\t1.Выберите тип значений из предложенных\n\t2.Напишите размерности массива через пробел без запятых" +
                "\n\t3.Если вы выбрали числовой тип данных, введите границы случайных чисел" +
                "\n\t4.Введите какие измерения стоит вывести" +
                "\n\t5.Если вы выбрали числовой тип данных, введите имя файла, в который будет записан результат");
            Utils.PrintAuthor();

            //MAIN PART

            /*
             *  Array matrix
             *  Type matrixType
             *  string charLimit
             *  int[] indices
             */
            #region Init array
            //type
            string type = Utils.Input<string>("Выбрать тип значений (целые, дробные числа, символы латинского и русского алфавита)", false).ToLower();
            Type matrixType = typeof(int);
            string charLimit = "";
            if (type.Contains("цел")) matrixType = typeof(int);
            else if (type.Contains("дроб")) matrixType = typeof(double);
            else
            {
                matrixType = typeof(string);
                if (type.Contains("лат")) charLimit += Utils.Constants.englishAlphabet;
                else if (type.Contains("рус")) charLimit += Utils.Constants.russianAlphabet;
                else Utils.RestartApp("Неверные входные данные", "Введите значение из предложенного списка");
            }

            //creating array
            int[] indices = Utils.Input<string>("Выбрать размерности массива", false).Split(' ').Select(int.Parse).ToArray();
            if (indices.Any(item => item <= 0)) Utils.RestartApp("Не удаётся создать массив", "Измерение не может быть меньше или равно 0");
            Array matrix = null;
            try
            {
                matrix = Array.CreateInstance(matrixType, indices);
            }
            catch (Exception _)
            {
                Utils.RestartApp("Неверные входные данные", "Введите числа через пробел без запятых");
            }
            #endregion

            #region Filling array
            if (matrixType != typeof(string))
            {
                double minRandom = Utils.Input<double>("Введите левую границу случайных чисел", false);
                double maxRandom = Utils.Input<double>("Введите правую границу случайных чисел", false);
                if (minRandom > maxRandom) Utils.RestartApp("Неверные входные данные", "Левая граница не может быть больше правой");

                if (matrixType == typeof(int)) matrix.Map(item => Utils.random.Next((int)minRandom, (int)maxRandom));
                else matrix.Map(item => Utils.RandomDouble(minRandom, maxRandom));
            }
            else
            {
                matrix.Map(item => charLimit.PickRandom());
            }
            #endregion

            /*
             *  Array resultMatrix
             */
            #region Splitting array
            int charAmount = 0;
            int[] alterableIndices = Utils.Input<string>("Задайте какие измерения использовать (например 01001, где 1 - измерения, которые будут выведены)", false)
                .Select((c, index) => {
                    charAmount++;
                    return (c == '1') ? index : -1; 
                })
                .Where(index => index != -1)
                .ToArray();

            if (charAmount != indices.Length) Utils.RestartApp("Неверные входные данные", "Количество введённых измерений не совпадает с измерениями массива");
            if (alterableIndices.Count() != 2) Utils.RestartApp("Неверные входные данные", "В условии должно быть только 2 единицы");

            Array resultMatrix = Array.CreateInstance(matrixType, indices[alterableIndices[0]], indices[alterableIndices[1]]);
            for(int y = 0; y < indices[alterableIndices[0]]; y++)
            {
                for(int x = 0; x < indices[alterableIndices[1]]; x++)
                {
                    try
                    {
                        var currIndices = new int[indices.Length];
                        currIndices[alterableIndices[0]] = y;
                        currIndices[alterableIndices[1]] = x;
                        resultMatrix.SetValue(matrix.GetValue(currIndices), y, x);
                    }
                    catch { }
                }
            }
            Console.WriteLine("\nЧасть массива:");
            resultMatrix.ArrayOutput();
            #endregion

            /*
             *  double avg
             *  double dispersion
             *  double matrixSum
             */
            #region Finding AVG DISP
            if(matrixType != typeof(string))
            {
                double matrixSum = 0;
                matrix.Map(item => { matrixSum += double.Parse(item.ToString()); });
                double avg = matrixSum / (double)matrix.Length;
                //finding dispersion
                double dispersion = 0;
                matrix.Map(item => { dispersion += Math.Pow(double.Parse(item.ToString()) - avg, 2); });
                dispersion /= matrix.Length;

                string fileName = Utils.Input<string>("имя текстового файла отчета латинскими буквами");
                string path = "S:\\ИСП211\\Дерин Лющенко\\Практика Учебная\\Лющенко\\10\\" + fileName + ".txt";
                using (StreamWriter outputFile = new StreamWriter(path))
                {
                    outputFile.WriteLine("Задание №10 “Массивы” выполнена Лющенко Артёмом.");
                    outputFile.WriteLine("Сумма всех чисел S=" + Math.Round(matrixSum, 2));
                    outputFile.WriteLine("Среднее значение равно M=" + Math.Round(avg, 2));
                    outputFile.WriteLine("Дисперсия D=" + Math.Round(dispersion, 2));
                }

                Utils.ColoredWriteLine("<fg=cyan>Результат успешно записан в файл\n" + path);
                Utils.NewWindow(path);
            }
            #endregion

            //ENDING
            Utils.Ending();
        }
    }
}