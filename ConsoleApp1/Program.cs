using DNS.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace ConsoleApp1
{
    public partial class Program
     {
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

            TestSubject[] testes = { 
                new TestSubject("http://www.facebook.com"),
                new TestSubject("https://www.facebook.com"),
                new TestSubject("http://www.google.com"),
                new TestSubject("https://www.google.com"),
                new TestSubject("https://consul.tcp.qa"),
                new TestSubject("http://rabbitmq.tcp.qa:15672"),
                new TestSubject("http://api.tcp.qa/vmi/swagger/index.html"),
                new TestSubject("https://api.tcp.qa/vmi/swagger/index.html"),
                new TestSubject("http://api.tcp.com.br/vmi/swagger/index.html"),
                new TestSubject("https://api.tcp.com.br/vmi/swagger/index.html"),
                new TestSubject("http://portal.tcp.com.br"),
                new TestSubject("https://portal.tcp.com.br"),
                new TestSubject("http://portal.tcp.qa"),
                new TestSubject("http://reefer.tcp.qa"),
                new TestSubject("http://inspecaokbt.tcp.qa"),
                new TestSubject("http://nexus.tcp.com.br"),
                new TestSubject("oracledb://srvoracledbdev01.tcp.com.br:1521"),
                new TestSubject("mongodb://mongodb.tcp.qa:27017"),
                new TestSubject("https://dashboard.tcp.qa"),
            };

            var servers = DNSTest.GetDnsAdresses(true, false);

            List<ITest> tasks = new List<ITest>();

            foreach(var t in testes)
            {
                tasks.Add(new PingTest(t));
                tasks.Add(new ProtocolTest(t));
                foreach(var s in servers)
                    tasks.Add(new DNSTest(t, s));
            }

            Console.WriteLine("Iniciando!", Color.Aquamarine);

            var remainingTasks = new HashSet<Task<TestResult>>();

            foreach(var t in tasks)
            {
                remainingTasks.Add(t.Test());
            }

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
