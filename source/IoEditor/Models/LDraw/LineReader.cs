using System.IO;

namespace IoEditor.Models.LDraw
{
    internal class LineReader: IDisposable
    {
        private StreamReader _reader;
        private int _currentLine = 0;

        public int CurrentLine => _currentLine;


        public LineReader(Stream stream)
        {
            this._reader = new StreamReader(stream);
        }

        public string ReadLine()
        {
            return this._reader.ReadLine();
        }


        public bool TryReadLine(out string line)
        {
            line = this._reader.ReadLine();
            
            if (line != null)
            {
                _currentLine++;
                return true;
            }

            return false;
        }

        public bool CanRead => this._reader.Peek() >= 0;

        public void Dispose()
        {
            var r = Interlocked.Exchange(ref this._reader, null);
            if (r != null)
            {
                r.Dispose();
            }
        }
    }
}