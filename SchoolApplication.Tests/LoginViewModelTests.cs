using Xunit;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.ViewModels;
using SchoolApplication.Services;
using SchoolApplication.Models;
using SchoolApplication.Messages;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading;
using System;

namespace SchoolApplication.Tests
{
    public class LoginViewModelTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly IMessenger _messenger;

        public LoginViewModelTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _messenger = WeakReferenceMessenger.Default;
            _messenger.Reset();
        }

        private void RunInStaThread(Action action)
        {
            var thread = new Thread(() =>
            {
                action();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        [Fact]
        public void PasswordChangedCommand_UpdatesPasswordProperty()
        {
            var viewModel = new LoginViewModel(_mockAuthService.Object);
            string expectedPassword = "TestPassword";

            RunInStaThread(() =>
            {
                var passwordBox = new PasswordBox();
                passwordBox.Password = expectedPassword;

                viewModel.PasswordChangedCommand.Execute(passwordBox);

                Assert.Equal(expectedPassword, viewModel.Password);
            });
        }
        [Fact]
        public async Task LoginCommand_AuthenticatesUserAndSendsMessage_OnSuccess()
        {
            var viewModel = new LoginViewModel(_mockAuthService.Object);
            string username = "testuser";
            string password = "password123";
            var authenticatedUser = new User { UserID = 1, Username = username, RoleID = 1, FirstName = "Test", LastName = "User" };

            _mockAuthService.Setup(s => s.AuthenticateUser(username, password))
                            .ReturnsAsync(authenticatedUser);

            viewModel.Username = username;
            viewModel.Password = password;

            UserAuthenticatedMessage? receivedMessage = null;
            _messenger.Register<LoginViewModelTests, UserAuthenticatedMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            await viewModel.LoginCommand.ExecuteAsync(null);

            _mockAuthService.Verify(s => s.AuthenticateUser(username, password), Times.Once());
            Assert.NotNull(receivedMessage);
            Assert.Equal(authenticatedUser.UserID, receivedMessage.Value.UserID);
            Assert.Equal(authenticatedUser.Username, receivedMessage.Value.Username);

            Assert.Equal(string.Empty, viewModel.ErrorMessage);
        }
        [Fact]
        public async Task LoginCommand_DisplaysErrorMessage_OnAuthenticationFailure()
        {
            var viewModel = new LoginViewModel(_mockAuthService.Object);
            string username = "stud";
            string password = "123";

            _mockAuthService.Setup(s => s.AuthenticateUser(username, password))
                            .ReturnsAsync((User?)null);

            viewModel.Username = username;
            viewModel.Password = password;

            UserAuthenticatedMessage? receivedMessage = null;
            _messenger.Register<LoginViewModelTests, UserAuthenticatedMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            await viewModel.LoginCommand.ExecuteAsync(null);

            _mockAuthService.Verify(s => s.AuthenticateUser(username, password), Times.Once());
            Assert.Null(receivedMessage);
            Assert.False(string.IsNullOrEmpty(viewModel.ErrorMessage));
            Assert.Equal("Неверный логин или пароль.", viewModel.ErrorMessage);

            Assert.Equal(username, viewModel.Username);
            Assert.Equal(password, viewModel.Password);
        }
        [Fact]
        public async Task LoginCommand_DisplaysErrorMessage_OnServiceException()
        {
            var viewModel = new LoginViewModel(_mockAuthService.Object);
            string username = "testuser";
            string password = "password123";
            string exceptionMessage = "Database connection error.";

            _mockAuthService.Setup(s => s.AuthenticateUser(username, password))
                            .ThrowsAsync(new Exception(exceptionMessage));

            viewModel.Username = username;
            viewModel.Password = password;

            UserAuthenticatedMessage? receivedMessage = null;
            _messenger.Register<LoginViewModelTests, UserAuthenticatedMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            await viewModel.LoginCommand.ExecuteAsync(null);

            _mockAuthService.Verify(s => s.AuthenticateUser(username, password), Times.Once());
            Assert.Null(receivedMessage);
            Assert.False(string.IsNullOrEmpty(viewModel.ErrorMessage));
            Assert.Contains(exceptionMessage, viewModel.ErrorMessage);
            Assert.Contains("Ошибка:", viewModel.ErrorMessage);
            Assert.Equal(username, viewModel.Username);
            Assert.Equal(password, viewModel.Password);
        }
    }
}