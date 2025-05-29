using System;
using System.Windows.Forms; // Required for Application
using ERLC; // Required for MainForm

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // ApplicationConfiguration.Initialize(); // This is for .NET Core 6 WinForms templates, may not be needed or available in .NET 6 directly without SDK changes.
        Application.Run(new MainForm());
    }
}
