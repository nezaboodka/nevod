namespace Nezaboodka.Nevod
{
    public struct TextRange
    {
        public int Start { get; }
        public int End { get; }

        public TextRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public bool IsEmpty => Start == End;
    }
}
