using Microsoft.Extensions.DependencyInjection;

namespace Harmonic.Hosting
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection services);

    }
}