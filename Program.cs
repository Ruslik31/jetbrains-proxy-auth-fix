using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace ASProxyFix
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private TextBox txtHost;
        private TextBox txtPort;
        private Button btnApply;
        private Button btnRestore;
        private Label lblStatus;

        public MainForm()
        {
            this.Text = "Android Studio Proxy Auth Fixer";
            this.Size = new Size(460, 310);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f);

            Label lblDesc = new Label
            {
                Text = "Устранение бага с бесконечным окном авторизации прокси в Android Studio / IntelliJ IDEA.",
                Location = new Point(20, 15),
                Size = new Size(405, 45),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            Label lblHost = new Label { Text = "Хост прокси:", Location = new Point(20, 75), AutoSize = true };
            txtHost = new TextBox { Text = "127.0.0.1", Location = new Point(130, 72), Size = new Size(160, 25) };

            Label lblPort = new Label { Text = "HTTP Порт:", Location = new Point(20, 115), AutoSize = true };
            txtPort = new TextBox { Text = "10809", Location = new Point(130, 112), Size = new Size(90, 25) };

            Label lblHint = new Label
            {
                Text = "(10809 для v2ray/Xray, 7890 для Clash)",
                Location = new Point(230, 115),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.5f)
            };

            btnApply = new Button
            {
                Text = "Применить исправление",
                Location = new Point(20, 160),
                Size = new Size(240, 38),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnApply.Click += BtnApply_Click;

            btnRestore = new Button
            {
                Text = "Сбросить всё",
                Location = new Point(275, 160),
                Size = new Size(150, 38),
                FlatStyle = FlatStyle.Flat
            };
            btnRestore.Click += BtnRestore_Click;

            lblStatus = new Label
            {
                Text = "Готово к работе.",
                Location = new Point(20, 215),
                Size = new Size(405, 45),
                ForeColor = Color.DarkSlateGray
            };

            this.Controls.Add(lblDesc);
            this.Controls.Add(lblHost);
            this.Controls.Add(txtHost);
            this.Controls.Add(lblPort);
            this.Controls.Add(txtPort);
            this.Controls.Add(lblHint);
            this.Controls.Add(btnApply);
            this.Controls.Add(btnRestore);
            this.Controls.Add(lblStatus);
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            string host = txtHost.Text.Trim();
            string port = txtPort.Text.Trim();

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port))
            {
                MessageBox.Show("Укажите хост и порт прокси!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int modifiedCount = 0;
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string googleDir = Path.Combine(appData, "Google");

                if (Directory.Exists(googleDir))
                {
                    string[] asDirs = Directory.GetDirectories(googleDir, "AndroidStudio*");
                    foreach (string dir in asDirs)
                    {
                        string vmFile = Path.Combine(dir, "studio64.exe.vmoptions");
                        string vmContent = File.Exists(vmFile) ? File.ReadAllText(vmFile) : "";
                        vmContent = RemoveProxyLines(vmContent);
                        vmContent += string.Format("\r\n-Dhttp.proxyHost={0}\r\n-Dhttp.proxyPort={1}\r\n-Dhttps.proxyHost={0}\r\n-Dhttps.proxyPort={1}\r\n-Dhttp.nonProxyHosts=localhost|127.0.0.1\r\n-Djdk.http.auth.tunneling.disabledSchemes=\"\"\r\n-Djdk.http.auth.proxying.disabledSchemes=\"\"\r\n", host, port);
                        File.WriteAllText(vmFile, vmContent.Trim() + "\r\n", Encoding.UTF8);

                        string optDir = Path.Combine(dir, "options");
                        if (!Directory.Exists(optDir)) Directory.CreateDirectory(optDir);
                        string proxyXml = Path.Combine(optDir, "proxy.settings.xml");
                        string xmlText = "<application>\r\n  <component name=\"HttpConfigurable\">\r\n    <option name=\"USE_HTTP_PROXY\" value=\"false\" />\r\n    <option name=\"USE_PROXY_PAC\" value=\"false\" />\r\n    <option name=\"PROXY_AUTHENTICATION\" value=\"false\" />\r\n    <option name=\"KEEP_PROXY_PASSWORD\" value=\"false\" />\r\n  </component>\r\n</application>";
                        File.WriteAllText(proxyXml, xmlText, Encoding.UTF8);

                        string pwdFile = Path.Combine(optDir, "proxy.settings.pwd");
                        if (File.Exists(pwdFile)) File.Delete(pwdFile);

                        modifiedCount++;
                    }
                }

                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string gradleDir = Path.Combine(userProfile, ".gradle");
                if (!Directory.Exists(gradleDir)) Directory.CreateDirectory(gradleDir);
                string gradleProps = Path.Combine(gradleDir, "gradle.properties");

                string gContent = File.Exists(gradleProps) ? File.ReadAllText(gradleProps) : "";
                gContent = RemoveGradleProxyLines(gContent);
                gContent += string.Format("\r\nsystemProp.http.proxyHost={0}\r\nsystemProp.http.proxyPort={1}\r\nsystemProp.https.proxyHost={0}\r\nsystemProp.https.proxyPort={1}\r\nsystemProp.http.nonProxyHosts=localhost|127.0.0.1\r\nsystemProp.https.nonProxyHosts=localhost|127.0.0.1\r\n", host, port);
                File.WriteAllText(gradleProps, gContent.Trim() + "\r\n", Encoding.UTF8);

                lblStatus.Text = string.Format("Применено к {0} версиям Android Studio!\r\nПерезапустите студию.", modifiedCount);
                lblStatus.ForeColor = Color.DarkGreen;
                MessageBox.Show("Исправление успешно применено!\n\n1. В студии: Settings -> HTTP Proxy выберите 'No proxy'.\n2. Перезапустите Android Studio.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Ошибка: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string googleDir = Path.Combine(appData, "Google");

                if (Directory.Exists(googleDir))
                {
                    string[] asDirs = Directory.GetDirectories(googleDir, "AndroidStudio*");
                    foreach (string dir in asDirs)
                    {
                        string vmFile = Path.Combine(dir, "studio64.exe.vmoptions");
                        if (File.Exists(vmFile))
                        {
                            string vmContent = RemoveProxyLines(File.ReadAllText(vmFile));
                            File.WriteAllText(vmFile, vmContent.Trim() + "\r\n", Encoding.UTF8);
                        }
                    }
                }

                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string gradleProps = Path.Combine(userProfile, ".gradle", "gradle.properties");
                if (File.Exists(gradleProps))
                {
                    string gContent = RemoveGradleProxyLines(File.ReadAllText(gradleProps));
                    File.WriteAllText(gradleProps, gContent.Trim() + "\r\n", Encoding.UTF8);
                }

                lblStatus.Text = "Параметры прокси сброшены.";
                lblStatus.ForeColor = Color.Blue;
                MessageBox.Show("Настройки прокси успешно сброшены.", "Сброс", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Ошибка: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
        }

        private string RemoveProxyLines(string text)
        {
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (line.StartsWith("-Dhttp.proxy") || line.StartsWith("-Dhttps.proxy") ||
                    line.StartsWith("-Dhttp.nonProxy") || line.StartsWith("-Djdk.http.auth"))
                    continue;
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        private string RemoveGradleProxyLines(string text)
        {
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (line.StartsWith("systemProp.http.") || line.StartsWith("systemProp.https."))
                    continue;
                sb.AppendLine(line);
            }
            return sb.ToString();
        }
    }
}
