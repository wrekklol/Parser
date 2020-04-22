using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Parser
{
    public class ParserColor
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public ParserColor(int R, int G, int B)
        {
            Red = R;
            Green = G;
            Blue = B;
        }

        public ParserColor() : this(0, 0, 0) { }



        //public static implicit operator Color(ParserColor p) => Color.FromArgb(p.Red, p.Green, p.Blue);
        //public static explicit operator ParserColor(Color c) => new ParserColor(c.R, c.G, c.B);
        public static implicit operator ParserColor(Color c) => new ParserColor(c.R, c.G, c.B);
        public static explicit operator Color(ParserColor p) => Color.FromArgb(p.Red, p.Green, p.Blue);



        public static bool operator ==(ParserColor Color1, ParserColor Color2)
        {
            if (ReferenceEquals(Color1, Color2))
                return true;
            if (ReferenceEquals(Color1, null))
                return false;
            if (ReferenceEquals(Color2, null))
                return false;

            return (Color1.Red == Color2.Red && Color1.Green == Color2.Green && Color1.Blue == Color2.Blue);
        }

        public static bool operator !=(ParserColor Color1, ParserColor Color2)
        {
            return !(Color1 == Color2);
        }

        public bool Equals(ParserColor Other)
        {
            if (ReferenceEquals(null, Other))
                return false;
            return ReferenceEquals(this, Other) || this == Other;
        }

        public override bool Equals(object InColor)
        {
            if (ReferenceEquals(null, InColor))
                return false;
            if (ReferenceEquals(this, InColor))
                return true;

            return InColor.GetType() == GetType() && Equals((ParserColor)InColor);
        }

        public override int GetHashCode()
        {
            return Red.GetHashCode() ^
                Green.GetHashCode() ^
                Blue.GetHashCode();
        }

        public override string ToString()
        {
            return $"[R={Red}, G={Green}, B={Blue}]";
        }
    }
}
