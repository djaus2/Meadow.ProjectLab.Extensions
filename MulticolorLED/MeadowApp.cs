using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DigitalInputPort
{
    // Grove LED connected to Grove UART socket
    // Grove Pushbutton connecetd to Grove Digital socket
    // Toggle LED with pushbutton presses
    public class MeadowApp : App<F7CoreComputeV2>
    {
        private List<IDigitalInputPort> inputs = new List<IDigitalInputPort>();
        private List<IDigitalOutputPort> outputs = new List<IDigitalOutputPort>();

        private IDigitalOutputPort outputLEDBlue;
        private IDigitalOutputPort outputLEDGreen;
        private IDigitalOutputPort outputLEDRed;
        private IAnalogInputPort analogIn;
        private int LEDindx;

        public override Task Initialize()
        {
            LEDindx = 0;
            outputLEDBlue = Device.CreateDigitalOutputPort(Device.Pins.D11, true);
            outputLEDGreen = Device.CreateDigitalOutputPort(Device.Pins.D10, false);
            outputLEDRed = Device.CreateDigitalOutputPort(Device.Pins.D09, false);
            outputs.Add(outputLEDBlue);
            outputs.Add(outputLEDGreen);
            outputs.Add(outputLEDRed);


            TimeSpan debounceDuration = TimeSpan.FromMilliseconds(20);;
            var pushButton = Device.Pins.D16.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, ResistorMode.ExternalPullDown);
            pushButton.DebounceDuration = debounceDuration;
            pushButton.Changed += OnStateChangedHandler;
            inputs.Add(pushButton);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            // Display the current input states
            // The general idea here is that you have a floating, internally pulled high,  internally pulled low, extranlly pulled low or external pulled high.
            // The Grove LED is externally pulled low. See the Grove-Red LED Schematic linked at https://www.seeedstudio.com/Grove-Red-LED.html
            {
                var line1 = string.Join(" ", inputs.Select(i => i.Pin.Name).ToArray());
                var line2 = string.Join(" ", inputs.Select(i => $" {(i.State ? 1 : 0)} ").ToArray());

                Resolver.Log.Info(line1);
                Resolver.Log.Info(line2 + "\n");

                await Task.Delay(2000);
            }
        }

        private void OnStateChangedHandler(object sender, DigitalPortResult e)
        {
            var port = sender as IDigitalInputPort;

            if (port == null)
            {
                Resolver.Log.Info($"sender is a {port.GetType().Name}");
            }
            else
            {
                Resolver.Log.Info($"{port.Pin.Name} Pushbutton  state changed to {e.New.State}");
                if (e.New.State)
                {
                    // Switch LED when pressed
                    var pin = outputs[LEDindx];
                    pin.State = false;
                    Resolver.Log.Info($"{pin.Pin.Name} LED state changed to {pin.State}");

                    LEDindx++;
                    if (LEDindx >= outputs.Count())
                        LEDindx = 0;

                    pin = outputs[LEDindx];
                    pin.State = true;
                    Resolver.Log.Info($"{pin.Pin.Name} LED state changed to {pin.State}");


                }
            }
        }
    }
}