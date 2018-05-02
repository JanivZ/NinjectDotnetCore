using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NinjectDotnetCore.Quartz
{
    public class HelloJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("hello job " + DateTime.Now.ToString());

            return Task.FromResult(new object());
        }
    }
}
