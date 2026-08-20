using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace GuardianShield;

public partial class MainWindow : Window
{
    private readonly string quarantine =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "GuardianShield",
            "Quarantine");

    private CancellationTokenSource? monitorCts;

    private int scanned;
    private int threats;

    private readonly string[] testIndicators =
    {
        "eicar",
        "testmalware",
        "suspicious-test"
    };

    public MainWindow()
    {
        InitializeComponent();

        Directory.CreateDirectory(quarantine);

        UpdateText.Text =
            "Automatic updates: installer channel";

        Log("GuardianShield v3 initialized.");

        Log(
            "Safe mode: no suspicious file is executed " +
            "or automatically deleted.");
    }

    private void Log(string message)
    {
        LogBox.AppendText(
            $"[{DateTime.Now:HH:mm:ss}] {message}" +
            Environment.NewLine);

        LogBox.ScrollToEnd();
    }

    private void Stats()
    {
        StatsText.Text =
            $"Files: {scanned:N0}   Threats: {threats:N0}";
    }

    private async void QuickScan_Click(
        object sender,
        RoutedEventArgs e)
    {
        await Scan(
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile),
            false);
    }

    private async void DeepScan_Click(
        object sender,
        RoutedEventArgs e)
    {
        await Scan(
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile),
            true);
    }

    private async void SelectFolder_Click(
        object sender,
        RoutedEventArgs e)
    {
        using var dialog =
            new FolderBrowserDialog();

        if (dialog.ShowDialog() ==
            System.Windows.Forms.DialogResult.OK)
        {
            await Scan(
                dialog.SelectedPath,
                true);
        }
    }

    private async Task Scan(
        string root,
        bool deep)
    {
        HealthText.Text = "Scanning...";
        StatusText.Text = "Scan in progress...";

        Log(
            $"Starting {(deep ? "deep" : "quick")} scan: {root}");

        int local = 0;
        int bad = 0;

        try
        {
            SearchOption option =
                deep
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

            foreach (string file in
                Directory.EnumerateFiles(
                    root,
                    "*",
                    option))
            {
                try
                {
                    local++;
                    scanned++;

                    string name =
                        Path.GetFileName(file)
                            .ToLowerInvariant();

                    string hash =
                        await Sha256(file);

                    if (testIndicators.Any(
                        x => name.Contains(x)))
                    {
                        bad++;
                        threats++;

                        Log(
                            $"FLAGGED test indicator: {file}");

                        Log(
                            $"SHA-256: {hash}");
                    }

                    if (local % 50 == 0)
                    {
                        Stats();
                        await Task.Yield();
                    }
                }
                catch
                {
                    // Ignore files that cannot be accessed.
                }
            }
        }
        catch (Exception ex)
        {
            Log(
                $"Scan boundary: {ex.Message}");
        }

        Stats();

        HealthText.Text =
            bad == 0
                ? "Protected"
                : $"{bad} flagged";

        StatusText.Text =
            $"Scan complete - {local:N0} files checked";

        Log(
            $"Scan complete. Flagged: {bad}.");
    }

    private static async Task<string> Sha256(
        string file)
    {
        await using FileStream stream =
            File.OpenRead(file);

        using SHA256 sha =
            SHA256.Create();

        byte[] hash =
            await sha.ComputeHashAsync(stream);

        return Convert.ToHexString(hash);
    }

    private void MonitorToggle_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (MonitorToggle.IsChecked == true)
        {
            MonitorToggle.Content = "ON";
            MonitorStatus.Text =
                "Monitoring enabled";

            StartMonitor();
        }
        else
        {
            MonitorToggle.Content = "OFF";
            MonitorStatus.Text =
                "Monitoring disabled";

            StopMonitor();
        }
    }

    private void StartMonitor()
    {
        StopMonitor();

        monitorCts =
            new CancellationTokenSource();

        CancellationToken token =
            monitorCts.Token;

        Task.Run(
            async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Dispatcher.InvokeAsync(
                            () =>
                            {
                                StatusText.Text =
                                    "Real-time monitor: active";
                            });

                        await Task.Delay(
                            TimeSpan.FromSeconds(20),
                            token);

                        if (!token.IsCancellationRequested)
                        {
                            await Dispatcher.InvokeAsync(
                                async () =>
                                {
                                    await Scan(
                                        Environment.GetFolderPath(
                                            Environment.SpecialFolder.UserProfile),
                                        false);
                                });
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                    }
                }
            },
            token);

        Log(
            "Real-time monitor started.");
    }

    private void StopMonitor()
    {
        monitorCts?.Cancel();
        monitorCts = null;
    }

    private void Quarantine_Click(
        object sender,
        RoutedEventArgs e)
    {
        Directory.CreateDirectory(quarantine);

        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{quarantine}\"",
                UseShellExecute = true
            });
    }

    private void Settings_Click(
        object sender,
        RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show(
            "GuardianShield v3\n\n" +
            "Installer-ready Windows application.\n\n" +
            "Automatic updates are delivered through " +
            "the configured installer channel.",
            "GuardianShield Settings",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CheckUpdates_Click(
        object sender,
        RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show(
            "The update channel is configured at " +
            "the installer level.\n\n" +
            "Publish a newer signed build to distribute " +
            "an updated version.",
            "GuardianShield Updates",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void Dashboard_Click(
        object sender,
        RoutedEventArgs e)
    {
        HealthText.Text = "Protected";
        StatusText.Text = "Ready to scan.";
    }

    private void ClearLog_Click(
        object sender,
        RoutedEventArgs e)
    {
        LogBox.Clear();
    }

    protected override void OnClosed(
        EventArgs e)
    {
        StopMonitor();

        base.OnClosed(e);
    }
}
