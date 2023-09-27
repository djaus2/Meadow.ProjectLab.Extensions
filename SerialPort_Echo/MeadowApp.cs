using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SerialPort_Echo
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        ISerialPort port;

        public override Task Initialize()
        {
            Resolver.Log.Info("Available serial ports:");
            foreach (var name in Device.PlatformOS.GetSerialPortNames())
            {
                Resolver.Log.Info($"  {name.FriendlyName}");
            }
            var serialPortName = Device.PlatformOS.GetSerialPortName("COM1");
            Resolver.Log.Info($"Using {serialPortName.FriendlyName}...");
            port = Device.CreateSerialPort(serialPortName, 115200);
            Resolver.Log.Info("\tCreated");
            port.Open();
            if (port.IsOpen)
            {
                Resolver.Log.Info("\tOpened");
            }
            else
            {
                Resolver.Log.Info("\tFailed to Open");
            }

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var buffer = new byte[1024];

            string msgIn = "Start typing";
            while (true)
            {
                
#if !EchoToTerminal
                string msgOut = "Hello Meadow!";
                Resolver.Log.Info($"Writing data: \"{msgOut}\"");
                port.Write(System.Text.Encoding.ASCII.GetBytes(msgOut));
                port.Write(new byte[] { 0 }); //Null terminate string
                port.Write(new byte[] { (byte)'\r',(byte)'\n',0 });//Make viewable in serial terminal.
#else
                if(!string.IsNullOrEmpty(msgIn))
                {
                    port.Write(System.Text.Encoding.ASCII.GetBytes(msgIn));
                    await Task.Delay(2000);
                }
                else
                {
                    await Task.Delay(200);
                }

#endif
                var dataLength = port.BytesToRead;
                var read = port.Read(buffer, 0, dataLength);
                if (read == 0)
                {
#if !EchoToTerminal
                    Resolver.Log.Info($"Read {read} bytes");
#else
                    msgIn = "";
#endif
                }
                else
                {
                    Resolver.Log.Info($"Read {read} bytes: {BitConverter.ToString(buffer, 0, read)}");
                    msgIn = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    Resolver.Log.Info($"Read {read} bytes. Received message: \"{msgIn}\"");;
                }
#if !EchoToTerminal
                await Task.Delay(2000);
#endif
            }
        }
    }
}