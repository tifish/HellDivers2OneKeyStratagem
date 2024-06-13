using System.Globalization;
using System.Speech.Recognition;

namespace HellDivers2OneKeyStratagem;

public class VoiceCommand : IDisposable
{
    public struct RecognitionResult
    {
        public string Text;
        public float Score;
    }
    private readonly SpeechRecognitionEngine _recognizer;

    public event EventHandler<RecognitionResult>? CommandRecognized;

    public static List<string> GetInstalledRecognizers()
    {
        return SpeechRecognitionEngine.InstalledRecognizers().Select(r => r.Culture.Name).ToList();
    }

    public VoiceCommand(string cultureName, string wakeupWord, string[] commands)
    {
        var culture = new CultureInfo(cultureName);
        _recognizer = new SpeechRecognitionEngine(culture);

        var choices = new Choices();
        choices.Add(commands);

        var grammarBuilder = wakeupWord == ""
            ? new GrammarBuilder()
            : new GrammarBuilder(wakeupWord);
        grammarBuilder.Culture = culture;
        grammarBuilder.Append(choices);

        _recognizer.LoadGrammar(new Grammar(grammarBuilder));

        var wakeupWordLength = wakeupWord.Length;

        // Attach event handlers.
        _recognizer.SpeechRecognized += (_, e) =>
        {
            CommandRecognized?.Invoke(this,new RecognitionResult { Text = e.Result.Text[wakeupWordLength..],Score = e.Result.Confidence});
            //if (e.Result.Confidence > Settings.VoiceConfidence)
                
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
