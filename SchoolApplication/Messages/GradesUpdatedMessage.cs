using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SchoolApplication.Messages
{
    public class GradesUpdatedMessage : ValueChangedMessage<bool>
    {
        public GradesUpdatedMessage(bool value) : base(value)
        {
        }
    }
}