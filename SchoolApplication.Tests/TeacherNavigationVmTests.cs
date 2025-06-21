using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore; // Необходимо для IDbContextFactory
using Moq;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.ViewModels;
using Xunit;

namespace SchoolApplication.Tests
{
    public class TeacherNavigationVmTests
    {
        private readonly Mock<HomeTeacherVm> _mockHomeTeacherVm;
        private readonly Mock<LessonTeacherVm> _mockLessonsTeacherVm;
        private readonly Mock<DiaryTeacherVm> _mockDiaryTeacherVm;

        private readonly Mock<IDbContextFactory<ApplicationDbContext>> _mockDbContextFactory;

        private readonly WeakReferenceMessenger _messenger;

        public TeacherNavigationVmTests()
        {
            _mockDbContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();

            _mockHomeTeacherVm = new Mock<HomeTeacherVm>(_mockDbContextFactory.Object);
            _mockLessonsTeacherVm = new Mock<LessonTeacherVm>(_mockDbContextFactory.Object);
            _mockDiaryTeacherVm = new Mock<DiaryTeacherVm>(_mockDbContextFactory.Object);

            _messenger = WeakReferenceMessenger.Default;
            _messenger.Reset();
        }

        [Fact]
        public void HomeTeacherCommand_SendsNavigateMessageWithHomeTeacherVm()
        {
            var vm = new TeacherNavigationVm(
                _mockHomeTeacherVm.Object,
                _mockLessonsTeacherVm.Object,
                _mockDiaryTeacherVm.Object,
                _messenger);

            NavigateMessage? receivedMessage = null;

            _messenger.Register<TeacherNavigationVmTests, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.HomeTeacherCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Same(_mockHomeTeacherVm.Object, receivedMessage.Value);
            Assert.IsAssignableFrom<HomeTeacherVm>(receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void LessonsTeacherCommand_SendsNavigateMessageWithLessonsTeacherVm()
        {
            var vm = new TeacherNavigationVm(
                _mockHomeTeacherVm.Object,
                _mockLessonsTeacherVm.Object,
                _mockDiaryTeacherVm.Object);

            NavigateMessage? receivedMessage = null;
            _messenger.Register<TeacherNavigationVmTests, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.LessonsTeacherCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockLessonsTeacherVm.Object, receivedMessage.Value);
            Assert.IsAssignableFrom<LessonTeacherVm>(receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void DiaryTeacherCommand_SendsNavigateMessageWithDiaryTeacherVm()
        {
            var vm = new TeacherNavigationVm(
                _mockHomeTeacherVm.Object,
                _mockLessonsTeacherVm.Object,
                _mockDiaryTeacherVm.Object);

            NavigateMessage? receivedMessage = null;
            _messenger.Register<TeacherNavigationVmTests, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.DiaryTeacherCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockDiaryTeacherVm.Object, receivedMessage.Value);
            Assert.IsAssignableFrom<DiaryTeacherVm>(receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }
    }
}