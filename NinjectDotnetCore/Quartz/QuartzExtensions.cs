using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ninject;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace NinjectDotnetCore.Quartz
{
    public static class QuartzExtensions
    {
        public static void UseQuartz(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<IScheduler>();
        }

        public static async void AddQuartz(this IServiceCollection services, IKernel kernel)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }


            var props = new NameValueCollection
            {
                {"quartz.serializer.type", "binary"}
            };
            var factory = new StdSchedulerFactory(props);
            var scheduler = await factory.GetScheduler();

            //var jobFactory = new IoCJobFactory(services.BuildServiceProvider());
            var jobFactory = new IoCJobFactory(kernel);
            scheduler.JobFactory = jobFactory;
            await scheduler.Start();
            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<HelloJob>()
                .WithIdentity("job1", "group1")
                .Build();

            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(5)
                    .RepeatForever())
                .Build();

            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, trigger);


            //kernel.Bind<IScheduler>().ToConstant(scheduler);
            //services.AddSingleton(scheduler);
        }
    }
}
