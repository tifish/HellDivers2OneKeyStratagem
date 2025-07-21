using System.Globalization;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using NAudio.Wave;

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

    public VoiceCommand(string cultureName, string wakeUpWord, string[] commands)
    {
        var culture = new CultureInfo(cultureName);
        _recognizer = new SpeechRecognitionEngine(culture);

        var choices = new Choices();
        choices.Add(commands);

        var grammarBuilder = new GrammarBuilder();
        if (wakeUpWord.Length > 0)
            grammarBuilder.Append(wakeUpWord);
        grammarBuilder.Culture = culture;
        grammarBuilder.Append(choices);

        _recognizer.RequestRecognizerUpdate();
        _recognizer.LoadGrammar(new Grammar(grammarBuilder));

        var wakeUpWordLength = wakeUpWord.Length;

        // Attach event handlers.
        _recognizer.SpeechRecognized += (_, e) =>
        {
            CommandRecognized?.Invoke(this, new RecognitionResult
            {
                Text = e.Result.Text[wakeUpWordLength..],
                Score = e.Result.Confidence,
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

    public async Task Stop()
    {
        if (!_isRecognizing)
            return;

        _recognizer.RecognizeAsyncCancel();

        while (_isRecognizing)
            await Task.Delay(100);
    }

    public void Dispose()
    {
        if (_isRecognizing)
            _recognizer.RecognizeAsyncCancel();
        _recognizer.Dispose();

        _waveInEvent?.StopRecording();
        _waveInEvent?.Dispose();
        _audioStreamer?.Dispose();

        GC.SuppressFinalize(this);
    }

    private static WaveFormat GetWaveFormat(SupportedWaveFormat eFormat, out int samplesRate, out int nChannel,
        out int bitsPerSample, out AudioBitsPerSample eBitsPerSample, out AudioChannel eChannel)
    {
        switch (eFormat)
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
            default:
                nChannel = 1;
                bitsPerSample = 8;
                samplesRate = 11025;
                break;
        }

        eBitsPerSample = bitsPerSample == 8 ? AudioBitsPerSample.Eight : AudioBitsPerSample.Sixteen;
        eChannel = nChannel == 2 ? AudioChannel.Stereo : AudioChannel.Mono;
        return new WaveFormat(samplesRate, bitsPerSample, nChannel);
    }

    public async Task<bool> UseMic(int deviceIndex)
    {
        // settings
        var device = WaveInEvent.GetCapabilities(deviceIndex);
        SupportedWaveFormat supportFormat = 0;
        foreach (SupportedWaveFormat format in Enum.GetValues(typeof(SupportedWaveFormat)))
            if (device.SupportsWaveFormat(format) && format <= SupportedWaveFormat.WAVE_FORMAT_48S16 && (int)Math.Log2((int)format) % 2 == 0) //找个单通道的
                supportFormat = format; //取最后一个支持的。

        if (supportFormat == 0)
            return false;

        var isRecognizingBeforeChangingMic = _isRecognizing;
        if (_isRecognizing)
            await Stop();

        // cleanup
        _waveInEvent?.Dispose();

        if (_audioStreamer != null)
        {
            _audioStreamer.Close();
            await _audioStreamer.DisposeAsync();
        }

        _waveInEvent = new WaveInEvent();
        _waveInEvent.DeviceNumber = deviceIndex; // device.ID;
        _waveInEvent.WaveFormat = GetWaveFormat(
            supportFormat, out var samplesRate, out var nChannel, out _,
            out var eBitsPerSample, out var eChannel);

        var audioStreamer = _audioStreamer = new SpeechStreamer(2 * 1024 * 1024); //2MB
        _waveInEvent.DataAvailable += (_, args) =>
        {
            if (audioStreamer.CanWrite)
                audioStreamer.Write(args.Buffer); // 闭包获取。不访问_audioStreamer，防止写入错误streamer
        };

        var audioFormat = new SpeechAudioFormatInfo(nChannel * samplesRate, eBitsPerSample, eChannel);

        _waveInEvent.StartRecording();
        _recognizer.SetInputToAudioStream(_audioStreamer, audioFormat);

        // restart _recognizer
        if (isRecognizingBeforeChangingMic)
            Start();

        return true;
    }

    public async Task<bool> UseMic(string deviceName)
    {
        for (var i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var device = WaveInEvent.GetCapabilities(i);
            if (device.ProductName != deviceName)
                continue;

            return await UseMic(i);
        }

        return false;
    }
}
