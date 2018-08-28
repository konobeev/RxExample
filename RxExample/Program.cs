using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using RxExample.Domain;

namespace RxExample
{
    class Program
    {
        public IObservable<SnapshotUpdate> SnapshotSource()
        {
            var xau = Observable.Interval(TimeSpan.FromMilliseconds(800)).Select(_ => new SnapshotUpdate("XAUUSD"));
            var xag = Observable.Interval(TimeSpan.FromMilliseconds(1200)).Select(_ => new SnapshotUpdate("XAGUSD"));
            var eur = Observable.Interval(TimeSpan.FromMilliseconds(300)).Select(_ => new SnapshotUpdate("EURUSD"));
            var jpy = Observable.Interval(TimeSpan.FromMilliseconds(1500)).Select(_ => new SnapshotUpdate("USDJPY"));
            return Observable.Merge(xau, xag, eur, jpy);
        }

        static void Main(string[] args)
        {
            var program = new Program();
            CompositeDisposable disposable = new CompositeDisposable();

            var subscriber = new Subscriber();
            disposable.Add(subscriber);

            var snapshots = 
            program.SnapshotSource()
                .ObserveOn(ThreadPoolScheduler.Instance)
                .GroupBy(x => x.CurrencyPair)
                .SelectMany(x =>
                {
                    var o = x.Sample(TimeSpan.FromSeconds(3));
                    disposable.Add(o.ObserveOn(ThreadPoolScheduler.Instance).Subscribe(new PriceObserver(x.Key, subscriber)));
                    return o;
                });

            var prices = subscriber.PriceObservable
                .Buffer(TimeSpan.FromSeconds(2))
                .Select(x => x.Distinct().ToList())
                .Select(x => new Wrapper(x));

            snapshots.SubscribeOn(ThreadPoolScheduler.Instance)
                .Select(x => new Wrapper(x))
                .Merge(prices)
                .Pace(TimeSpan.FromSeconds(1))
                .Subscribe(it => Console.WriteLine($"{DateTime.Now} - sending to ui {it.CurrencyPair} {it.Timestamp}"));

            Console.ReadLine();
            disposable.Dispose();
        }
    }

    internal class Subscriber : ISubscriber, IDisposable
    {
        private readonly Subject<Quote> initializeObserver = new Subject<Quote>();
        private readonly Subject<Quote> finalizeObserver = new Subject<Quote>();

        private readonly Subject<Price> priceObserver = new Subject<Price>();
        private readonly CompositeDisposable disposable = new CompositeDisposable();
        private readonly ConcurrentDictionary<string, IDisposable> dictionary = new ConcurrentDictionary<string, IDisposable>();

        public Subscriber()
        {
            disposable.Add(initializeObserver.Subscribe(OnPriceSubscribe));
            disposable.Add(finalizeObserver.Subscribe(OnFinalizePrice));
        }

        public IObserver<Quote> InitializeObserver => initializeObserver;
        public IObserver<Quote> FinalizeObserver => finalizeObserver;

        public IObservable<Price> PriceObservable => priceObserver.AsObservable();

        public void Dispose()
        {
            disposable.Dispose();
            initializeObserver?.Dispose();
            finalizeObserver?.Dispose();
            priceObserver?.Dispose();
        }

        private void OnPriceSubscribe(Quote quote)
        {
            dictionary.TryAdd(quote.OptionCode, Observable.Interval(TimeSpan.FromMilliseconds(300))
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Select(x => quote.OptionCode)
                .Select(x => new Price(x))
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .Subscribe(priceObserver));
        }

        private void OnFinalizePrice(Quote quote)
        {
        }
    }

    internal class PriceObserver : IObserver<SnapshotUpdate>
    {
        private readonly string key;
        private readonly ISubscriber subscriber;
        private SnapshotUpdate current;

        public PriceObserver(string key, ISubscriber subscriber)
        {
            this.key = key;
            this.subscriber = subscriber;
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(SnapshotUpdate value)
        {
            if (current != null)
            {
                var remove = current.Quotes.Except(value.Quotes);
                var insert = value.Quotes.Except(current.Quotes);
                foreach (var quote in insert)
                {
                    subscriber.InitializeObserver.OnNext(quote);
                }
                foreach (var quote in remove)
                {
                    subscriber.FinalizeObserver.OnNext(quote);
                }
            }
            current = value;
        }
    }
}