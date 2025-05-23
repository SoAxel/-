using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HonestSurvey
{
    public partial class MainWindow : Window
    {
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Дякую за чесну відповідь! 😊", "Результат");
        }

        private void NoButton_MouseEnter(object sender, MouseEventArgs e)
        {
            // Зміна позиції кнопки "Ні"
            double maxX = this.ActualWidth - NoButton.ActualWidth - 40;
            double maxY = this.ActualHeight - NoButton.ActualHeight - 60;

            double newX = random.NextDouble() * maxX;
            double newY = random.NextDouble() * maxY;

            NoButton.Margin = new Thickness(newX, newY, 0, 0);
        }
    }
}
