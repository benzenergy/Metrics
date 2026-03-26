using ClosedXML.Excel;
using Microsoft.Win32;
using uniconvert;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ver20;

namespace ver10
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnCalculate_Click(this, new RoutedEventArgs());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime today = DateTime.Today;
            CalendarPrev.SetMonth(today.AddMonths(-1));
            CalendarCurrent.SetMonth(today);
            CalendarNext.SetMonth(today.AddMonths(1));

            CalendarPrev.DateRangeChanged += Calendar_DateRangeChanged;
            CalendarCurrent.DateRangeChanged += Calendar_DateRangeChanged;
            CalendarNext.DateRangeChanged += Calendar_DateRangeChanged;

            PercentMethodText.Text = selectedPercentMethod;
        }

        private void FileButton_Click(object sender, RoutedEventArgs e) => FilePopup.IsOpen = true;

        private void SaveAsExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "Report.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Отчет");

                    string result = GetOutputText();

                    var lines = result.Split('\n');

                    for (int i = 0; i < lines.Length; i++)
                    {
                        ws.Cell(i + 1, 1).Value = lines[i].Trim();
                    }

                    ws.Column(1).AdjustToContents();

                    workbook.SaveAs(dlg.FileName);
                }

                DarkMessageBox.Show("Excel файл сохранён", this);
            }
        }

        public Tuple<DateTime?, DateTime?> DateRange => Tuple.Create(firstDate, secondDate);

        private void Calendar_DateRangeChanged(object sender, Tuple<DateTime?, DateTime?> range)
        {
            firstDate = range.Item1;
            secondDate = range.Item2;

            CalendarPrev.StartDate = firstDate;
            CalendarPrev.EndDate = secondDate;
            CalendarPrev.RenderCalendar();

            CalendarCurrent.StartDate = firstDate;
            CalendarCurrent.EndDate = secondDate;
            CalendarCurrent.RenderCalendar();

            CalendarNext.StartDate = firstDate;
            CalendarNext.EndDate = secondDate;
            CalendarNext.RenderCalendar();

            if (firstDate.HasValue && secondDate.HasValue)
                DaysBox.Text = ((secondDate.Value - firstDate.Value).Days + 1).ToString();
        }

        private DateTime? firstDate = null;
        private DateTime? secondDate = null;

        private string selectedPercentMethod = "общей суммы";

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(selectedPercentMethod)
            {
                Owner = this
            };

            if (settingsWindow.ShowDialog() == true)
            {
                selectedPercentMethod = settingsWindow.SelectedMethod;
                PercentMethodText.Text = selectedPercentMethod;
            }
        }

        private decimal ParseDecimal(string text)
        {
            if (decimal.TryParse(text.Replace(" ", "").Replace("₽", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;
            return 0;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow { Owner = this };
            aboutWindow.ShowDialog();
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private string Money(decimal value)
        {
            return string.Format("{0:N0} ₽", value);
        }

        private async void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            var requiredFields = new Dictionary<TextBox, string>
            {
                { CheckPriceBox, "Стоимость чека" },
                { RentPriceBox, "Стоимость аренды" },
                { DaysBox, "Количество дней" },
                { PercentBox, "Процент" }
            };

            List<string> emptyFields = new List<string>();
            foreach (var kvp in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key.Text))
                    emptyFields.Add(kvp.Value);
            }

            if (emptyFields.Count > 0)
            {
                string message = "Пожалуйста, заполните следующие поля:\n\n" + string.Join("\n", emptyFields);
                var error = new ErrorWindow(message);
                error.Owner = this;
                error.ShowDialog();
                return;
            }

            await AnimateOutput();
        }

        private async Task AnimateOutput()
        {
            OutputText.Text = "";
            string result = GetOutputText();

            foreach (char c in result)
            {
                OutputText.Text += c;
                await Task.Delay(3);
            }
        }

        private string GetOutputText()
        {
            decimal checkCost = ParseDecimal(CheckPriceBox.Text);
            decimal rentCost = ParseDecimal(RentPriceBox.Text);
            decimal Differ = checkCost - rentCost;

            decimal dailyAllowance = ParseDecimal(DailyBox.Text);
            int days = (int)ParseDecimal(DaysBox.Text);
            decimal extra = ParseDecimal(ExtraBox.Text);
            decimal percent = ParseDecimal(PercentBox.Text);

            decimal totalRent = (checkCost * days) - checkCost;
            decimal totalDaily = dailyAllowance * days;
            decimal totalSum = ((checkCost * days) + (dailyAllowance * days) + extra) - checkCost;
            decimal factPay = (rentCost * days) - rentCost;

            decimal paidPercent = 0;

            if (selectedPercentMethod == "общей суммы") 
                paidPercent = ((checkCost * days) - checkCost) * (percent / 100);

            else if (selectedPercentMethod == "разницы") 
                paidPercent = (Differ * days - (Differ)) * (percent / 100);

            decimal profit = totalRent - factPay - paidPercent;

            return $@"Отчетные документы:

Аренда: {Money(totalRent)}
Суточные: {Money(totalDaily)}
Доп. расходы: {Money(extra)}
Общая сумма: {Money(totalSum)}

Заплачено:

Фактически: {Money(factPay)}
Проценты: {Money(paidPercent)}
Выгода: {Money(profit)}";
        }

        private void Calculate()
        {
            OutputText.Text = GetOutputText();
        }
    }
}