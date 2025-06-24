using SchoolApplication.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace SchoolApplication.Views.Windows
{
    [ExcludeFromCodeCoverage]
    public class ApplicationContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? LoginTemplate { get; set; }
        public DataTemplate? MainAppTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is LoginViewModel)
            {
                return LoginTemplate;
            }
            else if (item is MainViewModel)
            {
                return MainAppTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}