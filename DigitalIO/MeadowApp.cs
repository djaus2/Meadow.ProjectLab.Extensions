using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private IDigitalOutputPort outputLED;
        private IAnalogInputPort analogIn;

        public override Task Initialize()
        {
            outputLED = Device.CreateDigitalOutputPort(Device.Pins.D00, true);
            outputs.Add(outputLED);

            analogIn = Device.CreateAnalogInputPort(Device.Pins.A00);

            TimeSpan debounceDuration = TimeSpan.FromMilliseconds(20);
            //var pushButton = Device.Pins.D16.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, ResistorMode.Disabled);
            //var pushButton = Device.Pins.D16.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            //var pushButton = Device.Pins.D16.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, ResistorMode.InternalPullDown);
            //var pushButton = Device.Pins.D16.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp);
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
                    // Toggle LED when pressed
                    var pin = outputLED; ;
                    pin.State = !pin.State;
                    Resolver.Log.Info($"{pin.Pin.Name} LED state changed to {pin.State}");

                    var res  = analogIn.Read().GetAwaiter();
                    var voltage = (Voltage)res.GetResult();
                    Resolver.Log.Info($"Voltage on A0: {voltage}");
                }
            }
        }
    }
}