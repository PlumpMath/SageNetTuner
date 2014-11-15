namespace TestClient
{
    public interface IMessageBuilder
    {
        bool CanHandle(string verb);

        string Handle(object options);
    }
}