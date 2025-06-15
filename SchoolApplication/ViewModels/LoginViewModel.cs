using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SchoolApplication.Services;
using SchoolApplication.Models;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.Messages;

namespace SchoolApplication.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _username = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            _username = string.Empty;
            _password = string.Empty;
            _errorMessage = string.Empty;
        }

        [RelayCommand]
        private void PasswordChanged(object? parameter)
        {
            if (parameter is PasswordBox passwordBox)
            {
                Password = passwordBox.Password;
            }
        }

        [RelayCommand]
        private async Task Login()
        {
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Пожалуйста, введите логин и пароль.";
                return;
            }

            try
            {
                User? user = await _authService.AuthenticateUser(Username, Password);

                if (user != null)
                {
                    WeakReferenceMessenger.Default.Send(new UserAuthenticatedMessage(user));

                    Username = "";
                    Password = "";
                    ErrorMessage = "";
                }
                else
                {
                    ErrorMessage = "Неверный логин или пароль.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
            }
        }
      }
    }
