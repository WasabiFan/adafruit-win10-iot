using Adafruit.PCA9685;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PCA9685Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        PCA9685PWMBreakout PwmBreakout;
        double SinInput = 0;
        public MainPage()
        {
            this.InitializeComponent();
            PwmBreakout = new PCA9685PWMBreakout();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await PwmBreakout.Initialize();
            await PwmBreakout.SetFrequency(1600);

            DispatcherTimer Timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            Timer.Tick += (s, evt) => PwmBreakout.SetPwm(0, (float)(Math.Sin(SinInput += 0.1) / 2 + 0.5));
            Timer.Start();
        }
    }
}
