using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HamnSimulering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static bool automatic = false;
        Port leftPort;
        Port rightPort;
        List<Boat> waitingBoats;
        DispatcherTimer automaticTimer;
        int timerSeconds = 5;
        int timerMilliseconds = 0;
        int timesSinceSave = 0;
        Harbour harbour;

        /// <summary>
        /// Räknar ut medelvikten av alla båtar i listan.
        /// </summary>
        static Func<List<Boat>, long> averageWeight = (port) =>
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


        /// <summary>
        /// Får ta in antalen som float för att kunna dividera dom direkt.
        /// (formel (x/(x+y)*100)
        /// </summary>
        Func<float, float, double> calculatePercentage = (ofTotal, ammount) =>
        {
            float p = (ofTotal / (ammount + ofTotal))*100;
            float percent = 100 - p;

            //returnerar en double för att Math.Round gör det
            return Math.Round(percent, 1);

        };

        /// <summary>
        /// Skapar en sträng med statistik om den aktuella listan med båtar.
        /// </summary>
        Func<List<Boat>, string> boatStats = (port) =>
        {
            return


            $"Roddbåtar: {port.Count(boat => boat is Rowboat)} \n" +
            $"Motorbåtar: {port.Count(boat => boat is Motorboat)} \n" +
            $"Segelbåtar: {port.Count(boat => boat is Sailboat)} \n" +
            $"Katamaraner: {port.Count(boat => boat is Catamaran)} \n" +
            $"Lastfartyg: {port.Count(boat => boat is Cargoship)} \n\n" +
            $"Totalvikt: {port.Sum(boat => boat.Weight)}\n" +
            $"Snittvikt: {averageWeight(port)} \n";

        };




        public MainWindow()
        {
            InitializeComponent();


            leftPort = new Port("LeftPort");
            rightPort = new Port("RightPort");
            SetupAutomatic();

            BoatData.Port_SetupDataTable(leftPort);
            BoatData.Port_SetupDataTable(rightPort);
            BoatData.Waiting_SetupDataTable();

            leftPortGrid.ItemsSource = BoatData.BoatDataViewer(leftPort.Name);
            leftPortGrid.RowHeight = 17;
            leftPortGrid.IsReadOnly = true;


            rightPortGrid.ItemsSource = BoatData.BoatDataViewer(rightPort.Name);
            rightPortGrid.RowHeight = 17;
            rightPortGrid.IsReadOnly = true;


            waitingBoatsGrid.ItemsSource = BoatData.BoatDataViewer("WaitingBoats");
            waitingBoatsGrid.IsReadOnly = true;




            LoadSaveFiles();

            BoatData.Update(leftPort);
            BoatData.Update(rightPort);
            harbour = Simulate.harbour;

            harbour.AddPort(leftPort);
            harbour.AddPort(rightPort);

            BoatData.UpdateVisitors(waitingBoats);
            UpdateLabels();



        }

        /// <summary>
        /// Uppdaterar de labels som är associerade med väntande båtar.
        /// </summary>
        void UpdateWaitingBoats()
        {

            waitingBoatsLabel.ToolTip = boatStats(waitingBoats);
            waitingBoatsLabel.Content = "Väntande båtar: " + waitingBoats.Count();
            newBoatsLabel.Content = "Nya båtar per dag: " + Simulate.boatsPerDay;
        }


        /// <summary>
        /// Uppdaterar alla labels.
        /// </summary>
        public void UpdateLabels()
        {
            var percentRejected = calculatePercentage(Simulate.boatsAccepted, Simulate.boatsRejected);
            var percentAccepted = calculatePercentage(Simulate.boatsRejected, Simulate.boatsAccepted);


            UpdateWaitingBoats();



            numberOfDaysLabel.Content = "Passerade dagar: " + Simulate.daysPassed;
            rejectedBoatsLabel.Content = "Avvisade båtar: " + Simulate.boatsRejected + $"  ({percentRejected}%)";
            acceptedBoatsLabel.Content = "Antagna båtar: " + Simulate.boatsAccepted + $"  ({percentAccepted}%)";

            leftPortBoatsLabel.Content = "Antal båtar i hamnen: " + leftPort.Boats.Count();
            leftPortSpotsRemainingLabel.Content = "Lediga platser: " + leftPort.SpotsLeft;

            leftPortBoatsLabel.ToolTip = boatStats(leftPort.Boats);


            rightPortBoatsLabel.Content = "Antal båtar i hamnen: " + rightPort.Boats.Count();
            rightPortSpotsRemainingLabel.Content = "Lediga platser: " + rightPort.SpotsLeft;


            rightPortBoatsLabel.ToolTip = boatStats(rightPort.Boats);
        }


        void LoadSaveFiles()
        {
            try
            {
                leftPort.Boats = SaveFileManager.Load("left.txt");
                rightPort.Boats = SaveFileManager.Load("right.txt");
                Simulate.waitingBoats = SaveFileManager.Load("waiting.txt");
                waitingBoats = Simulate.waitingBoats;

                
                
                if (leftPort.Boats.Any())
                {
                    leftPort.UpdateSpots();
                }



                if (rightPort.Boats.Any())
                {
                    rightPort.UpdateSpots();
                }


                BoatData.UpdateVisitors(waitingBoats);

                SaveFileManager.LoadStatistics("stats.txt", out Simulate.daysPassed, out Simulate.boatsRejected, out Simulate.boatsAccepted, out Simulate.boatsPerDay);

                newBoatsSlider.Value = Simulate.boatsPerDay;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }


        private void AutoButton_Clicked(object sender, RoutedEventArgs e)
        {
            AutoToggleOrChangeSpeed();
        }

        void SetTimerValue()
        {
            try
            {
                string[] splitTime = autoSpeedTextBox.Text.Split(".");
                timerSeconds = int.Parse(splitTime[0]);
                timerMilliseconds = splitTime.Length < 2 ? 0 : int.Parse(splitTime[1]);
                if(splitTime.Length > 1 && timerMilliseconds > 0 && timerMilliseconds < 100 && timerSeconds == 0)
                {
                    timerMilliseconds *= splitTime[1].Length == 1 ? 100 : splitTime[1].Length == 2 ? 10 : 1;
                }
                timerSeconds = timerSeconds == 0 && timerMilliseconds == 0 ? 1 : timerSeconds;
                automaticTimer.Interval = new TimeSpan(0, 0, 0, timerSeconds, timerMilliseconds);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void AutoToggleOrChangeSpeed(object sender=null)
        {
                if (sender is TextBox)
                {
                    SetTimerValue();
                }
                else
                {

                    automatic = !automatic; //toggle
                    manualContinue.IsEnabled = !automatic; // togglar manuella knappen
                    automaticTimer.IsEnabled = automatic; // togglar timern
                }
            }
     

        void SetupAutomatic()
        {
            toggleAuto.ToolTip = "För att ändra värdet: skriv nedan och avsluta med enter.";
            automaticTimer = new DispatcherTimer();
            automaticTimer.Tick += (sender, eventArgs) =>
            {
                SimulateTimePassed();
            };
            automaticTimer.Interval = new TimeSpan(0, 0, 0, timerSeconds, timerMilliseconds);
        }

        private void SimulateTimePassed()
        {
            Simulate.OneDay();
            UpdateLabels();
            timesSinceSave++;
            if(timesSinceSave > 30)
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
            SaveFileManager.Save(leftPort.Boats, "left.txt");
            SaveFileManager.Save(rightPort.Boats, "right.txt");
            SaveFileManager.Save(waitingBoats, "waiting.txt");
            SaveFileManager.SaveStatistics("stats.txt", Simulate.daysPassed, Simulate.boatsRejected, Simulate.boatsAccepted, Simulate.boatsPerDay);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }




        private void BoatSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(newBoatsLabel is null))
            {
                try
                {
                    int numberOfBoats = (int)e.NewValue;
                    Simulate.AddToWaiting(numberOfBoats);
                    UpdateWaitingBoats();
                    newBoatsLabel.Content = "Nya båtar per dag: " + Simulate.boatsPerDay;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }




        private void AutoSpeed_KeyDOwn(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AutoToggleOrChangeSpeed(sender);
                //rensar fokusen från textboxen
                System.Windows.Input.Keyboard.ClearFocus();
            }

        }

        private void ClearSave(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete your savefiles?", "WARNING!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                SaveFileManager.DeleteSaves();


                harbour.RemovePort(leftPort);
                harbour.RemovePort(rightPort);

                leftPort = new Port("LeftPort");
                rightPort = new Port("RightPort");


                harbour.AddPort(leftPort);
                harbour.AddPort(rightPort);

                Simulate.ClearWaiting();
                UpdateWaitingBoats();


                BoatData.ClearTable("LeftPort");
                BoatData.ClearTable("RightPort");
                BoatData.ClearTable("WaitingBoats");


                Simulate.AddToWaiting();
                BoatData.UpdateVisitors(waitingBoats);
                BoatData.Update(leftPort);
                BoatData.Update(rightPort);
                Simulate.daysPassed = 0;
                Simulate.boatsRejected = 0;
                Simulate.boatsAccepted = 0;

            }
        }

        private void HarbourGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

            //hårdkodade kolumnstorlekar
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