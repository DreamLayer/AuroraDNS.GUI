﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using AuroraGUI.DnsSvr;
using AuroraGUI.Tools;

namespace AuroraGUI
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MyTools.BackgroundLog(e.ExceptionObject.ToString());
            if (e.IsTerminating)
            {
                MessageBox.Show(
                    $"发生了可能致命性的严重错误，请从以下错误信息汲取灵感。{Environment.NewLine}" +
                    $"程序可能即将中止运行。{Environment.NewLine}" + e.ExceptionObject,
                    "意外的错误。", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MyTools.BackgroundLog(e.Exception.ToString());
            MessageBoxResult msgResult = MessageBox.Show(
                $"未经处理的异常，请从以下错误信息汲取灵感。{Environment.NewLine}" +
                $"点击取消中止程序运行，点击确定以继续。{Environment.NewLine}" + e.Exception,
                "意外的错误。", MessageBoxButton.OKCancel, MessageBoxImage.Error);

            if (MessageBoxResult.OK == msgResult)
                e.Handled = true;
            else
                Shutdown();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            string setupBasePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (e.Args.Length == 0) return;
            if (e.Args[0].Split(':')[0] == "aurora-doh")
            {
                if (File.Exists($"{setupBasePath}config.json"))
                    DnsSettings.ReadConfig($"{setupBasePath}config.json");
                DnsSettings.HttpsDnsUrl = e.Args[0].Replace("aurora-doh:", "https:");
                new SettingsWindow().ButtonSave_OnClick(sender, null);
            }
            foreach (var item in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                if (item.Id != Process.GetCurrentProcess().Id)
                    item.Kill();
            Process.Start(new ProcessStartInfo {FileName = GetType().Assembly.Location});
            Shutdown();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            string setupBasePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (!DnsSettings.AutoCleanLogEnable) return;
            foreach (var item in Directory.GetFiles($"{setupBasePath}Log"))
                if (item != $"{setupBasePath}Log" +
                    $"\\{DateTime.Today.Year}{DateTime.Today.Month:00}{DateTime.Today.Day:00}.log")
                    File.Delete(item);
            if (File.Exists(Path.GetTempPath() + "setdns.cmd")) File.Delete(Path.GetTempPath() + "setdns.cmd");
        }
    }
}
