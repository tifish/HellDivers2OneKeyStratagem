using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Jeek.Avalonia.Localization;
using JeekTools;

namespace Helldivers2OneKeyStratagem;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        AsyncHelper.RunSync(AppSettings.Load);
        if (Settings.Locale == "")
            Settings.Locale = CultureInfo.CurrentCulture.Name;

        Localizer.SetLocalizer(new TabLocalizer(Path.Combine(AppSettings.DataDirectory, "Languages.tab")));
        Localizer.Language = Settings.Locale;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}
