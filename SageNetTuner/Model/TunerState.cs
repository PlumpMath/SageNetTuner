namespace SageNetTuner.Model
{
    using System;
    using System.IO;

    public class TunerState
    {
        public TunerState()
        {
            IsRecording = false;
            Channel=new Channel();
        }

        public string Name { get; set; }

        public bool IsRecording { get; private set; }

        public string Filename { get; set; }
        public Channel Channel { get; set; }

        public DateTime StartDateTime { get; set; }

        public void RecordingStarted(string filename, Channel channel)
        {
            IsRecording = true;
            Filename = filename;
            Channel = channel;
        }

        public void RecordingStopped()
        {
            IsRecording = false;
            Filename = "";
            Channel = new Channel();

             
        }

        public override string ToString()
        {
            return string.Format("IsRecording={0}, Ch=({1}){2}, Filename={3}", IsRecording, Channel.GuideNumber, Channel.GuideName, Filename);
        }
    }
}