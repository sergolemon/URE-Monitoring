using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Identity;
using Microsoft.UI.Xaml;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using URE.Core.Contracts.Services;
using URE.Core.Models.Identity;
using URE.Helpers;
using URE.Properties;

namespace URE.ViewModels.Controls
{
    public class UserConfigVm : ObservableRecipient, INotifyDataErrorInfo
    {
        public UserConfigVm()
        {
        }

        public string Id { get; set; }

        private bool isActive;
        public bool IsActive { get => isActive; set => SetProperty(ref isActive, value); }

        private string shortName;
        public string ShortName { get => shortName; set => SetProperty(ref shortName, value); }

        private string carNumber;
        public string CarNumber 
        { 
            get => carNumber; 
            set 
            {
                ValidateCarNumber(value);
                SetProperty(ref carNumber, value); 
            } 
        }

        private string name;
        public string Name { get => name; 
            set 
            { 
                ValidateName(value);
                SetProperty(ref name, value);
                ShortName = string.Join(" ", new object[] { Surname, !string.IsNullOrEmpty(Name) ? Name.First() + "." : null, !string.IsNullOrEmpty(Patronymic) ? Patronymic.First() + "." : null }.Where(x => !string.IsNullOrEmpty(x?.ToString())));
            } 
        }

        private Visibility deleteBtnVisibility = Visibility.Visible;
        public Visibility DeleteBtnVisibility { get => deleteBtnVisibility; set => SetProperty(ref deleteBtnVisibility, value); }

        private Visibility editBtnVisibility = Visibility.Visible;
        public Visibility EditBtnVisibility { get => editBtnVisibility; set => SetProperty(ref editBtnVisibility, value); }

        private Visibility radioEditRoleBtnVisibility;
        public Visibility RadioEditRoleBtnVisibility { get => radioEditRoleBtnVisibility; set => SetProperty(ref radioEditRoleBtnVisibility, value); }

        private Visibility radioAddRoleBtnVisibility = Visibility.Visible;
        public Visibility RadioAddRoleBtnVisibility { get => radioAddRoleBtnVisibility; set => SetProperty(ref radioAddRoleBtnVisibility, value); }

        private string surname;
        public string Surname { get => surname; 
            set 
            {
                ValidateSurname(value);
                SetProperty(ref surname, value);
                ShortName = string.Join(" ", new object[] { Surname, !string.IsNullOrEmpty(Name) ? Name.First() + "." : null, !string.IsNullOrEmpty(Patronymic) ? Patronymic.First() + "." : null }.Where(x => !string.IsNullOrEmpty(x?.ToString())));
            } 
        }

        private string patronymic;
        public string Patronymic { get => patronymic; 
            set 
            { 
                ValidatePatronymic(value);
                SetProperty(ref patronymic, value);
                ShortName = string.Join(" ", new object[] { Surname, !string.IsNullOrEmpty(Name) ? Name.First() + "." : null, !string.IsNullOrEmpty(Patronymic) ? Patronymic.First() + "." : null }.Where(x => !string.IsNullOrEmpty(x?.ToString())));
            } 
        }

        private string login;
        public string Login 
        { 
            get => login;
            set 
            { 
                ValidateLogin(value);
                SetProperty(ref login, value); 
            } 
        }

        private string password;
        public string Password 
        { 
            get => password; 
            set 
            {
                ValidatePassword(value);
                SetProperty(ref password, value); 
            } 
        }

        public List<string> Roles { get; set; } = new();

        public event Func<UserConfigVm, Task> Delete;
        public ICommand DeleteCommand => new RelayCommand<UserConfigVm>(async (userConfig) => {
            if(Delete is not null && App.GetService<ISignInManager>().Identity.UserId != userConfig.Id) await Delete.Invoke(userConfig);
        });

        public event Func<UserConfigVm, Task> Edit;

        public ICommand EditCommand => new RelayCommand<UserConfigVm>(async (userConfig) => {
            if (Edit is not null) 
            {
                var userManager = App.GetService<UserManager<ApplicationUser>>();

                var copy = new UserConfigVm
                {
                    CarNumber = userConfig.CarNumber,
                    Name = userConfig.Name,
                    Surname = userConfig.Surname,
                    Id = userConfig.Id,
                    IsActive = userConfig.IsActive,
                    Login = userConfig.Login,
                    Patronymic = userConfig.Patronymic,
                    Password = userConfig.Password,
                    Roles = userConfig.Roles
                };

                copy.Edit += userConfig.Edit;
                copy.Delete += userConfig.Delete;

                await Edit.Invoke(copy); 
            }
        });

        public void SetUiStateByIdentity(UserIdentity identity)
        {
            if (identity.Roles.Contains(Role.SuperAdmin))
            {
                if (Roles.Contains(Role.SuperAdmin))
                {
                    RadioEditRoleBtnVisibility = Visibility.Collapsed;
                    DeleteBtnVisibility = Visibility.Collapsed;
                }
            }
            else if(identity.Roles.Contains(Role.Admin))
            {
                if (Roles.Contains(Role.Admin))
                {
                    RadioEditRoleBtnVisibility = Visibility.Collapsed;
                    RadioAddRoleBtnVisibility = Visibility.Collapsed;
                    DeleteBtnVisibility = Visibility.Collapsed;

                    if(identity.UserId != Id) EditBtnVisibility = Visibility.Collapsed;
                }
            }
        }

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new();

        public bool HasErrors => _validationErrors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) ||
                !_validationErrors.ContainsKey(propertyName))
                return null;

            return _validationErrors[propertyName];
        }

        private void SetErrors(string key, ICollection<string> errors)
        {
            if (errors.Any())
                _validationErrors[key] = errors;
            else
                _ = _validationErrors.Remove(key);

            OnErrorsChanged(key);
        }

        private void ValidateSurname(string surname)
        {
            var errors = new List<string>(1);
            if (string.IsNullOrEmpty(surname))
            {
                errors.Add("Прізвище не заповнено!");
            }
            else if (surname.Length > 60)
            {
                errors.Add("Прізвище має бути не довше 60 символів!");
            }

            SetErrors("Surname", errors);
        }

        private void ValidateCarNumber(string carNumber)
        {
            var errors = new List<string>(1);
            if (string.IsNullOrEmpty(carNumber))
            {
                errors.Add("Номер автомобіля не заповнено!");
            }
            else if(carNumber.Length > 8)
            {
                errors.Add("Номер автомобіля має бути не довше 8 символів!");
            }

            SetErrors("CarNumber", errors);
        }

        private void ValidateName(string name)
        {
            var errors = new List<string>(1);
            if (string.IsNullOrEmpty(name))
            {
                errors.Add("Ім'я не заповнено!");
            }
            else if (name.Length > 60)
            {
                errors.Add("Ім'я має бути не довше 60 символів!");
            }

            SetErrors("Name", errors);
        }

        private void ValidatePatronymic(string patronymic)
        {
            var errors = new List<string>(1);
            if (string.IsNullOrEmpty(patronymic))
            {
                errors.Add("Ім'я по батькові не заповнено!");
            }
            else if (patronymic.Length > 60)
            {
                errors.Add("Ім'я по батькові має бути не довше 60 символів!");
            }

            SetErrors("Patronymic", errors);
        }

        private void ValidateLogin(string login)
        {
            var errors = new List<string>(1);
            if (string.IsNullOrEmpty(login))
            {
                errors.Add("Логін не заповнено!");
            }
            else if (login.Length > 60)
            {
                errors.Add("Логін має бути не довше 60 символів!");
            }

            SetErrors("Login", errors);
        }

        private void ValidatePassword(string password)
        {
            var errors = new List<string>(1);
            if (string.IsNullOrEmpty(Id))
            {
                if (string.IsNullOrEmpty(password))
                {
                    errors.Add("Пароль не заповнено!");
                }
                else if (password.Length < 6 || password.Length > 16) 
                {
                    errors.Add("Пароль має бути від 6 до 16 символів!");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(password))
                {
                    if (password.Length < 6 || password.Length > 16)
                    {
                        errors.Add("Пароль має бути від 6 до 16 символів!");
                    }
                }
            }
            
            

            SetErrors("Password", errors);
        }

        public bool ValidateModel()
        {
            ValidateLogin(Login);
            ValidatePassword(Password);
            ValidateName(Name);
            ValidatePatronymic(Patronymic);
            ValidateSurname(Surname);
            ValidateCarNumber(CarNumber);

            return !HasErrors;
        }
    }
}
