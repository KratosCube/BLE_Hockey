namespace BLE_Hockey.Model
{
    public class DeviceH
    {
        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public int Rssi { get; internal set; }
        public string ConId { get; internal set; }
    }
}
