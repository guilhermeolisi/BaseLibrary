

namespace BaseLibrary
{
    public interface IUIThreadServices
    {
        bool IsOnUIThread { get; }

        void RunOnUIThread(Action action);
        Task RunOnUIThreadAsync(Action action);
    }
}