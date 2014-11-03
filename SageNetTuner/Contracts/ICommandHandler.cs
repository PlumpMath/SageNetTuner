namespace SageNetTuner.Contracts
{
    public interface ICommandHandler
    {
        bool CanHandle(string request);

        void Handle(string request);
    }
}
