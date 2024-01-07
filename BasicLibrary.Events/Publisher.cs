namespace BasicLibrary.Events;

public abstract class Publisher<TArg> : IPublisher<TArg> where TArg : EventArgs
{
    event EventHandler<TArg> Event;

    protected void Publish(TArg args)
    {
        Event?.Invoke(this, args);
    }
    public void Subscribe(Subscriber<TArg> subscriber)
    {
        if (Event is not null)
            Event += subscriber.OnEvent!;
    }
    public void Unsubscribe(Subscriber<TArg> subscriber)
    {
        if (Event is not null)
            Event -= subscriber.OnEvent!;
    }
}