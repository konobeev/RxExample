using System;

namespace RxExample.Domain
{
    public class Price : IEquatable<Price>
    {
        public Price(string optionCode)
        {
            OptionCode = optionCode;
            Vol = 0m;
            Timestamp = DateTime.Now;
        }

        public string OptionCode { get; private set; }

        public decimal Vol { get; private set; }
        public DateTime Timestamp { get; }

        public bool Equals(Price other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(OptionCode, other.OptionCode);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Price) obj);
        }

        public override int GetHashCode()
        {
            return (OptionCode != null ? OptionCode.GetHashCode() : 0);
        }
    }
}