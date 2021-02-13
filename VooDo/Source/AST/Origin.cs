﻿using System;

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

        public CodeOrigin(int _start, int _length, string _source, string? _sourcePath = null)
        {
            Start = _start;
            Length = _length;
            Source = _source;
            SourcePath = _sourcePath;
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

        public string? SourcePath { get; }
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
            _startColumn = Start - lastNewLine;
            _endLine = _startLine;
            for (int i = Start; i < End; i++)
            {
                if (Source[i] == '\n')
                {
                    _endLine++;
                    lastNewLine = i;
                }
            }
            _endColumn = End - lastNewLine;
        }

        public override string GetDisplayMessage()
        {
            string snippet = SourcePart;
            if (Length > 20)
            {
                snippet = snippet[0..10] + "…" + snippet[^10..];
            }
            GetLinePosition(out int startLine, out int startCol, out int endLine, out int endCol);
            string position;
            if (startLine == endLine)
            {
                position = $"line {startLine} ";
                if (Length < 2)
                {
                    position += $"column {startCol}";
                }
                else
                {
                    position += $"columns {startCol}…{endCol}";
                }
            }
            else
            {
                position = $"from line {startLine} column {startCol} to line {endLine} column {endCol}";
            }
            return (SourcePath is null ? "" : $"'{SourcePath}' ") + $"{position} «{snippet}»";
        }

    }

}
