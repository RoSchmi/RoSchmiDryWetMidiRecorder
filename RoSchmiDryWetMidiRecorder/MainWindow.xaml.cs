using RoSchmiDryWetMidiUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace RoSchmiDryWetMidiUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    // Changes taken from RS.Fritz.Manager
    // x:ClassModifier="internal" must be included in MainWindao.xaml
    internal partial class MainWindow : Window
    {

        MainWindowViewModel mainWindowViewModel;
        public MainWindow()
        {
            InitializeComponent();
            mainWindowViewModel = new MainWindowViewModel();
            DataContext = mainWindowViewModel;

        }
    }
}

