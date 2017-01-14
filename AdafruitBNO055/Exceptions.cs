using System;

namespace Adafruit.BNO055
{
    public class IMUNotConnectedException : InvalidOperationException
    {
        public IMUNotConnectedException(string message) : base(message)
        {
        }

        public IMUNotConnectedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class IMUCommunicationException : Exception
    {
        public IMUCommunicationException()
        {
        }

        public IMUCommunicationException(string message) : base(message)
        {
        }

        public IMUCommunicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}