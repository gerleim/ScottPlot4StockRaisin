using ScottPlot.WinForms;
using ScottPlot.WinForms.Testbed.Charts;

namespace ScottPlot.WinForms.Testbed;

public class MainForm : Form
{
    public MainForm()
    {
        Text = "ScottPlot Testbed";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;

        var tabControl = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(tabControl);

        tabControl.TabPages.Add(CreateTab("Candlestick", Candlestick.Setup));
        tabControl.TabPages.Add(CreateTab("Bar Chart", BarChart.Setup));
    }

    private static TabPage CreateTab(string title, Action<FormsPlot> setup)
    {
        var tab = new TabPage(title);
        var plot = new FormsPlot { Dock = DockStyle.Fill };
        tab.Controls.Add(plot);
        setup(plot);
        return tab;
    }
}
