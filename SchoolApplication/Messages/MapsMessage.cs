using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.Messages
{
    public class NavigateMessage : ValueChangedMessage<ObservableObject>
    {
        public NavigateMessage(ObservableObject viewModelToNavigate) : base(viewModelToNavigate)
        {
        }
    }
}
