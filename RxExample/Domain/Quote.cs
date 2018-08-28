using System;
using System.Collections.Generic;

namespace RxExample.Domain
{
    public class Quote : IEquatable<Quote>
    {
        public Quote(string optionCode)
        {
            OptionCode = optionCode;
        }

        public string  OptionCode { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Quote);
        }

        public bool Equals(Quote other)
        {
            return other != null &&
                   OptionCode == other.OptionCode;
        }

        public override int GetHashCode()
        {
            return 324117065 + EqualityComparer<string>.Default.GetHashCode(OptionCode);
        }
    }
}