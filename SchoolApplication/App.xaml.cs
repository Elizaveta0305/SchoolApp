using SchoolApplication.Views.Windows;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SchoolApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        private void OnStartup(object sender, StartupEventArgs e)
        {
            var stnd = new StudentView();
            stnd.Show();
        }
    }

}
