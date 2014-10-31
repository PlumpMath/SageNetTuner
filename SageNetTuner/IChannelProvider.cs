using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SageNetTuner
{
    using SageNetTuner.Configuration;
    using SageNetTuner.Model;

    public interface IChannelProvider
    {
        Lineup GetLineup(DeviceElement deviceSettings);
    }
}
