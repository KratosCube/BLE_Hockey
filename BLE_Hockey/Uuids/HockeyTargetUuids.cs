namespace BLE_Hockey.Uuids
{
    public class HockeyTargetUuids
    {
        public static Guid[] HockeyTargetServiceUuids { get; private set; } = new Guid[] { new Guid("00001800-0000-1000-8000-00805f9b34fb") };

        public static Guid HockeyTargetServiceUuid { get; private set; } = new Guid("49535343-fe7d-4ae5-8fa9-9fafd205e455");

        public static Guid HockeyTargetCharacteristicUuid { get; private set; } = new Guid("49535343-1e4d-4bd9-ba61-23c647249616");


        //public static Guid HockeyTargetServiceUuid2 { get; private set; } = new Guid("49535343-fe7d-4ae5-8fa9-9fafd205e455");

        //public static Guid HockeyTargetCharacteristicUuid2 { get; private set; } = new Guid("00002a00-0000-1000-8000-00805f9b34fb");
    }
}
