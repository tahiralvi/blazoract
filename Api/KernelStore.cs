using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;

[assembly: FunctionsStartup(typeof(blazoract.Api.Startup))]

namespace blazoract.Api
{
    // To avoid interference across notebooks, we need a separate kernel instance per notebook. Set
    // up a cache in memory that holds a certain maximum number of instances, and evicts entries if
    // they become idle for longer than a certain period.

    public class KernelStore
    {
        private const int MaxEntries = 1024;
        private const int MaxIdleMinutes = 15;

        private MemoryCache _kernelsCache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = MaxEntries
        });

        private ConcurrentDictionary<string, CompositeKernel> _kernels
            = new ConcurrentDictionary<string, CompositeKernel>();

        public CompositeKernel GetKernelForNotebook(string notebookId)
        {
            return _kernelsCache.GetOrCreate(notebookId, entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(MaxIdleMinutes));
                entry.Size = 1;
                return new CompositeKernel()
                {
                    new CSharpKernel().UseDefaultFormatting().UseDotNetVariableSharing().UseNugetDirective()
                };
            });
        }
    }
}