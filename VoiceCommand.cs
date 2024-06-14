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

    public async void UseMic(int DeviceIndex)
    {
        //cleanup
        if (_waveInEvent != null)
        {
            _waveInEvent.StopRecording();
            _waveInEvent.Dispose();
        }
        if (_audioStreamer != null)
        {
            _audioStreamer.Close();
            _audioStreamer.Dispose();
        }
        
        //settings
        _waveInEvent = new WaveInEvent();
        _waveInEvent.DeviceNumber = DeviceIndex; //device.ID;
        var device = WaveInEvent.GetCapabilities(DeviceIndex);
        SupportedWaveFormat supportFormat = 0;
        foreach (SupportedWaveFormat format in Enum.GetValues(typeof(SupportedWaveFormat)))
            if (device.SupportsWaveFormat(format) && format <= SupportedWaveFormat.WAVE_FORMAT_48S16 && ((int)Math.Log2((int)format))%2 == 0)//找个单通道的
            {
                supportFormat = format;//取最后一个支持的。
            }
        
        var samplesRate = 0;
        var nChannel = 0;
        var bitsPerSample = 0;
        AudioBitsPerSample eBitsPerSample;
        AudioChannel eChannel;
        _waveInEvent.WaveFormat = GetWaveFormat(supportFormat, out samplesRate, out nChannel, out bitsPerSample,
            out eBitsPerSample, out eChannel);
        
        var audioStreamer = _audioStreamer = new SpeechStreamer(2 * 1024 * 1024);//2MB
        _waveInEvent.DataAvailable += (_, args) =>
        {
            if (audioStreamer.CanWrite)
                audioStreamer.Write(args.Buffer);//闭包获取。不访问_audioStreamer，防止写入错误streamer
        };
        
        var audioFormat = new SpeechAudioFormatInfo(nChannel * samplesRate, eBitsPerSample, eChannel);
        
        //start record microphone
        _waveInEvent.StartRecording();
        
        //restart _recognizer
        if (!_isRecognizing)
        {
            _recognizer.SetInputToAudioStream(_audioStreamer, audioFormat);
            Start();
        }
        else
        {
            Task.Run(() =>
            {
                Stop();
                while(_isRecognizing)
                    Thread.Sleep(100);
                
                _recognizer.SetInputToAudioStream(_audioStreamer, audioFormat);
       
                Start();
            });
        }
    }

    public void SelectDevice(string selectedValue)
    {
        for (var i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var device = WaveInEvent.GetCapabilities(i);
            if (device.ProductName != selectedValue)
                continue;
            UseMic(i);
            break;
        }
    }
}
