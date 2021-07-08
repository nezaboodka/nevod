namespace Nezaboodka.Nevod
{
    public struct TextRange
    {
        public int Start { get; set; }
        public int End { get; set; }

        public TextRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public bool IsEmpty => Start == End;
    }
}
