using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ver10
{
    public partial class CustomCalendar : UserControl
    {
        public DateTime CurrentMonth { get; private set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public CustomCalendar()
        {
            InitializeComponent();
            CurrentMonth = DateTime.Today;
            RenderCalendar();
        }

        public void RenderCalendar()
        {
            DaysGrid.Children.Clear();
            var monthName = CurrentMonth.ToString("MMMM", new System.Globalization.CultureInfo("ru-RU"));
            monthName = char.ToUpper(monthName[0]) + monthName.Substring(1);
            MonthLabel.Text = monthName;

            DateTime firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;

            for (int i = 0; i < startOffset; i++)
                DaysGrid.Children.Add(new Border());

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);

                var border = new Border
                {
                    Style = (Style)FindResource("DayBorderStyle"),
                    Cursor = Cursors.Hand
                };

                if (IsInRange(currentDay))
                    border.Background = (Brush)FindResource("SelectedDayBrush");
                else if (currentDay == DateTime.Today)
                    border.Background = (Brush)FindResource("TodayBrush");


                var txt = new TextBlock
                {
                    Text = day.ToString(),
                    Style = (Style)FindResource("DayTextStyle")
                };

                border.Child = txt;

                border.MouseLeftButtonDown += (s, e) => OnDayClick(currentDay);

                DaysGrid.Children.Add(border);
            }
        }

        private bool IsInRange(DateTime day)
        {
            if (StartDate.HasValue && EndDate.HasValue)
                return day >= StartDate.Value && day <= EndDate.Value;

            if (StartDate.HasValue && !EndDate.HasValue)
                return day == StartDate.Value;

            return false;
        }

        private void OnDayClick(DateTime selectedDay)
        {
            if (!StartDate.HasValue)
            {
                StartDate = selectedDay;
                EndDate = null;
            }
            else if (!EndDate.HasValue)
            {
                EndDate = selectedDay;
                if (StartDate > EndDate)
                {
                    var temp = StartDate;
                    StartDate = EndDate;
                    EndDate = temp;
                }
            }
            else
            {
                StartDate = selectedDay;
                EndDate = null;
            }

            RenderCalendar();

            DateRangeChanged?.Invoke(this, new Tuple<DateTime?, DateTime?>(StartDate, EndDate));
        }

        public event Action<object, Tuple<DateTime?, DateTime?>> DateRangeChanged;

        public void SetMonth(DateTime month)
        {
            CurrentMonth = new DateTime(month.Year, month.Month, 1);
            RenderCalendar();
        }
    }
}