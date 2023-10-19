using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    // Change F7CoreComputeV2 to F7FeatherV2 (or F7FeatherV1) for Feather boards
    public class MeadowApp : App<F7CoreComputeV2>
    {
        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            Resolver.Log.Info("Hello, Meadow Core-Compute!");



            return base.Run();
        }

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            return base.Initialize();
        }

        private static HttpClient? sharedClient = null;
        static async Task Main()
        {
            // https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient
            Console.WriteLine("Hello, World!");
            // HttpClient lifecycle management best practices:
            // https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines#recommended-use
            HttpClient sharedClient = new()
            {
                BaseAddress = new Uri("https://api.what3words.com"),
            };
            await GetAsync(sharedClient);
        }

        public class W3W
        {
            // Generated at https://json2csharp.com/#:~:text=Convert%20Json%20to%20C%23%20Classes%20Online%201%20Step,second%20editor%20and%20deserialize%20using%20the%20%27Root%27%20class.
            // From Json at after Example Result heading  at https://developer.what3words.com/public-api/docs#convert-to-3wa
            // Som emods were needed though wrt Squre class
            public class Coordinates
            {
                public double lng { get; set; }
                public double lat { get; set; }
            }


            public class Square
            {
                public Coordinates southwest { get; set; }
                public Coordinates northeast { get; set; }
            }


            public string country { get; set; }
            public Square square { get; set; }
            public string nearestPlace { get; set; }
            public Coordinates coordinates { get; set; }
            public string words { get; set; }
            public string language { get; set; }
            public object locale { get; set; }
            public string map { get; set; }
        }
        static async Task GetAsync(HttpClient httpClient)
        {
            using HttpResponseMessage response = await httpClient.GetAsync("v3/convert-to-3wa?coordinates=-37.75%2C144.9&key=OU4PTX8K");

            var xx = response.EnsureSuccessStatusCode();
            //.WriteRequestToConsole();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            //var jsn = await httpClient.GetFromJsonAsync<W3W>("v3/convert-to-3wa?coordinates=-37.75%2C144.9&key=OU4PTX8K");

            var json = JsonSerializer.Deserialize<W3W>(jsonResponse);
            Console.WriteLine($"{jsonResponse}\n");

        }
    }
}