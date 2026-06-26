using System;
using System.Windows.Forms;

namespace ChatApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            System.Text.Encoding.RegisterProvider(
                System.Text.CodePagesEncodingProvider.Instance);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Global exception handlers to catch unhandled GDI+ errors
            Application.ThreadException += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"[GLOBAL UI ERROR] {args.Exception}");
                MessageBox.Show(
                    $"Đã xảy ra lỗi: {args.Exception.Message}\n\n" +
                    $"Vui lòng khởi động lại ứng dụng.",
                    "Lỗi không mong muốn",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"[GLOBAL ERROR] {args.ExceptionObject}");
            };

            Application.Run(new FrmLogin());
        }
    }
}
