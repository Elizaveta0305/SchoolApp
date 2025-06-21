using Xunit;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.ViewModels;
using SchoolApplication.Messages;
using Microsoft.EntityFrameworkCore;

namespace SchoolApplication.Tests
{
    public class NavigationVmTests
    {
        private readonly Mock<HomeVm> _mockHomeVm;
        private readonly Mock<LessonsVm> _mockLessonsVm;
        private readonly Mock<GradeVm> _mockGradeVm;

        private readonly Mock<IDbContextFactory<SchoolApplication.Data.ApplicationDbContext>> _mockDbContextFactory;
        // Добавляем мок для IMessenger, так как LessonsVm его использует.
        private readonly Mock<IMessenger> _mockMessenger; // <--- НОВОЕ

        private readonly WeakReferenceMessenger _messenger;

        public NavigationVmTests()
        {
            _mockDbContextFactory = new Mock<IDbContextFactory<SchoolApplication.Data.ApplicationDbContext>>();
            _mockMessenger = new Mock<IMessenger>(); // <--- НОВОЕ: Инициализируем мок мессенджера

            _mockHomeVm = new Mock<HomeVm>(_mockDbContextFactory.Object);
            // Теперь передаем ОБА мок-объекта в конструктор LessonsVm
            _mockLessonsVm = new Mock<LessonsVm>(_mockDbContextFactory.Object, _mockMessenger.Object); // <--- ИЗМЕНЕНО!
            _mockGradeVm = new Mock<GradeVm>(_mockDbContextFactory.Object);

            _messenger = WeakReferenceMessenger.Default;
            _messenger.Reset();
        }

        [Fact]
        public void HomeCommand_SendsNavigateMessageWithHomeVm()
        {
            // Arrange
            var vm = new NavigationVm(
                _mockHomeVm.Object,
                _mockLessonsVm.Object,
                _mockGradeVm.Object);

            NavigateMessage? receivedMessage = null;

            _messenger.Register<NavigationVmTests, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            // Act
            vm.HomeCommand.Execute(null);

            // Assert
            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockHomeVm.Object, receivedMessage.Value);
            Assert.IsAssignableFrom<HomeVm>(receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void LessonsCommand_SendsNavigateMessageWithLessonsVm()
        {
            // Arrange
            var vm = new NavigationVm(
                _mockHomeVm.Object,
                _mockLessonsVm.Object,
                _mockGradeVm.Object);

            NavigateMessage? receivedMessage = null;
            _messenger.Register<NavigationVmTests, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            // Act
            vm.LessonsCommand.Execute(null);

            // Assert
            Assert.NotNull(receivedMessage);
            Assert.Same(_mockLessonsVm.Object, receivedMessage.Value);
            Assert.IsAssignableFrom<LessonsVm>(receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void GradeCommand_SendsNavigateMessageWithGradeVm()
        {
            // Arrange
            var vm = new NavigationVm(
                _mockHomeVm.Object,
                _mockLessonsVm.Object,
                _mockGradeVm.Object);

            NavigateMessage? receivedMessage = null;
            _messenger.Register<NavigationVmTests, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            // Act
            vm.GradeCommand.Execute(null);

            // Assert
            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockGradeVm.Object, receivedMessage.Value);
            Assert.IsAssignableFrom<GradeVm>(receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }
    }
}