using System.Speech.Recognition;

namespace HellDivers2OneKeyStratagem;

public class VoiceCommand : IDisposable
{
    private readonly SpeechRecognitionEngine _recognizer = new();
    public List<string> Commands { get; } = [];

    public event EventHandler<string>? CommandRecognized;

    public VoiceCommand()
    {
        // Attach event handlers.
        _recognizer.SpeechRecognized += (_, e) =>
        {
            CommandRecognized?.Invoke(this, e.Result.Text);
        };
    }

    public void Start()
    {
        var choices = new Choices();
        choices.Add(Commands.ToArray());

        var gb = new GrammarBuilder();
        gb.Append(choices);

        var g = new Grammar(gb);
        _recognizer.LoadGrammar(g);

        // Configure the input to the recognizer.
        _recognizer.SetInputToDefaultAudioDevice();

        // Start asynchronous, continuous speech recognition.
        _recognizer.RecognizeAsync(RecognizeMode.Single);
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
