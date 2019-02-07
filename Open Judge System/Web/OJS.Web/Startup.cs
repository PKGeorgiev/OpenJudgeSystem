[assembly: Microsoft.Owin.OwinStartup(typeof(OJS.Web.Startup))]

namespace OJS.Web
{
    using Hangfire;
    using Microsoft.AspNet.SignalR;
    using OJS.Common;
    using OJS.Services.Data.Submissions;
    using OJS.Web.HangfireConfiguration;
    using OJS.Web.Infrastructure.Hubs;
    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);
            this.ConfigureHangfire(app);



                        GlobalHost.DependencyResolver.Register(
                            typeof(OjsHub),
                            () => new OjsHub(SimpleInjectorConfig.Container));
/*
            GlobalHost.DependencyResolver.Register(
                            typeof(SimpleInjector.Container),
                            () => SimpleInjectorConfig.Container);
*/            
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new SrOjsUserIdProvider());

            app.MapSignalR();
        }

        private void ConfigureHangfire(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            var options = new DashboardOptions
            {
                Authorization = new[] { new HangFireAuthenticationFilter(), }
            };
            app.UseHangfireDashboard("/hangfire", options);
        }
    }
}
