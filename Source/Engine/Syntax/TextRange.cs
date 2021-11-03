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
        
        public int Length => End - Start;
        
        public bool IsEmpty => Start == End;

        public override string ToString()
        {
            return $"Start: {Start}, End: {End}";
        }
    }
}
