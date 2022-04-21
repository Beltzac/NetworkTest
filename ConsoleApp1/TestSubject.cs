using System;

namespace ConsoleApp1
{
    public partial class Program
     {
        public class TestSubject
        {
            public TestSubject(string uriString)
            {
                this.Uri = new Uri(uriString);
            }

            public Uri Uri { get; set; }
        }

    }
}
