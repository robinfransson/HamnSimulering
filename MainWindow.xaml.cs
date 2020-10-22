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
using System.Data;

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
        WaitingBoats boatsAtSea = new WaitingBoats();
        int daysPassed = 0;
        int boatsRejected = 0;
        DispatcherTimer dispatcherTimer;
        int numberOfNewBoats = 5;




        public MainWindow()
        {
            LoadSaveFiles();
            InitializeComponent();
            SetupAutomatic();
            BoatData.Init();
            leftHarbourGrid.ItemsSource = BoatData.Info("LeftHarbour").DefaultView;
            rightHarbourGrid.ItemsSource = BoatData.Info("RightHarbour").DefaultView;
            waitingBoatsGrid.ItemsSource = BoatData.Info("WaitingBoats").DefaultView;
        }

        void LoadSaveFiles()
        {
            leftHarbour.Port = SaveFileManager.Load("left.txt");
            rightHarbour.Port = SaveFileManager.Load("right.txt");
            boatsAtSea.Waiting = SaveFileManager.Load("waiting.txt");
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
            SimulateOneDay();
            TryToDock();
            UpdateDataTables();
            daysPassed++;
        }

        private void UpdateDataTables()
        {
            DataTable leftHarbourData = BoatData.Info("LeftHarbour");
            DataTable rightHarbourData = BoatData.Info("RightHarbour");
            DataTable waitingBoats = BoatData.Info("WaitingBoats");
            foreach (Boat boat in leftHarbour.Port)
            {
                DataRow[] boatData = leftHarbourData.Select($"Nr = '{boat.ModelID}");
                if (boatData[0] == null)
                {
                    BoatData.UpdateTable()
                }
            }
        }

        private void SimulateOneDay()
        {
            if(leftHarbour.Port.Any())
            {
                foreach(Boat boat in leftHarbour.Port)
                {
                    boat.DaysSpentAtHarbour++;
                }
            }
            leftHarbourLabel.Content = "Antal båtar i hamnen: " + leftHarbour.Port.Count();
            if (rightHarbour.Port.Any())
            {
                foreach (Boat boat in leftHarbour.Port)
                {
                    boat.DaysSpentAtHarbour++;
                }
            }
            rightHarbourLabel.Content = "Antal båtar i hamnen: " + rightHarbour.Port.Count();
            boatsAtSea.AddBoatToWaiting(numberOfNewBoats);
        }
        private void TryToDock()
        {
            foreach(Boat boat in boatsAtSea.Waiting)
            {
                string assignedSpot = "";
                if(leftHarbour.AreThereFreeSpots(boat, out assignedSpot))
                {
                    boat.AssignSpot(assignedSpot);
                    leftHarbour.UpdateSpots(boat);
                    leftHarbour.Port.Add(boat);
                }
                else if(rightHarbour.AreThereFreeSpots(boat, out assignedSpot))
                {
                    boat.AssignSpot(assignedSpot);
                    rightHarbour.UpdateSpots(boat);
                    rightHarbour.Port.Add(boat);
                }
                else
                {
                    boatsRejected++;
                }
            }
            if(boatsAtSea.Waiting.Any())
            {
                boatsAtSea.Waiting.Clear();
            }
        }
        private void manualContinue_Click(object sender, RoutedEventArgs e)
        {
            SimulateTimePassed();
        }
        private void Save()
        {
            SaveFileManager.Save(leftHarbour.Port, "left.txt");
            SaveFileManager.Save(rightHarbour.Port, "right.txt");
            SaveFileManager.Save(boatsAtSea.Waiting, "waiting.txt");
        }
    }
}
