using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ninject;
using Ninject.Activation;
using Ninject.Infrastructure.Disposal;
using NinjectDotnetCore.Extensions;
using NinjectDotnetCore.Quartz;
using Quartz;

namespace NinjectDotnetCore
{
    public class Startup
    {
        private readonly AsyncLocal<Scope> scopeProvider = new AsyncLocal<Scope>();
        private IKernel _kernel { get; set; }

        private object Resolve(Type type) => _kernel.Get(type);
        private object RequestScope(IContext context) => scopeProvider.Value;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddRequestScopingMiddleware(() => scopeProvider.Value = new Scope());
            services.AddCustomControllerActivation(Resolve);
            services.AddCustomViewComponentActivation(Resolve);
            //services.AddScoped<HelloJob>();

            _kernel = new StandardKernel();
            services.AddQuartz(_kernel);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            RegisterApplicationComponents(app, loggerFactory);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private IKernel RegisterApplicationComponents(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            // IKernelConfiguration config = new KernelConfiguration();
            _kernel = new StandardKernel();

            // Register application services
            foreach (var ctrlType in app.GetControllerTypes())
            {
                _kernel.Bind(ctrlType).ToSelf().InScope(RequestScope);
            }

            _kernel.Bind<ITestService>().To<TestService>().InScope(RequestScope);
            //_kernel.Bind<HelloJob>().ToSelf().InTransientScope();

            // Cross-wire required framework services
            _kernel.BindToMethod(app.GetRequestService<IViewBufferScope>);
            

            return _kernel;
        }

        private sealed class Scope : DisposableObject { }
    }

    public static class BindingHelpers
    {
        public static void BindToMethod<T>(this IKernel config, Func<T> method) => config.Bind<T>().ToMethod(c => method());
    }
}
