using Xunit;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.Models;
using SchoolApplication.Messages;
using SchoolApplication.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Services;
using CommunityToolkit.Mvvm.Input;
using System;

namespace SchoolApplication.Tests
{
    [Collection("MessengerCollection")]
    public class MainViewModelTests : IDisposable
    {
        private readonly Mock<IDbContextFactory<ApplicationDbContext>> _mockDbContextFactory;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly IMessenger _messenger;

        // ИСПРАВЛЕНИЕ: _mockLessonAdminVm должен быть инициализирован с нужными параметрами
        private readonly Mock<LessonAdminVm> _mockLessonAdminVm;

        // Эти инстансы будут реальными объектами, но инициализированными с моками и инжектированным мессенджером
        private readonly LoginViewModel _loginViewModelInstance;
        private readonly HomeAdminVm _homeAdminVmInstance;
        private readonly HomeTeacherVm _homeTeacherVmInstance;
        private readonly HomeVm _homeStudentVmInstance;
        private readonly ClassroomsAdminVm _classroomsAdminVmInstance;
        private readonly DiaryAdminVm _diaryAdminVmInstance;
        private readonly GroupsAdminVm _groupsAdminVmInstance;
        private readonly SubjectAdminVm _subjectsAdminVmInstance;
        private readonly UsersAdminVm _usersAdminVmInstance;
        private readonly GradeVm _gradeVmInstance;
        private readonly LessonsVm _lessonsVmInstance;
        private readonly DiaryTeacherVm _diaryTeacherVmInstance;
        private readonly LessonTeacherVm _lessonTeacherVmInstance;

        private readonly NavigationAdminVm _navigationAdminVmInstance;
        private readonly NavigationVm _navigationVmInstance;
        private readonly TeacherNavigationVm _teacherNavigationVmInstance;

        public MainViewModelTests(MessengerFixture fixture)
        {
            _messenger = fixture.Messenger;

            _mockDbContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>(MockBehavior.Loose);
            _mockAuthService = new Mock<IAuthService>(MockBehavior.Loose);

            _mockAuthService.Setup(x => x.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>()))
                            .ReturnsAsync(new User { UserID = 1, Username = "testuser", RoleID = 1 });

            // ИСПРАВЛЕНИЕ ЗДЕСЬ: Инициализируем _mockLessonAdminVm с параметрами конструктора LessonAdminVm
            _mockLessonAdminVm = new Mock<LessonAdminVm>(_mockDbContextFactory.Object, _messenger); // Передаем требуемые зависимости

            // Инициализация ViewModels с учетом инжектированного мессенджера
            _loginViewModelInstance = new LoginViewModel(_mockAuthService.Object);

            _homeAdminVmInstance = new HomeAdminVm();
            _classroomsAdminVmInstance = new ClassroomsAdminVm();
            _diaryAdminVmInstance = new DiaryAdminVm();
            _groupsAdminVmInstance = new GroupsAdminVm();
            _subjectsAdminVmInstance = new SubjectAdminVm();
            _usersAdminVmInstance = new UsersAdminVm();

            _homeTeacherVmInstance = new HomeTeacherVm(_mockDbContextFactory.Object, _messenger);
            _homeStudentVmInstance = new HomeVm(_mockDbContextFactory.Object, _messenger);
            _gradeVmInstance = new GradeVm(_mockDbContextFactory.Object, _messenger);
            _lessonsVmInstance = new LessonsVm(_mockDbContextFactory.Object, _messenger);
            _diaryTeacherVmInstance = new DiaryTeacherVm(_mockDbContextFactory.Object, _messenger);
            _lessonTeacherVmInstance = new LessonTeacherVm(_mockDbContextFactory.Object, _messenger);

            _navigationAdminVmInstance = new NavigationAdminVm(
                _homeAdminVmInstance,
                _mockLessonAdminVm.Object, // ИСПОЛЬЗУЕМ КОРРЕКТНО ИНИЦИАЛИЗИРОВАННЫЙ МОК
                _diaryAdminVmInstance,
                _classroomsAdminVmInstance,
                _subjectsAdminVmInstance,
                _usersAdminVmInstance,
                _groupsAdminVmInstance
            );

            _navigationVmInstance = new NavigationVm(
                _homeStudentVmInstance,
                _lessonsVmInstance,
                _gradeVmInstance,
                _messenger
            );

            _teacherNavigationVmInstance = new TeacherNavigationVm(
                _homeTeacherVmInstance,
                _lessonTeacherVmInstance,
                _diaryTeacherVmInstance,
                _messenger
            );
        }

        public void Dispose()
        {
            // Здесь нет необходимости сбрасывать мессенджер, так как он управляется MessengerFixture.
        }

        private MainViewModel CreateViewModel()
        {
            return new MainViewModel(
                _loginViewModelInstance,
                _homeStudentVmInstance,
                _homeAdminVmInstance,
                _homeTeacherVmInstance,
                _classroomsAdminVmInstance,
                _diaryAdminVmInstance,
                _groupsAdminVmInstance,
                _subjectsAdminVmInstance,
                _usersAdminVmInstance,
                _gradeVmInstance,
                _lessonsVmInstance,
                _diaryTeacherVmInstance,
                _lessonTeacherVmInstance,
                _navigationAdminVmInstance,
                _navigationVmInstance,
                _teacherNavigationVmInstance,
                _messenger
            );
        }

        [Fact]
        public void Constructor_SetsCurrentApplicationContentToLoginViewModel()
        {
            var vm = CreateViewModel();
            Assert.Equal(_loginViewModelInstance, vm.CurrentApplicationContent);
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_WithUser_SetsApplicationShellViewModel()
        {
            var user = new User { UserID = 1, Username = "testuser", FirstName = "Test", LastName = "User", Role = new Role { RoleID = 1, RoleName = "SomeRole" } };
            var vm = CreateViewModel();

            _messenger.Send(new UserAuthenticatedMessage(user));
            await Task.Delay(100);

            Assert.IsType<ApplicationShellViewModel>(vm.CurrentApplicationContent);
            var applicationShellVm = vm.CurrentApplicationContent as ApplicationShellViewModel;
            Assert.NotNull(applicationShellVm);
        }

        [Fact]
        public async Task Receive_UserAuthenticatedMessage_WithNullUser_SetsLoginViewModel()
        {
            var user = new User { UserID = 1, Username = "testuser", FirstName = "Test", LastName = "User", Role = new Role { RoleID = 1, RoleName = "SomeRole" } };
            var vm = CreateViewModel();

            _messenger.Send(new UserAuthenticatedMessage(user));
            await Task.Delay(100);
            Assert.IsType<ApplicationShellViewModel>(vm.CurrentApplicationContent);

            _messenger.Send(new UserAuthenticatedMessage(null));
            await Task.Delay(100);

            Assert.IsType<LoginViewModel>(vm.CurrentApplicationContent);
            Assert.Equal(_loginViewModelInstance, vm.CurrentApplicationContent);
        }

        [Fact]
        public async Task LogoutCommand_SendsUserAuthenticatedMessageWithNull()
        {
            var vm = CreateViewModel();

            var user = new User { UserID = 1, Username = "testuser", FirstName = "Test", LastName = "User", Role = new Role { RoleID = 1, RoleName = "SomeRole" } };

            _messenger.Send(new UserAuthenticatedMessage(user));
            await Task.Delay(100);

            Assert.IsType<ApplicationShellViewModel>(vm.CurrentApplicationContent);

            vm.LogoutCommand.Execute(null);

            await Task.Delay(100);

            Assert.IsType<LoginViewModel>(vm.CurrentApplicationContent);

            var loginVm = vm.CurrentApplicationContent as LoginViewModel;

            Assert.NotNull(loginVm);
            Assert.Equal("", loginVm.Username);
            Assert.Equal("", loginVm.Password);
            Assert.Equal("", loginVm.ErrorMessage);
        }
    }
}