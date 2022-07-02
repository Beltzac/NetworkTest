using System;
using System.Net;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public partial class Program
    {
        public class ProtocolTest : ITest
        {
            public ProtocolTest(TestSubject t)
            {
                T = t;
            }

            public TestSubject T { get; }

            public async Task<TestResult> Test()
            {
                try
                {
                    var request = WebRequest.Create(T.Uri) as HttpWebRequest;
                    request.Timeout =
                        5000; //set the timeout to 5 seconds to keep the user from waiting too long for the page to load
                    request.Method = "HEAD"; //Get only the header information -- no need to download any content

                    using var response = await request.GetResponseAsync() as HttpWebResponse;
                    var statusCode = (int)response.StatusCode;
                    if (statusCode >= 100 && statusCode < 400) //Good requests
                        return new TestResult
                        {
                            Grade = Grade.Success,
                            Message = $"{T.Uri.OriginalString} retornou {statusCode}"
                        };
                    if (statusCode >= 500 && statusCode <= 510) //Server Errors
                        return new TestResult
                        {
                            Grade = Grade.Error,
                            Message =
                                $"The remote server has thrown an internal error. Url is not valid: {T.Uri.OriginalString}"
                        };
                }
                catch (Exception ex)
                {
                    return new TestResult
                    {
                        Grade = Grade.Error,
                        Message = $"Could not test url {T.Uri.OriginalString}, {ex.Message}"
                    };
                }

                return new TestResult
                {
                    Grade = Grade.Error,
                    Message = $"Could not test url {T.Uri.OriginalString}"
                };
            }
        }
    }
}