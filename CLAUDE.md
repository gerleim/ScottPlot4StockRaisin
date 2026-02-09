# ScottPlot4StockRaisin

A minimalized fork of [ScottPlot 5](https://github.com/ScottPlot/ScottPlot) (v5.1.58), stripped down to candlestick, line, and bar chart types only.

## Purpose

This project retains only candlestick, line, and bar chart capabilities from the full ScottPlot library, removing ~40 unused plottable types and all non-essential projects.

## Solution Structure

```
src/ScottPlot5/ScottPlot5.sln
  ScottPlot              - Core library (net462, netstandard2.0, net8.0, net9.0)
  ScottPlot.WinForms     - WinForms control
  ScottPlot.WPF          - WPF control
```

## Available Chart Types

**Financial:** CandlestickPlot, OHLCPlot
**Line/Scatter:** LinePlot, Scatter, Signal, SignalXY, SignalConst
**Bar:** BarPlot
**Helpers:** HorizontalLine, VerticalLine, HorizontalSpan, VerticalSpan, Crosshair, Annotation, AxisLine, AxisSpan

All chart types are added via `Plot.Add.*` methods (e.g., `Plot.Add.Candlestick(...)`, `Plot.Add.Scatter(...)`, `Plot.Add.Bars(...)`).

## Key Files

- `ScottPlot5/PlottableAdder.cs` - Factory methods for creating charts (`Plot.Add.*`)
- `ScottPlot5/Plot.cs` - Main Plot class
- `ScottPlot5/Plottables/` - All chart type implementations
- `ScottPlot5/DataSources/` - Data source abstractions (OHLC, Scatter, Signal)
- `ScottPlot5/AxisManager.cs` - Axis configuration and auto-scaling

## Build

```
dotnet build src/ScottPlot5/ScottPlot5.sln
```
