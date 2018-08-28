using System;
using System.Collections.Generic;
using System.Linq;

namespace RxExample.Domain
{
    public class SnapshotUpdate : IEquatable<SnapshotUpdate>
    {
        public static Random rnd = new Random();
        public SnapshotUpdate(string currencyPair)
        {
            CurrencyPair = currencyPair;
            Timestamp = DateTime.Now;
            Quotes = Enumerable.Range(0, rnd.Next(0, 4)).Select(x => new Quote($"{CurrencyPair} - {x}")).ToList();

        }

        public string CurrencyPair { get; }
        public DateTime Timestamp { get; }
        public IReadOnlyList<Quote> Quotes{ get; private set; }

        public bool Equals(SnapshotUpdate other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CurrencyPair, other.CurrencyPair) && Timestamp.Equals(other.Timestamp);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SnapshotUpdate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((CurrencyPair != null ? CurrencyPair.GetHashCode() : 0) * 397) ^ Timestamp.GetHashCode();
            }
        }
    }
}