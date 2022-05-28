using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
            openInstanceButton.Text = "请等待 5 秒左右...";
            var scProcesses = Process.GetProcessesByName("SC2_x64");
            if (scProcesses.Length == 0)
            {
                MessageBox.Show("未检测到星际2实例，请先打开一个星际2实例！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
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

                    Task.Delay(5000).ContinueWith(_ =>
                    {
                        Process.Start(Path.Combine(ConfigurationManager.AppSettings["GameFolderPath"], @"Support64\SC2Switcher_x64.exe"));
                        openInstanceButton.BeginInvoke(new Action(() =>
                        {
                            openInstanceButton.Text = "一键多开";
                        }));
                    });
                });
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            gameFolderTextBox.Text = ConfigurationManager.AppSettings["GameFolderPath"];
        }
    }
}
