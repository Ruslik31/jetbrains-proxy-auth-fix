using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Globalization;
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
            Application.Run(new UniversalMainForm());
        }
    }

    public class IdeProduct
    {
        public string Name { get; set; }
        public string BaseVendorDir { get; set; } // "Google" or "JetBrains"
        public string DirPattern { get; set; }
        public string VmOptionsName { get; set; }

        public IdeProduct(string name, string vendor, string pattern, string vmOptions)
        {
            Name = name;
            BaseVendorDir = vendor;
            DirPattern = pattern;
            VmOptionsName = vmOptions;
        }

        public override string ToString() { return Name; }
    }

    public class UniversalMainForm : Form
    {
        private ComboBox cmbLang;
        private CheckedListBox chkListIdes;
        private TextBox txtHost;
        private TextBox txtPort;
        private Button btnApply;
        private Button btnRestore;
        private Button btnHelp;
        private Label lblTitle;
        private Label lblDesc;
        private Label lblSelectIde;
        private Label lblHost;
        private Label lblPort;
        private Label lblPortHint;
        private Label lblStatus;
        private LinkLabel lnkGitHub;

        private int currentLang = 1; // 0 = RU, 1 = EN, 2 = ZH

        private List<IdeProduct> allProducts = new List<IdeProduct>
        {
            new IdeProduct("Android Studio", "Google", "AndroidStudio*", "studio64.exe.vmoptions"),
            new IdeProduct("IntelliJ IDEA", "JetBrains", "IntelliJIdea*", "idea64.exe.vmoptions"),
            new IdeProduct("PyCharm", "JetBrains", "PyCharm*", "pycharm64.exe.vmoptions"),
            new IdeProduct("WebStorm", "JetBrains", "WebStorm*", "webstorm64.exe.vmoptions"),
            new IdeProduct("CLion", "JetBrains", "CLion*", "clion64.exe.vmoptions"),
            new IdeProduct("Rider", "JetBrains", "Rider*", "rider64.exe.vmoptions"),
            new IdeProduct("GoLand", "JetBrains", "GoLand*", "goland64.exe.vmoptions"),
            new IdeProduct("PhpStorm", "JetBrains", "PhpStorm*", "phpstorm64.exe.vmoptions"),
            new IdeProduct("DataGrip", "JetBrains", "DataGrip*", "datagrip64.exe.vmoptions"),
            new IdeProduct("RustRover", "JetBrains", "RustRover*", "rustrover64.exe.vmoptions")
        };

        private readonly string[][] i18n = new string[][]
        {
            // 0: RU
            new string[] {
                "JetBrains & Android Studio Proxy Fixer", // 0: Win Title
                "Универсальный фикс авторизации прокси", // 1: Header
                "Устранение бага с окном Proxy Authentication для IDE на платформе IntelliJ (IJPL-37161)", // 2: Desc
                "Выберите целевые IDE:", // 3: Select IDEs
                "Хост прокси:", // 4: Host
                "HTTP Порт:", // 5: Port
                "(10809 для v2ray/Xray, 7890 для Clash)", // 6: Port hint
                "⚡ Применить исправление", // 7: Apply
                "↺ Сбросить настройки", // 8: Restore
                "📖 Инструкция", // 9: Help
                "Готово к работе. Закройте выбранные IDE перед применением.", // 10: Ready
                "Укажите хост и порт!", // 11: Warn empty
                "Выберите хотя бы одну IDE из списка!", // 12: Warn no IDE
                "Успешно применено к {0} конфигурациям IDE!\n\n1. В настройках IDE (Settings -> HTTP Proxy) выберите 'No proxy'.\n2. Перезапустите среду разработки.", // 13: Success msg
                "Готово", // 14: Success title
                "Параметры прокси успешно сброшены.", // 15: Reset msg
                "Ошибка: " // 16: Error prefix
            },
            // 1: EN
            new string[] {
                "JetBrains & Android Studio Proxy Fixer",
                "Universal Proxy Authentication Fixer",
                "Resolve recurring Proxy Authentication popup across IntelliJ Platform IDEs (IJPL-37161)",
                "Select Target IDEs:",
                "Proxy Host:",
                "HTTP Port:",
                "(10809 for v2ray/Xray, 7890 for Clash)",
                "⚡ Apply Fix",
                "↺ Restore Defaults",
                "📖 Guide",
                "Ready. Close selected IDEs before applying.",
                "Please specify host and port!",
                "Please select at least one IDE from the list!",
                "Successfully applied to {0} IDE configuration(s)!\n\n1. In IDE: Settings -> HTTP Proxy -> select 'No proxy'.\n2. Restart the IDE.",
                "Success",
                "Proxy settings successfully reset.",
                "Error: "
            },
            // 2: ZH
            new string[] {
                "JetBrains & Android Studio 代理弹窗修复工具",
                "通用代理认证弹窗修复",
                "彻底解决 IntelliJ 全系列 IDE 无限弹出 Proxy Authentication 窗口问题 (IJPL-37161)",
                "选择目标 IDE:",
                "代理主机:",
                "HTTP 端口:",
                "(v2ray/Xray 为 10809，Clash 为 7890)",
                "⚡ 一键应用修复",
                "↺ 恢复默认配置",
                "📖 说明指南",
                "就绪。应用前请先关闭正在运行的 IDE。",
                "请输入代理主机和端口！",
                "请至少从列表中勾选一款 IDE！",
                "已成功应用到 {0} 个 IDE 配置目录中！\n\n1. 打开 IDE: Settings -> HTTP Proxy 选择 'No proxy'。\n2. 重启 IDE 即可生效。",
                "完成",
                "代理设置已成功重置恢复。",
                "错误: "
            }
        };

        public UniversalMainForm()
        {
            string twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (twoLetter == "ru" || twoLetter == "be" || twoLetter == "uk") currentLang = 0;
            else if (twoLetter == "zh") currentLang = 2;
            else currentLang = 1;

            this.Text = i18n[currentLang][0];
            this.Size = new Size(560, 520);
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
                Text = i18n[currentLang][1],
                Font = new Font("Segoe UI Semibold", 13.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(22, 14),
                AutoSize = true
            };

            lblDesc = new Label
            {
                Text = i18n[currentLang][2],
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(150, 165, 185),
                Location = new Point(24, 45),
                Size = new Size(370, 32)
            };

            cmbLang = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(430, 18),
                Size = new Size(95, 26),
                BackColor = Color.FromArgb(32, 44, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbLang.Items.AddRange(new object[] { "🇷🇺 RU", "🇬🇧 EN", "🇨🇳 ZH" });
            cmbLang.SelectedIndex = currentLang;
            cmbLang.SelectedIndexChanged += (s, e) => SwitchLanguage(cmbLang.SelectedIndex);

            btnHelp = new Button
            {
                Text = i18n[currentLang][9],
                Location = new Point(430, 48),
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

            // IDE Selection Group
            lblSelectIde = new Label
            {
                Text = i18n[currentLang][3],
                Location = new Point(24, 95),
                AutoSize = true,
                ForeColor = Color.FromArgb(180, 200, 225),
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
            };

            chkListIdes = new CheckedListBox
            {
                Location = new Point(24, 120),
                Size = new Size(500, 95),
                BackColor = Color.FromArgb(24, 32, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true,
                MultiColumn = true,
                ColumnWidth = 160
            };

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            for (int i = 0; i < allProducts.Count; i++)
            {
                chkListIdes.Items.Add(allProducts[i]);
                // Check if installed on disk, or check Android Studio & IDEA by default
                string vendorPath = Path.Combine(appData, allProducts[i].BaseVendorDir);
                bool hasDirs = Directory.Exists(vendorPath) && Directory.GetDirectories(vendorPath, allProducts[i].DirPattern).Length > 0;
                if (hasDirs || i == 0 || i == 1)
                {
                    chkListIdes.SetItemChecked(i, true);
                }
            }

            // Connection Settings Panel
            Panel contentPanel = new Panel
            {
                Location = new Point(24, 225),
                Size = new Size(500, 105),
                BackColor = Color.FromArgb(24, 32, 50),
                Padding = new Padding(15)
            };

            lblHost = new Label
            {
                Text = i18n[currentLang][4],
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 215, 235)
            };

            txtHost = new TextBox
            {
                Text = "127.0.0.1",
                Location = new Point(135, 17),
                Size = new Size(160, 26),
                BackColor = Color.FromArgb(15, 20, 32),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPort = new Label
            {
                Text = i18n[currentLang][5],
                Location = new Point(20, 58),
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 215, 235)
            };

            txtPort = new TextBox
            {
                Text = "10809",
                Location = new Point(135, 55),
                Size = new Size(85, 26),
                BackColor = Color.FromArgb(15, 20, 32),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPortHint = new Label
            {
                Text = i18n[currentLang][6],
                Location = new Point(230, 58),
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
                Text = i18n[currentLang][7],
                Location = new Point(24, 342),
                Size = new Size(305, 42),
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
                Text = i18n[currentLang][8],
                Location = new Point(340, 342),
                Size = new Size(184, 42),
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
                Text = i18n[currentLang][10],
                Location = new Point(24, 395),
                Size = new Size(500, 42),
                ForeColor = Color.FromArgb(56, 189, 248),
                Font = new Font("Segoe UI", 8.8f)
            };

            // Bottom Footer Link
            lnkGitHub = new LinkLabel
            {
                Text = "GitHub: Ruslik31/jetbrains-proxy-auth-fix  •  PR #3770",
                Location = new Point(24, 445),
                Size = new Size(500, 20),
                LinkColor = Color.FromArgb(100, 140, 180),
                ActiveLinkColor = Color.FromArgb(56, 189, 248),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.2f)
            };
            lnkGitHub.LinkClicked += (s, e) => {
                Process.Start(new ProcessStartInfo("https://github.com/Ruslik31/jetbrains-proxy-auth-fix") { UseShellExecute = true });
            };

            this.Controls.Add(topBar);
            this.Controls.Add(lblSelectIde);
            this.Controls.Add(chkListIdes);
            this.Controls.Add(contentPanel);
            this.Controls.Add(btnApply);
            this.Controls.Add(btnRestore);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lnkGitHub);
        }

        private void SwitchLanguage(int langIdx)
        {
            currentLang = langIdx;
            string[] t = i18n[langIdx];

            this.Text = t[0];
            lblTitle.Text = t[1];
            lblDesc.Text = t[2];
            lblSelectIde.Text = t[3];
            lblHost.Text = t[4];
            lblPort.Text = t[5];
            lblPortHint.Text = t[6];
            btnApply.Text = t[7];
            btnRestore.Text = t[8];
            btnHelp.Text = t[9];
            lblStatus.Text = t[10];
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            try
            {
                string tempHtml = Path.Combine(Path.GetTempPath(), "JetBrains_Proxy_Guide.html");
                File.WriteAllText(tempHtml, EmbeddedGuide.HtmlContent, Encoding.UTF8);
                Process.Start(new ProcessStartInfo(tempHtml) { UseShellExecute = true });
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
                MessageBox.Show(i18n[currentLang][11], "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (chkListIdes.CheckedItems.Count == 0)
            {
                MessageBox.Show(i18n[currentLang][12], "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int modifiedCount = 0;
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                foreach (IdeProduct prod in chkListIdes.CheckedItems)
                {
                    string vendorDir = Path.Combine(appData, prod.BaseVendorDir);
                    if (!Directory.Exists(vendorDir)) continue;

                    string[] dirs = Directory.GetDirectories(vendorDir, prod.DirPattern);
                    foreach (string dir in dirs)
                    {
                        // 1. VM Options
                        string vmFile = Path.Combine(dir, prod.VmOptionsName);
                        string vmContent = File.Exists(vmFile) ? File.ReadAllText(vmFile) : "";
                        vmContent = RemoveProxyLines(vmContent);
                        vmContent += string.Format("\r\n-Dhttp.proxyHost={0}\r\n-Dhttp.proxyPort={1}\r\n-Dhttps.proxyHost={0}\r\n-Dhttps.proxyPort={1}\r\n-Dhttp.nonProxyHosts=localhost|127.0.0.1\r\n-Djdk.http.auth.tunneling.disabledSchemes=\"\"\r\n-Djdk.http.auth.proxying.disabledSchemes=\"\"\r\n", host, port);
                        File.WriteAllText(vmFile, vmContent.Trim() + "\r\n", Encoding.UTF8);

                        // 2. proxy.settings.xml
                        string optDir = Path.Combine(dir, "options");
                        if (!Directory.Exists(optDir)) Directory.CreateDirectory(optDir);
                        string proxyXml = Path.Combine(optDir, "proxy.settings.xml");
                        string xmlText = "<application>\r\n  <component name=\"HttpConfigurable\">\r\n    <option name=\"USE_HTTP_PROXY\" value=\"false\" />\r\n    <option name=\"USE_PROXY_PAC\" value=\"false\" />\r\n    <option name=\"PROXY_AUTHENTICATION\" value=\"false\" />\r\n    <option name=\"KEEP_PROXY_PASSWORD\" value=\"false\" />\r\n  </component>\r\n</application>";
                        File.WriteAllText(proxyXml, xmlText, Encoding.UTF8);

                        // 3. Remove .pwd
                        string pwdFile = Path.Combine(optDir, "proxy.settings.pwd");
                        if (File.Exists(pwdFile)) File.Delete(pwdFile);

                        modifiedCount++;
                    }
                }

                // 4. Global gradle.properties (for Android Studio & Gradle-based IntelliJ projects)
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string gradleDir = Path.Combine(userProfile, ".gradle");
                if (!Directory.Exists(gradleDir)) Directory.CreateDirectory(gradleDir);
                string gradleProps = Path.Combine(gradleDir, "gradle.properties");

                string gContent = File.Exists(gradleProps) ? File.ReadAllText(gradleProps) : "";
                gContent = RemoveGradleProxyLines(gContent);
                gContent += string.Format("\r\nsystemProp.http.proxyHost={0}\r\nsystemProp.http.proxyPort={1}\r\nsystemProp.https.proxyHost={0}\r\nsystemProp.https.proxyPort={1}\r\nsystemProp.http.nonProxyHosts=localhost|127.0.0.1\r\nsystemProp.https.nonProxyHosts=localhost|127.0.0.1\r\n", host, port);
                File.WriteAllText(gradleProps, gContent.Trim() + "\r\n", Encoding.UTF8);

                lblStatus.Text = string.Format(i18n[currentLang][13], modifiedCount);
                lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
                MessageBox.Show(string.Format(i18n[currentLang][13], modifiedCount), i18n[currentLang][14], MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = i18n[currentLang][16] + ex.Message;
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                MessageBox.Show(i18n[currentLang][16] + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                foreach (IdeProduct prod in chkListIdes.CheckedItems)
                {
                    string vendorDir = Path.Combine(appData, prod.BaseVendorDir);
                    if (!Directory.Exists(vendorDir)) continue;

                    string[] dirs = Directory.GetDirectories(vendorDir, prod.DirPattern);
                    foreach (string dir in dirs)
                    {
                        string vmFile = Path.Combine(dir, prod.VmOptionsName);
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

                lblStatus.Text = i18n[currentLang][15];
                lblStatus.ForeColor = Color.FromArgb(147, 197, 253);
                MessageBox.Show(i18n[currentLang][15], i18n[currentLang][14], MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStatus.Text = i18n[currentLang][16] + ex.Message;
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

    public static class EmbeddedGuide
    {
        public static string HtmlContent = @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Android Studio Proxy Authentication Fixer Guide</title>
  <style>
    :root {
      --bg: #090d16;
      --card-bg: #131c2e;
      --border: #24344d;
      --accent: #38bdf8;
      --accent-hover: #0ea5e9;
      --success: #22c55e;
      --warning: #f59e0b;
      --text: #f1f5f9;
      --text-muted: #94a3b8;
      --code-bg: #060911;
    }
    * {
      box-sizing: border-box;
      margin: 0;
      padding: 0;
    }
    body {
      font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;
      background-color: var(--bg);
      color: var(--text);
      line-height: 1.6;
      padding: 30px 20px;
    }
    .container {
      max-width: 860px;
      margin: 0 auto;
    }
    .lang-switcher {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      margin-bottom: 20px;
    }
    .lang-btn {
      background: var(--card-bg);
      border: 1px solid var(--border);
      color: var(--text);
      padding: 6px 14px;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.85rem;
      font-weight: 500;
      transition: all 0.2s;
    }
    .lang-btn:hover {
      border-color: var(--accent);
      color: var(--accent);
    }
    .lang-btn.active {
      background: var(--accent);
      color: #000;
      font-weight: 700;
      border-color: var(--accent);
    }
    header {
      text-align: center;
      margin-bottom: 35px;
      padding-bottom: 25px;
      border-bottom: 1px solid var(--border);
    }
    h1 {
      font-size: 2rem;
      color: var(--accent);
      margin-bottom: 12px;
    }
    p.lead {
      color: var(--text-muted);
      font-size: 1.05rem;
    }
    .badge {
      display: inline-block;
      padding: 4px 12px;
      background: rgba(56, 189, 248, 0.12);
      color: var(--accent);
      border-radius: 9999px;
      font-size: 0.85rem;
      font-weight: 600;
      margin-bottom: 14px;
      border: 1px solid rgba(56, 189, 248, 0.25);
    }
    .card {
      background: var(--card-bg);
      border: 1px solid var(--border);
      border-radius: 14px;
      padding: 24px;
      margin-bottom: 24px;
      box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.3);
    }
    .card-highlight {
      border-color: rgba(56, 189, 248, 0.35);
      background: linear-gradient(180deg, #131c2e 0%, #0e1524 100%);
    }
    h2 {
      font-size: 1.35rem;
      margin-bottom: 15px;
      display: flex;
      align-items: center;
      gap: 10px;
      color: #fff;
    }
    .step-num {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      background: var(--accent);
      color: #000;
      font-size: 0.9rem;
      font-weight: 700;
      border-radius: 50%;
    }
    ul, ol {
      margin-left: 22px;
      margin-bottom: 15px;
    }
    li {
      margin-bottom: 8px;
      color: var(--text);
    }
    .tip {
      background: rgba(34, 197, 94, 0.08);
      border-left: 4px solid var(--success);
      padding: 12px 16px;
      border-radius: 6px;
      margin: 15px 0;
      font-size: 0.95rem;
      color: #bbf7d0;
    }
    pre {
      background: var(--code-bg);
      border: 1px solid var(--border);
      border-radius: 8px;
      padding: 14px;
      overflow-x: auto;
      font-family: Consolas, Monaco, ""Courier New"", monospace;
      font-size: 0.88rem;
      color: #38bdf8;
      margin: 12px 0;
    }
    code {
      background: rgba(255, 255, 255, 0.1);
      padding: 2px 6px;
      border-radius: 4px;
      font-family: Consolas, Monaco, monospace;
      font-size: 0.9em;
      color: #e2e8f0;
    }
    .table {
      width: 100%;
      border-collapse: collapse;
      margin: 15px 0;
    }
    .table th, .table td {
      border: 1px solid var(--border);
      padding: 10px;
      text-align: left;
    }
    .table th {
      background: rgba(255, 255, 255, 0.04);
      color: var(--accent);
    }
    .lang-section {
      display: none;
    }
    .lang-section.active {
      display: block;
    }
  </style>
</head>
<body>
  <div class=""container"">
    <div class=""lang-switcher"">
      <button class=""lang-btn active"" onclick=""setLang('en')"">рџ‡¬рџ‡§ English</button>
      <button class=""lang-btn"" onclick=""setLang('ru')"">рџ‡·рџ‡є Р СѓСЃСЃРєРёР№</button>
      <button class=""lang-btn"" onclick=""setLang('zh')"">рџ‡Ёрџ‡і дё­ж–‡</button>
    </div>

    <!-- ENGLISH (DEFAULT) -->
    <div id=""lang-en"" class=""lang-section active"">
      <header>
        <div class=""badge"">Official JetBrains Bug (IJPL-37161) & Google Issue 348084787</div>
        <h1>Android Studio: Fix Recurring Proxy Authentication Dialog</h1>
        <p class=""lead"">Permanently resolve the infinite proxy credential popup loop, even when using an unauthenticated proxy or when ""Remember password"" is checked.</p>
      </header>

      <div class=""card card-highlight"">
        <h2>вљЎ Method 1: Automatic via JetBrains_Proxy_Fix.exe</h2>
        <p>The standalone tool automatically scans all Android Studio installations and patches the configs in 1 click.</p>
        <br>
        <ol>
          <li>Launch <code>JetBrains_Proxy_Fix.exe</code>.</li>
          <li>Verify your proxy HTTP port (default is <strong>10809</strong>):
            <table class=""table"">
              <tr><th>Proxy Client</th><th>Common HTTP Port</th></tr>
              <tr><td>v2rayN / Xray / Incy</td><td><strong>10809</strong></td></tr>
              <tr><td>Clash / Mihomo / Flclash</td><td><strong>7890</strong></td></tr>
              <tr><td>NekoBox / Hiddify</td><td><strong>2080</strong> or <strong>7890</strong></td></tr>
            </table>
          </li>
          <li>Click <strong>""Apply Fix""</strong>.</li>
          <li>In Android Studio: <code>Settings (Ctrl+Alt+S)</code> в†’ <code>System Settings</code> в†’ <code>HTTP Proxy</code> and select <strong>""No proxy""</strong> (disables the bugged Swing dialog).</li>
          <li>Restart Android Studio. If prompted to keep Gradle proxy settings in <code>gradle.properties</code>, select <strong>""Yes""</strong>.</li>
        </ol>
        <div class=""tip"">
          <strong>Done!</strong> Traffic routes cleanly via JVM and Gradle without modal interruptions.
        </div>
      </div>

      <div class=""card"">
        <h2>рџ› пёЏ Method 2: Manual Configuration</h2>
        <div style=""margin-top: 15px;"">
          <h3><span class=""step-num"">1</span> Disable GUI Proxy</h3>
          <p>In Android Studio: <strong>Settings в†’ Appearance & Behavior в†’ System Settings в†’ HTTP Proxy</strong>, choose <strong>""No proxy""</strong> and click <strong>OK</strong>.</p>
        </div>
        <div style=""margin-top: 20px;"">
          <h3><span class=""step-num"">2</span> Supply Proxy via Custom VM Options</h3>
          <p>Go to <strong>Help в†’ Edit Custom VM Options...</strong> and append:</p>
          <pre>-Dhttp.proxyHost=127.0.0.1
-Dhttp.proxyPort=10809
-Dhttps.proxyHost=127.0.0.1
-Dhttps.proxyPort=10809
-Dhttp.nonProxyHosts=localhost|127.0.0.1
-Djdk.http.auth.tunneling.disabledSchemes=""""
-Djdk.http.auth.proxying.disabledSchemes=""""</pre>
        </div>
        <div style=""margin-top: 20px;"">
          <h3><span class=""step-num"">3</span> Configure Gradle</h3>
          <p>In <code>C:\Users\&lt;User&gt;\.gradle\gradle.properties</code> add:</p>
          <pre>systemProp.http.proxyHost=127.0.0.1
systemProp.http.proxyPort=10809
systemProp.https.proxyHost=127.0.0.1
systemProp.https.proxyPort=10809
systemProp.http.nonProxyHosts=localhost|127.0.0.1
systemProp.https.nonProxyHosts=localhost|127.0.0.1</pre>
        </div>
      </div>

      <div class=""card"">
        <h2>рџ§  Root Cause Explanation</h2>
        <p>In IntelliJ Platform, <code>IdeProxyAuthenticator</code> requests credentials from <code>PasswordSafe</code>. When authentication is disabled, it returns <code>null</code>. The IDE mistakenly treats <code>null</code> as ""credentials not yet entered"", continuously triggering <code>AuthenticationDialog</code>.</p>
        <p>Configuring proxy via JVM system properties bypasses <code>PlatformProxyAuthentication</code> entirely.</p>
      </div>
    </div>

    <!-- RUSSIAN -->
    <div id=""lang-ru"" class=""lang-section"">
      <header>
        <div class=""badge"">РћС„РёС†РёР°Р»СЊРЅС‹Р№ Р±Р°Рі JetBrains (IJPL-37161) & Google (Issue 348084787)</div>
        <h1>Android Studio: РСЃРїСЂР°РІР»РµРЅРёРµ Р±Р°РіР° СЃ РѕРєРЅРѕРј Proxy Authentication</h1>
        <p class=""lead"">РљР°Рє РЅР°РІСЃРµРіРґР° СѓР±СЂР°С‚СЊ Р±РµСЃРєРѕРЅРµС‡РЅРѕ РІСЃРїР»С‹РІР°СЋС‰РµРµ РѕРєРЅРѕ Р»РѕРіРёРЅР° Рё РїР°СЂРѕР»СЏ РїСЂРѕРєСЃРё, РґР°Р¶Рµ РµСЃР»Рё РїСЂРѕРєСЃРё РЅРµ С‚СЂРµР±СѓРµС‚ Р°РІС‚РѕСЂРёР·Р°С†РёРё РёР»Рё РЅР°Р¶Р°С‚Р° РіР°Р»РѕС‡РєР° В«RememberВ».</p>
      </header>

      <div class=""card card-highlight"">
        <h2>вљЎ РЎРїРѕСЃРѕР± 1: РђРІС‚РѕРјР°С‚РёС‡РµСЃРєРё С‡РµСЂРµР· JetBrains_Proxy_Fix.exe</h2>
        <p>РЈС‚РёР»РёС‚Р° СЃР°РјР° РЅР°С…РѕРґРёС‚ РІСЃРµ РІРµСЂСЃРёРё Android Studio РЅР° РІР°С€РµРј РєРѕРјРїСЊСЋС‚РµСЂРµ Рё РїСЂРёРјРµРЅСЏРµС‚ РёСЃРїСЂР°РІР»РµРЅРёРµ РІ 1 РєР»РёРє.</p>
        <br>
        <ol>
          <li>Р—Р°РїСѓСЃС‚РёС‚Рµ <code>JetBrains_Proxy_Fix.exe</code>.</li>
          <li>РЈРєР°Р¶РёС‚Рµ РїРѕСЂС‚ РІР°С€РµРіРѕ РїСЂРѕРєСЃРё (РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ <strong>10809</strong>):
            <table class=""table"">
              <tr><th>РџСЂРѕРіСЂР°РјРјР° РїСЂРѕРєСЃРё</th><th>РћР±С‹С‡РЅС‹Р№ HTTP-РїРѕСЂС‚</th></tr>
              <tr><td>v2rayN / Xray / Incy</td><td><strong>10809</strong></td></tr>
              <tr><td>Clash / Mihomo / Flclash</td><td><strong>7890</strong></td></tr>
              <tr><td>NekoBox / Hiddify</td><td><strong>2080</strong> РёР»Рё <strong>7890</strong></td></tr>
            </table>
          </li>
          <li>РќР°Р¶РјРёС‚Рµ РєРЅРѕРїРєСѓ <strong>В«РџСЂРёРјРµРЅРёС‚СЊ РёСЃРїСЂР°РІР»РµРЅРёРµВ»</strong>.</li>
          <li>Р’ Android Studio: <code>Settings (Ctrl+Alt+S)</code> в†’ <code>System Settings</code> в†’ <code>HTTP Proxy</code> Рё РІС‹Р±РµСЂРёС‚Рµ <strong>В«No proxyВ»</strong> (СЌС‚Рѕ РѕС‚РєР»СЋС‡Р°РµС‚ РїСЂРѕР±Р»РµРјРЅС‹Р№ Swing-РґРёР°Р»РѕРі).</li>
          <li>РџРµСЂРµР·Р°РїСѓСЃС‚РёС‚Рµ Android Studio. Р•СЃР»Рё РїРѕСЏРІРёС‚СЃСЏ РѕРєРЅРѕ СЃ РІРѕРїСЂРѕСЃРѕРј Рѕ СЃРѕС…СЂР°РЅРµРЅРёРё РЅР°СЃС‚СЂРѕРµРє РІ <code>gradle.properties</code> вЂ” РЅР°Р¶РјРёС‚Рµ <strong>В«YesВ»</strong>.</li>
        </ol>
        <div class=""tip"">
          <strong>Р“РѕС‚РѕРІРѕ!</strong> РџСЂРѕРєСЃРё СЂР°Р±РѕС‚Р°РµС‚ РЅР°РїСЂСЏРјСѓСЋ С‡РµСЂРµР· РїР°СЂР°РјРµС‚СЂС‹ JVM Рё Gradle, Р° РґРёР°Р»РѕРі Р°РІС‚РѕСЂРёР·Р°С†РёРё Р±РѕР»СЊС€Рµ РЅРµ РїРѕСЏРІРёС‚СЃСЏ.
        </div>
      </div>

      <div class=""card"">
        <h2>рџ› пёЏ РЎРїРѕСЃРѕР± 2: Р СѓС‡РЅРѕР№ СЃРїРѕСЃРѕР±</h2>
        <div style=""margin-top: 15px;"">
          <h3><span class=""step-num"">1</span> РћС‚РєР»СЋС‡РёС‚Рµ РїСЂРѕРєСЃРё РІ РёРЅС‚РµСЂС„РµР№СЃРµ</h3>
          <p>Р’ Android Studio РїРµСЂРµР№РґРёС‚Рµ РІ <strong>Settings в†’ Appearance & Behavior в†’ System Settings в†’ HTTP Proxy</strong>, РІС‹Р±РµСЂРёС‚Рµ <strong>В«No proxyВ»</strong> Рё РЅР°Р¶РјРёС‚Рµ <strong>OK</strong>.</p>
        </div>
        <div style=""margin-top: 20px;"">
          <h3><span class=""step-num"">2</span> Р”РѕР±Р°РІСЊС‚Рµ РїСЂРѕРєСЃРё РІ VM Options</h3>
          <p>Р’ РјРµРЅСЋ РЅР°Р¶РјРёС‚Рµ <strong>Help в†’ Edit Custom VM Options...</strong> Рё РґРѕР±Р°РІСЊС‚Рµ РІ РєРѕРЅРµС†:</p>
          <pre>-Dhttp.proxyHost=127.0.0.1
-Dhttp.proxyPort=10809
-Dhttps.proxyHost=127.0.0.1
-Dhttps.proxyPort=10809
-Dhttp.nonProxyHosts=localhost|127.0.0.1
-Djdk.http.auth.tunneling.disabledSchemes=""""
-Djdk.http.auth.proxying.disabledSchemes=""""</pre>
        </div>
        <div style=""margin-top: 20px;"">
          <h3><span class=""step-num"">3</span> РќР°СЃС‚СЂРѕР№С‚Рµ Gradle</h3>
          <p>Р’ С„Р°Р№Р»Рµ <code>C:\Users\&lt;РРјСЏ&gt;\.gradle\gradle.properties</code> СѓРєР°Р¶РёС‚Рµ:</p>
          <pre>systemProp.http.proxyHost=127.0.0.1
systemProp.http.proxyPort=10809
systemProp.https.proxyHost=127.0.0.1
systemProp.https.proxyPort=10809
systemProp.http.nonProxyHosts=localhost|127.0.0.1
systemProp.https.nonProxyHosts=localhost|127.0.0.1</pre>
        </div>
      </div>

      <div class=""card"">
        <h2>рџ§  РџРѕС‡РµРјСѓ РІРѕР·РЅРёРєР°РµС‚ СЌС‚РѕС‚ Р±Р°Рі?</h2>
        <p>Р’ РїР»Р°С‚С„РѕСЂРјРµ IntelliJ РјРµС‚РѕРґ <code>IdeProxyAuthenticator</code> Р·Р°РїСЂР°С€РёРІР°РµС‚ СѓС‡РµС‚РЅС‹Рµ РґР°РЅРЅС‹Рµ РёР· <code>PasswordSafe</code>. Р”Р»СЏ РїСЂРѕРєСЃРё Р±РµР· Р°СѓС‚РµРЅС‚РёС„РёРєР°С†РёРё РјРµС‚РѕРґ РІРѕР·РІСЂР°С‰Р°РµС‚ <code>null</code>. РР·-Р·Р° Р°СЂС…РёС‚РµРєС‚СѓСЂРЅРѕРіРѕ РёР·СЉСЏРЅР° IntelliJ СЃС‡РёС‚Р°РµС‚ <code>null</code> РїРѕРІРѕРґРѕРј РЅРµРјРµРґР»РµРЅРЅРѕ РїРѕРєР°Р·Р°С‚СЊ РїРѕР»СЊР·РѕРІР°С‚РµР»СЋ РѕРєРЅРѕ РІРІРѕРґР° РїР°СЂРѕР»СЏ.</p>
        <p>РџСЂСЏРјР°СЏ РїРµСЂРµРґР°С‡Р° РїР°СЂР°РјРµС‚СЂРѕРІ РІ JVM (<code>-Dhttp.proxyHost</code>) РїРѕР»РЅРѕСЃС‚СЊСЋ РѕР±С…РѕРґРёС‚ РїСЂРѕР±Р»РµРјРЅС‹Р№ РєР»Р°СЃСЃ <code>PlatformProxyAuthentication</code>.</p>
      </div>
    </div>

    <!-- CHINESE -->
    <div id=""lang-zh"" class=""lang-section"">
      <header>
        <div class=""badge"">JetBrains е®ж–№зјєй™· (IJPL-37161) & Google Issue 348084787</div>
        <h1>Android Studio: еЅ»еє•и§Је†ід»Јзђ†и®¤иЇЃеј№зЄ—дёЌж–­еј№е‡єзљ„й—®йў</h1>
        <p class=""lead"">еЅ»еє•дї®е¤ЌеЌідЅїж— йњЂи®¤иЇЃж€–е‹ѕйЂ‰вЂњи®°дЅЏеЇ†з ЃвЂќеђЋпјЊIDE д»Ќж— й™ђж¬Ўеј№е‡єд»Јзђ†з”Ёж€·еђЌе’ЊеЇ†з ЃзЄ—еЏЈзљ„й—®йўгЂ‚</p>
      </header>

      <div class=""card card-highlight"">
        <h2>вљЎ ж–№жі• 1пјљдЅїз”Ё JetBrains_Proxy_Fix.exe и‡ЄеЉЁдї®е¤Ќ</h2>
        <p>е·Ґе…·дјљи‡ЄеЉЁжџҐж‰ѕз”µи„‘дё­ж‰Ђжњ‰з‰€жњ¬зљ„ Android StudioпјЊдёЂй”®и‡ЄеЉЁй…ЌзЅ®е®Њж€ђгЂ‚</p>
        <br>
        <ol>
          <li>иїђиЎЊ <code>JetBrains_Proxy_Fix.exe</code>гЂ‚</li>
          <li>иѕ“е…Ґжњ¬ењ°д»Јзђ† HTTP з«ЇеЏЈпј€й»и®¤ <strong>10809</strong>пј‰пјљ
            <table class=""table"">
              <tr><th>д»Јзђ†иЅЇд»¶</th><th>еёёи§Ѓ HTTP з«ЇеЏЈ</th></tr>
              <tr><td>v2rayN / Xray / Incy</td><td><strong>10809</strong></td></tr>
              <tr><td>Clash / Mihomo / Flclash</td><td><strong>7890</strong></td></tr>
              <tr><td>NekoBox / Hiddify</td><td><strong>2080</strong> ж€– <strong>7890</strong></td></tr>
            </table>
          </li>
          <li>з‚№е‡» <strong>вЂњеє”з”Ёдї®е¤ЌвЂќ</strong>пј€Apply Fixпј‰гЂ‚</li>
          <li>ењЁ Android Studio дё­пјљ<code>Settings (Ctrl+Alt+S)</code> в†’ <code>System Settings</code> в†’ <code>HTTP Proxy</code>пјЊйЂ‰ж‹© <strong>вЂњNo proxyвЂќ</strong>пј€е…ій—­еј‚еёёзљ„ UI еј№зЄ—пј‰гЂ‚</li>
          <li>й‡ЌеђЇ Android StudioгЂ‚и‹Ґеј№е‡єжЇеђ¦ењЁ <code>gradle.properties</code> дё­дїќз•™д»Јзђ†и®ѕзЅ®пјЊйЂ‰ж‹© <strong>вЂњYesвЂќ</strong>гЂ‚</li>
        </ol>
        <div class=""tip"">
          <strong>дї®е¤Ќе®Њж€ђпјЃ</strong> д»Јзђ†з›ґжЋҐењЁ JVM дёЋ Gradle е±‚з”џж•€пјЊдёЌдјље†Ќжњ‰д»»дЅ•зѓ¦дєєеј№зЄ—гЂ‚
        </div>
      </div>

      <div class=""card"">
        <h2>рџ› пёЏ ж–№жі• 2пјљж‰‹еЉЁй…ЌзЅ®</h2>
        <div style=""margin-top: 15px;"">
          <h3><span class=""step-num"">1</span> ењЁи®ѕзЅ®дё­е…ій—­ UI д»Јзђ†</h3>
          <p>ж‰“ејЂ Android Studio в†’ <strong>Settings в†’ System Settings в†’ HTTP Proxy</strong>пјЊе‹ѕйЂ‰ <strong>вЂњNo proxyвЂќ</strong> е№¶з‚№е‡» <strong>OK</strong>гЂ‚</p>
        </div>
        <div style=""margin-top: 20px;"">
          <h3><span class=""step-num"">2</span> ењЁ VM Options дё­й…ЌзЅ® JVM д»Јзђ†</h3>
          <p>з‚№е‡»иЏњеЌ•ж Џ <strong>Help в†’ Edit Custom VM Options...</strong>пјЊењЁжњ«е°ѕж·»еЉ пјљ</p>
          <pre>-Dhttp.proxyHost=127.0.0.1
-Dhttp.proxyPort=10809
-Dhttps.proxyHost=127.0.0.1
-Dhttps.proxyPort=10809
-Dhttp.nonProxyHosts=localhost|127.0.0.1
-Djdk.http.auth.tunneling.disabledSchemes=""""
-Djdk.http.auth.proxying.disabledSchemes=""""</pre>
        </div>
        <div style=""margin-top: 20px;"">
          <h3><span class=""step-num"">3</span> й…ЌзЅ® Gradle</h3>
          <p>ж‰“ејЂ <code>C:\Users\&lt;з”Ёж€·еђЌ&gt;\.gradle\gradle.properties</code> ж–‡д»¶е№¶ж·»еЉ пјљ</p>
          <pre>systemProp.http.proxyHost=127.0.0.1
systemProp.http.proxyPort=10809
systemProp.https.proxyHost=127.0.0.1
systemProp.https.proxyPort=10809
systemProp.http.nonProxyHosts=localhost|127.0.0.1
systemProp.https.nonProxyHosts=localhost|127.0.0.1</pre>
        </div>
      </div>

      <div class=""card"">
        <h2>рџ§  еЋџзђ†е€†жћђ</h2>
        <p>IntelliJ е†…йѓЁзљ„ <code>IdeProxyAuthenticator</code> дјљеђ‘ <code>PasswordSafe</code> иЇ·ж±‚е‡­жЌ®гЂ‚ењЁдёЌйњЂи¦Ѓи®¤иЇЃзљ„д»Јзђ†дёЉпјЊиЇҐж–№жі•иї”е›ћ <code>null</code>пјЊиЂЊжЎ†жћ¶й”™иЇЇењ°е°† <code>null</code> е€¤е®љдёєвЂњз”Ёж€·е°љжњЄиѕ“е…ҐеЇ†з ЃвЂќпјЊеЇји‡ґжЇЏж¬ЎзЅ‘з»њиЇ·ж±‚йѓЅејєе€¶еј№е‡є Swing и®¤иЇЃеЇ№иЇќжЎ†гЂ‚</p>
        <p>йЂљиї‡ JVM еЏ‚ж•°з›ґиїћд»Јзђ†еЏЇд»Ґе®Ње…Ёз»•иї‡еј‚еёёзљ„ <code>PlatformProxyAuthentication</code> жЁЎеќ—гЂ‚</p>
      </div>
    </div>

    <footer style=""text-align: center; color: var(--text-muted); font-size: 0.9rem; margin-top: 30px;"">
      Pull Request: <a href=""https://github.com/JetBrains/intellij-community/pull/3770"" target=""_blank"" style=""color: var(--accent);"">JetBrains/intellij-community#3770</a> | GitHub: <a href=""https://github.com/Ruslik31/jetbrains-proxy-auth-fix"" target=""_blank"" style=""color: var(--accent);"">Ruslik31/jetbrains-proxy-auth-fix</a>
    </footer>
  </div>

  <script>
    function setLang(lang) {
      document.querySelectorAll('.lang-section').forEach(el => el.classList.remove('active'));
      document.querySelectorAll('.lang-btn').forEach(el => el.classList.remove('active'));
      
      const target = document.getElementById('lang-' + lang);
      if (target) target.classList.add('active');
      
      const btn = Array.from(document.querySelectorAll('.lang-btn')).find(b => b.textContent.toLowerCase().includes(lang === 'zh' ? 'дё­ж–‡' : (lang === 'ru' ? 'СЂСѓСЃСЃРєРёР№' : 'english')));
      if (btn) btn.classList.add('active');
    }
  </script>
</body>
</html>
"; // Populated during build
    }
}


