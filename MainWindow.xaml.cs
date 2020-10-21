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
using System.Windows.Threading;

namespace HamnSimulering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool automatic = false;
        Harbour leftHarbour = new Harbour(true); // vänstra hamnen ska föredra segelbåtar
        Harbour rightHarbour = new Harbour();
        int daysPassed = 0;
        int boatsRejected = 0;
        DispatcherTimer dispatcherTimer;
        public MainWindow()
        {
            leftHarbour.Port = SaveFileManager.Load("left.txt");
            rightHarbour.Port = SaveFileManager.Load("right.txt");
            Water.Waiting = SaveFileManager.Load("waiting.txt");
            InitializeComponent();
            waitingBoatsGrid.ItemsSource = Water.Waiting;
            leftHarbourGrid.ItemsSource = leftHarbour.Port;
            rightHarbourGrid.ItemsSource = rightHarbour.Port;
            SetupAutomatic();
        }

        private void toggleAuto_Click(object sender, RoutedEventArgs e)
        {
            automatic = !automatic; //toggle
            manualContinue.IsEnabled = !automatic; // togglar manuella knappen
            dispatcherTimer.IsEnabled = automatic; // togglar timern
            
        }
        void SetupAutomatic()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (sender, eventArgs) =>
            {
                    SimulateTimePassed();
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5); // var 5:e sekund ska lambda expression utföras
        }
        private void SimulateTimePassed()
        {
            Generate.PopulateWater(5);
        }

        private void manualContinue_Click(object sender, RoutedEventArgs e)
        {
            SimulateTimePassed();
        }
    }
}
