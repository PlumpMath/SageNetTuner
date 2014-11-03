namespace SageNetTuner.Filters
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using NLog;

    using SageNetTuner.Model;

    public class PropertiesFilter : BaseFilter
    {
        public PropertiesFilter(Logger logger)
            : base(logger)
        {
        }

        protected override bool CanExecute(RequestContext context)
        {
            return (context.Command==CommandName.Properties);
        }

        protected override string OnExecute(RequestContext context)
        {
            var props = new List<string>();
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/available_channels=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/brightness=-1", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/broadcast_standard=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/contrast=-1", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/device_name=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/hue=-1", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/last_channel=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/saturation=-1", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/sharpness=-1", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/tuning_mode=Cable", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/tuning_plugin=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/tuning_plugin_port=0", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/video_crossbar_index=0", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/video_crossbar_type=1", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/audio_capture_device_name=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/broadcast_standard=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/capture_config=2050", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/default_device_quality=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/delay_to_wait_after_tuning=0", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/encoder_merit=0", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/fast_network_encoder_switch=false", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/forced_video_storage_path_prefix=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/last_cross_index=0", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/last_cross_type=1", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/live_audio_input=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/multicast_host=", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/never_stop_encoding=false", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/video_capture_device_name={1}", context.Settings.Tuner.Id, context.Settings.Tuner.Name));
            props.Add(string.Format(@"mmc/encoders/{0}/video_capture_device_num=0", context.Settings.Tuner.Id));
            props.Add(string.Format(@"mmc/encoders/{0}/video_encoding_params=Great", context.Settings.Tuner.Id));

            var response = new StringBuilder();
            response.AppendLine(props.Count.ToString(CultureInfo.InvariantCulture));
            foreach (var prop in props)
            {
                response.AppendLine(prop);
            }
            response.AppendLine("OK");

            return response.ToString();

        }


    }
}