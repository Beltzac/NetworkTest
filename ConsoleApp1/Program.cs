using DNS.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace ConsoleApp1
{
     public class Program
     {
        public class Teste
        {
            public Teste(string url, params string[] protocolos)
            {
                this.url = url;
                this.protocolos = protocolos;
            }

            public string[] protocolos { get; set; }
            public string url { get; set; }
        }

        static async Task Main(string[] args)
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                ) {
                    return true;
                };

            Teste[] testes = { 
                new Teste("www.facebook.com", "http", "https"),
                new Teste("www.google.com", "http", "https"),
                new Teste("consul.tcp.qa", "http", "https"),
                new Teste("rabbitmq.tcp.qa:15672", "http"),
                new Teste("api.tcp.qa/vmi/swagger/index.html", "http", "https"),
                new Teste("api.tcp.com.br/vmi/swagger/index.html", "http", "https"),
                new Teste("portal.tcp.com.br", "http", "https"),
                new Teste("portal.tcp.qa", "http"),
                new Teste("reefer.tcp.qa", "http"),
                new Teste("inspecaokbt.tcp.qa", "http"),
                new Teste("nexus.tcp.com.br", "http"),
                new Teste("srvoracledbdev01.tcp.com.br:1521", "oracledb"),
                new Teste("mongodb.tcp.qa:27017", "mongodb"),
                new Teste("dashboard.tcp.qa", "https"),
            };


            foreach(var u in testes)
            {
                Console.WriteLine("Testando: " + u.url, Color.Aquamarine);
                
                await Task.WhenAll(
                    Task.Run(() => Ping(u)),
                    Task.Run(() => UrlIsValidAsync(u)),
                    Task.Run(() => DnsAsync(u))
                );
                Console.WriteLine();
            }
            Console.WriteLine("Finished!", Color.Aquamarine);
            Console.ReadLine();
        }

        private static async Task DnsAsync(Teste url)
        {
      
                var servers = GetDnsAdresses(true, false);

                foreach(var server in servers)
                {
                try
                {
                    DnsClient client = new DnsClient(server);

                    // Create request bound to 8.8.8.8
                    ClientRequest request = client.Create();

                    // Returns a list of IPs
                    Uri u = new Uri("http://" + url.url);
                    IList<IPAddress> ips = await client.Lookup(u.Host);

                    Console.WriteLine($"Resposta do DNS {server}: ips {string.Join(",", ips.Select(x => x.ToString()))}",  Color.Green);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro DNS {server}:" + ex.Message, Color.Red);
                }
            }



      
        }

        private static void Ping(Teste url)
        {
            Ping p = new Ping();
            PingReply r;
            string s;

            try
            {
                var uri = new Uri("http://" + url.url);
                r = p.Send(uri.Host, 5000);

                if (r.Status == IPStatus.Success)
                {
                    Console.WriteLine("Ping to " + url.ToString() + "[" + r.Address.ToString() + "]" + " Successful"
                       + " Response delay = " + r.RoundtripTime.ToString() + " ms", Color.Green);
                }
                else
                {
                    Console.WriteLine("Erro ping:" + r.Status, Color.Red);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ping:" + ex.Message, Color.Red);
            }
        }

        public static async Task UrlIsValidAsync(Teste url)
        {
            await Task.WhenAll( url.protocolos.Select(x => Task.Run(() => UrlIsValid(x + "://" + url.url))));
        }

        public static bool UrlIsValid(string url)
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Timeout = 5000; //set the timeout to 5 seconds to keep the user from waiting too long for the page to load
                request.Method = "HEAD"; //Get only the header information -- no need to download any content

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    int statusCode = (int)response.StatusCode;
                    if (statusCode >= 100 && statusCode < 400) //Good requests
                    {
                        Console.WriteLine(String.Format("{0} retornou {1}", url, statusCode), Color.Green);
                        return true;
                    }
                    else if (statusCode >= 500 && statusCode <= 510) //Server Errors
                    {
                        //log.Warn(String.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                        Console.WriteLine(String.Format("The remote server has thrown an internal error. Url is not valid: {0}", url), Color.Red);
                        return false;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError) //400 errors
                {
                    return false;
                }
                else
                {
                    Console.WriteLine(String.Format("Unhandled status [{0}] returned for url: {1}", ex.Status, url), ex, Color.Red);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Could not test url {0}.", url), ex, Color.Red);
            }
            return false;
        }

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

        //public static async Task<bool> LogicalAny(this IEnumerable<Task<bool>> tasks)
        //{
        //    var remainingTasks = new HashSet<Task<bool>>(tasks);
        //    while (remainingTasks.Any())
        //    {
        //        var next = await Task.WhenAny(remainingTasks);
        //        if (next.Result)
        //            return true;
        //        remainingTasks.Remove(next);
        //    }
        //    return false;
        //}
    }
}
