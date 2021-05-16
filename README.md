# Bev.Instruments.P9710

A lightweight C# library for controlling the optometer P-9710 via the serial bus.

## Overview

The high quality portable optometer P-9710 by [Gigahertz-Optik GmbH](https://www.gigahertz-optik.com/) is basically a photo-current meter with high dynamic range. It can be controlled via its RS232 interface.

The API of this library allows to read the actual photo current or a radiometric quantity if a calibrated detector head is used. The interface allowes full controll including internal adjustments and parameter modifications of the instrument. However this library is intentionally  designed to restrict any possibilities to modify instrument settings. (There is a single exception, see explanation of constructor) 

## Constructor

The library is composed of a single class. The constructor `P9710(string)` creates a new instance of this class taking a string as the single argument. The string is interpreted as the port name of the serial port. Typical examples are `COM1` or `/dev/cu.usbmodem2`. The instrument is set to autorange mode. This is the only persistant setting caused by the use of this library.

### Methods

* `double GetDetectorCurrent()`
Gets the photo current in Ampere. On errors `double.NaN` is returned.
 
* `double GetPhotometricValue()`
Gets the radiometric/photometric quantity for the connected radiometer head. Internally this value is just the photo current multiplied by a calibration factor. This factor is stored in the head's connector and transfered to the optometer on power-on. On errors `double.NaN` is returned.
Error-prone! This method will be removed in future releases!
 
* `MeasurementRange GetMeasurementRange()`
Explanation. This method is usefull for the detector current only!

* `MeasurementRange EstimateMeasurementRange(double)`
Explanation. This method is usefull for the detector current only!

* `double GetMeasurementUncertainty(double)`
Explanation. This method is usefull for the detector current only!

* `double GetMeasurementUncertainty(double, MeasurementRange)`
Explanation. This method is usefull for the detector current only!

* `string Query(string)`
This is a low level, yet powerful method to communicate with the instrument. Will be made private in future releases of the library!
 
### Properties

* `InstrumentManufacturer`
Returns the string "Gigahertz-Optik".

* `InstrumentType`
Returns a string of the instrument designation.

* `InstrumentSerialNumber`
Returns the unique serial number of the instrument as a string. Usually this number is different from the serial number printed on the instrument case.

* `InstrumentFirmwareVersion`
Explanation.

* `InstrumentID`
Returns a combination of the previous properties which unambiguous identifies the instrument (hopefully).

* `DevicePort`
The port as passed to the constructor.

* `DetectorID`
Returns a string identifing the connected detector head.

* `PhotometricUnit`
Returns the symbol of the measurement unit. This unit is used for the value returned by `GetPhotometricValue()`. Error-prone! This method will be removed in future releases!

## Notes

All properties are getters only.

Once instantiated, it is not possible to modify the object's `DevicePort`. However swaping  instruments on the same port can work. Properties like `InstrumentID` will reflect the actual instrument.


