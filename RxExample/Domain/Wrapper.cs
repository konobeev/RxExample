using System;
using System.Collections.Generic;
using System.Linq;

namespace RxExample.Domain
{
    public class Wrapper
    {
        private readonly SnapshotUpdate update;
        private readonly IList<Price> prices;

        public Wrapper(IList<Price> prices)
        {
            this.prices = prices;
        }

        public Wrapper(SnapshotUpdate update)
        {
            this.update = update;
        }

        public SnapshotUpdate Update => update;
        public IList<Price> Prices => prices;

        public DateTime Timestamp
        {
            get { return Update?.Timestamp ?? Prices.FirstOrDefault()?.Timestamp ?? DateTime.MinValue; }
        }

        public string CurrencyPair
        {
            get { return Update?.CurrencyPair ?? "PRICE " + string.Join("; ", Prices.Select(x => x.OptionCode)); }
        }
    }
}