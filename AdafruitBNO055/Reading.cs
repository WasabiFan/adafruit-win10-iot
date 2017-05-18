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

        public readonly double X;
        public readonly double Y;
        public readonly double Z;

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

        public override string ToString() => $"({X}, {Y}, {Z})";
    }

    public struct Quaternion
    {
        public readonly double W;
        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        public Quaternion(double w, double x, double y, double z)
        {
            this.W = w;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector ToEuler()
        {
            double sqw = W * W;
            double sqx = X * X;
            double sqy = Y * Y;
            double sqz = Z * Z;

            double EulerX = Math.Atan2(2.0 * (X * Y + Z * W), (sqx - sqy - sqz + sqw));
            double EulerY = Math.Asin(-2.0 * (X * Z - Y * W) / (sqx + sqy + sqz + sqw));
            double EulerZ = Math.Atan2(2.0 * (Y * Z + X * W), (-sqx - sqy + sqz + sqw));
            
            return new Vector(EulerX, EulerY, EulerZ);
        }
    }

    public struct IMUReading
    {
        public IMUReading(
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
