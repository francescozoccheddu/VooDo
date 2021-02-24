using System;

namespace VooDo.AST
{
    public abstract class Origin
    {

        public static Origin Unknown { get; } = new UnknownOrigin();

        private sealed class UnknownOrigin : Origin
        {

            internal UnknownOrigin() { }

            public override string GetDisplayMessage() => "Unknown origin";

        }

        public abstract string GetDisplayMessage();

    }

    public sealed class CodeOrigin : Origin
    {

        public CodeOrigin(int _start, int _length, string _source)
        {
            Start = _start;
            Length = _length;
            Source = _source;
            if (_start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_start));
            }
            if (_length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_length));
            }
            if (End >= _source.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(_length));
            }
        }

        public string Source { get; }
        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
        public string SourcePart => Source.Substring(Start, Length);
        public void GetLinePosition(out int _startLine, out int _startColumn, out int _endLine, out int _endColumn)
        {
            _startLine = 0;
            int lastNewLine = 0;
            for (int i = 0; i < Start; i++)
            {
                if (Source[i] == '\n')
                {
                    _startLine++;
                    lastNewLine = i;
                }
            }
            _startColumn = Start - lastNewLine - 1;
            _endLine = _startLine;
            for (int i = Start; i < End; i++)
            {
                if (Source[i] == '\n')
                {
                    _endLine++;
                    lastNewLine = i;
                }
            }
            _endColumn = End - lastNewLine - 1;
        }

        public override string GetDisplayMessage()
        {
            GetLinePosition(out int startLine, out int startCol, out int endLine, out int endCol);
            string position;
            if (startLine == endLine)
            {
                position = $"LN {startLine + 1} ";
                if (Length < 2)
                {
                    position += $"COL {startCol + 1}";
                }
                else
                {
                    position += $"COLs {startCol + 1} to {endCol + 1}";
                }
            }
            else
            {
                position = $"LN {startLine + 1} COL {startCol + 1} to LN {endLine + 1} COL {endCol + 1}";
            }
            return position;
        }

    }

}
