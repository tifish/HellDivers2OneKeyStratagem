using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace HellDivers2OneKeyStratagem;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        await AppSettings.LoadSettings();
        if (Settings.Locale == "")
            Settings.Locale = CultureInfo.CurrentCulture.Name;

        Localizer.Instance.Load();
        Localizer.Instance.SetLanguage(Settings.Locale);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}
