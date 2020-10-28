using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using HamnSimulering;

namespace HamnSimulering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        bool errorShown = false;
        bool automatic = false;
        Harbour leftHarbour = new Harbour("LeftHarbour");
        Harbour rightHarbour = new Harbour("RightHarbour");
        WaitingBoats waitingBoats = new WaitingBoats();
        DispatcherTimer dispatcherTimer;
        int timerSeconds = 5;
        int timesSinceSave = 0;
        static Func<List<Boat>, long> AverageWeight = (p) =>
        {
            if (p.Any())
            {
                return p.Sum(boat => boat.Weight) / p.Count;
            }
            else
            {
                return 0;
            }

        };
        Func<Harbour, string> HarbourInfo = (harb) =>
        {
            return


            $"Roddbåtar: {harb.Port.Count(boat => boat is Rowboat)} \n" +
            $"Motorbåtar: {harb.Port.Count(boat => boat is Motorboat)} \n" +
            $"Segelbåtar: {harb.Port.Count(boat => boat is Sailboat)} \n" +
            $"Katamaraner: {harb.Port.Count(boat => boat is Catamaran)} \n" +
            $"Lastfartyg: {harb.Port.Count(boat => boat is Cargoship)} \n\n" +
            $"Totalvikt: {harb.Port.Sum(boat => boat.Weight)}\n" +
            $"Snittvikt: {AverageWeight(harb.Port)} \n";

        };




        public MainWindow()
        {
            InitializeComponent();


            SetupAutomatic();
            BoatData.SetupDataTables();


            leftHarbourGrid.ItemsSource = BoatData.BoatDataViewer("LeftHarbour");
            leftHarbourGrid.RowHeight = 17;
            rightHarbourGrid.ItemsSource = BoatData.BoatDataViewer("RightHarbour");
            rightHarbourGrid.RowHeight = 17;
            waitingBoatsGrid.ItemsSource = BoatData.BoatDataViewer("WaitingBoats");


            leftHarbourGrid.IsReadOnly = true;
            rightHarbourGrid.IsReadOnly = true;
            waitingBoatsGrid.IsReadOnly = true;




            LoadSaveFiles();
            Simulate.UpdateData(leftHarbour);
            Simulate.UpdateData(rightHarbour);

            BoatData.UpdateVisitors(waitingBoats.Waiting);
            UpdateLabels();



        }
        public void UpdateLabels()
        {
            waitingBoatsLabel.Content = "Väntande båtar: " + waitingBoats.Waiting.Count();
            numberOfDaysLabel.Content = "Passerade dagar: " + Simulate.daysPassed;
            rejectedBoatsLabel.Content = "Avvisade båtar: " + Simulate.boatsRejected;

            leftHarbourLabel.Content = "Antal båtar i hamnen: " + leftHarbour.Port.Count();
            leftHarbourSpotsLeftLabel.Content = "Lediga platser: " + leftHarbour.SpotsLeft;



            rightHarbourLabel.Content = "Antal båtar i hamnen: " + rightHarbour.Port.Count();
            rightHarbourSpotsLeftLabel.Content = "Lediga platser: " + rightHarbour.SpotsLeft;


            rightHarbourLabel.ToolTip = HarbourInfo(rightHarbour);
            leftHarbourLabel.ToolTip = HarbourInfo(leftHarbour);
        }


        void LoadSaveFiles()
        {
            try
            {
                leftHarbour.Port = SaveFileManager.Load("left.txt");
                rightHarbour.Port = SaveFileManager.Load("right.txt");
                waitingBoats.Waiting = SaveFileManager.Load("waiting.txt");

                if (leftHarbour.Port.Any())
                {
                    foreach (Boat boat in leftHarbour.Port)
                    {
                        leftHarbour.UpdateSpots(boat);
                    }
                }
                if (rightHarbour.Port.Any())
                {

                    foreach (Boat boat in rightHarbour.Port)
                    {
                        rightHarbour.UpdateSpots(boat);
                    }
                }
                if (!waitingBoats.Waiting.Any())
                {
                    Simulate.AddToWaiting(waitingBoats);
                }

                BoatData.UpdateVisitors(waitingBoats.Waiting);

                SaveFileManager.LoadStatistics("stats.txt", out Simulate.daysPassed, out Simulate.boatsRejected);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
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
            dispatcherTimer.Interval = new TimeSpan(0, 0, timerSeconds);
        }

        private void SimulateTimePassed()
        {
            Simulate.OneDay(waitingBoats, leftHarbour, rightHarbour);
            UpdateLabels();
        }

        private void ManualContinue_Click(object sender, RoutedEventArgs e)
        {
            SimulateTimePassed();
        }


        private void Save(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveFileManager.Save(leftHarbour.Port, "left.txt");
            SaveFileManager.Save(rightHarbour.Port, "right.txt");
            SaveFileManager.Save(waitingBoats.Waiting, "waiting.txt");
            SaveFileManager.SaveStatistics("stats.txt", Simulate.daysPassed, Simulate.boatsRejected);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Save(sender, null);
        }




        private void newBoatsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(newBoatsLabel is null))
            {
                try
                {
                    Simulate.numberOfNewBoats = (int)e.NewValue;
                    newBoatsLabel.Content = "Nya båtar per dag: " + Simulate.numberOfNewBoats;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }




        private void autoSpeedTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                try
                {

                    timerSeconds = Int32.Parse(autoSpeedTextBox.Text);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, timerSeconds);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void clearSave_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete your savefiles?", "WARNING!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                SaveFileManager.DeleteSaves();

                leftHarbour = new Harbour("LeftHarbour");
                rightHarbour = new Harbour("RightHarbour");
                waitingBoats = new WaitingBoats();

                BoatData.ClearTable("LeftHarbour");
                BoatData.ClearTable("RightHarbour");
                BoatData.ClearTable("WaitingBoats");


                Simulate.AddToWaiting(waitingBoats);
                BoatData.UpdateVisitors(waitingBoats.Waiting);
                Simulate.UpdateData(leftHarbour);
                Simulate.UpdateData(rightHarbour);
            }
        }

        private void HarbourGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Plats")
            {
                e.Column.Width = 40;
            }
            else if (e.Column.Header.ToString() == "Båttyp")
            {
                e.Column.Width = 100;
            }
        }

    }
}

















//private void ListEmptySpots()
//{
//    BoatData.ListFreeSpots(leftHarbour.IsCurrentSpotTaken, "LeftHarbour");
//    BoatData.ListFreeSpots(rightHarbour.IsCurrentSpotTaken, "RightHarbour");
//}

//private void UpdateDataTables(bool clear = false)
//{
//    if (clear)
//    {

//    }
//    else
//    {

//        BoatData.UpdateHarbour(leftHarbour.Port, "LeftHarbour");
//        BoatData.UpdateHarbour(rightHarbour.Port, "RightHarbour");
//        BoatData.UpdateVisitors(waitingBoats.Waiting);
//    }
//}




//private void TryToDock()
//{
//    foreach (Boat boat in waitingBoats.Waiting)
//    {
//        if (leftHarbour.HasFreeSpots(boat, out int[] assignedSpot))
//        {
//            boat.AssignedSpotAtHarbour = assignedSpot;
//            leftHarbour.UpdateSpots(boat);
//            leftHarbour.Port.Add(boat);
//        }
//        else if (rightHarbour.HasFreeSpots(boat, out assignedSpot))
//        {
//            boat.AssignedSpotAtHarbour = assignedSpot;
//            rightHarbour.UpdateSpots(boat);
//            rightHarbour.Port.Add(boat);
//        }
//        else
//        {
//            UpdateDataTables();
//            ListEmptySpots();
//            string message = $"{boat.GetBoatType()} {boat.ModelID} could not get a spot!\n" +
//                             $"It needed {boat.SizeInSpots} spots.";
//            MessageBox.Show(message);
//            boatsRejected++;
//        }
//    }
//    if (waitingBoats.Waiting.Any())
//    {
//        waitingBoats.Waiting.Clear();
//    }
//}