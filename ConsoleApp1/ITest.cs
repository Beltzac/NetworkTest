using System.Threading.Tasks;

namespace ConsoleApp1
{
    public partial class Program
    {
        public interface ITest
        {
            public Task<TestResult> Test();
        }
    }
}