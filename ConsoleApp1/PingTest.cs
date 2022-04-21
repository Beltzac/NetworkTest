﻿using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace ConsoleApp1
{
    public partial class Program
     {
        public class PingTest : ITest
        {
            public PingTest(TestSubject t)
            {
                T = t;
            }

            public TestSubject T { get; }

            public async Task<TestResult> Test()
            {
                Ping p = new Ping();
                PingReply r;
                string s;

                try
                {
              
                    r = await Task.Run(() => p.Send(T.Uri.Host, 5000));

                    if (r.Status == IPStatus.Success)
                    {
                        return new TestResult
                        {
                            Grade = Grade.Success,
                            Message = $"Ping to {T.Uri.OriginalString}[{r.Address}] Successful Response delay = {r.RoundtripTime} ms"
                        };
                    }
                    else
                    {
                        return new TestResult
                        {
                            Grade = Grade.Error,
                            Message = $"Erro ping: {r.Status}"
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new TestResult
                    {
                        Grade = Grade.Error,
                        Message = $"Erro ping: {ex.Message}"
                    };
                }
            }
        }

    }
}
