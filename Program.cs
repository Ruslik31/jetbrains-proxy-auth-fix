using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace ASProxyFix
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ModernMainForm());
        }
    }

    public class ModernMainForm : Form
    {
        private ComboBox cmbLang;
        private TextBox txtHost;
        private TextBox txtPort;
        private Button btnApply;
        private Button btnRestore;
        private Button btnHelp;
        private Label lblTitle;
        private Label lblDesc;
        private Label lblHost;
        private Label lblPort;
        private Label lblPortHint;
        private Label lblStatus;

        private int currentLang = 0; // 0 = RU, 1 = EN, 2 = ZH

        private readonly string[][] i18n = new string[][]
        {
            // 0: RU
            new string[] {
                "Android Studio Proxy Fixer", // 0: Win Title
                "Исправление авторизации прокси", // 1: Header
                "Устранение бесконечного диалога Proxy Authentication (IJPL-37161 / Bug 348084787)", // 2: Desc
                "Хост прокси:", // 3: Host
                "HTTP Порт:", // 4: Port
                "(10809 для v2ray/Xray, 7890 для Clash)", // 5: Port hint
                "⚡ Применить исправление", // 6: Apply
                "↺ Сбросить настройки", // 7: Restore
                "📖 Инструкция (HTML)", // 8: Help
                "Готово к работе. Закройте Android Studio перед применением.", // 9: Ready
                "Укажите хост и порт!", // 10: Warn empty
                "Успешно применено к {0} версиям Android Studio!\n\n1. В студии: Settings -> HTTP Proxy выберите 'No proxy'.\n2. Перезапустите Android Studio.", // 11: Success msg
                "Готово", // 12: Success title
                "Параметры прокси успешно сброшены.", // 13: Reset msg
                "Ошибка: " // 14: Error prefix
            },
            // 1: EN
            new string[] {
                "Android Studio Proxy Fixer",
                "Proxy Authentication Fixer",
                "Permanently resolve recurring Proxy Authentication popup (IJPL-37161 / Bug 348084787)",
                "Proxy Host:",
                "HTTP Port:",
                "(10809 for v2ray/Xray, 7890 for Clash)",
                "⚡ Apply Fix",
                "↺ Restore Defaults",
                "📖 Guide (HTML)",
                "Ready. Close Android Studio before applying.",
                "Please specify host and port!",
                "Successfully applied to {0} Android Studio installation(s)!\n\n1. In Studio: Settings -> HTTP Proxy -> select 'No proxy'.\n2. Restart Android Studio.",
                "Success",
                "Proxy settings successfully reset.",
                "Error: "
            },
            // 2: ZH
            new string[] {
                "Android Studio 代理弹窗修复工具",
                "代理认证弹窗修复",
                "彻底解决无限弹出 Proxy Authentication 验证窗口问题 (IJPL-37161 / Bug 348084787)",
                "代理主机:",
                "HTTP 端口:",
                "(v2ray/Xray 为 10809，Clash 为 7890)",
                "⚡ 一键应用修复",
                "↺ 恢复默认配置",
                "📖 查看详细指南",
                "就绪。应用前请先关闭 Android Studio。",
                "请输入代理主机和端口！",
                "已成功应用到 {0} 个版本的 Android Studio 配置中！\n\n1. 打开 Android Studio: Settings -> HTTP Proxy 选择 'No proxy'。\n2. 重启 Android Studio 即可。",
                "完成",
                "代理设置已成功重置恢复。",
                "错误: "
            }
        };

        public ModernMainForm()
        {
            this.Text = "Android Studio Proxy Auth Fixer";
            this.Size = new Size(540, 390);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(18, 24, 38);
            this.ForeColor = Color.FromArgb(240, 245, 250);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Top Header Bar
            Panel topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(26, 36, 56),
                Padding = new Padding(24, 15, 24, 15)
            };

            lblTitle = new Label
            {
                Text = i18n[0][1],
                Font = new Font("Segoe UI Semibold", 13.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(22, 14),
                AutoSize = true
            };

            lblDesc = new Label
            {
                Text = i18n[0][2],
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(150, 165, 185),
                Location = new Point(24, 45),
                Size = new Size(360, 32)
            };

            cmbLang = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(410, 18),
                Size = new Size(95, 26),
                BackColor = Color.FromArgb(32, 44, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbLang.Items.AddRange(new object[] { "🇷🇺 RU", "🇬🇧 EN", "🇨🇳 ZH" });
            cmbLang.SelectedIndex = 0;
            cmbLang.SelectedIndexChanged += (s, e) => SwitchLanguage(cmbLang.SelectedIndex);

            btnHelp = new Button
            {
                Text = "📖 Guide",
                Location = new Point(410, 48),
                Size = new Size(95, 26),
                BackColor = Color.FromArgb(38, 54, 82),
                ForeColor = Color.FromArgb(56, 189, 248),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnHelp.FlatAppearance.BorderColor = Color.FromArgb(56, 189, 248);
            btnHelp.Click += BtnHelp_Click;

            topBar.Controls.Add(lblTitle);
            topBar.Controls.Add(lblDesc);
            topBar.Controls.Add(cmbLang);
            topBar.Controls.Add(btnHelp);

            // Content Panel
            Panel contentPanel = new Panel
            {
                Location = new Point(24, 100),
                Size = new Size(480, 130),
                BackColor = Color.FromArgb(24, 32, 50),
                Padding = new Padding(15)
            };

            lblHost = new Label
            {
                Text = i18n[0][3],
                Location = new Point(20, 25),
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 215, 235)
            };

            txtHost = new TextBox
            {
                Text = "127.0.0.1",
                Location = new Point(135, 22),
                Size = new Size(160, 26),
                BackColor = Color.FromArgb(15, 20, 32),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPort = new Label
            {
                Text = i18n[0][4],
                Location = new Point(20, 68),
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 215, 235)
            };

            txtPort = new TextBox
            {
                Text = "10809",
                Location = new Point(135, 65),
                Size = new Size(85, 26),
                BackColor = Color.FromArgb(15, 20, 32),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPortHint = new Label
            {
                Text = i18n[0][5],
                Location = new Point(230, 68),
                AutoSize = true,
                ForeColor = Color.FromArgb(120, 135, 155),
                Font = new Font("Segoe UI", 8.5f)
            };

            contentPanel.Controls.Add(lblHost);
            contentPanel.Controls.Add(txtHost);
            contentPanel.Controls.Add(lblPort);
            contentPanel.Controls.Add(txtPort);
            contentPanel.Controls.Add(lblPortHint);

            // Action Buttons
            btnApply = new Button
            {
                Text = i18n[0][6],
                Location = new Point(24, 245),
                Size = new Size(290, 42),
                BackColor = Color.FromArgb(14, 165, 233),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += BtnApply_Click;

            btnRestore = new Button
            {
                Text = i18n[0][7],
                Location = new Point(325, 245),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(32, 42, 64),
                ForeColor = Color.FromArgb(200, 215, 235),
                Font = new Font("Segoe UI", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRestore.FlatAppearance.BorderColor = Color.FromArgb(60, 75, 105);
            btnRestore.Click += BtnRestore_Click;

            // Status Bar
            lblStatus = new Label
            {
                Text = i18n[0][9],
                Location = new Point(24, 305),
                Size = new Size(480, 40),
                ForeColor = Color.FromArgb(56, 189, 248),
                Font = new Font("Segoe UI", 8.8f)
            };

            this.Controls.Add(topBar);
            this.Controls.Add(contentPanel);
            this.Controls.Add(btnApply);
            this.Controls.Add(btnRestore);
            this.Controls.Add(lblStatus);
        }

        private void SwitchLanguage(int langIdx)
        {
            currentLang = langIdx;
            string[] t = i18n[langIdx];

            this.Text = t[0];
            lblTitle.Text = t[1];
            lblDesc.Text = t[2];
            lblHost.Text = t[3];
            lblPort.Text = t[4];
            lblPortHint.Text = t[5];
            btnApply.Text = t[6];
            btnRestore.Text = t[7];
            btnHelp.Text = (langIdx == 0 ? "📖 Инструкция" : (langIdx == 1 ? "📖 Guide" : "📖 说明指南"));
            lblStatus.Text = t[9];
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string htmlPath = Path.Combine(baseDir, "instruction.html");
                if (File.Exists(htmlPath))
                {
                    Process.Start(new ProcessStartInfo(htmlPath) { UseShellExecute = true });
                }
                else
                {
                    Process.Start(new ProcessStartInfo("https://github.com/Ruslik31/android-studio-proxy-auth-fix") { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open guide: " + ex.Message);
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            string host = txtHost.Text.Trim();
            string port = txtPort.Text.Trim();

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port))
            {
                MessageBox.Show(i18n[currentLang][10], "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                lblStatus.Text = string.Format(i18n[currentLang][11], modifiedCount);
                lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
                MessageBox.Show(string.Format(i18n[currentLang][11], modifiedCount), i18n[currentLang][12], MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = i18n[currentLang][14] + ex.Message;
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                MessageBox.Show(i18n[currentLang][14] + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                lblStatus.Text = i18n[currentLang][13];
                lblStatus.ForeColor = Color.FromArgb(147, 197, 253);
                MessageBox.Show(i18n[currentLang][13], i18n[currentLang][12], MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = i18n[currentLang][14] + ex.Message;
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
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
