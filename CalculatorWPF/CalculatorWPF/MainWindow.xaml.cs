using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CalculatorWPF
{
    public partial class MainWindow : Window
    {
        private double _currentValue = 0;
        private string _pendingOperator = null;
        private bool _isNewInput = true;
        private bool _isDecimal = false;
        private Stack<string> _inputHistory = new Stack<string>();


        public MainWindow()
        {

            InitializeComponent();
            Display.Text = "0";
        }


        private void Digit_Click(object sender, RoutedEventArgs e)
        {
            var digit = ((Button)sender).Content.ToString();

            if (_isNewInput)
            {
                // Якщо нове введення, встановлюємо значення дисплея
                Display.Text = digit;
                _isNewInput = false;
                _isDecimal = false;
            }
            else
            {
                // Запобігаємо введенню декількох початкових нулів
                if (Display.Text == "0" && digit == "0")
                {
                    // Ігноруємо додаткові нулі
                    return;
                }
                else if (Display.Text == "0" && digit != "0")
                {
                    // Замінюємо початковий нуль на іншу цифру
                    Display.Text = digit;
                }
                else
                {
                    // Додаємо цифру до поточного введення
                    Display.Text += digit;
                }
            }
        }



        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _currentValue = 0;
            _pendingOperator = null;
            Display.Text = "0";
            _isNewInput = true;
            _isDecimal = false;
        }

        private void CE_Click(object sender, RoutedEventArgs e)
        {
            if (_inputHistory.Count > 0)
            {
                Display.Text = _inputHistory.Pop();
                _isNewInput = false;
                _isDecimal = Display.Text.Contains(",");
            }
            else
            {
                Display.Text = "0";
                _isNewInput = true;
                _isDecimal = false;
            }
        }


        private void Op_Click(object sender, RoutedEventArgs e)
        {
            var newOperator = ((Button)sender).Content.ToString();
            if (!double.TryParse(Display.Text, out double newValue)) return;

            if (_pendingOperator != null && !_isNewInput)
            {
                _currentValue = PerformOperation(_currentValue, newValue, _pendingOperator);
                Display.Text = _currentValue.ToString();
            }
            else
            {
                _currentValue = newValue;
            }

            _pendingOperator = newOperator;
            _isNewInput = true;
            _isDecimal = false;
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingOperator == null) return;
            if (!double.TryParse(Display.Text, out double newValue)) return;

            _currentValue = PerformOperation(_currentValue, newValue, _pendingOperator);
            Display.Text = _currentValue.ToString();

            _pendingOperator = null;
            _isNewInput = true;
            _isDecimal = false;
        }

        private void Dot_Click(object sender, RoutedEventArgs e)
        {
            if (!_isDecimal)
            {
                Display.Text += ",";
                _isDecimal = true;
                _isNewInput = false;
            }
        }

        private void Percent_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Display.Text, out double value))
            {
                value /= 100;
                Display.Text = value.ToString();
                _isNewInput = true;
                _isDecimal = Display.Text.Contains(",");
            }
        }

        private double PerformOperation(double left, double right, string op)
        {
            switch (op)
            {
                case "+": return left + right;
                case "-": return left - right;
                case "*": return left * right;
                case "/": return right == 0 ? throw new DivideByZeroException() : left / right;
                default: return right;
            }
        }

    }
}
