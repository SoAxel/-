using System;
using System.Windows;

namespace Group
{
    public partial class PartiyaWindow : Window
    {
        public PartiyaTovaru Partiya { get; private set; }

        public PartiyaWindow()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Partiya = new PartiyaTovaru
                {
                    Gorodyna = new Gorodyna
                    {
                        Name = NameBox.Text,
                        Country = CountryBox.Text,
                        Season = int.Parse(SeasonBox.Text)
                    },
                    Quantity = int.Parse(QtyBox.Text),
                    UnitPrice = decimal.Parse(PriceBox.Text),
                    TransportCost = decimal.Parse(TransBox.Text),
                    Delivery = DeliveryMethod.Постачальник, // можна зробити змінним через ComboBox
                    DeliveryDate = DatePicker.SelectedDate ?? DateTime.Now
                };

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}