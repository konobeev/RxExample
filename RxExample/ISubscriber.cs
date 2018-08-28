using System;
using RxExample.Domain;

namespace RxExample
{
    interface ISubscriber
    {
        IObserver<Quote> InitializeObserver { get; }
        IObserver<Quote> FinalizeObserver { get; }
    }
}