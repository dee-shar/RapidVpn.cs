# RapidVpn.cs
Mobile-API for [Rapid VPN](https://play.google.com/store/apps/details?id=com.rapidconn.android) a fast, free, easy-to-use VPN service! Enjoy a safe, secure, and anonymous internet connection from anywhere in the world

## Example
```cs
using System;
using RapidVpnApi;

namespace Application
{
    internal class Program
    {
        static async Task Main()
        {
            var api = new RapidVpn();
            await api.Register();
            string servers = await api.GetServers();
            Console.WriteLine(servers);
        }
    }
}
```
