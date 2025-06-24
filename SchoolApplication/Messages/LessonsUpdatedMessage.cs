using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Diagnostics.CodeAnalysis;

namespace SchoolApplication.Messages
{
    [ExcludeFromCodeCoverage]
    public class LessonsUpdatedMessage : ValueChangedMessage<bool>
    {
        public LessonsUpdatedMessage(bool value) : base(value)
        {
        }
    }
}