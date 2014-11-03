namespace SageNetTuner.Contracts
{
    using SageNetTuner.Model;

    public interface IRequestParser
    {
        RequestContext Parse(string request);
    }
}