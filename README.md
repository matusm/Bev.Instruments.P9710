# Bev.Instruments.P9710

A lightweight C# library for controlling the optometer P-9710 via the serial bus.

## Overview

The high quality portable optometer P-9710 by [Gigahertz-Optik GmbH](https://www.gigahertz-optik.com/) is basically a photo-current meter with high dynamic range. It can be controlled via its RS232 interface.

The API of this library allows to read the actual photo current of the instrument. When a calibrated detector head is used on can also read the corresponding radiometric/photometric quantity. The RS232 interface allowes full controll including internal adjustments and parameter modifications of the instrument. However this library is purposefully designed to restrict any possibilities to modify instrument settings. There is a single exception: the instrument is set to auto-range mode.
## Constructor

The library is composed of a single class. The constructor `P9710(string)` creates a new instance of this class taking a string as the single argument. The string is interpreted as the port name of the serial port. Typical examples are `COM1` or `/dev/cu.usbmodem2`. The instrument is set to autorange mode. This is the only persistant setting caused by the use of this library.

### Methods

* `double GetDetectorCurrent()`
Gets the photo current in Ampere. This is the prefered way to query the instrument. On errors `double.NaN` is returned.
 
* `double GetPhotometricValue()`
Gets the radiometric/photometric quantity for the connected radiometer head. Internally this value is just the photo current multiplied by a calibration factor. This factor is stored in the head's connector and transfered to the optometer on power-on. On errors `double.NaN` is returned.
Error-prone! Might be depricated in future releases!
 
* `MeasurementRange GetMeasurementRange()`
Gets the actual measurement range (for detector current). The possible ranges are defined in the manual. The returned range is valid only for the very moment of the call. A previous or subsequent call to `GetDetectorCurrent()` may be performed in a differen range. This is the consequence of the auto-range functionality. This method is usefull for the detector current only!

* `MeasurementRange EstimateMeasurementRange(double)`
The measurement range is estimated from a current value passed to the method. No comunication with the instrument is initiated during the call. If the instrument is in fixed range mode, the returned range might be wrong. This method is usefull for the detector current only!

* `double GetMeasurementUncertainty(double)`
The measurement uncertainty as specified by the manufacturer for a valid calibrated instrument is returned. The measurement range is estimated as  before. This method is usefull for the detector current only!

* `double GetMeasurementUncertainty(double, MeasurementRange)`
Same as above for a fixed measurement range. This method is usefull for the detector current only!
 
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

* `DevicePort`
The port name as passed to the constructor.

* `DetectorID`
Returns a string identifing the detector head connected.

* `PhotometricUnit`
Returns the symbol of the measurement unit. This unit is used for the value returned by `GetPhotometricValue()`. Error-prone! Might be depricated in future releases!

## Notes

Once instantiated, it is not possible to modify the object's `DevicePort`. However swaping  instruments on the same port can work. Properties like `InstrumentID` will reflect the actual instrument.

With the low level, yet powerful method `string Query(string)` one can gain full control over the instrument. It is declared private in this library to restrict potential danger. You can declare it public if you are brave enough.

