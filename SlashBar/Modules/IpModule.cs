using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SlashBar.Modules;

public sealed class IpModule : IModule {

    private static readonly ArgCompletion[] Flags = [
        new("local", "Local IP address (LAN)")
    ];


    public string Prefix => "ip";
    public string Name => "IP address";
    public string Description => "Copy an IP address to the clipboard";


    public ModuleResult Execute(string argument) =>
        ExecuteAsync(argument).GetAwaiter().GetResult();


    public async Task<ModuleResult> ExecuteAsync(string argument) {
        argument = argument.Trim();

        try {
            if (argument.Equals("local", StringComparison.OrdinalIgnoreCase)) {
                var ip = GetLocalIp();
                ClipboardHelper.SetText(ip);
                return ModuleResult.Copied(ip);
            }

            if (argument.Length == 0) {
                var ip = await GetPublicIpAsync();
                ClipboardHelper.SetText(ip);
                return ModuleResult.Copied(ip);
            }

            return ModuleResult.Error("Unknown option");
        }
        catch {
            return ModuleResult.Error("IP unavailable");
        }
    }


    public IReadOnlyList<ArgCompletion> SuggestCompletions(string argument) =>
        ModuleArgs.SuggestFlags(argument, Flags);


    private static async Task<string> GetPublicIpAsync() {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var ip = await http.GetStringAsync("https://api.ipify.org");
        return ip.Trim();
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
                    continue;

                if (IPAddress.IsLoopback(addr.Address))
                    continue;

                return addr.Address.ToString();
            }
        }

        return "127.0.0.1";
    }
}
