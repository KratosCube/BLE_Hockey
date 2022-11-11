using Android.Content.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE_Hockey
{
    public partial class DeviceOrientationService
    {
        private static IReadOnlyDictionary<DisplayOrientation, ScreenOrientation> _androidDisplayOrientationMap = 
            new Dictionary<DisplayOrientation, ScreenOrientation>
            {
                [DisplayOrientation.Landscape] = ScreenOrientation.Landscape,
                [DisplayOrientation.Portrait] = ScreenOrientation.Portrait,
            };
        public partial void SetDeviceOrientation(DisplayOrientation displayOrientation)
        {
            var currentActivity = ActivityStateManager.Default.GetCurrentActivity();
            if(currentActivity is not null)
            {
                if (_androidDisplayOrientationMap.TryGetValue(displayOrientation, out ScreenOrientation screenOrientation))
                    currentActivity.RequestedOrientation = screenOrientation;
            }
        }
    }
}
