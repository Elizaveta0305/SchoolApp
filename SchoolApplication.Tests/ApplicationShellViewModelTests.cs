using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.Models;
using SchoolApplication.ViewModels;
using System;
using System.Linq;
using Xunit;

namespace SchoolApplication.Tests
{
    [Collection("MessengerCollection")]
    public class ApplicationShellViewModelTests : IDisposable
    {
        private readonly Mock<IDbContextFactory<ApplicationDbContext>> _mockDbContextFactory;
        private readonly IMessenger _messenger;

        private readonly Mock<HomeAdminVm> _mockHomeAdminVm;
        private readonly Mock<HomeTeacherVm> _mockHomeTeacherVm;
        private readonly Mock<HomeVm> _mockHomeStudentVm;
        private readonly Mock<ClassroomsAdminVm> _mockClassroomsAdminVm;
        private readonly Mock<DiaryAdminVm> _mockDiaryAdminVm;
        private readonly Mock<GroupsAdminVm> _mockGroupsAdminVm;
        private readonly Mock<SubjectAdminVm> _mockSubjectsAdminVm;
        private readonly Mock<UsersAdminVm> _mockUsersAdminVm;
        private readonly Mock<GradeVm> _mockGradeVm;
        private readonly Mock<LessonsVm> _mockLessonsVm;
        private readonly Mock<DiaryTeacherVm> _mockDiaryTeacherVm;
        private readonly Mock<LessonTeacherVm> _mockLessonTeacherVm;
        private readonly Mock<LessonAdminVm> _mockLessonAdminVm;

        private readonly Mock<NavigationAdminVm> _mockNavigationAdminVm;
        private readonly Mock<NavigationVm> _mockNavigationVm;
        private readonly Mock<TeacherNavigationVm> _mockTeacherNavigationVm;

        public ApplicationShellViewModelTests(MessengerFixture fixture)
        {
            _messenger = fixture.Messenger;
            _mockDbContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();

            _mockHomeAdminVm = new Mock<HomeAdminVm>();
            _mockClassroomsAdminVm = new Mock<ClassroomsAdminVm>();
            _mockDiaryAdminVm = new Mock<DiaryAdminVm>();
            _mockGroupsAdminVm = new Mock<GroupsAdminVm>();
            _mockSubjectsAdminVm = new Mock<SubjectAdminVm>();
            _mockUsersAdminVm = new Mock<UsersAdminVm>();

            _mockLessonAdminVm = new Mock<LessonAdminVm>(_mockDbContextFactory.Object, _messenger);

            _mockHomeStudentVm = new Mock<HomeVm>(_mockDbContextFactory.Object, _messenger);
            _mockHomeTeacherVm = new Mock<HomeTeacherVm>(_mockDbContextFactory.Object, _messenger);
            _mockGradeVm = new Mock<GradeVm>(_mockDbContextFactory.Object, _messenger);
            _mockLessonsVm = new Mock<LessonsVm>(_mockDbContextFactory.Object, _messenger);
            _mockDiaryTeacherVm = new Mock<DiaryTeacherVm>(_mockDbContextFactory.Object, _messenger);
            _mockLessonTeacherVm = new Mock<LessonTeacherVm>(_mockDbContextFactory.Object, _messenger);

            _mockNavigationAdminVm = new Mock<NavigationAdminVm>(
                _mockHomeAdminVm.Object,
                _mockLessonAdminVm.Object,
                _mockDiaryAdminVm.Object,
                _mockClassroomsAdminVm.Object,
                _mockSubjectsAdminVm.Object,
                _mockUsersAdminVm.Object,
                _mockGroupsAdminVm.Object
            );

            _mockNavigationVm = new Mock<NavigationVm>(
                _mockHomeStudentVm.Object,
                _mockLessonsVm.Object,
                _mockGradeVm.Object,
                _messenger
            );

            _mockTeacherNavigationVm = new Mock<TeacherNavigationVm>(
                _mockHomeTeacherVm.Object,
                _mockLessonTeacherVm.Object,
                _mockDiaryTeacherVm.Object,
                _messenger
            );
        }

        public void Dispose()
        {
          
        }

        private ApplicationShellViewModel CreateViewModel(User user)
        {
            return new ApplicationShellViewModel(
                user,
                _mockHomeStudentVm.Object,
                _mockHomeAdminVm.Object,
                _mockHomeTeacherVm.Object,
                _mockClassroomsAdminVm.Object,
                _mockDiaryAdminVm.Object,
                _mockGroupsAdminVm.Object,
                _mockSubjectsAdminVm.Object,
                _mockUsersAdminVm.Object,
                _mockGradeVm.Object,
                _mockLessonsVm.Object,
                _mockDiaryTeacherVm.Object,
                _mockLessonTeacherVm.Object,
                _mockNavigationAdminVm.Object,
                _mockNavigationVm.Object,
                _mockTeacherNavigationVm.Object,
                _messenger
            );
        }

        [Fact]
        public void InitializeShellContent_SetsAdminViewModels_WhenUserIsAdmin()
        {
            var adminUser = new User
            {
                UserID = 1,
                Username = "admin",
                Role = new Role { RoleID = 1, RoleName = "Администратор" }
            };

            var viewModel = CreateViewModel(adminUser);

            Assert.Equal(_mockHomeAdminVm.Object, viewModel.CurrentMainContentViewModel);
            Assert.Equal(_mockNavigationAdminVm.Object, viewModel.CurrentNavigationViewModel);
        }

        [Fact]
        public void InitializeShellContent_SetsTeacherViewModels_WhenUserIsTeacher()
        {
            var teacherUser = new User
            {
                UserID = 2,
                Username = "teacher",
                Role = new Role { RoleID = 2, RoleName = "Преподаватель" }
            };

            var viewModel = CreateViewModel(teacherUser);

            Assert.Equal(_mockHomeTeacherVm.Object, viewModel.CurrentMainContentViewModel);
            Assert.Equal(_mockTeacherNavigationVm.Object, viewModel.CurrentNavigationViewModel);
        }

        [Fact]
        public void InitializeShellContent_SetsStudentViewModels_WhenUserIsStudent()
        {
            var studentUser = new User
            {
                UserID = 3,
                Username = "student",
                Role = new Role { RoleID = 3, RoleName = "Ученик" }
            };

            var viewModel = CreateViewModel(studentUser);

            Assert.Equal(_mockHomeStudentVm.Object, viewModel.CurrentMainContentViewModel);
            Assert.Equal(_mockNavigationVm.Object, viewModel.CurrentNavigationViewModel);
        }

        [Fact]
        public void Receive_SetsHomeAdminVm_WhenNavigateMessageContainsHomeAdminVm()
        {
            var adminUser = new User
            {
                UserID = 1,
                Username = "admin",
                Role = new Role { RoleID = 1, RoleName = "Администратор" }
            };
            var viewModel = CreateViewModel(adminUser);

            _messenger.Send(new NavigateMessage(_mockHomeAdminVm.Object));

            Assert.Equal(_mockHomeAdminVm.Object, viewModel.CurrentMainContentViewModel);
        }

        [Fact]
        public void Receive_SetsClassroomsAdminVm_WhenNavigateMessageContainsClassroomsAdminVm()
        {
            var adminUser = new User { UserID = 1, Role = new Role { RoleName = "Администратор" } };
            var viewModel = CreateViewModel(adminUser);

            viewModel.CurrentMainContentViewModel = _mockHomeAdminVm.Object;

            _messenger.Send(new NavigateMessage(_mockClassroomsAdminVm.Object));

            Assert.Equal(_mockClassroomsAdminVm.Object, viewModel.CurrentMainContentViewModel);
        }

        [Fact]
        public void Receive_SetsGradeVm_WhenNavigateMessageContainsGradeVm()
        {
            var studentUser = new User { UserID = 3, Role = new Role { RoleName = "Ученик" } };
            var viewModel = CreateViewModel(studentUser);

            viewModel.CurrentMainContentViewModel = _mockHomeStudentVm.Object;

            _messenger.Send(new NavigateMessage(_mockGradeVm.Object));

            Assert.Equal(_mockGradeVm.Object, viewModel.CurrentMainContentViewModel);
        }

        [Fact]
        public void Receive_SetsDiaryTeacherVm_WhenNavigateMessageContainsDiaryTeacherVm()
        {
            var teacherUser = new User { UserID = 2, Role = new Role { RoleName = "Преподаватель" } };
            var viewModel = CreateViewModel(teacherUser);

            viewModel.CurrentMainContentViewModel = _mockHomeTeacherVm.Object;

            _messenger.Send(new NavigateMessage(_mockDiaryTeacherVm.Object));

            Assert.Equal(_mockDiaryTeacherVm.Object, viewModel.CurrentMainContentViewModel);
        }

        [Fact]
        public void Receive_DoesNotChangeViewModel_WhenNavigateMessageContainsUnknownVm()
        {
            var adminUser = new User { UserID = 1, Role = new Role { RoleName = "Администратор" } };
            var viewModel = CreateViewModel(adminUser);
            var initialViewModel = viewModel.CurrentMainContentViewModel;

            var mockUnknownVm = new Mock<ObservableObject>().Object;

            _messenger.Send(new NavigateMessage(mockUnknownVm));

            Assert.Equal(initialViewModel, viewModel.CurrentMainContentViewModel);
        }

        [Fact]
        public void Receive_DoesNotChangeViewModel_WhenNavigateMessageIsNull()
        {
            var adminUser = new User { UserID = 1, Role = new Role { RoleName = "Администратор" } };
            var viewModel = CreateViewModel(adminUser);
            var initialViewModel = viewModel.CurrentMainContentViewModel;

            viewModel.Receive(null);

            Assert.Equal(initialViewModel, viewModel.CurrentMainContentViewModel);
        }
    }
}