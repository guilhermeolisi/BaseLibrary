namespace BaseLibrary;

public interface IInteractionInvokeServices
{
    void SetInteractionInvoke(Func<char, object, Task<object?>>? interactionInvoke);
}
