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

        public static bool automatic = false;
        Harbour leftHarbour = new Harbour("LeftHarbour");
        Harbour rightHarbour = new Harbour("RightHarbour");
        List<Boat> waitingBoats;
        DispatcherTimer dispatcherTimer;
        int timerSeconds = 5;
        int timesSinceSave = 0;
        static Func<List<Boat>, long> AverageWeight = (port) =>
        {
            if (port.Any())
            {
                return port.Sum(boat => boat.Weight) / port.Count;
            }
            else
            {
                return 0;
            }

        };

        static Func<int, int, string> CalculatePercentage = (accepted, rejected) =>
        {
            try
            {
                int percent = accepted / rejected;
                return $"{percent}";
            }
            catch(DivideByZeroException)
            {
                return "100%";
            }
        };
        Func<List<Boat>, string> BoatStats = (port) =>
        {
            return


            $"Roddbåtar: {port.Count(boat => boat is Rowboat)} \n" +
            $"Motorbåtar: {port.Count(boat => boat is Motorboat)} \n" +
            $"Segelbåtar: {port.Count(boat => boat is Sailboat)} \n" +
            $"Katamaraner: {port.Count(boat => boat is Catamaran)} \n" +
            $"Lastfartyg: {port.Count(boat => boat is Cargoship)} \n\n" +
            $"Totalvikt: {port.Sum(boat => boat.Weight)}\n" +
            $"Snittvikt: {AverageWeight(port)} \n";

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


            Simulate.SetupAssigner(leftHarbour);
            Simulate.SetupAssigner(rightHarbour);

            BoatData.UpdateVisitors(waitingBoats);
            UpdateLabels();



        }
        public void UpdateLabels()
        {
            waitingBoatsLabel.Content = "Väntande båtar: " + waitingBoats.Count();
            numberOfDaysLabel.Content = "Passerade dagar: " + Simulate.daysPassed;
            rejectedBoatsLabel.Content = "Avvisade båtar: " + Simulate.boatsRejected;
            acceptedBoatsLabel.Content = "Antagna båtar: " + Simulate.boatsAccepted + $"  ({CalculatePercentage(Simulate.boatsAccepted, Simulate.boatsRejected)}";

            leftHarbourLabel.Content = "Antal båtar i hamnen: " + leftHarbour.Port.Count();
            leftHarbourSpotsLeftLabel.Content = "Lediga platser: " + leftHarbour.SpotsLeft;



            rightHarbourLabel.Content = "Antal båtar i hamnen: " + rightHarbour.Port.Count();
            rightHarbourSpotsLeftLabel.Content = "Lediga platser: " + rightHarbour.SpotsLeft;


            rightHarbourLabel.ToolTip = BoatStats(rightHarbour.Port);
            leftHarbourLabel.ToolTip = BoatStats(leftHarbour.Port);
            waitingBoatsLabel.ToolTip = BoatStats(waitingBoats);
        }


        void LoadSaveFiles()
        {
            try
            {
                leftHarbour.Port = SaveFileManager.Load("left.txt");
                rightHarbour.Port = SaveFileManager.Load("right.txt");
                Simulate.waitingBoats = SaveFileManager.Load("waiting.txt");
                waitingBoats = Simulate.waitingBoats;

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
                if (!waitingBoats.Any())
                {
                    Simulate.AddToWaiting();
                }

                BoatData.UpdateVisitors(waitingBoats);

                SaveFileManager.LoadStatistics("stats.txt", out Simulate.daysPassed, out Simulate.boatsRejected, out Simulate.boatsAccepted);
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
            Simulate.OneDay(leftHarbour, rightHarbour);
            UpdateLabels();
            timesSinceSave++;
            if(timesSinceSave > 9)
            {
                Save();
            }
        }

        private void ManualContinue_Click(object sender, RoutedEventArgs e)
        {
            SimulateTimePassed();
        }


        private void Save()
        {
            SaveFileManager.Save(leftHarbour.Port, "left.txt");
            SaveFileManager.Save(rightHarbour.Port, "right.txt");
            SaveFileManager.Save(waitingBoats, "waiting.txt");
            SaveFileManager.SaveStatistics("stats.txt", Simulate.daysPassed, Simulate.boatsRejected, Simulate.boatsAccepted);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
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
                    if(!automatic)
                    {

                        automatic = !automatic; //toggle
                        manualContinue.IsEnabled = !automatic; // togglar manuella knappen
                        dispatcherTimer.IsEnabled = automatic; // togglar timern
                    }
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
                Simulate.SetupAssigner(leftHarbour, true);
                Simulate.SetupAssigner(rightHarbour, true);
                Simulate.waitingBoats = new List<Boat>();

                BoatData.ClearTable("LeftHarbour");
                BoatData.ClearTable("RightHarbour");
                BoatData.ClearTable("WaitingBoats");


                Simulate.AddToWaiting();
                BoatData.UpdateVisitors(waitingBoats);
                Simulate.UpdateData(leftHarbour);
                Simulate.UpdateData(rightHarbour);
                Simulate.daysPassed = 0;
                Simulate.boatsRejected = 0;

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
                e.Column.Width = 65;
            }
            else if (e.Column.Header.ToString() == "Vikt")
            {
                e.Column.Width = 45;
            }
            else if (e.Column.Header.ToString() == "Nr")
            {
                e.Column.Width = 50;
            }
            else if (e.Column.Header.ToString() == "Dagar")
            {
                e.Column.Width = 50;
            }
            else if (e.Column.Header.ToString() == "Maxhast")
            {
                e.Column.Width = 60;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Save();
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