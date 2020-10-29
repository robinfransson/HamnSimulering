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
        Harbour leftHarbour = new Harbour("LeftHarbour");
        Harbour rightHarbour = new Harbour("RightHarbour");
        List<Boat> waitingBoats;
        DispatcherTimer dispatcherTimer;
        int timerSeconds = 5;
        int timerMilliseconds = 0;
        int timesSinceSave = 0;


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
        /// </summary>
        Func<float, float, double> calculatePercentage = (ofTotal, ammount) =>
        {
            float p = (ofTotal / (ammount + ofTotal))*100;
            float percent = 100 - p;
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

            leftHarbourLabel.Content = "Antal båtar i hamnen: " + leftHarbour.Port.Count();
            leftHarbourSpotsLeftLabel.Content = "Lediga platser: " + leftHarbour.SpotsLeft;

            leftHarbourLabel.ToolTip = boatStats(leftHarbour.Port);


            rightHarbourLabel.Content = "Antal båtar i hamnen: " + rightHarbour.Port.Count();
            rightHarbourSpotsLeftLabel.Content = "Lediga platser: " + rightHarbour.SpotsLeft;


            rightHarbourLabel.ToolTip = boatStats(rightHarbour.Port);
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
                    leftHarbour.UpdateSpots();
                }



                if (rightHarbour.Port.Any())
                {
                    rightHarbour.UpdateSpots();
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
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, timerSeconds, timerMilliseconds);
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
                    dispatcherTimer.IsEnabled = automatic; // togglar timern
                }
            }
     

        void SetupAutomatic()
        {
            toggleAuto.ToolTip = "Klicka en gång för att starta, om du ändrar hastigheten kan du\n" +
                "trycka enter när du skrivit klart eller trycka på knappen igen.\n" +
                "Vill du stänga av auto får du trycka 2 gånger om du har\n" +
                "ändrat värdet.";
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (sender, eventArgs) =>
            {
                SimulateTimePassed();
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, timerSeconds, timerMilliseconds);
        }

        private void SimulateTimePassed()
        {
            Simulate.OneDay(leftHarbour, rightHarbour);
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
            SaveFileManager.Save(leftHarbour.Port, "left.txt");
            SaveFileManager.Save(rightHarbour.Port, "right.txt");
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
                System.Windows.Input.Keyboard.ClearFocus();
            }

        }

        private void ClearSave(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete your savefiles?", "WARNING!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                SaveFileManager.DeleteSaves();

                leftHarbour = new Harbour("LeftHarbour");
                rightHarbour = new Harbour("RightHarbour");
                Simulate.SetupAssigner(leftHarbour, true);
                Simulate.SetupAssigner(rightHarbour, true);
                Simulate.ClearWaiting();
                UpdateWaitingBoats();
                BoatData.ClearTable("LeftHarbour");
                BoatData.ClearTable("RightHarbour");
                BoatData.ClearTable("WaitingBoats");


                Simulate.AddToWaiting();
                BoatData.UpdateVisitors(waitingBoats);
                Simulate.UpdateData(leftHarbour);
                Simulate.UpdateData(rightHarbour);
                Simulate.daysPassed = 0;
                Simulate.boatsRejected = 0;
                Simulate.boatsAccepted = 0;

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