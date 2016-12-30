using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adafruit.PCA9685
{
    public enum PCA9685Register : byte
    {
        SubAddr1 = 0x2,
        SubAddr2 = 0x3,
        SubAddr3 = 0x4,

        Mode1 = 0x0,
        Prescale = 0xFE,

        Led0OnL = 0x6,
        Led0OnH = 0x7,
        Led0OffL = 0x8,
        Led0OffH = 0x9,

        AllLedOnL = 0xFA,
        AllLedOnH = 0xFB,
        AllLedOffL = 0xFC,
        AllLedOffH = 0xFD
    }
}
