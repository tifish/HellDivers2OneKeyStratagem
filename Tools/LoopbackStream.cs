using System.Collections.Concurrent;

public class LoopbackStream
{
    public Stream ReaderStream { get; }
    public Stream WriterStream { get; }

    private readonly BlockingCollection<byte[]> _buffer;

    public LoopbackStream()
    {
        _buffer = new BlockingCollection<byte[]>();
        ReaderStream = new ReaderStreamInternal(_buffer);
        WriterStream = new WriterStreamInternal(_buffer);
    }

    private class WriterStreamInternal : Stream
    {
        private readonly BlockingCollection<byte[]> _buffer;

        public WriterStreamInternal(BlockingCollection<byte[]> buffer)
        {
            _buffer = buffer;
            CanRead = false;
            CanWrite = true;
            CanSeek = false;
        }

        public override void Close()
        {
            _buffer.CompleteAdding();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var newData = new byte[count];
            Array.Copy(buffer, offset, newData, 0, count);
            _buffer.Add(newData);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }

    private class ReaderStreamInternal : Stream
    {
        private readonly BlockingCollection<byte[]> _buffer;
        private readonly IEnumerator<byte[]> _readerEnumerator;
        private byte[]? _currentBuffer;
        private int _currentBufferIndex;

        public ReaderStreamInternal(BlockingCollection<byte[]> buffer)
        {
            _buffer = buffer;
            CanRead = true;
            CanWrite = false;
            CanSeek = false;
            _readerEnumerator = _buffer.GetConsumingEnumerable().GetEnumerator();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _readerEnumerator.Dispose();
            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_currentBuffer == null)
            {
                var read = _readerEnumerator.MoveNext();
                if (!read)
                    return 0;
                _currentBuffer = _readerEnumerator.Current;
            }

            var remainingBytes = _currentBuffer.Length - _currentBufferIndex;
            var readBytes = Math.Min(remainingBytes, count);
            Array.Copy(_currentBuffer, _currentBufferIndex, buffer, offset, readBytes);
            _currentBufferIndex += readBytes;

            if (_currentBufferIndex == _currentBuffer.Length)
            {
                _currentBuffer = null;
                _currentBufferIndex = 0;
            }

            return readBytes;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}

class SpeechStreamer : Stream
{
    private AutoResetEvent? _writeEvent;
    private readonly List<byte> _buffer;
    private readonly int _buffersize;
    private int _readposition;
    private int _writeposition;
    private bool _reset;

    public SpeechStreamer(int bufferSize)
    {
        _writeEvent = new AutoResetEvent(false);
        _buffersize = bufferSize;
        _buffer = new List<byte>(_buffersize);
        for (var i = 0; i < _buffersize; i++)
            _buffer.Add(new byte());
        _readposition = 0;
        _writeposition = 0;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => -1L;

    public override long Position
    {
        get => 0L;
        set { }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return 0L;
    }

    public override void SetLength(long value)
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var i = 0;
        while (i < count && _writeEvent != null)
        {
            if (!_reset && _readposition >= _writeposition)
            {
                _writeEvent.WaitOne(100, true);
                continue;
            }

            buffer[i] = _buffer[_readposition + offset];
            _readposition++;
            if (_readposition == _buffersize)
            {
                _readposition = 0;
                _reset = false;
            }

            i++;
        }

        return count;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        for (var i = offset; i < offset + count; i++)
        {
            _buffer[_writeposition] = buffer[i];
            _writeposition++;
            if (_writeposition == _buffersize)
            {
                _writeposition = 0;
                _reset = true;
            }
        }

        _writeEvent?.Set();
    }

    public override void Close()
    {
        if (_writeEvent != null)
            _writeEvent.Close();
        _writeEvent = null;
        base.Close();
    }

    public override void Flush()
    {
    }
}
