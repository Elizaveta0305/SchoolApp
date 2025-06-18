using System.Linq;
using System.Windows.Controls;
using SchoolApplication.ViewModels;
using ScottPlot;
using System.ComponentModel;
using ScottPlot.Palettes;
using ScottPlot.Colormaps;
using ScottPlot.Stylers;
using ScottPlot.Plottables;

namespace SchoolApplication.Views.UserControls.StudentUC
{
    public partial class Chart : UserControl
    {

        public Chart()
        {
            InitializeComponent();

            double[] values = [10];
            GaugePlot.Plot.Add.RadialGaugePlot(values);

            GaugePlot.Refresh();
        }

    }
}