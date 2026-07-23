using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SlashBar.Modules;

public sealed class IpModule : IModule {


    private static readonly ArgCompletion[] Flags = [
        new("local", "Adresse IP locale (LAN)")
    ];


    public string Prefix => "ip";
    public string Name => "Adresse IP";
    public string Description => "Copie l'adresse IP (presse-papiers)";


    public void Execute(string argument) {
        argument = argument.Trim();

        if (argument.Equals("local", StringComparison.OrdinalIgnoreCase)) {
            ClipboardHelper.SetText(GetLocalIp());
            return;
        }

        if (argument.Length == 0) {
            ClipboardHelper.SetText(GetPublicIp());
            return;
        }
    }

    
    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) =>
        ModuleArgs.SuggestFlags(argument, Flags);


    private static string GetPublicIp() {
        using var http = new HttpClient();
        return http.GetStringAsync("https://api.ipify.org")
            .GetAwaiter().GetResult()
            .Trim();
    }


    private static string GetLocalIp() {
        
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces()) {

            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            if (ni.NetworkInterfaceType is NetworkInterfaceType.Loopback
                or NetworkInterfaceType.Tunnel)
                continue;
            
            foreach (var addr in ni.GetIPProperties().UnicastAddresses) {

                if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                    continue; // IPv4 only 
                
                if (IPAddress.IsLoopback(addr.Address))
                    continue;

                return addr.Address.ToString(); // ex. 192.168.1.42
            }
        }

        return "127.0.0.1"; // fallback
    }
}