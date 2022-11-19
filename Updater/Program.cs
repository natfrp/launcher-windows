using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SakuraUpdater
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string TempDir = null;

        public static string GetTempPath(string file) => Path.Combine(TempDir, file);

        public static IEnumerable<Process> FindBundleProcess(string name)
        {
            var result = new List<Process>();
            try
            {
                var testPath = Path.GetFullPath(name + ".exe");
                foreach (var p in Process.GetProcessesByName(name))
                {
                    try
                    {
                        uint size = 256;
                        var sb = new StringBuilder((int)size - 1);
                        if (QueryFullProcessImageName(p.Handle, 0, sb, ref size) && Path.GetFullPath(sb.ToString()) == testPath)
                        {
                            result.Add(p);
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return result;
        }

        public static IEnumerable<Process> FindBundleProcess(params string[] names) => names.SelectMany(n => FindBundleProcess(n));

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] argv)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (argv.Length == 0)
            {
                MessageBox.Show("SakuraFrp Updater v" + Assembly.GetExecutingAssembly().GetName().Version +
                    "\nUsage: SakuraUpdater <Dir> [wpf|legacy]" +
                    "\nInstall update package from <Dir> and launch launcher when specified.", "Usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                TempDir = Path.GetFullPath(argv[0]);
                if (!Path.GetFileName(TempDir).StartsWith("Sakura-"))
                {
                    throw new Exception("临时目录格式不匹配, 无法执行更新任务");
                }

                Console.Write("等待启动器彻底退出 ...\t");
                Thread.Sleep(1000);
                Console.WriteLine("完成");

                for (int i = 0; i < 3; i++)
                {
                    Console.Write("搜索残留进程 [" + (i + 1) + " / 3] ..\t");

                    var processes = FindBundleProcess("SakuraLauncher", "LegacyLauncher", "SakuraFrpService", "frpc").ToArray();
                    if (processes.Length == 0)
                    {
                        Console.WriteLine("未发现");
                        break;
                    }
                    Console.WriteLine(processes.Length + " 个进程");

                    foreach (var p in processes)
                    {
                        try
                        {
                            Console.Write("\t - 正在结束残留进程 " + p.Id + " ...\t");
                            p.Kill();
                            Console.WriteLine(p.WaitForExit(10000) ? "完成" : "失败");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("错误: " + e.Message);
                        }
                    }
                }

                Console.Write("等待资源释放 ...\t");
                Thread.Sleep(1000);
                Console.WriteLine("完成");

                var tasks = XDocument.Parse(File.ReadAllText(GetTempPath("tasks.xml"))).Element("tasks").Elements();
                Console.WriteLine("读取到 " + tasks.Count() + " 个更新任务");

                foreach (var task in tasks)
                {
                    var source = GetTempPath(Path.GetFileName(task.Attribute("url").Value));
                    var target = task.Attribute("target").Value;

                    switch (task.Attribute("type").Value)
                    {
                    case "Binary":
                        Console.WriteLine("更新文件 " + target + " ...");
                        using (var src = File.OpenRead(source))
                        using (var dst = File.Open(target, FileMode.Create))
                        {
                            src.CopyTo(dst);
                        }
                        break;
                    case "ZipPackage":
                        Console.WriteLine("读取压缩包 " + source + " ...");
                        using (var src = File.OpenRead(source))
                        using (var zip = new ZipArchive(src, ZipArchiveMode.Read))
                        {
                            foreach (var entry in zip.Entries)
                            {
                                if (entry.Length == 0)
                                {
                                    continue;
                                }
                                Console.WriteLine("\t - 解压文件 " + entry.FullName + " ...");
                                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(target, entry.FullName)));
                                using (var es = entry.Open())
                                using (var fs = File.Open(Path.Combine(target, entry.FullName), FileMode.Create))
                                {
                                    es.CopyTo(fs);
                                }
                            }
                        }
                        break;
                    case "Executable":
                        Console.WriteLine("运行安装程序 " + source + " ...");
                        var p = Process.Start(source, target);
                        p.WaitForExit();
                        if (p.ExitCode != 0)
                        {
                            throw new Exception("安装程序运行失败");
                        }
                        break;
                    }
                }

                Console.Write("清理临时文件 ...\t");
                Directory.Delete(TempDir, true);
                Console.WriteLine("完成");

                Console.WriteLine("更新完成");
            }
            catch (Exception e)
            {
                Console.WriteLine("更新失败: " + e.ToString());
                MessageBox.Show("请重试，若持续看到此提示请下载最新版安装包手动进行更新。\n\n" + e.ToString(), "更新失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (argv.Length >= 2)
            {
                switch (argv[1].ToLower())
                {
                case "wpf":
                    Console.WriteLine("运行 WPF 启动器...");
                    Process.Start("SakuraLauncher.exe");
                    break;
                case "legacy":
                    Console.WriteLine("运行传统启动器...");
                    Process.Start("LegacyLauncher.exe");
                    break;
                }
            }
        }
    }
}
