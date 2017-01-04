# Adafruit Library Implementations for Windows 10 IoT Core

A set of **unofficial** libraries to interface with various Adafruit products, including their "Absolute
Orientation IMU" (BNO055) and PWM expansion boards, from Windows 10 IoT Core. This does not include
comprehensive coverage of their product lines or feature sets; while I have generally tried to implement
large amounts of their official interfaces, some advanced features may have been left out. Pull requests are
welcome!

While these were written specifically for the Adafruit-distributed breakout boards, they should work with the
base chips used in those boards as well.

## Notes on individual libraries

### Absolute Orientation IMU (BNO055 breakout board)

**Adafruit product page:** <https://www.adafruit.com/product/2472> <br/>
**Official library for Arduino:** <https://github.com/adafruit/Adafruit_BNO055> <br/>
**Chip datasheet:** <https://www.adafruit.com/datasheets/BST_BNO055_DS000_12.pdf> <br/>

While there are interfaces for setting all sensor-supported modes, certain ones (such as quaternion-based
rotation) don't have pre-written logic to utilize them in this library. If you want to use those modes, you
should use the raw `ReadByte`/`WriteByte` methods along with the `BNO055Register` enum.

Special thanks to Gunter Logemann and his [ROV10 project](https://github.com/glogemann/ROV10) for his
implementation of the BNO055 interface. Some of the base logic in this implementation is based on his.

### 16-Channel 12-bit PWM/Servo Driver (I<sup>2</sup>C interface)

**Adafruit product page:** <https://www.adafruit.com/products/815> <br/>
**Official library for Arduino:** <https://github.com/adafruit/Adafruit-PWM-Servo-Driver-Library> <br/>
**Chip datasheet:** <https://www.adafruit.com/datasheets/PCA9685.pdf> <br/>

The interface for the PWM board in this library is slightly different than that in the official Adafruit
package. While low-level interfaces are provided, this library exposes higher-level functions differently to
align with typical C# design practices. This should be considered when translating Arduino code into C# with
this library.
