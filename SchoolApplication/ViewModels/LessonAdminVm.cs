using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolApplication.ViewModels
{
    public class LessonAdminVm : ObservableObject
    {
        public LessonAdminVm(IDbContextFactory<ApplicationDbContext> dbContextFactory, IMessenger messenger)
        {

        }
    }
}
