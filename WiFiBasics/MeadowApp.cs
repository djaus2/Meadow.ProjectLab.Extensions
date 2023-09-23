using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;
using Meadow.Gateway.WiFi;
using Meadow.Hardware;
using ProjectLab_Demo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace WiFi_Basics
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        private const string WIFI_NAME = "[SSID]";
        private const string WIFI_PASSWORD = "[PASSWORD]";

        private List<string> accessPoints = new List<string>();
        
        private const string pwds = "billy1z9971,alexander,pilo6t,hoqmer234,Marj678,fred2020,harr9y191,23456789,qwerty765,fisvhy89,Qasdf345";
        private List<string> passwords = new List<string>();


        private DisplayController displayController;
        private MicroAudio audio;
        private IProjectLabHardware projLab;

        static IWiFiNetworkAdapter wifi;

        public override Task Initialize()
        {
            passwords = new List<string> ( pwds.Split(new char[] { ',' }) );
            Resolver.Log.LogLevel = Meadow.Logging.LogLevel.Trace;

            Resolver.Log.Info("Initialize hardware...");

            //==== instantiate the project lab hardware
            projLab = ProjectLab.Create();
            

            Resolver.Log.Info($"Running on ProjectLab Hardware {projLab.RevisionString}");

            projLab.Speaker.SetVolume(0.5f);
            audio = new MicroAudio(projLab.Speaker);

            //---- display controller (handles display updates)
            if (projLab.Display is { } display)
            {
                Resolver.Log.Trace("Creating DisplayController");
                displayController = new DisplayController(display);
                Resolver.Log.Trace("DisplayController up");
            }

            displayController.InitMenu(projLab, audio);

            //---- heartbeat
            Resolver.Log.Info("Initialization complete");

            return base.Initialize();
        }


        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            // connected event test.
            wifi.NetworkConnected += WiFiAdapter_NetworkConnected;

            // enumerate the public WiFi channels
            await ScanForAccessPoints(wifi);

            try
            {
                // connect to the wifi network.
                Resolver.Log.Info($"Connecting to WiFi Network {WIFI_NAME}");

                await wifi.Connect(WIFI_NAME, WIFI_PASSWORD, TimeSpan.FromSeconds(45));
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect: {ex.Message}");
            }
            if (wifi.IsConnected)
            {
                DisplayNetworkInformation();

                while (true)
                {
                    await GetWebPageViaHttpClient("https://postman-echo.com/get?foo1=bar1&foo2=bar2");
                }
            }
        }

        void WiFiAdapter_NetworkConnected(INetworkAdapter networkAdapter, NetworkConnectionEventArgs e)
        {
            Resolver.Log.Info("Connection request completed");
        }

        async Task ScanForAccessPoints(IWiFiNetworkAdapter wifi)
        {
            Resolver.Log.Info("Getting list of access points");
            var networks = await wifi.Scan(TimeSpan.FromSeconds(60));

            if (networks.Count > 0)
            {
                Resolver.Log.Info("|-------------------------------------------------------------|---------|");
                Resolver.Log.Info("|         Network Name             | RSSI |       BSSID       | Channel |");
                Resolver.Log.Info("|-------------------------------------------------------------|---------|");

                displayController.Update();

                accessPoints = new List<string>();
                int i = 1;
                foreach (WifiNetwork accessPoint in networks)
                {
                    accessPoints.Add(accessPoint.Ssid);
                    Resolver.Log.Info($"| {i, -4}. {accessPoint.Ssid,-28} | {accessPoint.SignalDbStrength,4} | {accessPoint.Bssid,17} |   {accessPoint.ChannelCenterFrequency,3}   |");
                }

                accessPoints = accessPoints.OrderBy(x => x).ToList();
                displayController.SetMenu(accessPoints,GetPassword);
            }
            else
            {
                Resolver.Log.Info($"No access points detected");
            }
        }

        public void DisplayNetworkInformation()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            if (adapters.Length == 0)
            {
                Resolver.Log.Warn("No adapters available");
            }
            else
            {
                foreach (NetworkInterface adapter in adapters)
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    Resolver.Log.Info("");
                    Resolver.Log.Info(adapter.Description);
                    Resolver.Log.Info(string.Empty.PadLeft(adapter.Description.Length, '='));
                    Resolver.Log.Info($"  Adapter name: {adapter.Name}");
                    Resolver.Log.Info($"  Interface type .......................... : {adapter.NetworkInterfaceType}");
                    Resolver.Log.Info($"  Physical Address ........................ : {adapter.GetPhysicalAddress()}");
                    Resolver.Log.Info($"  Operational status ...................... : {adapter.OperationalStatus}");

                    string versions = string.Empty;

                    if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        versions = "IPv4";
                    }

                    if (adapter.Supports(NetworkInterfaceComponent.IPv6))
                    {
                        if (versions.Length > 0)
                        {
                            versions += " ";
                        }
                        versions += "IPv6";
                    }

                    Resolver.Log.Info($"  IP version .............................. : {versions}");

                    if (adapter.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        IPv4InterfaceProperties ipv4 = properties.GetIPv4Properties();
                        Resolver.Log.Info($"  MTU ..................................... : {ipv4.Mtu}");
                    }

                    if ((adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) || (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                    {
                        foreach (UnicastIPAddressInformation ip in adapter.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                Resolver.Log.Info($"  IP address .............................. : {ip.Address}");
                                Resolver.Log.Info($"  Subnet mask ............................. : {ip.IPv4Mask}");
                            }
                        }
                    }
                }
            }
        }

        int AccessPointIndex = -1;
        void  GetPassword(int ap)
        {
            AccessPointIndex = ap;
            passwords = new List<string>(pwds.Split(new char[] { ',' }));
            passwords = passwords.OrderBy(x => x).ToList();
            displayController.SetMenu(passwords,null, ConnectAP);
        }
        

        async Task ConnectAP(int pwdIndx)
        {
            int ap = AccessPointIndex;
            if ((ap > -1) && (ap < accessPoints.Count))
            {
                string accessPoint = accessPoints[ap];
                if ((pwdIndx > -1) && (pwdIndx < passwords.Count))
                {
                    string pwd = passwords[pwdIndx];
                    await wifi.Connect(accessPoint, pwd, TimeSpan.FromSeconds(45));
                    if (wifi.IsConnected)
                    {
                        await audio.PlayGameSound(GameSoundEffect.SecretFound);
                        DisplayNetworkInformation();
                        while (true)
                        {
                            await GetWebPageViaHttpClient("https://postman-echo.com/get?foo1=bar1&foo2=bar2");
                        }
                    }
                    else
                    {
                        await audio.PlayGameSound(GameSoundEffect.Warning);
                    }
                }
                else
                {
                    await audio.PlayGameSound(GameSoundEffect.Explosion);
                }
            }
            else
            {
                await audio.PlayGameSound(GameSoundEffect.EnemyDeath);
            }
        }


        public async Task GetWebPageViaHttpClient(string uri)
        {
            Resolver.Log.Info($"Requesting {uri} - {DateTime.Now}");

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 5, 0);

                HttpResponseMessage response = await client.GetAsync(uri);

                try
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Resolver.Log.Info(responseBody);
                }
                catch (TaskCanceledException)
                {
                    Resolver.Log.Info("Request time out.");
                }
                catch (Exception e)
                {
                    Resolver.Log.Info($"Request went sideways: {e.Message}");
                }
            }
        }
    }
}
