using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SchoolApplication.Messages
{
    public class LessonsUpdatedMessage : ValueChangedMessage<bool>
    {
        public LessonsUpdatedMessage(bool value) : base(value)
        {
        }
    }
}