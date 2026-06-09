using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.IO;

namespace ScottPlot.WPF;

public class WpfPlotMenu : IPlotMenu
{
    public string DefaultSaveImageFilename { get; set; } = "Plot.png";
    public List<ContextMenuItem> ContextMenuItems { get; set; } = new();
    public ContextMenuStyle Style { get; set; } = new();
    readonly WpfPlotBase ThisControl;

    public WpfPlotMenu(WpfPlotBase control)
    {
        ThisControl = control;
        Reset();
    }

    public ContextMenuItem[] GetDefaultContextMenuItems()
    {
        ContextMenuItem saveImage = new()
        {
            Label = "Save Image",
            OnInvoke = OpenSaveImageDialog
        };

        ContextMenuItem copyImage = new()
        {
            Label = "Copy to Clipboard",
            OnInvoke = CopyImageToClipboard
        };

        ContextMenuItem autoscale = new()
        {
            Label = "Autoscale",
            OnInvoke = Autoscale,
        };

        ContextMenuItem newWindow = new()
        {
            Label = "Open in New Window",
            OnInvoke = OpenInNewWindow,
        };

        return new ContextMenuItem[]
        {
            saveImage,
            copyImage,
            autoscale,
            newWindow,
        };
    }

    public ContextMenu GetContextMenu(Plot plot)
    {
        ContextMenu menu = new();

        // Override the ContextMenu ControlTemplate to eliminate the default top/bottom
        // gap. The Aero2 theme template uses ItemsPresenter Margin="{TemplateBinding Padding}"
        // which is a one-time binding — impossible to override after template application.
        var menuBorderFactory = new FrameworkElementFactory(typeof(Border), "ContextMenuBorder");
        menuBorderFactory.SetBinding(Border.BackgroundProperty,
            new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        menuBorderFactory.SetBinding(Border.BorderBrushProperty,
            new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        menuBorderFactory.SetBinding(Border.BorderThicknessProperty,
            new System.Windows.Data.Binding("BorderThickness") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        menuBorderFactory.SetValue(Border.PaddingProperty, Style.MenuPadding ?? new Thickness(0));

        var presenterFactory = new FrameworkElementFactory(typeof(ItemsPresenter));
        presenterFactory.SetValue(ItemsPresenter.MarginProperty, new Thickness(0));
        presenterFactory.SetValue(System.Windows.Input.KeyboardNavigation.DirectionalNavigationProperty,
            System.Windows.Input.KeyboardNavigationMode.Cycle);
        presenterFactory.SetValue(UIElement.SnapsToDevicePixelsProperty, true);

        menuBorderFactory.AppendChild(presenterFactory);
        var menuTemplate = new ControlTemplate(typeof(ContextMenu));
        menuTemplate.VisualTree = menuBorderFactory;
        menu.Template = menuTemplate;

        // Build a compact MenuItem style that replaces the default bulky template
        var itemStyle = new System.Windows.Style(typeof(MenuItem));
        itemStyle.Setters.Add(new Setter(MenuItem.MinHeightProperty, 0d));
        itemStyle.Setters.Add(new Setter(MenuItem.MinWidthProperty, 0d));
        if (Style.ItemFontSize.HasValue)
            itemStyle.Setters.Add(new Setter(MenuItem.FontSizeProperty, Style.ItemFontSize.Value));
        if (Style.ItemPadding.HasValue)
            itemStyle.Setters.Add(new Setter(MenuItem.PaddingProperty, Style.ItemPadding.Value));

        var gridFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Grid));

        var borderFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Border), "Bd");
        borderFactory.SetValue(System.Windows.Controls.Border.BackgroundProperty, System.Windows.Media.Brushes.Transparent);
        borderFactory.SetBinding(System.Windows.Controls.Border.PaddingProperty,
            new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });

        var dockFactory = new FrameworkElementFactory(typeof(DockPanel));

        var arrowFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.TextBlock), "Arrow");
        arrowFactory.SetValue(System.Windows.Controls.TextBlock.TextProperty, "▸");
        arrowFactory.SetValue(DockPanel.DockProperty, Dock.Right);
        arrowFactory.SetValue(System.Windows.Controls.TextBlock.FontSizeProperty, 10d);
        arrowFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
        arrowFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 0, 0));
        arrowFactory.SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);

        var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
        contentFactory.SetValue(ContentPresenter.ContentSourceProperty, "Header");
        contentFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);

        dockFactory.AppendChild(arrowFactory);
        dockFactory.AppendChild(contentFactory);
        borderFactory.AppendChild(dockFactory);
        gridFactory.AppendChild(borderFactory);

        var popupFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Primitives.Popup), "PART_Popup");
        popupFactory.SetValue(System.Windows.Controls.Primitives.Popup.AllowsTransparencyProperty, true);
        popupFactory.SetValue(System.Windows.Controls.Primitives.Popup.PlacementProperty,
            System.Windows.Controls.Primitives.PlacementMode.Right);
        popupFactory.SetBinding(System.Windows.Controls.Primitives.Popup.IsOpenProperty,
            new System.Windows.Data.Binding("IsSubmenuOpen") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });

        var popupBorderFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Border), "SubMenuBorder");
        popupBorderFactory.SetBinding(System.Windows.Controls.Border.BackgroundProperty,
            new System.Windows.Data.Binding("Background") { Source = menu });
        popupBorderFactory.SetBinding(System.Windows.Controls.Border.BorderBrushProperty,
            new System.Windows.Data.Binding("BorderBrush") { Source = menu });
        popupBorderFactory.SetValue(System.Windows.Controls.Border.BorderThicknessProperty, new Thickness(1));
        popupBorderFactory.SetValue(System.Windows.Controls.Border.PaddingProperty, Style.MenuPadding ?? new Thickness(0));
        popupBorderFactory.SetBinding(System.Windows.Documents.TextElement.ForegroundProperty,
            new System.Windows.Data.Binding("Foreground") { Source = menu });

        var itemsPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
        itemsPanelFactory.SetValue(StackPanel.IsItemsHostProperty, true);

        popupBorderFactory.AppendChild(itemsPanelFactory);
        popupFactory.AppendChild(popupBorderFactory);
        gridFactory.AppendChild(popupFactory);

        var itemTemplate = new ControlTemplate(typeof(MenuItem));
        itemTemplate.VisualTree = gridFactory;

        var highlightTrigger = new Trigger { Property = MenuItem.IsHighlightedProperty, Value = true };
        highlightTrigger.Setters.Add(new Setter(System.Windows.Controls.Border.BackgroundProperty, SystemColors.HighlightBrush, "Bd"));
        highlightTrigger.Setters.Add(new Setter(MenuItem.ForegroundProperty, SystemColors.HighlightTextBrush));
        itemTemplate.Triggers.Add(highlightTrigger);

        var hasItemsTrigger = new Trigger { Property = MenuItem.HasItemsProperty, Value = true };
        hasItemsTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible, "Arrow"));
        itemTemplate.Triggers.Add(hasItemsTrigger);

        itemStyle.Setters.Add(new Setter(MenuItem.TemplateProperty, itemTemplate));
        menu.Resources[typeof(MenuItem)] = itemStyle;

        foreach (ContextMenuItem curr in ContextMenuItems)
        {
            if (curr.IsSeparator)
            {
                menu.Items.Add(new Separator());
            }
            else if (curr.Children is { Count: > 0 } children)
            {
                MenuItem parentItem = new() { Header = curr.Label };
                foreach (var child in children)
                {
                    if (child.IsSeparator)
                    {
                        parentItem.Items.Add(new Separator());
                    }
                    else
                    {
                        MenuItem childItem = new() { Header = child.Label };
                        if (!string.IsNullOrEmpty(child.Tooltip))
                            childItem.ToolTip = child.Tooltip;
                        childItem.Click += (s, e) => child.OnInvoke(plot);
                        parentItem.Items.Add(childItem);
                    }
                }
                menu.Items.Add(parentItem);
            }
            else
            {
                MenuItem menuItem = new() { Header = curr.Label };
                if (!string.IsNullOrEmpty(curr.Tooltip))
                    menuItem.ToolTip = curr.Tooltip;
                menuItem.Click += (s, e) => curr.OnInvoke(plot);
                menu.Items.Add(menuItem);
            }
        }

        return menu;
    }

    public void ShowContextMenu(Pixel pixel)
    {
        Plot? plot = ThisControl.GetPlotAtPixel(pixel);
        if (plot is null)
            return;
        var menu = GetContextMenu(plot);
        menu.PlacementTarget = ThisControl;
        menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
        menu.IsOpen = true;
    }

    public void OpenSaveImageDialog(Plot plot)
    {
        SaveFileDialog dialog = new()
        {
            FileName = DefaultSaveImageFilename,
            Filter = "PNG Files (*.png)|*.png" +
                     "|JPEG Files (*.jpg, *.jpeg)|*.jpg;*.jpeg" +
                     "|BMP Files (*.bmp)|*.bmp" +
                     "|WebP Files (*.webp)|*.webp" +
                     "|SVG Files (*.svg)|*.svg" +
                     "|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() is true)
        {
            if (string.IsNullOrEmpty(dialog.FileName))
                return;

            ImageFormat format;

            try
            {
                format = ImageFormats.FromFilename(dialog.FileName);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Unsupported image file format", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                PixelSize lastRenderSize = plot.RenderManager.LastRender.FigureRect.Size;
                plot.Save(dialog.FileName, (int)lastRenderSize.Width, (int)lastRenderSize.Height, format);
            }
            catch (Exception)
            {
                MessageBox.Show("Image save failed", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }

    public static void CopyImageToClipboard(Plot plot)
    {
        PixelSize lastRenderSize = plot.RenderManager.LastRender.FigureRect.Size;
        Image bmp = plot.GetImage((int)lastRenderSize.Width, (int)lastRenderSize.Height);
        byte[] bmpBytes = bmp.GetImageBytes();

        using MemoryStream ms = new();
        ms.Write(bmpBytes, 0, bmpBytes.Length);
        BitmapImage bmpImage = new();
        bmpImage.BeginInit();
        bmpImage.StreamSource = ms;
        bmpImage.EndInit();
        Clipboard.SetImage(bmpImage);
    }

    public void Autoscale(Plot plot)
    {
        plot.Axes.AutoScale();
        ThisControl.Refresh();
    }

    public void OpenInNewWindow(Plot plot)
    {
        WpfPlotViewer.Launch(plot, "Interactive Plot");
        ThisControl.Refresh();
    }

    public void Reset()
    {
        Clear();
        ContextMenuItems.AddRange(GetDefaultContextMenuItems());
    }

    public void Clear()
    {
        ContextMenuItems.Clear();
    }

    public void Add(string Label, Action<Plot> action)
    {
        ContextMenuItems.Add(new ContextMenuItem() { Label = Label, OnInvoke = action });
    }

    public void Add(string Label, Action<Plot> action, string? tooltip)
    {
        ContextMenuItems.Add(new ContextMenuItem() { Label = Label, OnInvoke = action, Tooltip = tooltip });
    }

    public void AddSubmenu(string label, List<ContextMenuItem> children)
    {
        ContextMenuItems.Add(new ContextMenuItem { Label = label, Children = children });
    }

    public void AddSeparator()
    {
        ContextMenuItems.Add(new ContextMenuItem() { IsSeparator = true });
    }
}
