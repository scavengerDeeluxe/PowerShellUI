using PowerShellUI.ViewModels.Windows;
using System.Diagnostics;
using System.Net;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace PowerShellUI.Views.Windows
{
    public partial class MainWindow : INavigationWindow
    {
        private JObject config;
        private Dictionary<string, Control> inputControls = new();

        public MainWindowViewModel ViewModel { get; }

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider navigationViewPageProvider,
            INavigationService navigationService
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

                InitializeComponent();
                LoadScriptConfiguration();
                SetPageService(navigationViewPageProvider);

            navigationService.SetNavigationControl(RootNavigation);
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        private void LoadScriptConfiguration()
        {
            string jsonUrl = "https://raw.githubusercontent.com/your-repo/scripts/main/Get-ServiceStatus.json";
            using var client = new WebClient();
            string json = client.DownloadString(jsonUrl);

            config = JObject.Parse(json);

            ScriptTitle.Text = config["script"]?["name"]?.ToString();
            ScriptDescription.Text = config["script"]?["description"]?.ToString();

            foreach (var input in config["inputs"])
            {
                string type = input["type"].ToString();
                string name = input["name"].ToString();
                string label = input["label"].ToString();

                var lbl = new TextBlock { Text = label, Margin = new Thickness(0, 5, 0, 2) };
                InputFields.Children.Add(lbl);

                Control ctrl = type switch
                {
                    "string" => new TextBox(),
                    "bool" => new CheckBox(),
                    _ => new TextBox()
                };

                InputFields.Children.Add(ctrl);
                inputControls[name] = ctrl;
            }
        }

        private void RunScript_Click(object sender, RoutedEventArgs e)
        {
            string scriptUrl = config["script"]?["source_url"]?.ToString();
            string scriptPath = Path.GetTempFileName() + ".ps1";

            using var client = new WebClient();
            client.DownloadFile(scriptUrl, scriptPath);

            string args = "";
            foreach (var kv in inputControls)
            {
                string name = kv.Key;
                Control ctrl = kv.Value;
                string value = ctrl switch
                {
                    TextBox tb => tb.Text,
                    CheckBox cb => cb.IsChecked == true ? "$true" : "$false",
                    _ => ""
                };
                args += $" -{name} {value}";
            }

            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" {args}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            int exitCode = process.ExitCode;
            string combinedOutput = output + "\n" + error;
            OutputBox.Text = combinedOutput;

            // Evaluate success/failure
            var successKeywords = config["conditions"]?["success"]?["keywords"]?.ToObject<List<string>>();
            var failureKeywords = config["conditions"]?["failure"]?["keywords"]?.ToObject<List<string>>();
            var successExit = config["conditions"]?["success"]?["exit_code"]?.ToObject<int?>();
            var failureExits = config["conditions"]?["failure"]?["exit_code"]?.ToObject<List<int>>();

            bool isSuccess = successExit == exitCode || (successKeywords != null && successKeywords.Exists(k => combinedOutput.Contains(k)));
            bool isFailure = (failureExits != null && failureExits.Contains(exitCode)) || (failureKeywords != null && failureKeywords.Exists(k => combinedOutput.Contains(k)));

            if (isSuccess && !isFailure)
            {
                MessageBox.Show("Script executed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (isFailure)
            {
                MessageBox.Show("Script execution failed!", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Script execution completed with unknown status.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
}
}
