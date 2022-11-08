using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using PowerArgs;
using Statik.Embedded;
using Statik.Files;
using Statik.Mvc;
using Statik.Web;

namespace Site
{
    class Program
    {
        private static IWebBuilder _webBuilder;
        private static string _contentDirectory = Directory.GetCurrentDirectory();

        static int Main(string[] args)
        {
            try
            {
                _webBuilder = Statik.Statik.GetWebBuilder();
                _webBuilder.RegisterMvcServices();
                _webBuilder.RegisterServices(services =>
                {
                    services.Configure<MvcRazorRuntimeCompilationOptions>(options => {
                        options.FileProviders.Clear();
                        options.FileProviders.Add(new Statik.Embedded.EmbeddedFileProvider(typeof(Program).Assembly, "Site.Resources"));
                        //options.FileProviders.Add(new PhysicalFileProvider("/Users/paul.knopf/git/electric-rose-events/src/Site/Resources"));
                    });
                });

                RegisterPages();
                RegisterResources();

                try
                {
                    Args.InvokeAction<Program>(args);
                }
                catch (ArgException ex)
                {
                    Console.WriteLine(ex.Message);
                    ArgUsage.GenerateUsageFromTemplate<Program>().WriteLine();
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }

        private static void RegisterResources()
        {
            //_webBuilder.RegisterFileProvider(new PhysicalFileProvider("/Users/paul.knopf/git/electric-rose-events/src/Site/Resources/wwwroot"));
            _webBuilder.RegisterFileProvider(new Statik.Embedded.EmbeddedFileProvider(typeof(Program).Assembly, "Site.Resources.wwwroot"));
            var staticDirectory = Path.Combine(_contentDirectory, "static");
            if (Directory.Exists(staticDirectory))
            {
                _webBuilder.RegisterDirectory(staticDirectory);
            }
        }

        private static void RegisterPages()
        {
            _webBuilder.RegisterMvc("/", new
            {
                controller = "Site",
                action = "Index"
            });
        }

        [ArgActionMethod, ArgIgnoreCase]
        public void Serve()
        {
            Console.WriteLine("serve");
            using (var host = _webBuilder.BuildWebHost(port: 8000))
            {
                host.Listen();
                Console.WriteLine("Listening on port 8000...");
                Console.ReadLine();
            }
        }

        public class BuildArgs
        {
            [ArgDefaultValue("output"), ArgShortcut("o")]
            public string Output { get; set; }
        }

        [ArgActionMethod, ArgIgnoreCase]
        public async Task Build(BuildArgs args)
        {
            using (var host = _webBuilder.BuildVirtualHost())
            {
                await Statik.Statik.ExportHost(host, args.Output);
            }
        }
    }
}