using Xunit;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using SchoolApplication.ViewModels;
using SchoolApplication.Messages;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;

namespace SchoolApplication.Tests
{
    [Collection("MessengerCollection")]
    public class NavigationVmTests
    {
        private readonly Mock<HomeVm> _mockHomeVm;
        private readonly Mock<LessonsVm> _mockLessonsVm;
        private readonly Mock<GradeVm> _mockGradeVm;

        private readonly Mock<IDbContextFactory<ApplicationDbContext>> _mockDbContextFactory;
        private readonly IMessenger _messenger;

        private readonly Mock<IMessenger> _mockLessonsVmMessenger;

        public NavigationVmTests(MessengerFixture fixture)
        {
            _mockDbContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
            _messenger = fixture.Messenger;
            _mockHomeVm = new Mock<HomeVm>(_mockDbContextFactory.Object, _messenger);

            _mockLessonsVm = new Mock<LessonsVm>(_mockDbContextFactory.Object, _messenger);

            _mockGradeVm = new Mock<GradeVm>(_mockDbContextFactory.Object, _messenger);
        }

        [Fact]
        public void HomeCommand_SendsNavigateMessageWithHomeVm()
        {
            var vm = new NavigationVm(
                _mockHomeVm.Object,
                _mockLessonsVm.Object,
                _mockGradeVm.Object,
                _messenger);

            NavigateMessage? receivedMessage = null;

            _messenger.Register<NavigationVmTests, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.HomeCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Same(_mockHomeVm.Object, receivedMessage.Value);
            Assert.IsAssignableFrom<HomeVm>(receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void LessonsCommand_SendsNavigateMessageWithLessonsVm()
        {
            var vm = new NavigationVm(
                _mockHomeVm.Object,
                _mockLessonsVm.Object,
                _mockGradeVm.Object,
                _messenger);

            NavigateMessage? receivedMessage = null;
            _messenger.Register<NavigationVmTests, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.LessonsCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Same(_mockLessonsVm.Object, receivedMessage.Value);
            Assert.IsAssignableFrom<LessonsVm>(receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void GradeCommand_SendsNavigateMessageWithGradeVm()
        {
            var vm = new NavigationVm(
                _mockHomeVm.Object,
                _mockLessonsVm.Object,
                _mockGradeVm.Object,
                _messenger);

            NavigateMessage? receivedMessage = null;
            _messenger.Register<NavigationVmTests, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.GradeCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Same(_mockGradeVm.Object, receivedMessage.Value);
            Assert.IsAssignableFrom<GradeVm>(receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }
    }
}