using ScottPlot;
using ScottPlot.WinForms;

namespace ScottPlot.WinForms.Testbed.Charts;

public static class Candlestick
{
    public static void Setup(FormsPlot plot)
    {
        OHLC[] ohlcs = GenerateData(100);

        plot.Plot.Add.Candlestick(ohlcs);
        plot.Plot.Axes.DateTimeTicksBottom();
        plot.Plot.Axes.AutoScale();
        plot.Plot.Title("Sample Candlestick Chart");
        plot.Plot.YLabel("Price");
        plot.Plot.XLabel("Date");
        plot.Refresh();
    }

    private static OHLC[] GenerateData(int count)
    {
        var ohlcs = new OHLC[count];
        var random = new Random(42);
        var startDate = new DateTime(2024, 1, 1);
        var timeSpan = TimeSpan.FromDays(1);

        double price = 100.0;

        for (int i = 0; i < count; i++)
        {
            double change = (random.NextDouble() - 0.48) * 3;
            double volatility = random.NextDouble() * 2 + 0.5;

            double open = price;
            double close = price + change;

            double high = Math.Max(open, close) + random.NextDouble() * volatility;
            double low = Math.Min(open, close) - random.NextDouble() * volatility;

            ohlcs[i] = new OHLC(
                open: open,
                high: high,
                low: low,
                close: close,
                start: startDate.AddDays(i),
                span: timeSpan
            );

            price = close;
        }

        return ohlcs;
    }
}
