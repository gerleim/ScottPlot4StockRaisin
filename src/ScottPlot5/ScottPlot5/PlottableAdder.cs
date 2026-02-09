using ScottPlot.DataSources;
using ScottPlot.Plottables;

namespace ScottPlot;

/// <summary>
/// Helper methods to create plottable objects and add them to the plot
/// </summary>
public class PlottableAdder(Plot plot)
{
    public Plot Plot { get; } = plot;

    /// <summary>
    /// Color set used for adding new plottables
    /// </summary>
    public IPalette Palette { get; set; } = new Palettes.Category10();

    private int NextColorIndex = 0;

    /// <summary>
    /// Return the next color of the <see cref="Palette"/>.
    /// Colors reset if <see cref="Plot.PlottableList"/> is cleared.
    /// </summary>
    public Color GetNextColor(bool incrementCounter = true)
    {
        if (Plot.PlottableList.Count == 0)
            NextColorIndex = 0;

        Color color = Palette.GetColor(NextColorIndex);

        if (incrementCounter)
            NextColorIndex++;

        return color;
    }

    public Annotation Annotation(string text, Alignment alignment = Alignment.UpperLeft)
    {
        Annotation an = new()
        {
            Alignment = alignment,
            Text = text,
            LabelBackgroundColor = Colors.Yellow.WithAlpha(.75),
            LabelBorderColor = Colors.Black,
            LabelPadding = 5,
        };

        Plot.PlottableList.Add(an);

        return an;
    }

    public Annotation BackgroundText(string text, Color? color = null, double size = 48)
    {
        Annotation an = new()
        {
            Text = text,
            LabelFontColor = color ?? Colors.Gray.WithAlpha(.3),
            LabelFontSize = (float)size,
            LabelBackgroundColor = Colors.Transparent,
            LabelShadowColor = Colors.Transparent,
            LabelBorderColor = Colors.Transparent,
            Alignment = Alignment.MiddleCenter,
            OffsetX = 0,
            OffsetY = 0,
        };

        Plot.PlottableList.Insert(0, an);

        return an;
    }

    public (Annotation line1, Annotation line2) BackgroundText(string line1, string line2, Color? color = null, double size1 = 48, double size2 = 24)
    {
        Annotation an1 = BackgroundText(line1, color, size1);
        an1.Alignment = Alignment.LowerCenter;
        an1.FractionRect = FractionRect.Row(0, 2);

        Annotation an2 = BackgroundText(line2, color, size2);
        an2.Alignment = Alignment.UpperCenter;
        an2.FractionRect = FractionRect.Row(1, 2);

        return (an1, an2);
    }

    public BarPlot Bar(Bar bar)
    {
        List<Bar> bars = [bar];
        BarPlot bp = new(bars);
        Plottable(bp);
        return bp;
    }

    public BarPlot Bar(double position, double value, double error = 0)
    {
        Bar bar = new()
        {
            Position = position,
            Value = value,
            Error = error,
            FillColor = GetNextColor(),
        };
        return Bar(bar);
    }

    public BarPlot Bars(List<Bar> bars)
    {
        BarPlot bp = new(bars);
        Plottable(bp);
        return bp;
    }

    public BarPlot Bars(Bar[] bars)
    {
        BarPlot bp = new([.. bars]);
        Plottable(bp);
        return bp;
    }

    public BarPlot Bars(double[] values)
    {
        var positions = Enumerable.Range(0, values.Length).Select(x => (double)x);
        return Bars(positions, values);
    }

    public BarPlot Bars<T>(IEnumerable<double> positions, IEnumerable<T> values)
    {
        double[] values2 = NumericConversion.GenericToDoubleArray(values);
        return Bars(positions, values2);
    }

    public BarPlot Bars(IEnumerable<double> positions, IEnumerable<double> values)
    {
        if (positions.Count() != values.Count())
        {
            throw new ArgumentException($"{nameof(positions)} and {nameof(positions)} have different lengths");
        }

        Color color = GetNextColor();

        List<Bar> bars = new();
        foreach (var item in positions.Zip(values, (a, b) => new { a, b }))
        {
            bars.Add(new Bar()
            {
                Position = item.a,
                Value = item.b,
                FillColor = color
            });
        }

        return Bars(bars);
    }

    public CandlestickPlot Candlestick(OHLC[] ohlcs)
    {
        OHLCSourceArray dataSource = new(ohlcs);
        CandlestickPlot candlestickPlot = new(dataSource);
        Plot.PlottableList.Add(candlestickPlot);
        return candlestickPlot;
    }

    public CandlestickPlot Candlestick(List<OHLC> ohlcs)
    {
        OHLCSourceList dataSource = new(ohlcs);
        CandlestickPlot candlestickPlot = new(dataSource);
        Plot.PlottableList.Add(candlestickPlot);
        return candlestickPlot;
    }

    public Crosshair Crosshair(double x, double y)
    {
        Crosshair ch = new()
        {
            Position = new(x, y)
        };
        Color color = GetNextColor();
        ch.LineColor = color;
        ch.TextColor = color;
        Plot.PlottableList.Add(ch);
        return ch;
    }

    public HorizontalLine HorizontalLine(double y, float width = 2, Color? color = null, LinePattern pattern = default)
    {
        Color color2 = color ?? GetNextColor();
        HorizontalLine line = new()
        {
            LineWidth = width,
            LineColor = color2,
            LabelBackgroundColor = color2,
            LabelFontColor = Colors.White,
            LinePattern = pattern,
            Y = y
        };
        Plot.PlottableList.Add(line);
        return line;
    }

    public HorizontalSpan HorizontalSpan(double x1, double x2, Color? color = null)
    {
        HorizontalSpan span = new() { X1 = x1, X2 = x2 };
        span.FillStyle.Color = color ?? GetNextColor().WithAlpha(.2);
        span.LineStyle.Color = span.FillStyle.Color.WithAlpha(.5);
        Plot.PlottableList.Add(span);
        return span;
    }

    public Legend Legend()
    {
        Legend legend = new(Plot) { IsVisible = true };
        Plot.PlottableList.Add(legend);
        return legend;
    }

    public LinePlot Line(Coordinates start, Coordinates end)
    {
        LinePlot lp = new()
        {
            Start = start,
            End = end,
        };

        lp.LineStyle.Color = GetNextColor();
        lp.MarkerStyle.FillColor = lp.LineStyle.Color;

        Plot.PlottableList.Add(lp);

        return lp;
    }

    public LinePlot Line(CoordinateLine line)
    {
        return Line(line.Start, line.End);
    }

    public LinePlot Line(double x1, double y1, double x2, double y2)
    {
        Coordinates start = new(x1, y1);
        Coordinates end = new(x2, y2);
        return Line(start, end);
    }

    public Marker Marker(double x, double y, MarkerShape shape = MarkerShape.FilledCircle, float size = 10, Color? color = null)
    {
        Marker mp = new()
        {
            MarkerShape = shape,
            MarkerSize = size,
            Color = color ?? GetNextColor(),
            Location = new Coordinates(x, y),
        };

        Plot.PlottableList.Add(mp);

        return mp;
    }

    public Marker Marker(Coordinates location, MarkerShape shape = MarkerShape.FilledCircle, float size = 10, Color? color = null)
    {
        return Marker(location.X, location.Y, shape, size, color);
    }

    public Plottables.Markers Markers(double[] xs, double[] ys, MarkerShape shape = MarkerShape.FilledCircle, float size = 10, Color? color = null)
    {
        ScatterSourceDoubleArray ss = new(xs, ys);

        Plottables.Markers mp = new(ss)
        {
            MarkerShape = shape,
            MarkerSize = size,
            Color = color ?? GetNextColor()
        };

        Plot.PlottableList.Add(mp);

        return mp;
    }

    public Plottables.Markers Markers(Coordinates[] coordinates, MarkerShape shape = MarkerShape.FilledCircle, float size = 10, Color? color = null)
    {
        ScatterSourceCoordinatesArray ss = new(coordinates);

        Plottables.Markers mp = new(ss)
        {
            MarkerShape = shape,
            MarkerSize = size,
            Color = color ?? GetNextColor()
        };

        Plot.PlottableList.Add(mp);

        return mp;
    }

    public Plottables.Markers Markers(List<Coordinates> coordinates, MarkerShape shape = MarkerShape.FilledCircle, float size = 10, Color? color = null)
    {
        ScatterSourceCoordinatesList ss = new(coordinates);

        Plottables.Markers mp = new(ss)
        {
            MarkerShape = shape,
            MarkerSize = size,
            Color = color ?? GetNextColor()
        };

        Plot.PlottableList.Add(mp);

        return mp;
    }

    public Plottables.Markers Markers<TX, TY>(TX[] xs, TY[] ys, MarkerShape shape = MarkerShape.FilledCircle, float size = 10, Color? color = null)
    {
        ScatterSourceGenericArray<TX, TY> ss = new(xs, ys);

        Plottables.Markers mp = new(ss)
        {
            MarkerShape = shape,
            MarkerSize = size,
            Color = color ?? GetNextColor()
        };

        Plot.PlottableList.Add(mp);

        return mp;
    }

    public Plottables.Markers Markers<TX, TY>(List<TX> xs, List<TY> ys, MarkerShape shape = MarkerShape.FilledCircle, float size = 10, Color? color = null)
    {
        ScatterSourceGenericList<TX, TY> ss = new(xs, ys);

        Plottables.Markers mp = new(ss)
        {
            MarkerShape = shape,
            MarkerSize = size,
            Color = color ?? GetNextColor()
        };

        Plot.PlottableList.Add(mp);

        return mp;
    }

    public OhlcPlot OHLC(List<OHLC> ohlcs)
    {
        OHLCSourceList dataSource = new(ohlcs);
        OhlcPlot ohlc = new(dataSource);
        Plot.PlottableList.Add(ohlc);
        return ohlc;
    }

    public IPlottable Plottable(IPlottable plottable)
    {
        Plot.PlottableList.Add(plottable);
        return plottable;
    }

    /// <summary>
    /// Create a bar plot to represent a collection of named ranges
    /// </summary>
    public BarPlot Ranges(List<(string name, CoordinateRange range)> ranges, Color? color = null, bool horizontal = false)
    {
        Color barColor = color ?? GetNextColor();

        // create a bar plot from the collection of ranges
        Bar[] bars = new Bar[ranges.Count];
        for (int i = 0; i < ranges.Count; i++)
        {
            bars[i] = new()
            {
                ValueBase = ranges[i].range.Min,
                Value = ranges[i].range.Max,
                Position = i,
                FillColor = barColor,
            };
        }
        BarPlot bp = Bars(bars);
        bp.Horizontal = horizontal;

        // use manaul tick labels displaying category names
        double[] positions = bars.Select(x => x.Position).ToArray();
        string[] labels = ranges.Select(x => x.name).ToArray();
        if (horizontal)
        {
            Plot.Axes.Left.SetTicks(positions, labels);
        }
        else
        {
            Plot.Axes.Bottom.SetTicks(positions, labels);
        }

        return bp;
    }

    public Scatter Scatter(IScatterSource source, Color? color = null)
    {
        Color nextColor = color ?? GetNextColor();
        Scatter scatter = new(source)
        {
            LineColor = nextColor,
            MarkerFillColor = nextColor,
            MarkerLineColor = nextColor,
        };
        Plot.PlottableList.Add(scatter);
        return scatter;
    }

    public Scatter Scatter(double x, double y, Color? color = null)
    {
        double[] xs = { x };
        double[] ys = { y };
        ScatterSourceDoubleArray source = new(xs, ys);
        return Scatter(source, color);
    }

    public Scatter Scatter(double[] xs, double[] ys, Color? color = null)
    {
        ScatterSourceDoubleArray source = new(xs, ys);
        return Scatter(source, color);
    }

    public Scatter Scatter(Coordinates point, Color? color = null)
    {
        Coordinates[] coordinates = { point };
        ScatterSourceCoordinatesArray source = new(coordinates);
        return Scatter(source, color);
    }

    public Scatter Scatter(Coordinates[] coordinates, Color? color = null)
    {
        ScatterSourceCoordinatesArray source = new(coordinates);
        return Scatter(source, color);
    }

    public Scatter Scatter(List<Coordinates> coordinates, Color? color = null)
    {
        ScatterSourceCoordinatesList source = new(coordinates);
        return Scatter(source, color);
    }

    public Scatter Scatter<T1, T2>(T1[] xs, T2[] ys, Color? color = null)
    {
        Color nextColor = color ?? GetNextColor();
        ScatterSourceGenericArray<T1, T2> source = new(xs, ys);
        Scatter scatter = new(source);
        scatter.LineStyle.Color = nextColor;
        scatter.MarkerStyle.FillColor = nextColor;
        Plot.PlottableList.Add(scatter);
        return scatter;
    }

    public Scatter Scatter<T1, T2>(List<T1> xs, List<T2> ys, Color? color = null)
    {
        Color nextColor = color ?? GetNextColor();
        ScatterSourceGenericList<T1, T2> source = new(xs, ys);
        Scatter scatter = new(source);
        scatter.LineStyle.Color = nextColor;
        scatter.MarkerStyle.FillColor = nextColor;
        Plot.PlottableList.Add(scatter);
        return scatter;
    }

    public Scatter ScatterLine(IScatterSource source, Color? color = null)
    {
        var scatter = Scatter(source, color);
        scatter.MarkerSize = 0;
        return scatter;
    }

    public Scatter ScatterLine(double[] xs, double[] ys, Color? color = null)
    {
        var scatter = Scatter(xs, ys, color);
        scatter.MarkerSize = 0;
        return scatter;
    }

    public Scatter ScatterLine(Coordinates[] coordinates, Color? color = null)
    {
        var scatter = Scatter(coordinates, color);
        scatter.MarkerSize = 0;
        return scatter;
    }

    public Scatter ScatterLine(List<Coordinates> coordinates, Color? color = null)
    {
        var scatter = Scatter(coordinates, color);
        scatter.MarkerSize = 0;
        return scatter;
    }

    public Scatter ScatterLine<T1, T2>(T1[] xs, T2[] ys, Color? color = null)
    {
        var scatter = Scatter(xs, ys, color);
        scatter.MarkerSize = 0;
        return scatter;
    }

    public Scatter ScatterLine<T1, T2>(List<T1> xs, List<T2> ys, Color? color = null)
    {
        var scatter = Scatter(xs, ys, color);
        scatter.MarkerSize = 0;
        return scatter;
    }

    public Scatter ScatterPoints(IScatterSource source, Color? color = null)
    {
        var scatter = Scatter(source, color);
        scatter.LineWidth = 0;
        return scatter;
    }

    public Scatter ScatterPoints(double[] xs, double[] ys, Color? color = null)
    {
        var scatter = Scatter(xs, ys, color);
        scatter.LineWidth = 0;
        return scatter;
    }

    public Scatter ScatterPoints(Coordinates[] coordinates, Color? color = null)
    {
        var scatter = Scatter(coordinates, color);
        scatter.LineWidth = 0;
        return scatter;
    }

    public Scatter ScatterPoints(List<Coordinates> coordinates, Color? color = null)
    {
        var scatter = Scatter(coordinates, color);
        scatter.LineWidth = 0;
        return scatter;
    }

    public Scatter ScatterPoints<T1, T2>(T1[] xs, T2[] ys, Color? color = null)
    {
        var scatter = Scatter(xs, ys, color);
        scatter.LineWidth = 0;
        return scatter;
    }

    public Scatter ScatterPoints<T1, T2>(List<T1> xs, List<T2> ys, Color? color = null)
    {
        var scatter = Scatter(xs, ys, color);
        scatter.LineWidth = 0;
        return scatter;
    }

    public Signal Signal(ISignalSource source, Color? color = null)
    {
        Signal sig = new(source)
        {
            Color = color ?? GetNextColor()
        };

        Plot.PlottableList.Add(sig);

        return sig;
    }

    public Signal Signal(double[] ys, double period = 1, Color? color = null)
    {
        SignalSourceDouble source = new(ys, period);
        return Signal(source, color);
    }

    public Signal Signal<T>(T[] ys, double period = 1, Color? color = null)
    {
        SignalSourceGenericArray<T> source = new(ys, period);
        return Signal(source, color);
    }

    public Signal Signal<T>(IReadOnlyList<T> ys, double period = 1, Color? color = null)
    {
        SignalSourceGenericList<T> source = new(ys, period);
        return Signal(source, color);
    }

    public Signal SignalConst<T>(T[] ys, double period = 1, Color? color = null)
        where T : struct, IComparable
    {
        SignalConstSource<T> source = new(ys, period);
        return Signal(source, color);
    }

    public SignalXY SignalXY(ISignalXYSource source, Color? color = null)
    {
        SignalXY sig = new(source)
        {
            Color = color ?? GetNextColor()
        };

        Plot.PlottableList.Add(sig);

        return sig;
    }

    public SignalXY SignalXY(double[] xs, double[] ys, Color? color = null)
    {
        SignalXYSourceDoubleArray source = new(xs, ys);
        return SignalXY(source, color);
    }

    public SignalXY SignalXY<TX, TY>(TX[] xs, TY[] ys, Color? color = null)
    {
        var source = new SignalXYSourceGenericArray<TX, TY>(xs, ys);
        return SignalXY(source, color);
    }

    public SignalXY SignalXY<TX, TY>(IReadOnlyList<TX> xs, IReadOnlyList<TY> ys, Color? color = null)
    {
        SignalXYSourceGenericList<TX, TY> source = new(xs, ys);
        return SignalXY(source, color);
    }

    /// <summary>
    /// Place a stacked bar chart at a single position
    /// </summary>
    public BarPlot[] StackedRanges(List<(string name, double[] edgeValues)> ranges, IPalette? palette = null, bool horizontal = false)
    {
        BarPlot[] bps = new BarPlot[ranges.Count];
        for (int i = 0; i < ranges.Count; i++)
        {
            double[] edgeValues = ranges[i].edgeValues;
            Bar[] bars = new Bar[edgeValues.Length - 1];
            for (int j = 0; j < bars.Length; j++)
            {
                bars[j] = new()
                {
                    ValueBase = edgeValues[j],
                    Value = edgeValues[j + 1],
                    Position = i,
                    FillColor = (palette ?? Palette).GetColor(j),
                };
            }

            bps[i] = Bars(bars);
            bps[i].Horizontal = horizontal;
        }

        string[] labels = ranges.Select(x => x.name).ToArray();
        double[] positions = Generate.Consecutive(labels.Length);
        if (horizontal)
        {
            Plot.Axes.Left.SetTicks(positions, labels);
        }
        else
        {
            Plot.Axes.Bottom.SetTicks(positions, labels);
        }

        return bps;
    }

    public VerticalLine VerticalLine(double x, float width = 2, Color? color = null, LinePattern pattern = default)
    {
        Color color2 = color ?? GetNextColor();
        VerticalLine line = new()
        {
            LineWidth = width,
            LineColor = color2,
            LabelBackgroundColor = color2,
            LinePattern = pattern,
            X = x
        };
        Plot.PlottableList.Add(line);
        return line;
    }

    public VerticalSpan VerticalSpan(double y1, double y2, Color? color = null)
    {
        VerticalSpan span = new() { Y1 = y1, Y2 = y2 };
        span.FillStyle.Color = color ?? GetNextColor().WithAlpha(.2);
        span.LineStyle.Color = span.FillStyle.Color.WithAlpha(.5);
        Plot.PlottableList.Add(span);
        return span;
    }
}
