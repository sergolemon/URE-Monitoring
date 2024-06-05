using Microsoft.UI;
using WinRT.Interop;
using URE.Helpers;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using System.Diagnostics;

namespace URE;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon("Assets/Logo.ico");
        Content = null;
        Title = "AppDisplayName".GetLocalized();

        Closed += CloseProgramConfirm;

        MaximizeWindow();
    }

    bool? closing = false;

    async void CloseProgramConfirm(object sender, WindowEventArgs args)
    {
        if (closing == false)
        {
            closing = null;
            args.Handled = true;

            var dialog = new ContentDialog();
            dialog.XamlRoot = Content.XamlRoot;
            dialog.Title = "CloseProgramConfirmText".GetLocalized();
            dialog.PrimaryButtonText = "ApproveProgramClosing".GetLocalized();
            dialog.SecondaryButtonText = "CancelProgramClosing".GetLocalized();
            dialog.PrimaryButtonClick += (s, e) => DispatcherQueue.TryEnqueue(() => { closing = true; Close(); });
            dialog.SecondaryButtonClick += (s, e) => DispatcherQueue.TryEnqueue(() => { dialog.Hide(); });
            var result = await dialog.ShowAsync();
            closing = false;
        }
        else if(closing == null)
        {
            args.Handled = true;
        }
    }

    private void MaximizeWindow()
    {
        IntPtr _windowHandle = WindowNative.GetWindowHandle(this);
        PInvoke.User32.ShowWindow(_windowHandle, PInvoke.User32.WindowShowStyle.SW_MAXIMIZE);
    }
}
