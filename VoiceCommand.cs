using System.Globalization;
using System.Speech.Recognition;

namespace HellDivers2OneKeyStratagem;

public class VoiceCommand : IDisposable
{
    private readonly SpeechRecognitionEngine _recognizer;

    public event EventHandler<string>? CommandRecognized;

    public static List<string> GetInstalledRecognizers()
    {
        return SpeechRecognitionEngine.InstalledRecognizers().Select(r => r.Culture.Name).ToList();
    }

    public VoiceCommand(string cultureName, string wakeupWord, string[] commands)
    {
        _recognizer = new SpeechRecognitionEngine(CultureInfo.GetCultureInfo(cultureName));

        var choices = new Choices();
        choices.Add(commands);

        var gb = wakeupWord == ""
            ? new GrammarBuilder()
            : new GrammarBuilder(wakeupWord);
        gb.Append(choices);

        var g = new Grammar(gb);
        _recognizer.LoadGrammar(g);

        var wakeupWordLength = wakeupWord.Length;

        // Attach event handlers.
        _recognizer.SpeechRecognized += (_, e) =>
        {
            if (e.Result.Confidence > Settings.VoiceConfidence)
                CommandRecognized?.Invoke(this, e.Result.Text[wakeupWordLength..]);
        };

        _recognizer.RecognizeCompleted += (sender, args) => { _isRecognizing = false; };

        // Configure the input to the recognizer.
        _recognizer.SetInputToDefaultAudioDevice();
    }

    private bool _isRecognizing;

    public void Start()
    {
        if (_isRecognizing)
            return;

        // Start asynchronous, continuous speech recognition.
        _recognizer.RecognizeAsync(RecognizeMode.Multiple);

        _isRecognizing = true;
    }

    public void Stop()
    {
        _recognizer.RecognizeAsyncStop();
    }

    public void Dispose()
    {
        _recognizer.Dispose();
        GC.SuppressFinalize(this);
    }
}
