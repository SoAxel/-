using System;
using System.Windows;
using Group;

namespace Delivery
{
    public partial class MainWindow : Window
    {
        private Sklad sklad;

        public MainWindow()
        {
            InitializeComponent();
            sklad = Storage.Load();
            NumberBox.Text = sklad.Number.ToString();
            CostBox.Text = sklad.MaintenanceCost.ToString();
            RefreshList();
            InitializeComponent();
            LoadSklad();
        }
        private void LoadSklad()
        {
            sklad = Storage.Load();
            NumberBox.Text = sklad.Number.ToString();
            CostBox.Text = sklad.MaintenanceCost.ToString("N2");
            RefreshList();
        }

        private void RefreshList()
        {

            PartiiList.ItemsSource = null;
            PartiiList.ItemsSource = sklad.Partiyi;
            this.Title = $"Склад - Загальна вартість товару: {sklad.TotalValue} грн";
        }


        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var form = new PartiyaWindow();
            if (form.ShowDialog() == true)
            {
                sklad.Partiyi.Add(form.Partiya);
                RefreshList();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (PartiiList.SelectedItem is PartiyaTovaru selected)
            {
                sklad.Partiyi.Remove(selected);
                RefreshList();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            sklad.Number = int.TryParse(NumberBox.Text, out int n) ? n : 0;
            sklad.MaintenanceCost = decimal.TryParse(CostBox.Text, out decimal c) ? c : 0;
            Storage.Save(sklad);
            MessageBox.Show("Збережено");
        }
    }
}
