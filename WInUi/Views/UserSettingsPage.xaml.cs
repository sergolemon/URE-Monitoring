using Microsoft.EntityFrameworkCore.Metadata;
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
using URE.Core.Models.Identity;
using URE.ViewModels;
using URE.ViewModels.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace URE.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserSettingsPage : Page
    {
        public UserSettingsVm ViewModel { get; }

        public UserSettingsPage()
        {
            ViewModel = App.GetService<UserSettingsVm>();
            foreach (var item in ViewModel.UserConfigs)
            {
                item.Delete += async (userConfig) => { 
                    ViewModel.SelectedUserConfig = userConfig; 
                    deleteUserForm.Visibility = Visibility.Visible; 
                    deleteText.Text = $"Ви дійсно хочете видалити {userConfig.ShortName}?";
                };
                item.Edit += async (userConfig) => 
                { 
                    ViewModel.SelectedUserConfig = userConfig;
                    editUserAdminCheckbox.IsEnabled = ViewModel.SignInManager.Identity.Roles.Contains(Role.SuperAdmin) && !userConfig.Roles.Contains(Role.SuperAdmin);
                    editUserOperatorCheckbox.IsEnabled = ViewModel.SignInManager.Identity.Roles.Contains(Role.SuperAdmin) && !userConfig.Roles.Contains(Role.SuperAdmin);
                    if (userConfig.Roles.Any(x => x == Role.SuperAdmin))
                    {
                        editUserSuperadminCheckbox.IsChecked = true;
                    }
                    else if(userConfig.Roles.Any(x => x == Role.Admin))
                    {
                        editUserAdminCheckbox.IsChecked = true;
                    }
                    else if (userConfig.Roles.Any(x => x == Role.Operator))
                    {
                        editUserOperatorCheckbox.IsChecked = true;
                    }

                    if(userConfig.Id == ViewModel.SignInManager.Identity.UserId)
                    {
                        userActiveToggleSwitch.IsEnabled = false;
                    }
                    else
                    {
                        userActiveToggleSwitch.IsEnabled = true;
                    }

                    editUserForm.Visibility = Visibility.Visible; 
                };
            }

            DataContext = ViewModel;
            InitializeComponent();
        }

        private void ShowNewUserFormButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedUserConfig = new();
            ViewModel.SelectedUserConfig.Roles = new List<string> { Role.Operator };
            newUserAdminCheckbox.IsEnabled = ViewModel.SignInManager.Identity.Roles.Contains(Role.SuperAdmin);
            newUserOperatorCheckbox.IsChecked = true;
            newUserForm.Visibility = Visibility.Visible;
        }

        private void CloseNewUserFormButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedUserConfig = null;
            newUserForm.Visibility = Visibility.Collapsed;
        }

        private async void SaveNewUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.SelectedUserConfig.ValidateModel()) return;
            await ViewModel.AddNewUserAsync();

            ViewModel.SelectedUserConfig.Delete += async (userConfig) => { ViewModel.SelectedUserConfig = userConfig; deleteText.Text = $"Ви дійсно хочете видалити {userConfig.ShortName}?"; deleteUserForm.Visibility = Visibility.Visible; };
            ViewModel.SelectedUserConfig.Edit += async (userConfig) => { 
                ViewModel.SelectedUserConfig = userConfig;

                editUserAdminCheckbox.IsEnabled = ViewModel.SignInManager.Identity.Roles.Contains(Role.SuperAdmin) && !userConfig.Roles.Contains(Role.SuperAdmin);
                editUserOperatorCheckbox.IsEnabled = ViewModel.SignInManager.Identity.Roles.Contains(Role.SuperAdmin) && !userConfig.Roles.Contains(Role.SuperAdmin);
                if (userConfig.Roles.Any(x => x == Role.SuperAdmin))
                {
                    editUserSuperadminCheckbox.IsChecked = true;
                }
                else if (userConfig.Roles.Any(x => x == Role.Admin))
                {
                    editUserAdminCheckbox.IsChecked = true;
                }
                else if (userConfig.Roles.Any(x => x == Role.Operator))
                {
                    editUserOperatorCheckbox.IsChecked = true;
                }

                editUserForm.Visibility = Visibility.Visible; 
            };
            ViewModel.UserConfigs.Add(ViewModel.SelectedUserConfig);
            ViewModel.SelectedUserConfig = null;

            newUserForm.Visibility = Visibility.Collapsed;
        }

        private async void UpdateUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.SelectedUserConfig.ValidateModel()) return;
            await ViewModel.UpdateUserAsync();
            ViewModel.SelectedUserConfig = null;
            editUserForm.Visibility = Visibility.Collapsed;
        }

        private void CloseUpdateUserFormButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedUserConfig = null;
            editUserForm.Visibility = Visibility.Collapsed;
        }

        private void CloseDeleteUserFormButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedUserConfig = null;
            deleteText.Text = string.Empty;
            deleteUserForm.Visibility = Visibility.Collapsed;
        }

        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.DeleteUserAsync();
            ViewModel.SelectedUserConfig = null;
            deleteText.Text = string.Empty;
            deleteUserForm.Visibility = Visibility.Collapsed;
        }

        private void NewUser_SuperadminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedUserConfig == null) return;
            ViewModel.SelectedUserConfig.Roles = new List<string>() { Role.Admin, Role.SuperAdmin, Role.Operator };
        }

        private void NewUser_AdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedUserConfig == null) return;
            ViewModel.SelectedUserConfig.Roles = new List<string>() { Role.Admin, Role.Operator };
        }

        private void NewUser_OperatorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedUserConfig == null) return;
            ViewModel.SelectedUserConfig.Roles = new List<string>() { Role.Operator };
        }

        private void EditUser_SuperadminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedUserConfig == null) return;
            ViewModel.SelectedUserConfig.Roles = new List<string>() { Role.Admin, Role.SuperAdmin, Role.Operator };
        }

        private void EditUser_AdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedUserConfig == null) return;
            ViewModel.SelectedUserConfig.Roles = new List<string>() { Role.Admin, Role.Operator };
        }

        private void EditUser_OperatorCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedUserConfig == null) return;
            ViewModel.SelectedUserConfig.Roles = new List<string>() { Role.Operator };
        }

        private void passwordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
