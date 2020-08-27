using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SakuraUpdater
{
    public partial class MainForm : Form
    {
        public bool Working = false;
        public List<UpdateJob> Jobs = new List<UpdateJob>();

        public MainForm()
        {
            InitializeComponent();
            Text += " v" + Assembly.GetExecutingAssembly().GetName().Version;

            Log("正在获取更新数据...");
            Program.HttpGet("https://api.natfrp.com/launcher/get_version?xml" +
                (Program.UpdateFrpc ? "&frpc" : "") +
                (Program.UpdateLauncher == LauncherType.WPF ? "&launcher" : "") +
                (Program.UpdateLauncher == LauncherType.Legacy ? "&legacy" : "")).ContinueWith(t =>
            {
                if (t.Result == null)
                {
                    Log("出现未知错误, 请重试");
                    Log(t.Exception);
                    return;
                }
                try
                {
                    using (var resp = t.Result)
                    using (var reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        var xml = XDocument.Parse(reader.ReadToEnd());
                        foreach (var task in xml.Element("tasks").Elements())
                        {
                            Jobs.Add(new UpdateJob()
                            {
                                URL = task.Attribute("url").Value,
                                Hash = task.Attribute("hash").Value,
                                Name = task.Attribute("name").Value + " v" + task.Attribute("version").Value,
                                Type = (UpdateType)Enum.Parse(typeof(UpdateType), task.Attribute("type").Value, true),
                                Target = task.Attribute("target").Value
                            });
                        }
                        Invoke(new Action(() =>
                        {
                            foreach (var j in Jobs)
                            {
                                Log("-----");
                                Log(j.Name);
                                Log("URL: " + j.URL);
                                Log("Hash: " + j.Hash);
                            }
                            Log("-----");
                            Log("请确认上述更新内容无误, 点击下方按钮开始更新");
                            button_start.Enabled = true;
                        }));
                    }
                }
                catch (Exception e)
                {
                    Log("出现未知错误, 请重试");
                    Log(e);
                }
            });
        }

        private void Log(object data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Log(data)));
                return;
            }
            textBox_log.AppendText(DateTime.Now.ToString() + " " + data + Environment.NewLine);
        }

        private void UpdateProgress(long complete, long total)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(complete, total)));
                return;
            }
            progressBar_progress.Value = (int)((float)complete / total * 500);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Working)
            {
                if (MessageBox.Show("更新进行中, 取消操作可能导致文件损坏, 确定要退出吗?", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    Environment.Exit(0);
                }
                e.Cancel = true;
            }
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            if (button_start.Text != "开始更新")
            {
                switch (Program.LaunchLauncher)
                {
                case LauncherType.None:
                    break;
                case LauncherType.WPF:
                    Process.Start("SakuraLauncher.exe");
                    break;
                case LauncherType.Legacy:
                    Process.Start("LegacyLauncher.exe");
                    break;
                }
                Close();
                return;
            }
            Working = true;
            button_start.Visible = false;
            ThreadPool.QueueUserWorkItem(s =>
            {
                foreach (var job in Jobs)
                {
                    Log("正在更新 " + job.Name + " ...");
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        var t = Program.HttpGet(job.URL);
                        t.Wait();

                        UpdateProgress(0, 1);
                        using (var ms = new MemoryStream())
                        {
                            using (var resp = t.Result)
                            using (var ws = resp.GetResponseStream())
                            using (var hasher = MD5.Create())
                            {
                                hasher.Initialize();

                                byte[] buffer = new byte[4096];
                                long complete = 0, total = resp.ContentLength;

                                Log("开始下载: " + Math.Round(total / 1024.0 / 1024, 2) + " MiB");

                                while (complete < total)
                                {
                                    int count = ws.Read(buffer, 0, buffer.Length);
                                    ms.Write(buffer, 0, count);
                                    hasher.TransformBlock(buffer, 0, count, null, 0);
                                    UpdateProgress(complete += count, total);
                                }
                                hasher.TransformFinalBlock(new byte[0], 0, 0);

                                if (BitConverter.ToString(hasher.Hash).Replace("-", "").ToLowerInvariant() != job.Hash.ToLowerInvariant())
                                {
                                    Log("更新失败: Hash 校验失败, 请重试");
                                    continue;
                                }
                            }
                            switch (job.Type)
                            {
                            case UpdateType.Binary:
                                File.WriteAllBytes(job.Target, ms.ToArray());
                                Log("更新文件 " + job.Target + " ...");
                                break;
                            case UpdateType.ZipPackage:
                                ms.Position = 0;
                                using (var zip = new ZipArchive(ms, ZipArchiveMode.Read))
                                {
                                    foreach (var entry in zip.Entries)
                                    {
                                        if (entry.Length == 0)
                                        {
                                            continue;
                                        }
                                        Log("更新文件 " + entry.FullName + " ...");
                                        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(job.Target, entry.FullName)));
                                        using (var es = entry.Open())
                                        using (var fs = File.Open(Path.Combine(job.Target, entry.FullName), FileMode.Create))
                                        {
                                            es.CopyTo(fs);
                                        }
                                    }
                                }
                                break;
                            }
                        }

                        sw.Stop();
                        Log("更新成功! 耗时 " + sw.ElapsedMilliseconds / 1000.0 + "s");
                    }
                    catch (Exception ex)
                    {
                        Log("更新失败, 请重试");
                        Log(ex);
                        continue;
                    }
                }
                Log("操作完成, 请点击下方按钮" + (Program.LaunchLauncher == LauncherType.None ? "退出更新程序" : "打开启动器"));
                Invoke(new Action(() =>
                {
                    Working = false;
                    button_start.Text = Program.LaunchLauncher == LauncherType.None ? "退出" : "打开启动器";
                    button_start.Visible = true;
                }));
            });
        }
    }
}
