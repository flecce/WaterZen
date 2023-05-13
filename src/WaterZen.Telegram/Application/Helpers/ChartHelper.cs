using System.Drawing;

namespace WaterZen.Telegram.Application.Helpers
{
    public class ChartHelper
    {
        public static byte[] CreateGraph(double[] data)
        {
            //double[] dataX = Enumerable
            //    .Range(0, data.Length)
            //    .Select(i => 0 + i * 0.5)
            //    .ToArray();

            //double[] dataY = data;

            var plt = new ScottPlot.Plot(400, 300);
            var sig = plt.AddSignal(data);
            sig.FillBelow();

            return plt.GetImageBytes();
        }

        static int GetMaxValue(int[] data)
        {
            int maxValue = int.MinValue;
            foreach (int value in data)
            {
                if (value > maxValue)
                {
                    maxValue = value;
                }
            }
            return maxValue;
        }
    }
}
