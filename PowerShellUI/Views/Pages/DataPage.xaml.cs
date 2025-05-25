using Newtonsoft.Json.Linq;
using PowerShellUI.ViewModels.Pages;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Wpf.Ui.Abstractions.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Windows;
namespace PowerShellUI.Views.Pages
{
    public partial class DataPage : INavigableView<DataViewModel>
    {
        private string rawOutput = "";
        private string formattedJson = "";
        
       public DataViewModel ViewModel { get; }

        public DataPage(DataViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

        }

        private void SendReportByEmail(string filePath)
        {
            string senderEmail = "your-sender@gmail.com";
            string senderPassword = "your-app-password";

            string recipient = EmailRecipient.Text;
            if (string.IsNullOrWhiteSpace(recipient))
            {
                MessageBox.Show("Please enter a recipient email.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var message = new MailMessage(senderEmail, recipient)
            {
                Subject = "PowerShell Report",
                Body = "Please find the attached report.",
            };
            message.Attachments.Add(new Attachment(filePath));

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            try
            {
                client.Send(message);
                MessageBox.Show("Report emailed successfully!", "Email Sent", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send email: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExportVisual_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap bitmap = CaptureMappedOutput();
            if (bitmap == null) return;

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG Image|*.png|PDF Document|*.pdf",
                FileName = "VisualReport"
            };

            if (dlg.ShowDialog() == true)
            {
                if (dlg.FilterIndex == 1) // PNG
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var fs = new FileStream(dlg.FileName, FileMode.Create);
                    encoder.Save(fs);
                }
                else if (dlg.FilterIndex == 2) // PDF
                {
                    using var ms = new MemoryStream();
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(ms);
                    ms.Position = 0;

                    var doc = new PdfSharp.Pdf.PdfDocument();
                    var page = doc.AddPage();
                    using var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
                    var img = PdfSharp.Drawing.XImage.FromStream(ms);
                    page.Width = img.PixelWidth * 72 / img.HorizontalResolution;
                    page.Height = img.PixelHeight * 72 / img.VerticalResolution;
                    gfx.DrawImage(img, 0, 0, page.Width, page.Height);
                    doc.Save(dlg.FileName);
                }

                MessageBox.Show("Visual export completed!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private RenderTargetBitmap CaptureMappedOutput()
        {
            Size size = new Size(MappedOutputPanel.ActualWidth, MappedOutputPanel.ActualHeight);
            MappedOutputPanel.Measure(size);
            MappedOutputPanel.Arrange(new Rect(size));

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)size.Width, (int)size.Height,
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(MappedOutputPanel);
            return rtb;
        }
        private void SaveReport_Click(object sender, RoutedEventArgs e)
        {
            if (lastParsedJson == null || config["output"]?["mapping"] == null)
            {
                MessageBox.Show("No parsed output available to save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv|JSON file (*.json)|*.json",
                FileName = "ScriptReport"
            };

            if (dialog.ShowDialog() == true)
            {
                var mappings = config["output"]["mapping"];
                var mappedDict = new Dictionary<string, string>();

                foreach (var map in mappings)
                {
                    string field = map["field"]?.ToString();
                    string label = map["label"]?.ToString();
                    string value = lastParsedJson[field]?.ToString() ?? "";
                    mappedDict[label] = value;
                }

                if (dialog.FilterIndex == 1) // CSV
                {
                    var lines = new List<string>
            {
                string.Join(",", mappedDict.Keys),
                string.Join(",", mappedDict.Values.Select(v => $"\"{v}\""))
            };
                    File.WriteAllLines(dialog.FileName, lines);
                }
                else // JSON
                {
                    File.WriteAllText(dialog.FileName, JsonConvert.SerializeObject(mappedDict, Formatting.Indented));
                }

                MessageBox.Show("Report saved successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RunScript_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Text = combinedOutput;
            rawOutput = combinedOutput;
            formattedJson = "";
            JsonGrid.Visibility = Visibility.Collapsed;

            if (config["output"]?["type"]?.ToString() == "json")
            {
                try
                {
                    var parsedJson = JToken.Parse(output.Trim());
                    formattedJson = parsedJson.ToString(Formatting.Indented);

                    if (parsedJson is JArray array)
                    {
                        JsonGrid.ItemsSource = array.Select(token => token.ToObject<Dictionary<string, object>>()).ToList();
                        JsonGrid.Visibility = Visibility.Visible;
                    }
                    else if (parsedJson is JObject obj)
                    {
                        JsonGrid.ItemsSource = new List<Dictionary<string, object>> { obj.ToObject<Dictionary<string, object>>() };
                        JsonGrid.Visibility = Visibility.Visible;
                    }

                    if (ToggleView.IsChecked == true)
                        OutputBox.Text = rawOutput;
                    else
                        OutputBox.Text = formattedJson;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to parse JSON: {ex.Message}", "Parse Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

        }

        private readonly string githubApiUrl = "https://api.github.com/repos/your-username/your-repo/contents/scripts";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            LoadScriptList();
        }

        private void LoadScriptList()
        {
            using var client = new WebClient();
            client.Headers.Add("User-Agent", "WPF-App");
            string response = client.DownloadString(githubApiUrl);

            var files = JArray.Parse(response);
            foreach (var file in files)
            {
                string name = file["name"]?.ToString();
                string downloadUrl = file["download_url"]?.ToString();
                if (name.EndsWith(".json"))
                {
                    ScriptSelector.Items.Add(new ComboBoxItem
                    {
                        Content = name,
                        Tag = downloadUrl
                    });
                }
            }
        }

        private void ScriptSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScriptSelector.SelectedItem is ComboBoxItem item && item.Tag is string jsonUrl)
            {
                LoadScriptConfiguration(jsonUrl);
            }
        }

        private void LoadScriptConfiguration(string jsonUrl)
        {
            InputFields.Children.Clear();
            inputControls.Clear();

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

    }
}
