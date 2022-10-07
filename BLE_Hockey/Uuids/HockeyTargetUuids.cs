using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE_Hockey.Uuids
{
    public class HockeyTargetUuids
    {
        public static Guid[] HockeyTargetServiceUuids { get; private set; } = new Guid[] { new Guid("00001800-0000-1000-8000-00805f9b34fb") };
        public static Guid HockeyTargetServiceUuid { get; private set; } = new Guid("00001800-0000-1000-8000-00805f9b34fb");
    }
}
