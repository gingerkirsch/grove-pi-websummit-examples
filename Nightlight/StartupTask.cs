using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using GrovePi;
using GrovePi.Sensors;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Nightlight
{
    public sealed class StartupTask : IBackgroundTask
    {
        ILed blueLed;
        ILightSensor sensor;
        const int ambientLightThreshold = 700;
        private int actualAmbientLight;
        private int brightness;
        ThreadPoolTimer timer;
        BackgroundTaskDeferral deferral;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            blueLed = DeviceFactory.Build.Led(Pin.DigitalPin4);
            sensor = DeviceFactory.Build.LightSensor(Pin.AnalogPin2);
            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(200));

        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            try
            {
                actualAmbientLight = sensor.SensorValue();
                if (actualAmbientLight < ambientLightThreshold)
                {
                    brightness = Map(ambientLightThreshold - actualAmbientLight, 0, ambientLightThreshold, 0, 255);
                }
                else
                {
                    brightness = 0;
                }
                blueLed.AnalogWrite(Convert.ToByte(brightness));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write("Something happened:" + ex.ToString());
                throw;
            }
            
        }

        private int Map(int v1, int v2, int ambientLightThreshold, int v3, int v4)
        {
            return (v1 - v2) * (v4 - v3) / (ambientLightThreshold - v2) + v3;
        }
    }
}
