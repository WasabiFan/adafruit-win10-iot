using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Adafruit.PCA9685
{
    public class PCA9685PWMBreakout
    {
        private readonly byte Address;
        private readonly string I2CFriendlyName;

        I2cDevice DeviceConnection = null;

        public PCA9685PWMBreakout(string i2cFriendlyName = "I2C1", byte breakoutAddress = 0x40)
        {
            I2CFriendlyName = i2cFriendlyName;
            Address = breakoutAddress;
        }

        public async void Initialize()
        {
            var i2cSettings = new I2cConnectionSettings(Address);
            i2cSettings.BusSpeed = I2cBusSpeed.StandardMode;

            string DeviceSelector = I2cDevice.GetDeviceSelector(I2CFriendlyName);
            var I2CDeviceControllers = await DeviceInformation.FindAllAsync(DeviceSelector);
            if (I2CDeviceControllers.Count <= 0)
                throw new Exception("No valid I2C controllers were found");

            DeviceConnection = await I2cDevice.FromIdAsync(I2CDeviceControllers.First().Id, i2cSettings);
            Reset();
        }

        public void Reset()
        {
            WriteByte(PCA9685Register.Mode1, 0x0);
        }

        public async void SetFrequency(float frequency)
        {
            // Correct for overshoot in provided frequency: https://github.com/adafruit/Adafruit-PWM-Servo-Driver-Library/issues/11
            frequency *= 0.9f;

            float PreScaleFloat = 25000000f / 4096f / frequency - 1;
            byte PreScaleByte = (byte)Math.Floor(PreScaleFloat + 0.5f);

            byte OldMode = ReadByte(PCA9685Register.Mode1);

            // Temporarily sleep
            byte SleepMode = (byte)((OldMode & 0x7F) | 0x10);
            WriteByte(PCA9685Register.Mode1, SleepMode);

            // Write prescale value
            WriteByte(PCA9685Register.Prescale, PreScaleByte);

            // Wake up
            WriteByte(PCA9685Register.Mode1, OldMode);
            await Task.Delay(5);

            // Set MODE1 register to turn on auto increment
            WriteByte(PCA9685Register.Mode1, (byte)(OldMode | 0xa1));
        }

        /// <summary>
        /// Writes raw values to the on and off registers of the given pin. Special values outside
        /// of the valid tick range are allowed as specified by the chip manufacturer, without any
        /// bounds checks.
        /// </summary>
        /// <seealso cref="SetPwmWaveform(byte, ushort, ushort, bool?)"/>
        /// <seealso cref="SetPwm(byte, float, float)"/>
        public void WriteRawPwmConfig(byte pinNumber, ushort on, ushort off)
        {
            AssertConnected();

            if (pinNumber > 15)
                throw new ArgumentOutOfRangeException(nameof(pinNumber), pinNumber, "The given pin must correspond to a pin number printed on the board. Valid values are 0-15, inclusive.");

            byte[] I2CBuffer = new byte[]
            {
                (byte)((byte)PCA9685Register.Led0OnL + 4 * pinNumber),
                (byte)on,
                (byte)(on >> 8),
                (byte)off,
                (byte)(off >> 8)
            };
            DeviceConnection.Write(I2CBuffer);
        }

        /// <summary>
        /// Sets the desired PWM waveform on the specified pin.
        /// 
        /// The PCA9685 uses a 4096-tick-long cycle counter to define where the pin switches states.
        /// By specifying "on" and "off" times, you are defining the point in that cycle at which
        /// the pin becomes high or low. For example, if you set an "on" timestamp of 2048 and an
        /// "off" timestamp of 4095, the pin would turn on roughly half way through the cycle and 
        /// turn off at the end.
        /// 
        /// If overrideToBinary is non-null, the pin will be configured to always send the suppplied
        /// value (true for high, false for low). If <code>overrideToBinary</code> is
        /// <code>true</code>, the value from <code>onTimestamp</code> will be used for the delay
        /// before first setting the pin high.
        /// </summary>
        /// 
        /// <param name="pinNumber">The pin number to configure</param>
        /// <param name="onTimestamp">The time within the 4096-tick-long cycle at which the pin should be set high. Valid values are 0-4095, inclusive.</param>
        /// <param name="offTimestamp">The time within the 4096-tick-long cycle at which the pin should be set low. Valid values are 0-4095, inclusive.</param>
        /// <param name="overrideToBinary">When non-null, defines the constant logic level to set, instead of a normal PWM signal.</param>
        /// <seealso cref="WriteRawPwmConfig(byte, ushort, ushort)"/>
        /// <seealso cref="SetPwm(byte, float, float)"/>
        public void SetPwmWaveform(byte pinNumber, ushort onTimestamp, ushort offTimestamp, bool? overrideToBinary = null)
        {
            if(onTimestamp > 4095)
                throw new ArgumentOutOfRangeException(nameof(onTimestamp), onTimestamp, "The given timestamps must be within the board's tick counter range. Valid values are 0-4095, inclusive.");

            if (offTimestamp > 4095)
                throw new ArgumentOutOfRangeException(nameof(offTimestamp), offTimestamp, "The given timestamps must be within the board's tick counter range. Valid values are 0-4095, inclusive.");

            if (overrideToBinary == true)
                WriteRawPwmConfig(pinNumber, (byte)(onTimestamp & (1 >> 12)), 0);
            else if (overrideToBinary == false)
                WriteRawPwmConfig(pinNumber, 0, (byte)(offTimestamp & (1 >> 12)));
            else
                WriteRawPwmConfig(pinNumber, onTimestamp, offTimestamp);
        }

        /// <summary>
        /// Sets the desired duty cycle on the specified pin.
        /// </summary>
        /// <param name="pinNumber">The pin number to configure</param>
        /// <param name="dutyCycle">A value in the range 0-1, inclusive, specifying the percentage of time that the pin is high.</param>
        /// <param name="outerDeadZone">A threshold applied to the extremes of the valid duty cycle range within which the PWM signal is overridden to be always-on or always-off.</param>
        /// <seealso cref="SetFrequency(float)"/>
        /// <seealso cref="SetPwmWaveform(byte, ushort, ushort, bool?)"/>
        void SetPwm(byte pinNumber, float dutyCycle, float outerDeadZone = 10e-5f)
        {
            if (dutyCycle > 1 || dutyCycle < 0)
                throw new ArgumentOutOfRangeException(nameof(dutyCycle), dutyCycle, "The given duty cycle value must be in the range 0-1, inclusive.");

            if (dutyCycle <= outerDeadZone)
                SetPwmWaveform(pinNumber, 0, 0, false);
            else if (dutyCycle >= 1 - outerDeadZone)
                SetPwmWaveform(pinNumber, 0, 0, true);
            else
                SetPwmWaveform(pinNumber, 0, (ushort)Math.Floor(dutyCycle * 4095));
        }

        private void AssertConnected()
        {
            if (DeviceConnection == null)
                throw new InvalidOperationException("An attempt was made to communicate with the sensor before the"
                    + $" sensor was initialized. Call {nameof(Initialize)}() before reading or writing data.");
        }

        public byte ReadByte(byte addr)
        {
            AssertConnected();

            byte[] I2CBuffer = new byte[20];
            DeviceConnection.Write(new byte[] { addr });
            DeviceConnection.Read(I2CBuffer);
            return I2CBuffer[0];
        }

        public byte ReadByte(PCA9685Register register)
        {
            return ReadByte((byte)register);
        }

        public void WriteByte(byte addr, byte data)
        {
            AssertConnected();

            byte[] I2CBuffer = new byte[2];
            I2CBuffer[0] = addr;
            I2CBuffer[1] = data;
            DeviceConnection.Write(I2CBuffer);
        }

        public void WriteByte(PCA9685Register register, byte data)
        {
            WriteByte((byte)register, data);
        }
    }
}
