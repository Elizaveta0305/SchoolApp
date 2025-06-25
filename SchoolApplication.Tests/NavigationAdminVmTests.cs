using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolApplication.Data;
using SchoolApplication.Messages;
using SchoolApplication.ViewModels;
using System.Threading.Tasks;
using Xunit;

namespace SchoolApplication.Tests
{
    [Collection("MessengerCollection")]
    public class NavigationAdminVmTests : IDisposable
    {
        private IMessenger _messenger;

        private HomeAdminVm _mockHomeAdminVm;
        private LessonAdminVm _mockLessonsAdminVm;
        private DiaryAdminVm _mockDiaryAdminVm;
        private ClassroomsAdminVm _mockClassroomsAdminVm;
        private SubjectAdminVm _mockSubjectAdminVm;
        private UsersAdminVm _mockUsersAdminVm;
        private GroupsAdminVm _mockGroupsAdminVm;

        public NavigationAdminVmTests(MessengerFixture fixture)
        {
            _messenger = fixture.Messenger;
            _mockHomeAdminVm = new Mock<HomeAdminVm>().Object;
            _mockLessonsAdminVm = new Mock<LessonAdminVm>(
                new Mock<IDbContextFactory<ApplicationDbContext>>().Object,
                _messenger
            ).Object;
            _mockDiaryAdminVm = new Mock<DiaryAdminVm>().Object;
            _mockClassroomsAdminVm = new Mock<ClassroomsAdminVm>().Object;
            _mockSubjectAdminVm = new Mock<SubjectAdminVm>().Object;
            _mockUsersAdminVm = new Mock<UsersAdminVm>().Object;
            _mockGroupsAdminVm = new Mock<GroupsAdminVm>().Object;
        }
        public void Dispose()
        {

        }

        private NavigationAdminVm CreateViewModel()
        {
            return new NavigationAdminVm(
                _mockHomeAdminVm,
                _mockLessonsAdminVm,
                _mockDiaryAdminVm,
                _mockClassroomsAdminVm,
                _mockSubjectAdminVm,
                _mockUsersAdminVm,
                _mockGroupsAdminVm
            );
        }

        [Fact]
        public void Constructor_InitializesAllCommands()
        {
            var vm = CreateViewModel();

            Assert.NotNull(vm.HomeAdminCommand);
            Assert.NotNull(vm.LessonsAdminCommand);
            Assert.NotNull(vm.DiaryAdminCommand);
            Assert.NotNull(vm.ClassroomsAdminCommand);
            Assert.NotNull(vm.SubjectAdminCommand);
            Assert.NotNull(vm.UsersAdminCommand);
            Assert.NotNull(vm.GroupsAdminCommand);
        }

        [Fact]
        public void HomeAdminCommand_SendsNavigateMessageWithHomeAdminVm()
        {
            var vm = CreateViewModel();
            NavigateMessage receivedMessage = null;

            _messenger.Register<object, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.HomeAdminCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockHomeAdminVm, receivedMessage.Value);

            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void LessonsAdminCommand_SendsNavigateMessageWithLessonsAdminVm()
        {
            var vm = CreateViewModel();
            NavigateMessage receivedMessage = null;

            _messenger.Register<object, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.LessonsAdminCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockLessonsAdminVm, receivedMessage.Value);
            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void DiaryAdminCommand_SendsNavigateMessageWithDiaryAdminVm()
        {
            var vm = CreateViewModel();
            NavigateMessage receivedMessage = null;

            _messenger.Register<object, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.DiaryAdminCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockDiaryAdminVm, receivedMessage.Value);
            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void ClassroomsAdminCommand_SendsNavigateMessageWithClassroomsAdminVm()
        {
            var vm = CreateViewModel();
            NavigateMessage receivedMessage = null;

            _messenger.Register<object, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.ClassroomsAdminCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockClassroomsAdminVm, receivedMessage.Value);
            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void SubjectAdminCommand_SendsNavigateMessageWithSubjectAdminVm()
        {
            var vm = CreateViewModel();
            NavigateMessage receivedMessage = null;

            _messenger.Register<object, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.SubjectAdminCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockSubjectAdminVm, receivedMessage.Value);
            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void UsersAdminCommand_SendsNavigateMessageWithUsersAdminVm()
        {
            var vm = CreateViewModel();
            NavigateMessage receivedMessage = null;

            _messenger.Register<object, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.UsersAdminCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockUsersAdminVm, receivedMessage.Value);
            _messenger.UnregisterAll(this);
        }

        [Fact]
        public void GroupsAdminCommand_SendsNavigateMessageWithGroupsAdminVm()
        {
            var vm = CreateViewModel();
            NavigateMessage receivedMessage = null;

            _messenger.Register<object, NavigateMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            vm.GroupsAdminCommand.Execute(null);

            Assert.NotNull(receivedMessage);
            Assert.Equal(_mockGroupsAdminVm, receivedMessage.Value);
            _messenger.UnregisterAll(this);
        }
    }
}