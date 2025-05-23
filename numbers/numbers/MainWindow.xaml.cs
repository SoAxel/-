using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NumberFilterApp
{
    public partial class MainWindow : Window
    {
        private List<Button> numberButtons = new List<Button>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateRangeButton_Click(object sender, RoutedEventArgs e)
        {
            NumberPanel.Children.Clear();
            numberButtons.Clear();

            if (int.TryParse(FromBox.Text, out int from) &&
                int.TryParse(ToBox.Text, out int to) &&
                from <= to)
            {
                for (int i = from; i <= to; i++)
                {
                    Button btn = new Button
                    {
                        Content = i.ToString(),
                        Width = 50,
                        Height = 30,
                        Margin = new Thickness(5),
                        Background = Brushes.White,
                        Tag = i
                    };

                    btn.Click += NumberButton_Click;

                    numberButtons.Add(btn);
                    NumberPanel.Children.Add(btn);
                }
            }
            else
            {
                MessageBox.Show("Введіть коректний діапазон чисел.");
            }
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                int value = (int)btn.Tag;

                if (int.TryParse(FilterBox.Text, out int divisor) && divisor != 0)
                {
                    if (value % divisor == 0)
                    {
                        MessageBox.Show($"Число {value} є кратним {divisor}.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Число {value} НЕ є кратним {divisor}.", "Результат", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Введіть коректне число для перевірки (у полі фільтрації).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(FilterBox.Text, out int divisor) && divisor != 0)
            {
                foreach (var btn in numberButtons)
                {
                    int value = (int)btn.Tag;
                    btn.Background = value % divisor == 0 ? Brushes.LightGreen : Brushes.White;
                }
            }
            else
            {
                MessageBox.Show("Введіть коректне число (не 0)!");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(FilterBox.Text, out int divisor) && divisor != 0)
            {
                var toRemove = numberButtons
                    .Where(btn => (int)btn.Tag % divisor == 0)
                    .ToList();

                foreach (var btn in toRemove)
                {
                    NumberPanel.Children.Remove(btn);
                    numberButtons.Remove(btn);
                }
            }
            else
            {
                MessageBox.Show("Введіть коректне число для видалення!");
            }
        }
    }
}
