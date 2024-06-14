using System.Globalization;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using NAudio.Wave;

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

        var grammarBuilder = new GrammarBuilder();
        if (wakeupWord.Length > 0)
            grammarBuilder.Append(wakeupWord);
        grammarBuilder.Culture = culture;
        grammarBuilder.Append(choices);

        _recognizer.RequestRecognizerUpdate();
        _recognizer.LoadGrammar(new Grammar(grammarBuilder));

        var wakeupWordLength = wakeupWord.Length;

        // Attach event handlers.
        _recognizer.SpeechRecognized += (_, e) =>
        {
            CommandRecognized?.Invoke(this, new RecognitionResult
            {
                Text = e.Result.Text[wakeupWordLength..], Score = e.Result.Confidence,
            });
        };

        _recognizer.RecognizeCompleted += (_, _) => { _isRecognizing = false; };

        // Configure the input to the recognizer.
        _recognizer.SetInputToDefaultAudioDevice();
    }

    private SpeechStreamer? _audioStreamer;

    private WaveInEvent? _waveInEvent;
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

    public void SelectDevice(string selectedValue)
    {
        for (var i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var device = WaveInEvent.GetCapabilities(i);
            if (device.ProductName != selectedValue)
                continue;

            if (_waveInEvent != null)
            {
                _waveInEvent.Dispose();
                _waveInEvent = null;
            }

            _waveInEvent = new WaveInEvent();
            _waveInEvent.DeviceNumber = i; //device.ID;
            _waveInEvent.DataAvailable += (_, args) =>
            {
                _audioStreamer?.Write(args.Buffer);
                //AudioStream.WriterStream.Write(args.Buffer);
            };

            _audioStreamer = new SpeechStreamer(2 * 1024 * 1024);
            SupportedWaveFormat supportFormat = 0;
            foreach (SupportedWaveFormat format in Enum.GetValues(typeof(SupportedWaveFormat)))
                if (device.SupportsWaveFormat(format))
                {
                    supportFormat = format;

                    break;
                }

            var samplesRate = 0;
            var nChannel = 0;
            var bitsPerSample = 0;
            switch (supportFormat)
            {
                case SupportedWaveFormat.WAVE_FORMAT_1M08: //11.025 kHz, Mono, 8-bit
                    nChannel = 1;
                    bitsPerSample = 8;
                    samplesRate = 11025;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_1S08: //     11.025 kHz, Stereo, 8-bit
                    nChannel = 2;
                    bitsPerSample = 8;
                    samplesRate = 11025;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_1M16: //     11.025 kHz, Mono, 16-bit
                    nChannel = 1;
                    bitsPerSample = 16;
                    samplesRate = 11025;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_1S16: //     11.025 kHz, Stereo, 16-bit
                    nChannel = 2;
                    bitsPerSample = 16;
                    samplesRate = 11025;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_2M08: //     22.05 kHz, Mono, 8-bit
                    nChannel = 1;
                    bitsPerSample = 8;
                    samplesRate = 22050;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_2S08: //     22.05 kHz, Stereo, 8-bit
                    nChannel = 2;
                    bitsPerSample = 8;
                    samplesRate = 22050;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_2M16: //     22.05 kHz, Mono, 16-bit
                    nChannel = 1;
                    bitsPerSample = 16;
                    samplesRate = 22050;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_2S16: //     22.05 kHz, Stereo, 16-bit
                    nChannel = 2;
                    bitsPerSample = 16;
                    samplesRate = 22050;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_4M08: //     44.1 kHz, Mono, 8-bit
                    nChannel = 1;
                    bitsPerSample = 8;
                    samplesRate = 44100;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_4S08: //     44.1 kHz, Stereo, 8-bit
                    nChannel = 2;
                    bitsPerSample = 8;
                    samplesRate = 44100;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_4M16: //     44.1 kHz, Mono, 16-bit
                    nChannel = 1;
                    bitsPerSample = 16;
                    samplesRate = 44100;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_4S16: //     44.1 kHz, Stereo, 16-bit
                    nChannel = 2;
                    bitsPerSample = 16;
                    samplesRate = 44100;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_48M08: //     48 kHz, Mono, 8-bit
                    nChannel = 1;
                    bitsPerSample = 8;
                    samplesRate = 48000;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_48S08: //     48 kHz, Stereo, 8-bit
                    nChannel = 2;
                    bitsPerSample = 8;
                    samplesRate = 48000;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_48M16: //     48 kHz, Mono, 16-bit
                    nChannel = 1;
                    bitsPerSample = 16;
                    samplesRate = 48000;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_48S16: //     48 kHz, Stereo, 16-bit
                    nChannel = 2;
                    bitsPerSample = 16;
                    samplesRate = 48000;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_96M08: //     96 kHz, Mono, 8-bit
                    nChannel = 1;
                    bitsPerSample = 8;
                    samplesRate = 96000;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_96S08: //     96 kHz, Stereo, 8-bit
                    nChannel = 2;
                    bitsPerSample = 8;
                    samplesRate = 96000;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_96M16: //     96 kHz, Mono, 16-bit
                    nChannel = 1;
                    bitsPerSample = 16;
                    samplesRate = 96000;
                    break;
                case SupportedWaveFormat.WAVE_FORMAT_96S16: //     96 kHz, Stereo, 16-bit
                    nChannel = 2;
                    bitsPerSample = 16;
                    samplesRate = 96000;
                    break;
            }

            _waveInEvent.WaveFormat = new WaveFormat(samplesRate, bitsPerSample, nChannel);
            _waveInEvent.StartRecording();

            var shouldStart = _isRecognizing;
            if (_isRecognizing)
                Stop();

            _recognizer.SetInputToAudioStream(_audioStreamer, new SpeechAudioFormatInfo(nChannel * samplesRate, bitsPerSample == 8 ? AudioBitsPerSample.Eight : AudioBitsPerSample.Sixteen, nChannel == 2 ? AudioChannel.Stereo : AudioChannel.Mono));

            if (shouldStart)
                Start();

            break;
        }
    }
}
