using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Adafruit.BNO055
{
    public class BNO055IMU
    {
        private readonly AddressMode IMUAddress;
        private readonly string I2CFriendlyName;
        private const byte BNO055_ID = 0xA0;

        I2cDevice DeviceConnection = null;

        public BNO055IMU(string i2cFriendlyName = "I2C1", AddressMode imuAddress = AddressMode.AddressA)
        {
            this.I2CFriendlyName = i2cFriendlyName;
            this.IMUAddress = imuAddress;
        }

        public async Task Initialize(int numRetries = 1, int retryDelay = 1000)
        {
            var i2cSettings = new I2cConnectionSettings((byte)IMUAddress);
            i2cSettings.BusSpeed = I2cBusSpeed.StandardMode;

            string DeviceSelector = I2cDevice.GetDeviceSelector(I2CFriendlyName);
            var I2CDeviceControllers = await DeviceInformation.FindAllAsync(DeviceSelector);
            if (I2CDeviceControllers.Count <= 0)
                throw new IMUCommunicationException("No valid I2C controllers were found");

            DeviceConnection = await I2cDevice.FromIdAsync(I2CDeviceControllers.First().Id, i2cSettings);

            for (int AttemptIndex = 0; ReadByte(BNO055Register.BNO055_CHIP_ID_ADDR) != BNO055_ID; AttemptIndex++)
            {
                await Task.Delay(retryDelay);
                if (AttemptIndex >= numRetries)
                    throw new IMUCommunicationException("The IMU didn't properly respond over the I2C bus. Confirm that it is properly"
                        + " connected and that the supplied I2C bus name is correct.");
            }

            // This is the default mode, but setting it manually ensures proper operation
            await SetOperationMode(OperationMode.Config);

            // Reset the sensor and wait for it to wake up
            WriteByte(BNO055Register.BNO055_SYS_TRIGGER_ADDR, 0x20);
            await Task.Delay(1000);
            while (ReadByte(BNO055Register.BNO055_CHIP_ID_ADDR) != BNO055_ID)
            {
                await Task.Delay(40);
            }
            await Task.Delay(50);

            
            await SetPowerMode(PowerMode.Normal);

            WriteByte(BNO055Register.BNO055_PAGE_ID_ADDR, 0);

            /* Set the output units */
            /*
            uint8_t unitsel = (0 << 7) | // Orientation = Android
                              (0 << 4) | // Temperature = Celsius
                              (0 << 2) | // Euler = Degrees
                              (1 << 1) | // Gyro = Rads
                              (0 << 0);  // Accelerometer = m/s^2
            write8(BNO055_UNIT_SEL_ADDR, unitsel);
            */
            WriteByte((byte)BNO055Register.BNO055_SYS_TRIGGER_ADDR, 0x0);
            await Task.Delay(10);

            /* Set the requested operating mode (see section 3.3) */
            await SetOperationMode(OperationMode.NineDegsOfFreedom);
            await Task.Delay(10);
        }

        private void AssertConnected()
        {
            if (DeviceConnection == null)
                throw new IMUNotConnectedException("An attempt was made to communicate with the sensor before the"
                    + $" sensor was initialized. Call {nameof(Initialize)}() before reading or writing data.");
        }

        public Quaternion GetOrientationQuaternion()
        {
            AssertConnected();

            byte[] I2CBuffer = new byte[8];
            WriteRaw(new byte[] { (byte)BNO055Register.BNO055_QUATERNION_DATA_W_LSB_ADDR });
            ReadRaw(I2CBuffer);

            short x, y, z, w;
            
            w = (short)(I2CBuffer[1] << 8 | I2CBuffer[0]);
            x = (short)(I2CBuffer[3] << 8 | I2CBuffer[2]);
            y = (short)(I2CBuffer[5] << 8 | I2CBuffer[4]);
            z = (short)(I2CBuffer[7] << 8 | I2CBuffer[6]);
            
            const double Scale = (1.0 / (1 << 14));
            return new Quaternion(Scale * w, Scale * x, Scale * y, Scale * z);
        }


        public IMUReading GetNew9DOFReading()
        {
            return new IMUReading(
                ReadCalibration(),
                ReadTemp(),
                ReadVector(VectorType.Accelerometer),
                ReadVector(VectorType.Euler),
                ReadVector(VectorType.Gravity),
                ReadVector(VectorType.Gyroscope),
                ReadVector(VectorType.LinearAccel),
                ReadVector(VectorType.Magnetometer));

        }

        public CalibrationData ReadCalibration()
        {
            return new CalibrationData(ReadByte((byte)BNO055Register.BNO055_CALIB_STAT_ADDR));
        }

        public async Task SetOperationMode(OperationMode newMode)
        {
            WriteByte((byte)BNO055Register.BNO055_OPR_MODE_ADDR, (byte)newMode);
            await Task.Delay(30);
        }

        public async Task SetPowerMode(PowerMode newMode)
        {
            WriteByte((byte)BNO055Register.BNO055_PWR_MODE_ADDR, (byte)newMode);
            await Task.Delay(30);
        }

        public int ReadTemp()
        {
            return ReadByte((byte)BNO055Register.BNO055_TEMP_ADDR);
        }

        public Vector ReadVector(VectorType type)
        {
            AssertConnected();

            byte[] I2CBuffer = new byte[6];
            WriteRaw(new byte[] { (byte)type });
            ReadRaw(I2CBuffer);

            Vector RawVector = new Vector(
                I2CBuffer[0] | (I2CBuffer[1] << 8),
                I2CBuffer[2] | (I2CBuffer[3] << 8),
                I2CBuffer[4] | (I2CBuffer[5] << 8));

            /* Convert the value to an appropriate range (section 3.6.4) */
            /* and assign the value to the Vector type */

            switch (type)
            {
                case VectorType.Magnetometer:
                    /* 1uT = 16 LSB */
                    return RawVector / 16.0;
                case VectorType.Gyroscope:
                    /* 1rps = 900 LSB */
                    return RawVector / 900.0;
                case VectorType.Euler:
                    /* 1 degree = 16 LSB */
                    return RawVector / 16.0;
                case VectorType.Accelerometer:
                case VectorType.LinearAccel:
                case VectorType.Gravity:
                    /* 1m/s^2 = 100 LSB */
                    return RawVector / 100.0;
            }

            throw new ArgumentException($"Invalid vector type {type}", nameof(type));
        }

        public byte ReadByte(byte addr)
        {
            AssertConnected();

            byte[] I2CBuffer = new byte[20];

            WriteRaw(new byte[] { addr });
            ReadRaw(I2CBuffer);

            return I2CBuffer[0];
        }

        public byte ReadByte(BNO055Register register)
        {
            return ReadByte((byte)register);
        }

        public void WriteByte(byte addr, byte data)
        {
            AssertConnected();

            byte[] I2CBuffer = new byte[2];
            I2CBuffer[0] = addr;
            I2CBuffer[1] = data;
            WriteRaw(I2CBuffer);
        }

        public void WriteByte(BNO055Register register, byte data)
        {
            WriteByte((byte)register, data);
        }

        private void WriteRaw(byte[] data)
        {
            try
            {
                DeviceConnection.Write(data);
            }
            catch (FileNotFoundException e)
            {
                throw new IMUCommunicationException("An error occurred while communicating whith the device.", e);
            }
        }

        private void ReadRaw(byte[] buffer)
        {
            try
            {
                DeviceConnection.Read(buffer);
            }
            catch (FileNotFoundException e)
            {
                throw new IMUCommunicationException("An error occurred while communicating whith the device.", e);
            }
        }
    }
}
