namespace MailClient.Tests.Common
{
    internal class TrackingReader : TextReader
    {
        private readonly Queue<string> _lines;
        public bool IsDisposed { get; private set; }
        public TrackingReader(IEnumerable<string> lines)
        {
            _lines = new Queue<string>(lines);
        }
        public override ValueTask<string> ReadLineAsync(CancellationToken ct)
        {
            if(_lines.Count == 0) return ValueTask.FromResult<string>(null);
            return ValueTask.FromResult(_lines.Dequeue());
        }
        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}