using ScottPlot.WinForms;

namespace ScottPlot.WinForms.Testbed.Charts;

public static class BarChart
{
    public static void Setup(FormsPlot plot)
    {
        double[] values = GenerateData(12);

        plot.Plot.Add.Bars(values);
        plot.Plot.Axes.AutoScale();
        plot.Plot.Axes.SetLimitsY(0, values.Max() * 1.1);
        plot.Plot.Title("Monthly Sales Data");
        plot.Plot.YLabel("Sales ($K)");
        plot.Plot.XLabel("Month");
        plot.Refresh();
    }

    private static double[] GenerateData(int count)
    {
        var random = new Random(123);
        var values = new double[count];

        for (int i = 0; i < count; i++)
        {
            values[i] = 50 + random.NextDouble() * 100;
        }

        return values;
    }
}
