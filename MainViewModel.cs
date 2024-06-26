using CommunityToolkit.Mvvm.ComponentModel;

namespace HellDivers2OneKeyStratagem;

public partial class MainViewModel : ObservableObject
{
    public static MainViewModel Instance { get; } = new();

    public MainViewModel()
    {
    }

    private int _isLoadingCounter;

    public bool IsLoading
    {
        get => _isLoadingCounter > 0;
        set
        {
            if (value)
                _isLoadingCounter++;
            else
                _isLoadingCounter--;
        }
    }

    public bool SettingsChanged;
    public bool KeySettingsChanged;
    public bool SpeechSettingsChanged;

    [ObservableProperty]
    private double _speechConfidence;

    partial void OnSpeechConfidenceChanged(double oldValue, double newValue)
    {
        if (IsLoading)
            return;

        Settings.VoiceConfidence = Math.Round(newValue, 3);
        SettingsChanged = true;
    }
}
