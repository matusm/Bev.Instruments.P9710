# Bev.Instruments.P9710

A lightweight C# library for controlling the optometer P-9710 via the serial bus.

## Overview

The high quality portable optometer P-9710 by [Gigahertz-Optik GmbH](https://www.gigahertz-optik.com/) is basically a photo-current meter with high dynamic range. It can be controlled via its RS232 interface.

The API of this library allows to identify and to read the actual photo current of the instrument. On using a calibrated detector head, one can also read the corresponding radiometric/photometric quantity. The RS232 interface allowes full control including internal adjustments and persistant parameter modifications of the instrument. However this library is purposefully designed to restrict any possibilities to modify instrument settings (with the exception of range setting).

### Constructor

The library is composed of a single class. The constructor `P9710(string)` creates a new instance of this class taking a string as the single argument. The string is interpreted as the port name of the serial port. Typical examples are `COM1` or `/dev/tty.usbserial-FTY594BQ`. The instrument is set to autorange mode which is the only persistant setting caused by the use of this library.

### Methods

* `double GetCurrent()`
Gets the photo current in Ampere. The prefered way to query the instrument. On errors `double.NaN` is returned.
 
* `double GetPhotometricValue()`
Gets the radiometric/photometric quantity for the connected radiometer head. Internally this value is just the photo current divided by a calibration factor stored in the head's connector. On errors `double.NaN` is returned.
Danger of confusion - might be depricated in future releases!
 
* `MeasurementRange GetMeasurementRange()`
Gets the actual measurement range (for photo current) by querying the instrument. The possible ranges are defined in the manual and coded in an `enum`. The returned range is valid only for the very moment of the call. As a consequence of the auto-range functionality, previous or subsequent calls to `GetCurrent()` might be performed in a different range. This method is usefull for the photo current only!

The methods below are purely numeric and can be used without instrument connected. No communication with the instrument is initiated during the calls.

* `MeasurementRange EstimateMeasurementRange(double)`
The measurement range is estimated from a current value passed to the method. If the instrument is in fixed range mode, the returned value might be incorrect. This method is usefull for the photo current only!

* `double GetMeasurementUncertainty(double)`
The measurement uncertainty as specified by the manufacturer is returned. The measurement range is estimated as before. This method is usefull for the photo current only!

* `double GetMeasurementUncertainty(double, MeasurementRange)`
Same as above for a fixed measurement range. This method is usefull for the photo current only!
 
### Properties

All properties are getters only.

* `InstrumentManufacturer`
Returns the string "Gigahertz-Optik".

* `InstrumentType`
Returns a string of the instrument designation.

* `InstrumentSerialNumber`
Returns the unique serial number of the instrument as a string. Usually this number is different from the serial number printed on the instrument case.

* `InstrumentFirmwareVersion`
Returns a string for the firmware version.

* `InstrumentID`
Returns a combination of the previous properties which unambiguously identifies the instrument (hopefully).

* `InstrumentBatteryLevel`
Returns the percentage of the actual battery capacity. Always 100 with plug-in power supply connected.

* `DetectorID`
Returns a string identifing the detector head connected.

* `DetectorPhotometricUnit`
Returns the symbol of the measurement unit. This unit is used for the value returned by `GetPhotometricValue()`. Danger of confusion - might be depricated in future releases!

* `DetectorCalibrationFactor`
Returns calibration factor for the detector head used. Danger of confusion - might be depricated in future releases!

* `DevicePort`
The port name as passed to the constructor.

## Notes

Once instantiated, it is not possible to modify the object's `DevicePort`. However swaping  instruments on the same port may work. Properties like `InstrumentID` etc. will reflect the actual instrument.

With the low level, yet powerful method `string Query(string)`, one can gain full control over the instrument. It is declared private in this library to restrict potential danger. You can declare it public if you are brave enough.

## Usage

The following code fragment demonstrate the use of this class.

```cs
using Bev.Instruments.P9710;
using System;

namespace PhotoPlayground
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var optometer = new P9710("COM1");

            Console.WriteLine($"Instrument: {optometer.InstrumentID}");
            Console.WriteLine($"Detector:   {optometer.DetectorID}");
            Console.WriteLine($"CalFactor:  {optometer.DetectorCalibrationFactor} A/{optometer.DetectorPhotometricUnit}"); 
            Console.WriteLine($"Battery:    {optometer.InstrumentBatteryLevel} %");
            Console.WriteLine();
            
            for (int i = 0; i < 10; i++)
            {
                double current = optometer.GetCurrent();
                Console.WriteLine($"{i,3} : {current} A  -  (Measurement range: {optometer.GetMeasurementRange()})");
            }
        }
    }
}
```
