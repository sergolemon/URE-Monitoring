using System;
using System.Collections.Specialized;
using System.Web;

using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using URE.Contracts.Services;
using URE.Core.Models.Equipment;
using URE.Extensions;
using URE.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace URE.Services;

public class AppNotificationService : IAppNotificationService
{
    public AppNotificationService()
    {
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
        AppNotificationManager.Default.Register();
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
    }

    public bool Show(string payload)
    {
        AppNotificationBuilder builder = new AppNotificationBuilder()
            .AddText(payload);

        AppNotification notification = builder.BuildNotification();
        AppNotificationManager.Default.Show(notification);

        return notification.Id != 0;
    }

    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
