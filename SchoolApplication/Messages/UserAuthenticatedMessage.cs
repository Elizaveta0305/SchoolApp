using CommunityToolkit.Mvvm.Messaging.Messages;
using SchoolApplication.Models;

namespace SchoolApplication.Messages
{
    public class UserAuthenticatedMessage : ValueChangedMessage<User>
    {
        public UserAuthenticatedMessage(User user) : base(user)
        {
        }
    }
}