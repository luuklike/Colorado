using System;
using System.Collections.Generic;

namespace WeatherMonitoringSystem
{
    public enum EnvironmentalMetric
    {
        Temperature,
        Humidity,
        Pressure
    }

    public interface ISubscriber
    {
        void ReceiveUpdate(EnvironmentalMetric metric, double value);
    }

    public class MeteorologicalCenter
    {
        private Dictionary<EnvironmentalMetric, double> _readings;
        private Dictionary<EnvironmentalMetric, HashSet<ISubscriber>> _subscribers;

        public MeteorologicalCenter()
        {
            _readings = new Dictionary<EnvironmentalMetric, double>();
            _subscribers = new Dictionary<EnvironmentalMetric, HashSet<ISubscriber>>();

            foreach (EnvironmentalMetric metric in Enum.GetValues(typeof(EnvironmentalMetric)))
            {
                _readings[metric] = 0;
                _subscribers[metric] = new HashSet<ISubscriber>();
            }
        }

        public void RegisterSubscriber(EnvironmentalMetric metric, ISubscriber subscriber)
        {
            if (!_subscribers[metric].Contains(subscriber))
            {
                _subscribers[metric].Add(subscriber);
                Console.WriteLine($"Successfully registered for {metric} updates");
            }
            else
            {
                Console.WriteLine($"Registration for {metric} already exists");
            }
        }

        public void RemoveSubscriber(EnvironmentalMetric metric, ISubscriber subscriber)
        {
            if (_subscribers[metric].Contains(subscriber))
            {
                _subscribers[metric].Remove(subscriber);
                Console.WriteLine($"Registration removed for {metric}");
            }
        }

        public void UpdateMeasurement(EnvironmentalMetric metric, double value)
        {
            if (Math.Abs(_readings[metric] - value) < 0.001)
            {
                return;
            }

            _readings[metric] = value;
            BroadcastUpdate(metric);
        }

        private void BroadcastUpdate(EnvironmentalMetric metric)
        {
            foreach (var recipient in _subscribers[metric])
            {
                recipient.ReceiveUpdate(metric, _readings[metric]);
            }
        }
    }

    public class Participant : ISubscriber
    {
        private string _identifier;

        public Participant(string identifier)
        {
            _identifier = identifier;
        }

        public void ReceiveUpdate(EnvironmentalMetric metric, double value)
        {
            Console.WriteLine($"{_identifier} received update: {metric} = {value}");
        }
    }

    public class Program
    {
        public static void Main()
        {
            var weatherHub = new MeteorologicalCenter();

            var firstParticipant = new Participant("Julie");
            var secondParticipant = new Participant("Michael");

            weatherHub.RegisterSubscriber(EnvironmentalMetric.Temperature, firstParticipant);
            weatherHub.RegisterSubscriber(EnvironmentalMetric.Humidity, firstParticipant);
            weatherHub.RegisterSubscriber(EnvironmentalMetric.Pressure, secondParticipant);

            Console.WriteLine("\nFirst round of measurements:");
            weatherHub.UpdateMeasurement(EnvironmentalMetric.Temperature, 25.5);
            weatherHub.UpdateMeasurement(EnvironmentalMetric.Humidity, 65.0);
            weatherHub.UpdateMeasurement(EnvironmentalMetric.Pressure, 1013.2);

            Console.WriteLine("\nRemoving humidity updates registration for first participant...");
            weatherHub.RemoveSubscriber(EnvironmentalMetric.Humidity, firstParticipant);

            Console.WriteLine("\nSecond round of measurements:");
            weatherHub.UpdateMeasurement(EnvironmentalMetric.Temperature, 26.0);
            weatherHub.UpdateMeasurement(EnvironmentalMetric.Humidity, 67.0);
            weatherHub.UpdateMeasurement(EnvironmentalMetric.Pressure, 1014.0);
            Console.WriteLine("\nAttempting duplicate registration:");
            weatherHub.RegisterSubscriber(EnvironmentalMetric.Temperature, firstParticipant);
        }
    }
}