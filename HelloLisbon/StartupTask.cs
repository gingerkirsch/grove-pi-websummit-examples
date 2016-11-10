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

namespace HelloLisbon
{
    public sealed class StartupTask : IBackgroundTask
    {
        ThreadPoolTimer timer;
        BackgroundTaskDeferral deferral;
        ILed led;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            led = DeviceFactory.Build.Led(Pin.DigitalPin4);
            timer = ThreadPoolTimer.CreatePeriodicTimer(this.Timer_Tick, TimeSpan.FromSeconds(1));
        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            try
            {
                led.ChangeState((led.CurrentState == SensorStatus.Off) ? SensorStatus.On : SensorStatus.Off);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.Write("Something happened");
                throw;
            }
        }
    }
}
