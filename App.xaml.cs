using Microsoft.Extensions.DependencyInjection;
using Path.Services;
using Path.ViewModels;
using System.Windows;

namespace Path
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        public App()
        {
            // 配置依赖注入容器
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 注册服务
            services.AddSingleton<IPathDataService, XmlPathDataService>();
            services.AddSingleton<IDialogService, WpfDialogService>();
            services.AddSingleton<IStepImportService, StepImportService>();
            services.AddSingleton<ILoftService, LoftService>();
            services.AddSingleton<ICurveMergeService, CurveMergeService>();

            // 注册 ViewModels
            services.AddTransient<MainViewModel>();

            // 注册 Windows
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 从 DI 容器获取 MainWindow
            var mainWindow = _serviceProvider?.GetService<MainWindow>();
            mainWindow?.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
