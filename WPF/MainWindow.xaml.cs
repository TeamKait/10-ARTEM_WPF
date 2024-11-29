using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public ComboBox typeSelector => LogicalTreeHelper.FindLogicalNode(this, "TypeSelector") as ComboBox;
        public TextBox dimensionSelector => LogicalTreeHelper.FindLogicalNode(this, "DimensionSelector") as TextBox;
        public TextBox leftRandom => LogicalTreeHelper.FindLogicalNode(this, "LeftRandom") as TextBox;
        public TextBox rightRandom => LogicalTreeHelper.FindLogicalNode(this, "RightRandom") as TextBox;
        public TextBox outputDimensionSelector => LogicalTreeHelper.FindLogicalNode(this, "OutputDimensionSelector") as TextBox;
        public TextBox outputBox => LogicalTreeHelper.FindLogicalNode(this, "OutputBox") as TextBox;
        public TextBox fileSelector => LogicalTreeHelper.FindLogicalNode(this, "FileSelector") as TextBox;

        Type matrixType = typeof(int);
        Array matrix = null;

        public MainWindow()
        {
            InitializeComponent();
        }
        public void ShowInfoButton(object sender, RoutedEventArgs e)
        {
            UtilsWPF.ProgramInfo.Show();
        }
        public void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Init(
                    author: "Лющенко Артём",
                    name: "10. Массивы",
                    description: "Работа с двумерным массивом",
                    instruction: "Программа выполняет действия над массивами." +
                "\n\t1.Выберите тип значений из предложенных" +
                "\n\t2.Напишите размерности массива через пробел без запятых" +
                "\n\t3.Если вы выбрали числовой тип данных, введите границы случайных чисел" +
                "\n\t4.Введите какие измерения стоит вывести" +
                "\n\t5.Если вы выбрали числовой тип данных, введите имя файла, в который будет записан результат");
            var label = LogicalTreeHelper.FindLogicalNode(this, "NameLabel") as Label;
            if (label != null)
            {
                label.Content = UtilsWPF.ProgramInfo.name;
                Console.WriteLine(label.Uid);
            }
        }
        public void Calculate(object sender, RoutedEventArgs e)
        {
            #region Init array
            //type
            string type = (typeSelector.SelectedValue as TextBlock).Text.ToLower();
            string charLimit = "";
            if (type.Contains("цел")) matrixType = typeof(int);
            else if (type.Contains("дроб")) matrixType = typeof(double);
            else
            {
                matrixType = typeof(string);
                if (type.Contains("лат")) charLimit += UtilsWPF.Constants.englishAlphabet;
                else if (type.Contains("рус")) charLimit += UtilsWPF.Constants.russianAlphabet;
                else UtilsWPF.RestartApp("Неверные входные данные", "Введите значение из предложенного списка");
            }

            //creating array
            int[] indices = dimensionSelector.Text.Split(' ').Select(int.Parse).ToArray();
            UtilsWPF.Log(string.Join(", ", indices)); //log
            if (indices.Any(item => item <= 0)) UtilsWPF.RestartApp("Не удаётся создать массив", "Измерение не может быть меньше или равно 0");
            try
            {
                matrix = Array.CreateInstance(matrixType, indices);
            }
            catch (Exception err)
            {
                err.RestartApp("Неверные входные данные", "Введите числа через пробел без запятых");
            }
            #endregion

            #region Filling array
            if (matrixType != typeof(string))
            {
                double minRandom = double.Parse(leftRandom.Text);
                double maxRandom = double.Parse(rightRandom.Text);
                if (minRandom > maxRandom) UtilsWPF.RestartApp("Неверные входные данные", "Левая граница не может быть больше правой");

                if (matrixType == typeof(int)) matrix.Map(item => UtilsWPF.random.Next((int)minRandom, (int)maxRandom));
                else matrix.Map(item => UtilsWPF.RandomDouble(minRandom, maxRandom));
            }
            else
            {
                matrix.Map(item => charLimit.PickRandom());
            }
            #endregion

            #region Splitting array
            int charAmount = 0;
            int[] alterableIndices = outputDimensionSelector.Text
                .Select((c, index) =>
                {
                    charAmount++;
                    return (c == '1') ? index : -1;
                })
                .Where(index => index != -1)
                .ToArray();

            if (charAmount != indices.Length) UtilsWPF.RestartApp("Неверные входные данные", "Количество введённых измерений не совпадает с измерениями массива");
            if (alterableIndices.Count() != 2) UtilsWPF.RestartApp("Неверные входные данные", "В условии должно быть только 2 единицы");

            Array resultMatrix = Array.CreateInstance(matrixType, indices[alterableIndices[0]], indices[alterableIndices[1]]);
            for (int y = 0; y < indices[alterableIndices[0]]; y++)
            {
                for (int x = 0; x < indices[alterableIndices[1]]; x++)
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
            OutputBox.Text = "";
            for (int y = 0; y < resultMatrix.GetLength(1); y++)
            {
                for (int x = 0; x < resultMatrix.GetLength(0); x++)
                {
                    OutputBox.Text += $"[{x},{y}]{resultMatrix.GetValue(x, y)} ";
                }
                OutputBox.Text += "\n";
            }
            #endregion
        }
        public void WriteToFile(object sender, RoutedEventArgs e)
        {
            if (fileSelector.Text.Length > 0 && matrixType != typeof(string))
            {
                using (StreamWriter writer = new StreamWriter(UtilsWPF.path + "../" + fileSelector.Text + ".txt"))
                {
                    double matrixSum = 0;
                    matrix.Map(item => { matrixSum += double.Parse(item.ToString()); });
                    double avg = matrixSum / (double)matrix.Length;
                    //finding dispersion
                    double dispersion = 0;
                    matrix.Map(item => { dispersion += Math.Pow(double.Parse(item.ToString()) - avg, 2); });
                    dispersion /= matrix.Length;

                    writer.WriteLine("Задание №10 “Массивы” выполнена Лющенко Артёмом.");
                    writer.WriteLine("Сумма всех чисел S=" + Math.Round(matrixSum, 2));
                    writer.WriteLine("Среднее значение равно M=" + Math.Round(avg, 2));
                    writer.WriteLine("Дисперсия D=" + Math.Round(dispersion, 2));

                    UtilsWPF.NewWindow(UtilsWPF.path + "../" + fileSelector.Text + ".txt");
                }
            }
        }
    }
}
