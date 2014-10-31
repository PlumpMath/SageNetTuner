namespace SageNetTuner
{
    using SageNetTuner.Model;

    public interface ICaptureManager
    {

        void Start(Channel channel, string filename);

        void Stop();

        long GetFileSize();

    }
}