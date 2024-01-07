namespace BasicLibrary.Events;

public interface IPublisher<TArg> where TArg : EventArgs
{
    public void Subscribe(Subscriber<TArg> subscriber);
    public void Unsubscribe(Subscriber<TArg> subscriber);
}