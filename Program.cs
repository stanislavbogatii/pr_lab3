using System.Net;
using System.Text.Json;
using DnsClient;
using Entities;
using DotNetEnv;

class Program {

     private static string apikey;
    static async Task Main(string[] args)
    {
        Env.Load();
        apikey = Env.GetString("API_KEY") ?? "not_exist";

        while (true) {
            Console.WriteLine("1 - Get IPs by Domain");
            Console.WriteLine("2 - Get Domains by IP");
            Console.WriteLine("3 - Request with other DNS");
            Console.WriteLine("0 - Ext");
            Console.Write("Select option: ");
            string optionString = Console.ReadLine();
            if (!int.TryParse(optionString, out int option))
            {
                Console.WriteLine("Please enter a valid number.");
                continue;
            }

            var methsHashMap = new Dictionary<int, Func<Task>>
            {
                { 1, async () => await PrintIPsByDomain() },
                { 2, async () => await PrintDomainsByIP() },
                { 3, async () => await ResolveDomain() }
            };

            if (methsHashMap.ContainsKey(option))
            {
                await methsHashMap[option](); // Call the selected method
            }
            else
            {
                if (option == 0) return;
                Console.WriteLine("Invalid option selected.");
            }
            Console.WriteLine();
        }

    }

    static async Task PrintIPsByDomain() {
        Console.WriteLine("Enter domain name:");
        string domainName = Console.ReadLine();
        IPHostEntry hostEntry = Dns.GetHostEntry(domainName);
        Console.WriteLine($"Ip address of {domainName}: ");
        foreach (var ipAddress in hostEntry.AddressList) {
            Console.WriteLine(ipAddress);
        }
    }

    static async Task PrintDomainsByIP() {
        Console.WriteLine("Input IP address: ");
        string host = Console.ReadLine();
        string output = "json";
        string apiUrl = $"https://api.viewdns.info/reverseip/?host={host}&apikey={apikey}&output={output}";

        using HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var deserializedData = JsonSerializer.Deserialize<ApiResponse>(responseBody);
            if (deserializedData?.response?.domains != null)
            {
                Console.WriteLine($"Domains {host}:");
                foreach (var domain in deserializedData.response.domains)
                {
                    Console.WriteLine(domain.name);
                }
            }
            else
            {
                Console.WriteLine("Данные о доменах отсутствуют.");
            }
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }

static async Task ResolveDomain()
{
    Console.WriteLine("Enter domain: ");
    string domain = Console.ReadLine();
    Console.WriteLine("Enter DNS server: ");
    string dnsServer = Console.ReadLine();
    
    if (!IPAddress.TryParse(dnsServer, out IPAddress ipAddress))
    {
        Console.WriteLine($"Error: {dnsServer} is not a valid IP address.");
        return;
    }

    try
    {
        var lookup = new LookupClient(new IPEndPoint(ipAddress, 53));
        Console.WriteLine($"Sending request to DNS server: {dnsServer}");

        var aResult = await lookup.QueryAsync(domain, QueryType.A);

        var aRecords = aResult.Answers.ARecords();
        if (aRecords.Any())
        {
            foreach (var record in aRecords)
            {
                Console.WriteLine($"{domain} -> {record.Address}");
            }
        }
        else
        {
            Console.WriteLine($"No A records found for {domain} using DNS server {dnsServer}. The domain may not exist or the DNS server may not have information about it.");
        }
    }
    catch (DnsResponseException ex)
    {
        Console.WriteLine($"Error sending request to DNS: {ex.Message}. The DNS server may not have been able to resolve the domain.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}. Please check the DNS server address and try again.");
    }
}

}
