using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adafruit.BNO055
{
    public struct CalibrationData
    {
        public CalibrationData(byte calibrationData)
        {
            System = (ushort)((calibrationData >> 6) & 0x03);
            Gyro = (ushort)((calibrationData >> 4) & 0x03);
            Accel = (ushort)((calibrationData >> 2) & 0x03);
            Mag = (ushort)(calibrationData & 0x03);
        }

        public readonly ushort System;
        public readonly ushort Gyro;
        public readonly ushort Accel;
        public readonly ushort Mag;
    }

    public struct Vector
    {
        public Vector(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z; 
        }

        public double X;
        public double Y;
        public double Z;

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector operator *(Vector a, Vector b)
        {
            return new Vector(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector operator *(Vector a, double c)
        {
            return new Vector(a.X * c, a.Y * c, a.Z * c);
        }

        public static Vector operator /(Vector a, Vector b)
        {
            return new Vector(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static Vector operator /(Vector a, double c)
        {
            return new Vector(a.X / c, a.Y / c, a.Z / c);
        }
    }

    public struct BNO055IMUReading
    {
        public BNO055IMUReading(
            CalibrationData calibration,
            int temperature,
            Vector acceleration,
            Vector eulerRotation,
            Vector gravity,
            Vector gyro,
            Vector linearAcceleration,
            Vector magnetometer)
        {
            this.Calibration = calibration;
            this.Temperature = temperature;
            this.Acceleration = acceleration;
            this.EulerRotation = eulerRotation;
            this.Gravity = gravity;
            this.Gyro = gyro;
            this.LinearAcceleration = linearAcceleration;
            this.Magnetometer = magnetometer;
        }

        public readonly CalibrationData Calibration;

        public readonly int Temperature;
        public readonly Vector Acceleration;
        public readonly Vector EulerRotation;
        public readonly Vector Gravity;
        public readonly Vector Gyro;
        public readonly Vector LinearAcceleration;
        public readonly Vector Magnetometer;
    }
}
