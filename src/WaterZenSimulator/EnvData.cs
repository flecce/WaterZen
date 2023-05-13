namespace WaterZenSimulator
{
    public class RootData
    {
        public DateTime Time { get; set; }
        public EnvData ATC53305e { get; set; }
        public string TempUnit { get; set; }
    }

    public class EnvData
    {
        public string mac { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float DewPoint { get; set; }
        public int Btn { get; set; }
        public int Battery { get; set; }
        public int RSSI { get; set; }
    }

    public class WaterData
    {
        public bool WaterOn { get; set; }
        public float Temperature { get; set; }
        public float FlowRate { get; set; }
       
    }
}