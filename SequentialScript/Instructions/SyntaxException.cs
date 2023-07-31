using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript.Instructions
{
    public sealed class SyntaxException : FormatException
    {

        public string OriginalMessage { get; set; }
        public int Line { get; }
        public int Pos { get; }


        public SyntaxException(string message) : this(message, -1, -1) { }

        public SyntaxException(string message, int line) : this(message, line, -1) { }

        public SyntaxException(string message, int line, int pos) : this(message, line, pos, null) { }

        public SyntaxException(string message, int line, int pos, Exception innerException) :
            base(GetMessage(message, line, pos), innerException)
        {
            this.OriginalMessage = message;
            this.Line = line;
            this.Pos = pos;
        }

        static string GetMessage(string message, int line, int pos)
        {
            string result;

            if (line == -1)
            {
                result = message;
            }
            else
            {
                var builder = new System.Text.StringBuilder(message);

                builder.Append($" Line {line}");
                if (pos == -1)
                {
                    builder.Append(".");
                }
                else
                {
                    builder.Append($"; pos: {pos}.");
                }
                result = builder.ToString();
            }
            return result;
        }

    }
}
