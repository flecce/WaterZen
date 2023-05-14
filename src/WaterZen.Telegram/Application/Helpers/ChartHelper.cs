namespace WaterZen.Telegram.Application.Helpers
{
    public class ChartHelper
    {
        public static byte[] CreateGraph(double[] data, string title)
        {
            var plt = new ScottPlot.Plot(400, 300);
            plt.Title(title);
            var sig = plt.AddSignal(data);
            sig.FillBelow();

            return plt.GetImageBytes();
        }
    }
}
