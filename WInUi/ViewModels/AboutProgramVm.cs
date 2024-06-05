using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;

namespace URE.ViewModels
{
    public class AboutProgramVm : ObservableRecipient
    {
        private string appVersion = GetVersion();
        public string AppVersion 
        { 
            get => appVersion;
            set => SetProperty(ref appVersion, value);
        }

        private static string GetVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
