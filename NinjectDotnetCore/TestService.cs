using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NinjectDotnetCore
{
    public class TestService : ITestService
    {
        public int Data { get; private set; }

        public TestService()
        {
            Data = 42;
        }
    }

    public interface ITestService
    {
        int Data { get; }
    }
}
