using System;

namespace Peaky
{
    public class Percentage : IComparable<Percentage>
    {
        private int value;

        public Percentage(int value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}%", value);
        }

        public int CompareTo(Percentage other)
        {
            return value.CompareTo(other.value);
        }
    }
}