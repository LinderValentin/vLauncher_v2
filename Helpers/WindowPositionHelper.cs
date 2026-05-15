using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Windows;

namespace vLauncher.Helpers
{
    public static class WindowPositionHelper
    {
        public static void CenterToOwner(Window child, Window owner, double offsetX = 0, double offsetY = 0)
        {
            child.Owner = owner;
            child.WindowStartupLocation = WindowStartupLocation.Manual;

            child.Loaded += (s, e) =>
            {
                child.Left = owner.Left + (owner.Width - child.ActualWidth) / 2 + offsetX;
                child.Top = owner.Top + (owner.Height - child.ActualHeight) / 2 + offsetY;
            };
        }

        // Position window by loading saved coordinates for key; if not present center on primary screen and save those coordinates.
        public static void PositionOrCenterOnPrimaryAndSave(Window window, string key)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;

            var appData = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = System.IO.Path.Combine(appData, "vLauncher");
            var file = System.IO.Path.Combine(folder, "window_positions.vdata");

            // ensure folder
            try { System.IO.Directory.CreateDirectory(folder); } catch { }

            window.Loaded += (s, e) =>
            {
                try
                {
                    var primaryW = SystemParameters.PrimaryScreenWidth;
                    var primaryH = SystemParameters.PrimaryScreenHeight;

                    if (System.IO.File.Exists(file))
                    {
                        var lines = System.IO.File.ReadAllLines(file);
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var parts = line.Split(new[] { '=' }, 2);
                            if (parts.Length != 2) continue;
                            if (parts[0] != key) continue;

                            var coords = parts[1].Split(new[] { ';' }, 2);
                            if (coords.Length == 2 &&
                                double.TryParse(coords[0], out double left) &&
                                double.TryParse(coords[1], out double top))
                            {
                                // ensure saved coords are on the primary screen; if not, ignore and center
                                if (left >= 0 && top >= 0 && left + window.ActualWidth <= primaryW && top + window.ActualHeight <= primaryH)
                                {
                                    window.Left = left;
                                    window.Top = top;
                                    return;
                                }
                                else
                                {
                                    // found coords but off primary/invalid -> break and center below
                                    break;
                                }
                            }
                        }
                    }

                    // if not found or invalid, center on primary screen and save
                    window.Left = (primaryW - window.ActualWidth) / 2;
                    window.Top = (primaryH - window.ActualHeight) / 2;

                    // save
                    var entry = key + "=" + window.Left.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";" + window.Top.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    List<string> outLines = new List<string>();
                    if (System.IO.File.Exists(file)) outLines.AddRange(System.IO.File.ReadAllLines(file));
                    // remove old key if present
                    outLines.RemoveAll(l => l.StartsWith(key + "=", StringComparison.Ordinal));
                    outLines.Add(entry);
                    System.IO.File.WriteAllLines(file, outLines);
                }
                catch { }
            };
        }
    }
}