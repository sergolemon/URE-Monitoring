using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Windows.ApplicationModel;
using Windows.Storage;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Extensions.Logging;
using URE.Activation;
using URE.Contracts.Services;
using URE.Services;
using URE.ViewModels;
using URE.Views;
using URE.Helpers;
using URE.Core.Models.Db;
using URE.ViewModels.Controls;
using URE.Services.Equipment;
using URE.Extensions;
using URE.Core.Repositories;
using URE.Core.Contracts.Services;
using URE.Core.Contracts.Repositories;
using URE.Core.Models;
using URE.Core.Services;
using URE.Core.Models.Identity;

namespace URE;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public App()
    {
        InitializeComponent();

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Package.Current.InstalledLocation.Path)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string systemDir = Environment.SystemDirectory;
        string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:";

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        UseSerilog((context, services, configuration) => configuration
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .WriteTo.File(new JsonFormatter(), $"{systemDrive}/ProgramData/URE/Logs/logs-.json", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)).
        ConfigureServices((context, services) =>
        {
            IConfigurationSection appSettings = configuration.GetSection("App");

            //Db
            services.AddSingleton(new MeteoDbContextFactory(appSettings["ConnectionString"]));
            services.AddDbContext<MeteoDbContext>(opts =>
            {
                opts.UseSqlServer(appSettings["ConnectionString"], x => x.MigrationsAssembly("URE.Core"));
            });

            string mapDbConnectionString = $"Data Source={systemDrive}/ProgramData/URE/map.db";
            services.AddSingleton(new MapTileDbContextFactory(mapDbConnectionString));

            //Identity
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<MeteoDbContext>();

            services.AddSingleton<ISignInManager, SignInManager>();

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IMeteoStreamService, MeteoStreamService>();

            // Core Services
            services.AddSingleton<ISampleDataService, SampleDataService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IMeteoStreamService, MeteoStreamService>();
            services.AddSingleton<IAlarmService, AlarmService>();

            services.AddSingleton<MapTileCacheService>();

            services.AddEquipment();

            if (Convert.ToBoolean(appSettings["IsDevelopment"]))
            {
                services.AddSingleton<IMeteoDataService, MeteoDataService>();
            }
            else
            {
                services.AddSingleton<IMeteoDataService, MeteoComDataService>();
            }

            // Repositories
            //services.AddSingleton<IGpsSettingsRepository, GpsSettingsRepository>();
            services.AddSingleton<IGMSettingsRepository, GmSettingsRepository>();
            services.AddSingleton<IMeteoDataRepository, MeteoDataRepository>();
            services.AddSingleton<IMeteoStreamRepository, MeteoStreamRepository>();
            services.AddSingleton<IMapTileRepository, MapTileRepository>();
            services.AddTransient<IGPSSettingsRepository, GPSSettingsRepository>();
            services.AddTransient<IMeteoSettingsRepository, MeteoSettingRepository>();

            services.AddSingleton<IUserIdentityRepository, UserIdentityRepository>();

            // Views and ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();
            services.AddTransient<EquipmentSettingsPage>();
            services.AddTransient<EquipmentSettingsVm>();
            services.AddTransient<HistoryPage>();
            services.AddTransient<HistoryVm>();
            services.AddTransient<UserSettingsPage>();
            services.AddTransient<UserSettingsVm>();
            services.AddTransient<AboutProgramPage>();
            services.AddTransient<AboutProgramVm>();

            // Configuration
            services.AddSingleton(configuration);
            services.Configure<AppSettings>(appSettings);
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

  
    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        ILogger<App> logger = App.GetService<ILogger<App>>();
        logger.LogError(e.Exception, "UnhandledException");
    }

    private async void Window_Closed(object sender, WindowEventArgs args)
    {
        ISignInManager signInManager = App.GetService<ISignInManager>();
        await signInManager.SignOutAsync();
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        MainWindow.Closed += Window_Closed;

        MeteoDbContext dbContext = App.GetService<MeteoDbContext>();
        if (!dbContext.Database.GetAppliedMigrations().Any())
        {
            dbContext.Database.EnsureDeleted();
        }

        dbContext.Database.Migrate();

        using MapTileDbContext mapTileDbContext = App.GetService<MapTileDbContextFactory>().CreateDbContext(null);
        mapTileDbContext.Database.Migrate();

        
        ISignInManager signInManager = App.GetService<ISignInManager>();
        await signInManager.Initialize();

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
