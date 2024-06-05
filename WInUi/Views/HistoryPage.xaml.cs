using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using URE.Core.Models.Meteo;
using URE.ViewModels;
using URE.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Mapsui;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace URE.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        private ListView _listView { get; set; }

        public HistoryVm ViewModel { get; }

        public HistoryPage()
        {
            ViewModel = App.GetService<HistoryVm>();
            ViewModel.PropertyChanged += OnPropertyChanged;

            DataContext = ViewModel;
            this.InitializeComponent();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedHistogram):
                    _listView.SelectedIndex = ViewModel.SelectedHistogram;
                    _listView.ScrollIntoView(_listView.Items[ViewModel.SelectedHistogram]);
                    break;
                default:
                    break;

            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".csv");

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(filePicker, hwnd);

            var files = await filePicker.PickMultipleFilesAsync();
            if (files != null && files.Any())
            {
                string protocol =  await ViewModel.Import(files);
                await ViewModel.ShellViewModel.LoadStreamsPage(isImported: ViewModel.IsImportedShow);
                ViewModel.StreamsData = ViewModel.ShellViewModel.Streams;
                ViewModel.Streams.Clear();

                if (ViewModel.StreamsData != null) await ViewModel.InitialAsync();

                var lv = new ListView();

                lv.ItemsSource = ViewModel.Streams;
                lv.ItemTemplate = Resources["historyItemTemplate"] as DataTemplate;

                contentBorder.Child = lv;
                _listView = lv;

                var dialog = new ContentDialog
                {
                    Title = "ImportProtocol".GetLocalized(),
                    Content = new ScrollViewer
                    {
                        Padding = new Thickness(10, 10, 10, 10),
                        Content = new TextBlock
                        {
                            Text = protocol
                        }
                    },
                    CloseButtonText = "Cancel".GetLocalized(),
                    XamlRoot = this.XamlRoot,
                };

                await dialog.ShowAsync();
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null && !string.IsNullOrEmpty(folder.Path))
            {
                ViewModel.Export(folder.Path);
            }
        }

        private async void PipsPager_SelectedIndexChanged(PipsPager sender, PipsPagerSelectedIndexChangedEventArgs args)
        {
            await ViewModel.ShellViewModel.LoadStreamsPage(sender.SelectedPageIndex, ViewModel.IsImportedShow);
            ViewModel.StreamsData = ViewModel.ShellViewModel.Streams;
            ViewModel.Streams.Clear();

            if (ViewModel.StreamsData != null) await ViewModel.InitialAsync();
            var lv = new ListView();

            lv.ItemsSource = ViewModel.Streams;
            lv.ItemTemplate = Resources["historyItemTemplate"] as DataTemplate;

            contentBorder.Child = lv;
            _listView = lv;
        }
    }
}
