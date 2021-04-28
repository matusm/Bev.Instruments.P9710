﻿using System;
using System.IO.Ports;
using System.Threading;
using System.Globalization;

namespace Bev.Instruments.P9710
{
    public class P9710
    {
        public P9710(string portName)
        {
            comPort = new SerialPort(portName, 9600);
            DevicePort = portName;
            SelectAutorange();
        }

        public string InstrumentManufacturer => "Gigahertz-Optik";
        public string InstrumentType => $"{ParseSoftwareVersion()[0]}-{GetDeviceVariant()}";
        public string InstrumentSerialNumber => GetDeviceSerialNumber();
        public string InstrumentFirmwareVersion => ParseSoftwareVersion()[1];
        public string InstrumentID => $"{InstrumentType} {InstrumentFirmwareVersion} SN:{InstrumentSerialNumber} @ {DevicePort}";
        public double InstrumentBattery => GetBatteryLevel();
        public string DevicePort { get; }
        public string DetectorID => GetDetectorID();
        public string PhotometricUnit => Query("GU");

        public double GetDetectorCurrent()
        {
            double current;
            string answer = Query("MA");
            if (double.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture, out current))
                return current;
            else
                return double.NaN;
        }

        public double GetPhotometricValue()
        {
            double illuminance;
            string answer = Query("MV");
            if (Double.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture, out illuminance))
                return illuminance;
            else
                return double.NaN;
        }

        public MeasurementRange GetMeasurementRange()
        {
            string rangeString = Query("GR");
            MeasurementRange range = MeasurementRange.Unknown;
            switch (rangeString)
            {
                case "0":
                    range = MeasurementRange.Range0;
                    break;
                case "1":
                    range = MeasurementRange.Range1;
                    break;
                case "2":
                    range = MeasurementRange.Range2;
                    break;
                case "3":
                    range = MeasurementRange.Range3;
                    break;
                case "4":
                    range = MeasurementRange.Range4;
                    break;
                case "5":
                    range = MeasurementRange.Range5;
                    break;
                case "6":
                    range = MeasurementRange.Range6;
                    break;
                case "7":
                    range = MeasurementRange.Range7;
                    break;
                default:
                    break;
            }
            return range;
        }

        public MeasurementRange EstimateMeasurementRange(double current)
        {
            if (double.IsNaN(current)) return MeasurementRange.Unknown;
            current = Math.Abs(current);
            if (current > 2.0E-4) return MeasurementRange.Range0;
            if (current > 2.0E-5) return MeasurementRange.Range1;
            if (current > 2.0E-6) return MeasurementRange.Range2;
            if (current > 2.0E-7) return MeasurementRange.Range3;
            if (current > 2.0E-8) return MeasurementRange.Range4;
            if (current > 2.0E-9) return MeasurementRange.Range5;
            if (current > 2.0E-10) return MeasurementRange.Range6;
            return MeasurementRange.Range7;
        }

        public double GetMeasurementUncertainty(double current)
        {
            var range = EstimateMeasurementRange(current);
            return GetMeasurementUncertainty(current, range);
        }

        public double GetMeasurementUncertainty(double current, MeasurementRange range)
        {
            double error = 0;
            current = Math.Abs(current);
            switch (range)
            {
                case MeasurementRange.Unknown:
                    error = double.NaN;
                    break;
                case MeasurementRange.Range0:
                    error = 0.002 * current + 1.0E-6;
                    break;
                case MeasurementRange.Range1:
                    error = 0.002 * current + 1.0E-7;
                    break;
                case MeasurementRange.Range2:
                    error = 0.002 * current + 1.0E-8;
                    break;
                case MeasurementRange.Range3:
                    error = 0.002 * current + 1.0E-9;
                    break;
                case MeasurementRange.Range4:
                    error = 0.002 * current + 1.0E-10;
                    break;
                case MeasurementRange.Range5:
                    error = 0.002 * current + 1.0E-11;
                    break;
                case MeasurementRange.Range6:
                    error = 0.005 * current + 2.0E-12;
                    break;
                case MeasurementRange.Range7:
                    error = 0.005 * current + 2.0E-12;
                    break;
                default:
                    break;
            }
            // divide by Sqrt(3) for standard uncertainty
            return error * 0.57735;
        }

        // TODO make private to avoid unintended re-setting of the instrument
        public string Query(string command)
        {
            if (!comPort.IsOpen) comPort.Open();
            comPort.WriteLine(command);
            string answer = "***";
            try
            {
                answer = comPort.ReadLine();
            }
            catch (Exception)
            {
                // just do nothing
            }
            if (comPort.IsOpen) comPort.Close();
            Thread.Sleep(delayOnClose);
            return answer;
        }

        private string[] ParseSoftwareVersion()
        {
            string str = GetSoftwareVersion();
            string[] tokens = str.Split(' ');
            return tokens.Length < 2 ? (new string[] { "?", "?" }) : tokens;
        }

        private string GetSoftwareVersion()
        {
            return Query("GI");
        }

        private string GetDeviceSerialNumber()
        {
            return Query("TT");
        }

        private string GetDeviceVariant()
        {
            return Query("TF");
        }

        private string GetDetectorID()
        {
            string id = Query("GK");
            if (string.IsNullOrWhiteSpace(id))
                return "";
            for (int i = 0; i < 6; i++)
            {
                string s = Query($"GC{i}");
                int answ = 0;
                Int32.TryParse(s, out answ);
                id += $"{Convert.ToChar(answ)}";
            }
            return id;
        }

        private double GetBatteryLevel()
        {
            double battery;
            string answer = Query("MB");
            if (double.TryParse(answer, NumberStyles.Any, CultureInfo.InvariantCulture, out battery))
                return battery;
            else
                return double.NaN;
        }

        private void SelectAutorange()
        {
            Query("SB1");
        }

        private static SerialPort comPort;

        // https://docs.microsoft.com/en-us/dotnet/api/system.io.ports.serialport.close?view=dotnet-plat-ext-5.0
        // The best practice for any application is to wait for some amount of time
        // after calling the Close method before attempting to call the Open method,
        // as the port may not be closed instantly.
        // No amount is given! one has to experiment with this value
        private const int delayOnClose = 100;

    }

    public enum MeasurementRange
    {
        Unknown,
        Range0, //   2 mA
        Range1, // 200 uA
        Range2, //  20 uA
        Range3, //   2 uA
        Range4, // 200 nA
        Range5, //  20 nA
        Range6, //   2 nA
        Range7  // 200 pA
    }

}
