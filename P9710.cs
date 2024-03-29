﻿using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading;

namespace Bev.Instruments.P9710
{
    public class P9710
    {
        protected readonly SerialPort comPort;
        protected const int waitOnClose = 100;

        public P9710(string portName)
        {
            DevicePort = portName.Trim();
            comPort = new SerialPort(DevicePort, 9600);
            SelectAutoRange();
        }

        public string DevicePort { get; }

        public string InstrumentManufacturer => "Gigahertz-Optik";
        public string InstrumentType => $"{ParseSoftwareVersion()[0]}-{GetDeviceVariant()}";
        public string InstrumentSerialNumber => GetDeviceSerialNumber();
        public string InstrumentFirmwareVersion => ParseSoftwareVersion()[1];
        public string InstrumentID => $"{InstrumentType} {InstrumentFirmwareVersion} SN:{InstrumentSerialNumber} @ {DevicePort}";
        public double InstrumentBatteryLevel => GetBatteryLevel();

        public string DetectorID => GetDetectorID();
        public double DetectorCalibrationFactor => GetCalibrationFactor();
        public string DetectorPhotometricUnit => Query("GU");

        public double GetCurrent() => ParseDoubleFrom(Query("MA"));

        public double GetPhotometricValue() => ParseDoubleFrom(Query("MV"));

        public MeasurementRange GetMeasurementRange()
        {
            string rangeString = Query("GR");
            MeasurementRange range = MeasurementRange.Unknown;
            switch (rangeString)
            {
                case "0":
                    range = MeasurementRange.Range03;
                    break;
                case "1":
                    range = MeasurementRange.Range04;
                    break;
                case "2":
                    range = MeasurementRange.Range05;
                    break;
                case "3":
                    range = MeasurementRange.Range06;
                    break;
                case "4":
                    range = MeasurementRange.Range07;
                    break;
                case "5":
                    range = MeasurementRange.Range08;
                    break;
                case "6":
                    range = MeasurementRange.Range09;
                    break;
                case "7":
                    range = MeasurementRange.Range10;
                    break;
                default:
                    break;
            }
            return range;
        }

        public void SetMeasurementRange(MeasurementRange measurementRange)
        {
            switch (measurementRange)
            {
                case MeasurementRange.Unknown:
                case MeasurementRange.RangeOverflow:
                    return;
                default:
                    DeselectAutoRange();
                    Query($"SR{(int)measurementRange}");
                    break;
            }
        }

        public void SelectAutoRange() => Query("SB1");

        public void DeselectAutoRange() => Query("SB0");

        public MeasurementRange EstimateMeasurementRange(double current)
        {
            if (double.IsNaN(current)) return MeasurementRange.Unknown;
            current = Math.Abs(current);
            if (current > 1.999E-3) return MeasurementRange.RangeOverflow;
            if (current > 1.999E-4) return MeasurementRange.Range03;
            if (current > 1.999E-5) return MeasurementRange.Range04;
            if (current > 1.999E-6) return MeasurementRange.Range05;
            if (current > 1.999E-7) return MeasurementRange.Range06;
            if (current > 1.999E-8) return MeasurementRange.Range07;
            if (current > 1.999E-9) return MeasurementRange.Range08;
            if (current > 1.999E-10) return MeasurementRange.Range09;
            return MeasurementRange.Range10;
        }

        public double GetSpecification(double current) => GetSpecification(current, EstimateMeasurementRange(current));

        public double GetSpecification(double current, MeasurementRange range)
        {
            double errorInterval = 0;
            current = Math.Abs(current);
            switch (range)
            {
                case MeasurementRange.Unknown:
                case MeasurementRange.RangeOverflow:
                    errorInterval = double.NaN;
                    break;
                case MeasurementRange.Range03:
                    errorInterval = 0.002 * current + 1.0E-6;
                    break;
                case MeasurementRange.Range04:
                    errorInterval = 0.002 * current + 1.0E-7;
                    break;
                case MeasurementRange.Range05:
                    errorInterval = 0.002 * current + 1.0E-8;
                    break;
                case MeasurementRange.Range06:
                    errorInterval = 0.002 * current + 1.0E-9;
                    break;
                case MeasurementRange.Range07:
                    errorInterval = 0.002 * current + 1.0E-10;
                    break;
                case MeasurementRange.Range08:
                    errorInterval = 0.002 * current + 1.0E-11;
                    break;
                case MeasurementRange.Range09:
                    errorInterval = 0.005 * current + 2.0E-12;
                    break;
                case MeasurementRange.Range10:
                    errorInterval = 0.005 * current + 2.0E-12;
                    break;
                default:
                    break;
            }
            return errorInterval;
        }

        public double GetMeasurementUncertainty(double current) => GetMeasurementUncertainty(current, EstimateMeasurementRange(current));

        public double GetMeasurementUncertainty(double current, MeasurementRange range) => Math.Sqrt(1.0 / 3.0) * GetSpecification(current, range);

        // By making this method public one can gain full controll over the instrument
        protected string Query(string command)
        {
            string answer = "???";
            OpenPort();
            try
            {
                comPort.WriteLine(command);
                answer = comPort.ReadLine();
            }
            catch (Exception)
            {
                // just do nothing
            }
            ClosePort();
            Thread.Sleep(waitOnClose);
            return answer;
        }

        protected void OpenPort()
        {
            try
            {
                if (!comPort.IsOpen)
                    comPort.Open();
            }
            catch (Exception)
            { }
        }

        protected void ClosePort()
        {
            try
            {
                if (comPort.IsOpen)
                {
                    comPort.Close();
                    Thread.Sleep(waitOnClose);
                }
            }
            catch (Exception)
            { }
        }

        private string[] ParseSoftwareVersion()
        {
            string str = GetSoftwareVersion();
            string[] tokens = str.Split(' ');
            return tokens.Length < 2 ? (new string[] { "?", "?" }) : tokens;
        }

        private string GetSoftwareVersion() => Query("GI");

        private string GetDeviceSerialNumber() => Query("TT");

        private string GetDeviceVariant() => Query("TF");

        private string GetDetectorID()
        {
            string id = Query("GK");
            if (string.IsNullOrWhiteSpace(id))
                return "";
            // Check if the magic string "PT9610" is present
            string magicString = "";
            for (int i = 0; i < 6; i++)
            {
                string s = Query($"GC{i}");
                int.TryParse(s, out int answ);
                magicString += $"{Convert.ToChar(answ)}";
            }
            if (magicString != "PT9610")
                return "";
            string secretString = "";
            for (int i = 0x10; i < 0x20; i++)
            {
                string s = Query($"GC{i}");
                int.TryParse(s, out int answ);
                secretString += $"{Convert.ToChar(answ)}";
            }
            int lowByte = int.Parse(Query("GC6"));
            int highByte = int.Parse(Query("GC7"));
            int sn = highByte * 256 + lowByte;
            id += $" SN:{sn} ({secretString.Trim()})";
            return id;
        }

        private double GetBatteryLevel() => ParseDoubleFrom(Query("MB"));

        private double GetCalibrationFactor() => ParseDoubleFrom(Query("GS4"));

        private double ParseDoubleFrom(string s)
        {
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double numericValue))
                return numericValue;
            else
                return double.NaN;
        }

    }

}
