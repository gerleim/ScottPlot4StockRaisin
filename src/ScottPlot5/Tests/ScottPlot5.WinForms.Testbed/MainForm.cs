using ScottPlot;
using ScottPlot.WinForms;

namespace ScottPlot.WinForms.Testbed;

public class MainForm : Form
{
    private readonly FormsPlot formsPlot;

    public MainForm()
    {
        Text = "ScottPlot Candlestick Testbed";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;

        formsPlot = new FormsPlot
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(formsPlot);

        SetupCandlestickChart();
    }

    private void SetupCandlestickChart()
    {
        // Generate test OHLC data
        OHLC[] ohlcs = GenerateTestData(100);

        // Add candlestick plot
        var candlestickPlot = formsPlot.Plot.Add.Candlestick(ohlcs);

        // Configure axes
        formsPlot.Plot.Axes.DateTimeTicksBottom();
        formsPlot.Plot.Axes.AutoScale();

        // Add title
        formsPlot.Plot.Title("Sample Candlestick Chart");
        formsPlot.Plot.YLabel("Price");
        formsPlot.Plot.XLabel("Date");

        formsPlot.Refresh();
    }

    private static OHLC[] GenerateTestData(int count)
    {
        var ohlcs = new OHLC[count];
        var random = new Random(42); // Fixed seed for reproducibility
        var startDate = new DateTime(2024, 1, 1);
        var timeSpan = TimeSpan.FromDays(1);

        double price = 100.0; // Starting price

        for (int i = 0; i < count; i++)
        {
            // Random walk with some volatility
            double change = (random.NextDouble() - 0.48) * 3; // Slight upward bias
            double volatility = random.NextDouble() * 2 + 0.5;

            double open = price;
            double close = price + change;

            // Generate high and low based on open/close
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

            price = close; // Next candle opens at previous close
        }

        return ohlcs;
    }
}
