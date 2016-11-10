using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using GrovePi;
using GrovePi.Sensors;
using GrovePi.I2CDevices;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Thingy
{
    public sealed class StartupTask : IBackgroundTask
    {
        IBuzzer buzzer;
        IButtonSensor button;
        ILed redLed;
        ILed blueLed;
        ILightSensor lightSensor;
        IRgbLcdDisplay display;
        const int ambientLightThreshold = 700;
        private int brightness;
        private int actualAmbientLight;
        private SensorStatus buttonState;
        private ThreadPoolTimer timer;
        private BackgroundTaskDeferral deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();
            buzzer = DeviceFactory.Build.Buzzer(Pin.DigitalPin2);
            button = DeviceFactory.Build.ButtonSensor(Pin.DigitalPin4);
            blueLed = DeviceFactory.Build.Led(Pin.DigitalPin5);
            redLed = DeviceFactory.Build.Led(Pin.DigitalPin6);
            lightSensor = DeviceFactory.Build.LightSensor(Pin.AnalogPin2);
            display = DeviceFactory.Build.RgbLcdDisplay();
            buttonState = SensorStatus.Off;
            try
            {
                display.SetBacklightRgb(255, 0, 0);
                display.SetText("Hey Web Summit");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write("Something happened: " + ex.ToString());
                throw;
            }
            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromMilliseconds(200));
        }

        private void Timer_Tick(ThreadPoolTimer timer)
        {
            try
            {
                if (button.CurrentState != buttonState)
                {
                    buttonState = button.CurrentState;
                    blueLed.ChangeState(buttonState);
                    buzzer.ChangeState(buttonState);
                }
                actualAmbientLight = lightSensor.SensorValue();
                if (actualAmbientLight < ambientLightThreshold)
                {
                    brightness = Map(ambientLightThreshold - actualAmbientLight, 0, ambientLightThreshold, 0, 255);
                }
                else {
                    brightness = 0;
                }
                redLed.AnalogWrite(Convert.ToByte(brightness));
                byte rgbVal = Convert.ToByte(brightness);
                display.SetBacklightRgb(rgbVal, rgbVal, 255);
                display.SetText(String.Format("Thingy\nLight: {0}", actualAmbientLight));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write("Something happened: " + ex.ToString());
                throw;
            }
        }

        private int Map(int v1, int v2, int ambientLightThreshold, int v3, int v4)
        {
            return (v1 - v2) * (v4 - v3) / (ambientLightThreshold - v2) + v3;
        }
    }
}
