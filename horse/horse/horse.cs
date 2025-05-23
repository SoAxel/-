using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HorseRacingSimulator
{
    public class Horse
    {
        public string Name { get; private set; }
        public Color Color { get; private set; }
        public TimeSpan RaceTime { get; private set; }
        public double Speed { get; private set; }
        public double Acceleration { get; private set; }
        public double Position { get; set; }
        public double Coefficient { get; set; }
        public double Cost { get; set; }
        public Point CurrentLocation { get; set; }
        public DateTime LastFrameUpdate { get; internal set; }

        private readonly Random _random;
        private readonly int _trackLength;
        internal int CurrentFrame;

        public Horse(string name, Color color, int trackLength, double coefficient, double cost)
        {
            Name = name;
            Color = color;
            _trackLength = trackLength;
            _random = new Random(Guid.NewGuid().GetHashCode());
            Speed = _random.Next(5, 11);
            Coefficient = coefficient;
            Cost = cost;
            Reset();
        }

        public async Task RunAsync(Barrier barrier)
        {
            var startTime = DateTime.Now;

            while (Position < _trackLength)
            {
                await Task.Delay(100);

                Acceleration = Speed * (_random.NextDouble() * 0.3 + 0.7);
                Position += Acceleration;

                if (Position > _trackLength)
                    Position = _trackLength;

                CurrentLocation = new Point(Position, CurrentLocation.Y);

                barrier.SignalAndWait();
            }

            RaceTime = DateTime.Now - startTime;
        }

        public void Reset()
        {
            Position = 0;
            RaceTime = TimeSpan.Zero;
        }
    }
}