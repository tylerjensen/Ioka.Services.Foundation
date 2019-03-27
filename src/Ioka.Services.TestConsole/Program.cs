using Ioka.Services.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ioka.Services.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press Enter to launch test.");
            Console.ReadLine();
            try
            {
                var baseUrl = "https://localhost:44370/";
                var identityServerUrl = "https://localhost:44336/";
                Func<HttpClient> httpClientFactory = () => new HttpClient();
                var client = new Client.Client(baseUrl, httpClientFactory, identityServerUrl, "sam.smith", "testpwd", "test.api");
                if (null == client.LoginException)
                {
                    var result = await client.GetAsync<IEnumerable<string>>("api/sectest/");
                    foreach (var item in result)
                    {
                        Console.WriteLine(item);
                    }
                    Console.WriteLine($"Access Token: {client.TokenResponse.AccessToken}");
                    Console.WriteLine($"Expires In: {client.TokenResponse.ExpiresIn}");
                    Console.WriteLine($"Refresh Token: {client.TokenResponse.RefreshToken}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();
        }
    }
}
