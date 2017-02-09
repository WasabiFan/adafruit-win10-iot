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
using Adafruit.BNO055;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BNO055Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BNO055IMU Imu;
        public MainPage()
        {
            this.InitializeComponent();
            Imu = new BNO055IMU();
        }

        private void Timer_Tick(object sender, object e)
        {
            Vector v = Imu.ReadVector(VectorType.Euler);
            System.Diagnostics.Debug.WriteLine($"{v.X},\t {v.Y},\t {v.Z}");
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Imu.Initialize();

            DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(0.01) };
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }
    }
}
