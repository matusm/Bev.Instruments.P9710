namespace Bev.Instruments.P9710
{
    public static class Extensions
    {
        public static MeasurementRange Increment(this MeasurementRange range)
        {
            switch (range)
            {
                case MeasurementRange.Unknown:
                case MeasurementRange.RangeOverflow:
                case MeasurementRange.Range03:
                    return range;
                case MeasurementRange.Range04:
                    return MeasurementRange.Range03;
                case MeasurementRange.Range05:
                    return MeasurementRange.Range04;
                case MeasurementRange.Range06:
                    return MeasurementRange.Range05;
                case MeasurementRange.Range07:
                    return MeasurementRange.Range06;
                case MeasurementRange.Range08:
                    return MeasurementRange.Range07;
                case MeasurementRange.Range09:
                    return MeasurementRange.Range08;
                case MeasurementRange.Range10:
                    return MeasurementRange.Range09;
                default:
                    return MeasurementRange.Unknown;
            }
        }
        public static MeasurementRange Decrement(this MeasurementRange range)
        {
            switch (range)
            {
                case MeasurementRange.Unknown:
                case MeasurementRange.RangeOverflow:
                case MeasurementRange.Range10:
                    return range;
                case MeasurementRange.Range03:
                    return MeasurementRange.Range04;
                case MeasurementRange.Range04:
                    return MeasurementRange.Range05;
                case MeasurementRange.Range05:
                    return MeasurementRange.Range06;
                case MeasurementRange.Range06:
                    return MeasurementRange.Range07;
                case MeasurementRange.Range07:
                    return MeasurementRange.Range08;
                case MeasurementRange.Range08:
                    return MeasurementRange.Range09;
                case MeasurementRange.Range09:
                    return MeasurementRange.Range10;
                default:
                    return MeasurementRange.Unknown;
            }
        }
    }

    public enum MeasurementRange
    {
        Unknown = -2,
        RangeOverflow = -1, // >1.999 mA
        Range03 = 0, // 1.999 mA -  0.200 mA
        Range04 = 1, // 199.9 uA -   20.0 uA
        Range05 = 2, // 19.99 uA -   2.00 uA
        Range06 = 3, // 1.999 uA -  0.200 uA
        Range07 = 4, // 199.9 nA -   20.0 nA
        Range08 = 5, // 19.99 nA -   2.00 nA
        Range09 = 6, // 1.999 nA -  0.200 nA
        Range10 = 7  // 199.9 pA -  000.0 pA (this range seems to be not implemented in our instruments)
    }
}
