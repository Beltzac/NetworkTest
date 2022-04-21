using DNS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public partial class Program
    {
        public class DNSTest : ITest
        {
            public DNSTest(TestSubject t)
            {
                T = t;
            }

            public DNSTest(TestSubject t, IPAddress server) : this(t)
            {
                Server = server;
            }

            public TestSubject T { get; }
            public IPAddress Server { get; }

            public static IPAddress[] GetDnsAdresses(bool ip4Wanted, bool ip6Wanted)
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                HashSet<IPAddress> dnsAddresses = new HashSet<IPAddress>();

                foreach (NetworkInterface networkInterface in interfaces)
                {
                    if (networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                        foreach (IPAddress forAddress in ipProperties.DnsAddresses)
                        {
                            if ((ip4Wanted && forAddress.AddressFamily == AddressFamily.InterNetwork) || (ip6Wanted && forAddress.AddressFamily == AddressFamily.InterNetworkV6))
                            {
                                dnsAddresses.Add(forAddress);
                            }
                        }
                    }
                }

                return dnsAddresses.ToArray();
            }

            public async Task<TestResult> Test()
            {
    
                try
                {
                    DnsClient client = new DnsClient(Server);

                    // Create request bound to 8.8.8.8
                    //ClientRequest request = client.Create();

                    // Returns a list of IPs
                    IList<IPAddress> ips = await client.Lookup(T.Uri.Host);

                    return new TestResult
                    {
                        Grade = Grade.Success,
                        Message = $"Resposta do DNS {Server}: ips {string.Join(",", ips.Select(x => x.ToString()))}"
                    };
                }
                catch (Exception ex)
                {
                    return new TestResult
                    {
                        Grade = Grade.Error,
                        Message = $"Erro DNS {Server}:{ex.Message}"
                    };
                }
            }

        }
    }
}

