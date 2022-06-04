using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenStarcraft2Instance
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void openFolderButton_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                gameFolderTextBox.Text = dialog.SelectedPath;
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings["GameFolderPath"] == null)
                {
                    config.AppSettings.Settings.Add("GameFolderPath", dialog.SelectedPath);
                }
                else
                {
                    config.AppSettings.Settings["GameFolderPath"].Value = dialog.SelectedPath;
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }


        private void openInstanceButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(gameFolderTextBox.Text))
            {
                MessageBox.Show("请选择星际2安装目录", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var scProcesses = Process.GetProcessesByName("SC2_x64");
                if (scProcesses.Length == 0)
                {
                    Process.Start(Path.Combine(ConfigurationManager.AppSettings["GameFolderPath"], @"Support64\SC2Switcher_x64.exe"));
                }
                else
                {
                    openInstanceButton.Text = "正在等待句柄结束...";
                    openInstanceButton.Enabled = false;
                    Task.Run(() =>
                    {
                        var handlesToClose = new List<HandleInfo>();
                        handlesToClose.Add(new HandleInfo { Type = SystemHandleType.OB_TYPE_EVENT, Name = @"\Sessions\1\BaseNamedObjects\StarCraft II Game Application" });
                        handlesToClose.Add(new HandleInfo { Type = SystemHandleType.OB_TYPE_SECTION, Name = @"\Sessions\1\BaseNamedObjects\StarCraft II IPC Mem" });

                        using (var handles = HandleHelpers.GetEnumerator(scProcesses[0].Id))
                        {
                            while (handles.MoveNext())
                            {
                                foreach (var handle in handlesToClose)
                                {
                                    if (handles.Current.Type == handle.Type && handles.Current.Name == handle.Name)
                                    {
                                        Console.WriteLine("Handle detected: {0}, {1}", handles.Current.Type, handles.Current.Name);
                                        HandleHelpers.DuplicateCloseHandle(scProcesses[0].Handle, handles.Current.Handle);
                                    }
                                }
                            }
                        }

                        var ready = true;
                        for (var i = 1; i < 10; i++)
                        {
                            openInstanceButton.BeginInvoke(new Action(() =>
                            {
                                openInstanceButton.Text = $"正在等待句柄结束({i})...";
                            }));

                            Thread.Sleep(1000);
                            ready = true;
                            using (var handles = HandleHelpers.GetEnumerator(scProcesses[0].Id))
                            {
                                while (handles.MoveNext())
                                {
                                    foreach (var handle in handlesToClose)
                                    {
                                        if (handles.Current.Type == handle.Type && handles.Current.Name == handle.Name)
                                        {
                                            ready = false;
                                        }
                                    }
                                }
                            }

                            if (ready)
                            {
                                Process.Start(Path.Combine(ConfigurationManager.AppSettings["GameFolderPath"], @"Support64\SC2Switcher_x64.exe"));
                                openInstanceButton.BeginInvoke(new Action(() =>
                                {
                                    openInstanceButton.Text = "一键多开";
                                    openInstanceButton.Enabled = true;
                                }));
                                break;
                            }
                        }
                        if (!ready)
                        {
                            MessageBox.Show("检测到句柄未关闭，请尝试重试", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    });
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            gameFolderTextBox.Text = ConfigurationManager.AppSettings["GameFolderPath"];
        }
    }
}
