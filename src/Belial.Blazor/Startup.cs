using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Belial.Blazor
{
    public class Startup
    {
        static Startup()
        {
            // Work around an issue loading json.net in page: https://github.com/mono/mono/issues/11848
            typeof(System.Collections.Specialized.INotifyCollectionChanged).GetHashCode();
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
