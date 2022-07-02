using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Colorful;

namespace ConsoleApp1
{
    public partial class Program
    {
        private static async Task Main(string[] args)
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate { return true; };

            TestSubject[] testes =
            {
                new("http://www.facebook.com"),
                new("https://www.facebook.com"),
                new("http://www.google.com"),
                new("https://www.google.com"),
                new("https://consul.tcp.qa"),
                new("http://rabbitmq.tcp.qa:15672"),
                new("http://api.tcp.qa/vmi/swagger/index.html"),
                new("https://api.tcp.qa/vmi/swagger/index.html"),
                new("http://api.tcp.com.br/vmi/swagger/index.html"),
                new("https://api.tcp.com.br/vmi/swagger/index.html"),
                new("http://portal.tcp.com.br"),
                new("https://portal.tcp.com.br"),
                new("http://portal.tcp.qa"),
                new("http://reefer.tcp.qa"),
                new("http://inspecaokbt.tcp.qa"),
                new("http://nexus.tcp.com.br"),
                new("oracledb://srvoracledbdev01.tcp.com.br:1521"),
                new("mongodb://mongodb.tcp.qa:27017"),
                new("https://dashboard.tcp.qa")
            };

            var servers = DNSTest.GetDnsAdresses(true, false);

            var tasks = new List<ITest>();

            foreach (var t in testes)
            {
                tasks.Add(new PingTest(t));
                tasks.Add(new ProtocolTest(t));
                foreach (var s in servers)
                    tasks.Add(new DNSTest(t, s));
            }

            Console.WriteLine("Iniciando!", Color.Aquamarine);

            var remainingTasks = new HashSet<Task<TestResult>>();

            foreach (var t in tasks) remainingTasks.Add(t.Test());

            while (remainingTasks.Any())
            {
                var next = await Task.WhenAny(remainingTasks);

                Console.WriteLine(next.Result.Message, GradeToColor(next.Result.Grade));

                remainingTasks.Remove(next);
            }

            Console.WriteLine("Finished!", Color.Aquamarine);
            Console.ReadLine();
        }

        private static Color GradeToColor(Grade g)
        {
            switch (g)
            {
                case Grade.Success:
                    return Color.Green;
                case Grade.Error:
                    return Color.Red;
                case Grade.Warning:
                    return Color.Yellow;
                case Grade.NotTested:
                    return Color.Gray;
                default:
                    return Color.White;
            }
        }
    }
}