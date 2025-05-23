using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace HorseRacingSimulator
{
    public partial class MainWindow : Window
    {
        private List<Horse> _horses = new List<Horse>();
        private readonly Random _random = new Random();
        private readonly int _trackLength = 800;
        private int _currentCameraHorseIndex = -1;
        private bool _isRaceInProgress = false;
        private double _balance = 1000;
        private Dictionary<Horse, double> _bets = new Dictionary<Horse, double>();
        private Dictionary<Color, BitmapImage[]> _horseAnimations = new Dictionary<Color, BitmapImage[]>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrackBackground();
            InitializeTrack();
            InitializeHorses(5);
            InitializeBettingControls();
            UpdateResultsGrid();
        }

        #region Initialization Methods

        private void InitializeTrackBackground()
        {
            try
            {
                var bgImage = new Image
                {
                    Source = LoadBitmapFromResource("track.png"),
                    Stretch = Stretch.Fill
                };
                RaceTrackCanvas.Children.Insert(0, bgImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження фону: {ex.Message}");
            }
        }

        private void InitializeTrack()
        {
            try
            {
                var geometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = new Point(50, 100), IsClosed = false };

                figure.Segments.Add(new LineSegment(new Point(750, 100), true));
                figure.Segments.Add(new ArcSegment(
                    new Point(750, 300),
                    new Size(200, 100),
                    0, false, SweepDirection.Clockwise, true));
                figure.Segments.Add(new LineSegment(new Point(50, 300), true));
                figure.Segments.Add(new ArcSegment(
                    new Point(50, 100),
                    new Size(200, 100),
                    0, false, SweepDirection.Clockwise, true));

                geometry.Figures.Add(figure);
                TrackPath.Data = geometry;

                var finishLine = new Line()
                {
                    X1 = 740,
                    Y1 = 90,
                    X2 = 740,
                    Y2 = 310,
                    Stroke = Brushes.Red,
                    StrokeThickness = 3,
                    StrokeDashArray = new DoubleCollection() { 5, 5 }
                };
                RaceTrackCanvas.Children.Add(finishLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка ініціалізації треку: {ex.Message}");
            }
        }

        private void InitializeHorses(int count)
        {
            _horses.Clear();
            _horseAnimations.Clear();

            // Видаляємо старі зображення коней (зберігаючи фон, трек і фінішну лінію)
            while (RaceTrackCanvas.Children.Count > 2)
                RaceTrackCanvas.Children.RemoveAt(2);

            var colors = new[]
            {
                Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow,
                Colors.Purple, Colors.Orange, Colors.Pink, Colors.Brown
            };

            var names = new[]
            {
                "Блискавка", "Вітер", "Гром", "Зірка", "Метеор", "Стріла", "Торнадо", "Ураган"
            };

            for (int i = 0; i < count; i++)
            {
                var coefficient = Math.Round(1.5 + _random.NextDouble(), 2);
                var cost = Math.Round(50 + _random.NextDouble() * 100, 2);
                var horse = new Horse(names[i], colors[i], _trackLength, coefficient, cost);
                horse.CurrentLocation = new Point(0, 120 + i * 40);
                _horses.Add(horse);

                // Завантажуємо анімаційні кадри для кожного кольору
                if (!_horseAnimations.ContainsKey(horse.Color))
                {
                    _horseAnimations[horse.Color] = LoadHorseAnimationFrames(horse.Color);
                }

                var horseImage = new Image
                {
                    Width = 60,
                    Height = 45,
                    Source = _horseAnimations[horse.Color][0],
                    Tag = horse
                };

                RaceTrackCanvas.Children.Add(horseImage);
            }
        }

        private BitmapImage[] LoadHorseAnimationFrames(Color horseColor)
        {
            var frames = new BitmapImage[4];
            try
            {
                var mask = LoadBitmapFromResource("mask_0000.png");

                for (int i = 0; i < 4; i++)
                {
                    var frame = LoadBitmapFromResource($"horse_frame_{i}.png");
                    frames[i] = (BitmapImage)ApplyColorMask(frame, mask, horseColor);
                }
            }
            catch
            {
                // Резервний варіант - створюємо прості кадри
                for (int i = 0; i < 4; i++)
                {
                    frames[i] = CreateSimpleHorseFrame(horseColor, i);
                }
            }
            return frames;
        }

        private BitmapImage CreateSimpleHorseFrame(Color color, int frameIndex)
        {
            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                // Проста анімація - зміна положення ніг
                var legY = 25 + (frameIndex % 2) * 5;
                dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(10, 10, 40, 20)); // Тіло
                dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(15, legY, 5, 15)); // Нога 1
                dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(30, legY, 5, 15)); // Нога 2
            }

            // Створюємо RenderTargetBitmap
            var renderTarget = new RenderTargetBitmap(60, 45, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);

            // Конвертуємо в BitmapImage
            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        private void InitializeBettingControls()
        {
            HorseToBetComboBox.ItemsSource = _horses;
            if (_horses.Count > 0)
                HorseToBetComboBox.SelectedIndex = 0;

            UpdateBalanceDisplay();
        }

        #endregion

        #region Core Game Logic

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRaceInProgress) return;

            _isRaceInProgress = true;
            StartButton.IsEnabled = false;
            PlaceBetButton.IsEnabled = false;

            _horses.ForEach(h => h.Reset());
            _bets.Clear();

            using (var barrier = new Barrier(_horses.Count + 1))
            {
                var tasks = _horses.Select(horse => Task.Run(() => horse.RunAsync(barrier))).ToList();

                var renderTask = Task.Run(() =>
                {
                    while (_horses.Any(h => h.Position < _trackLength))
                    {
                        barrier.SignalAndWait();

                        Dispatcher.Invoke(() =>
                        {
                            UpdateResultsGrid();
                            RenderHorses();
                        });

                        Thread.Sleep(50);
                    }

                    Dispatcher.Invoke(() => RaceFinished());
                });

                await Task.WhenAll(tasks);
                await renderTask;
            }
        }

        private void RenderHorses()
        {
            for (int i = 0; i < _horses.Count; i++)
            {
                var horse = _horses[i];
                var horseImage = RaceTrackCanvas.Children[i + 2] as Image;

                if (horseImage != null)
                {
                    // Оновлюємо позицію
                    Canvas.SetLeft(horseImage, horse.CurrentLocation.X);
                    Canvas.SetTop(horseImage, horse.CurrentLocation.Y);

                    // Оновлюємо анімацію
                    if (DateTime.Now - horse.LastFrameUpdate > TimeSpan.FromMilliseconds(100))
                    {
                        horse.CurrentFrame = (horse.CurrentFrame + 1) % 4;
                        horseImage.Source = _horseAnimations[horse.Color][horse.CurrentFrame];
                        horse.LastFrameUpdate = DateTime.Now;
                    }
                }
            }
        }

        private void RaceFinished()
        {
            _isRaceInProgress = false;
            StartButton.IsEnabled = true;
            PlaceBetButton.IsEnabled = true;

            UpdateCoefficients();
            CalculateWinnings();
            UpdateResultsGrid();
        }

        #endregion

        #region Helper Methods
        public BitmapImage LoadHorseImage(int frameNumber, Color horseColor)
        {
            try
            {
                // Шлях до кадру анімації
                var framePath = $"pack://application:,,,/Images/Horses/WithOutBorder_{frameNumber:D3}.png";
                // Шлях до маски
                var maskPath = $"pack://application:,,,/Images/HorsesMask/mask_{frameNumber:D4}.png";

                var frame = new BitmapImage(new Uri(framePath));
                var mask = new BitmapImage(new Uri(maskPath));

                return (BitmapImage)ApplyColorMask(frame, mask, horseColor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                return CreatePlaceholderImage(horseColor);
            }
        }

        private BitmapImage CreatePlaceholderImage(Color horseColor)
        {
            throw new NotImplementedException();
        }

        private BitmapImage LoadBitmapFromResource(string filename)
        {
            var uri = new Uri($"pack://application:,,,/Resources/{filename}");
            return new BitmapImage(uri);
        }

        private BitmapSource ApplyColorMask(BitmapImage original, BitmapImage mask, Color color)
        {
            // 1. Створюємо WriteableBitmap на основі оригінального зображення
            var writableOriginal = new WriteableBitmap(original);
            var writableMask = new WriteableBitmap(mask);

            // 2. Отримуємо пікселі
            int width = writableOriginal.PixelWidth;
            int height = writableOriginal.PixelHeight;
            int stride = width * 4; // 4 bytes per pixel (BGRA)
            byte[] originalPixels = new byte[height * stride];
            byte[] maskPixels = new byte[height * stride];

            writableOriginal.CopyPixels(originalPixels, stride, 0);
            writableMask.CopyPixels(maskPixels, stride, 0);

            // 3. Обробляємо пікселі
            for (int i = 0; i < originalPixels.Length; i += 4)
            {
                if (maskPixels[i + 3] > 0) // Перевіряємо альфа-канал маски
                {
                    // Застосовуємо колір
                    originalPixels[i + 2] = (byte)(color.R * originalPixels[i + 2] / 255); // R
                    originalPixels[i + 1] = (byte)(color.G * originalPixels[i + 1] / 255); // G
                    originalPixels[i] = (byte)(color.B * originalPixels[i] / 255);     // B
                }
            }

            // 4. Створюємо новий WriteableBitmap
            var result = new WriteableBitmap(
                width, height,
                original.DpiX, original.DpiY,
                PixelFormats.Pbgra32, null);

            result.WritePixels(
                new Int32Rect(0, 0, width, height),
                originalPixels,
                stride,
                0);

            return result;
        }

        private BitmapImage ConvertToBitmapImage(WriteableBitmap writeableBitmap)
        {
            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private void UpdateCoefficients()
        {
            var sortedHorses = _horses.OrderByDescending(h => h.Position).ToList();

            for (int i = 0; i < sortedHorses.Count; i++)
            {
                sortedHorses[i].Coefficient = Math.Round(1.0 + (sortedHorses.Count - i) * 0.2, 2);
            }
        }

        private void CalculateWinnings()
        {
            var winner = _horses.OrderByDescending(h => h.Position).First();

            foreach (var bet in _bets)
            {
                if (bet.Key == winner)
                {
                    _balance += bet.Value * winner.Coefficient;
                    MessageBox.Show($"Ви виграли {bet.Value * winner.Coefficient:N2} на коні {winner.Name}!");
                }
            }

            UpdateBalanceDisplay();
        }

        private void UpdateResultsGrid()
        {
            var horseViewModels = _horses
                .OrderByDescending(h => h.Position)
                .Select((h, index) => new
                {
                    Position = index + 1,
                    h.Name,
                    ColorBrush = new SolidColorBrush(h.Color),
                    ColorText = h.Color.ToString(),
                    RaceTime = h.RaceTime == TimeSpan.Zero ? "В бігу..." : h.RaceTime.ToString(@"mm\:ss\.fff"),
                    h.Coefficient,
                    h.Cost,
                    CurrentBet = _bets.ContainsKey(h) ? _bets[h] : 0,
                    PotentialWin = _bets.ContainsKey(h) ? _bets[h] * h.Coefficient : 0
                })
                .ToList();

            ResultsDataGrid.ItemsSource = horseViewModels;
        }

        private void UpdateBalanceDisplay()
        {
            BalanceText.Text = _balance.ToString("N2");
        }

        #endregion

        #region Event Handlers

        private void PlaceBetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRaceInProgress)
            {
                MessageBox.Show("Не можна робити ставки під час гонки!");
                return;
            }

            if (!double.TryParse(BetAmountTextBox.Text, out double amount) || amount <= 0)
            {
                MessageBox.Show("Введіть коректну суму ставки!");
                return;
            }

            if (amount > _balance)
            {
                MessageBox.Show("Недостатньо коштів на балансі!");
                return;
            }

            var selectedHorse = HorseToBetComboBox.SelectedItem as Horse;
            if (selectedHorse == null)
            {
                MessageBox.Show("Виберіть коня!");
                return;
            }
            _balance -= amount;
            _bets[selectedHorse] = _bets.ContainsKey(selectedHorse) ? _bets[selectedHorse] + amount : amount;

            UpdateBalanceDisplay();
            UpdateResultsGrid();

            MessageBox.Show($"Ставка {amount:N2} на {selectedHorse.Name} прийнята! Коефіцієнт: {selectedHorse.Coefficient:N2}");
        }

        private void SwitchCameraButton_Click(object sender, RoutedEventArgs e)
        {
            if (_horses.Count == 0) return;

            _currentCameraHorseIndex = (_currentCameraHorseIndex + 1) % (_horses.Count + 1);

            if (_currentCameraHorseIndex == _horses.Count)
            {
                // Загальний план
                RaceTrackCanvas.RenderTransform = null;
                CameraInfoText.Text = "Камера: Загальний план";
            }
            else
            {
                // Фокус на коня
                var horse = _horses[_currentCameraHorseIndex];
                RaceTrackCanvas.RenderTransform = new TranslateTransform(
                    -horse.CurrentLocation.X + 400,
                    -horse.CurrentLocation.Y + 200);
                CameraInfoText.Text = $"Камера: {horse.Name}";
            }
        }

        #endregion
    }
}