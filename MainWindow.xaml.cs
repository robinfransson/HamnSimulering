using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        int saveInterval = 30;
        int timeSinceSave = 0;
        Port leftPort;
        Port rightPort;
        List<Boat> waitingBoats;
        DispatcherTimer automaticTimer;
        DispatcherTimer saveTimer;
        Harbour harbour;









        /// <summary>
        /// Räknar ut medelvikten av alla båtar i listan.
        /// </summary>
        static Func<List<Boat>, float> averageWeight = (port) =>
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
        Func<float, float, float> calculatePercentage = (ammount, total) =>
        {
            float result = (ammount / (total + ammount))*100;
            float percent = 100 - result;
            return (float)Math.Round(percent, 1);

        };

        static Func<List<Boat>, float> averageSpeed = (boats) => boats.Sum(boat => boat.TopSpeedKMH) / boats.Count;


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
            $"Snitthastighet: {averageSpeed(port)}\n" +
            $"Totalvikt: {port.Sum(boat => boat.Weight)}\n" +
            $"Snittvikt: {averageWeight(port)} \n";

        };




        public MainWindow()
        {
            InitializeComponent();


            SetupTimers();

            SetupHarbour();
            LoadSaveFiles();
            SetupGridsAndTables();

            UpdateLabels();


            newBoatsSlider.Value = Simulate.BoatsPerDay;



        }

        private void SetupGridsAndTables()
        {
            double rowHeight = 17.8;
            BoatData.SetupPortDataTable(leftPort.PortName);
            BoatData.SetupPortDataTable(rightPort.PortName);
            BoatData.SetupWaitingBoatsDataTable();

            leftPortGrid.ItemsSource = BoatData.BoatDataViewer(leftPort.PortName);
            leftPortGrid.RowHeight = rowHeight;
            leftPortGrid.IsReadOnly = true;


            rightPortGrid.ItemsSource = BoatData.BoatDataViewer(rightPort.PortName);
            rightPortGrid.RowHeight = rowHeight;
            rightPortGrid.IsReadOnly = true;


            waitingBoatsGrid.ItemsSource = BoatData.BoatDataViewer("WaitingBoats");
            waitingBoatsGrid.IsReadOnly = true;

            BoatData.Update(leftPort);
            BoatData.Update(rightPort);
            BoatData.UpdateVisitors(waitingBoats);
        }

        private void SetupHarbour()
        {

            leftPort = new Port("LeftPort");
            rightPort = new Port("RightPort");

            harbour = Simulate.harbour;

            harbour.AddPort(leftPort);
            harbour.AddPort(rightPort);

        }

        /// <summary>
        /// Uppdaterar de labels som är associerade med väntande båtar.
        /// </summary>
        void UpdateWaitingBoats()
        {

            waitingBoatsLabel.ToolTip = boatStats(waitingBoats);
            waitingBoatsLabel.Content = "Väntande båtar: " + waitingBoats.Count();
            newBoatsLabel.Content = "Nya båtar per dag: " + Simulate.BoatsPerDay;
        }


        /// <summary>
        /// Uppdaterar alla labels.
        /// </summary>
        public void UpdateLabels()
        {
            var percentRejected = calculatePercentage(Simulate.BoatsAccepted, Simulate.BoatsRejected);
            var percentAccepted = calculatePercentage(Simulate.BoatsRejected, Simulate.BoatsAccepted);


            UpdateWaitingBoats();



            numberOfDaysLabel.Content = "Passerade dagar: " + Simulate.DaysPassed;
            rejectedBoatsLabel.Content = "Avvisade båtar: " + Simulate.BoatsRejected + $"  ({percentRejected}%)";
            acceptedBoatsLabel.Content = "Antagna båtar: " + Simulate.BoatsAccepted + $"  ({percentAccepted}%)";

            leftPortBoatsLabel.Content = "Antal båtar vid kajen: " + leftPort.boats.Count();
            leftPortSpotsRemainingLabel.Content = "Lediga platser: " + leftPort.SpotsLeft;

            leftPortBoatsLabel.ToolTip = boatStats(leftPort.boats);
            rightPortBoatsLabel.Content = "Antal båtar vid kajen: " + rightPort.boats.Count();
            rightPortSpotsRemainingLabel.Content = "Lediga platser: " + rightPort.SpotsLeft;
            rightPortBoatsLabel.ToolTip = boatStats(rightPort.boats);
        }


        void LoadSaveFiles()
        {
            try
            {
                leftPort.boats = SaveFileManager.Load("left.txt");
                leftPort.RemovedBoats = SaveFileManager.Load("left_removed.txt");


                rightPort.boats = SaveFileManager.Load("right.txt");
                rightPort.RemovedBoats = SaveFileManager.Load("right_removed.txt");


                Simulate.waitingBoats = SaveFileManager.Load("waiting.txt");
                waitingBoats = Simulate.waitingBoats;

                
                
                if (leftPort.boats.Any())
                {
                    leftPort.UpdateSpots();
                }



                if (rightPort.boats.Any())
                {
                    rightPort.UpdateSpots();
                }


                SaveFileManager.LoadStatistics("stats.txt");

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }


        private void AutoButton_Clicked(object sender, RoutedEventArgs e)
        {
            AutoToggle();
        }

        void SetTimerValue()
        {
            try
            {
                //om användaren skriver ett komma istället för en punkt ska den ersättas med en punkt
                string userSetSpeed = autoSpeedTextBox.Text.Replace(',', '.');
                string[] splitTime = userSetSpeed.Split("."); 

                int seconds = int.Parse(splitTime[0]);

                //om användaren bara skrivit in en siffra ska millisekunderna bli 0
                int milliseconds = splitTime.Length < 2 ? 0 : int.Parse(splitTime[1]);

                bool milliLessThanHundred = milliseconds > 0 && milliseconds < 100;
                if (splitTime.Length > 1 && milliLessThanHundred)
                {
                    //skriver användaren in t.ex 1.1 så ska det bli 100ms, 1.20 ska bli 200ms osv
                    milliseconds *= splitTime[1].Length == 1 ? 100
                                       : splitTime[1].Length == 2 ? 10 : 1;
                }

                //om båda är noll ska sekunderna bli 1
                if(seconds == 0 && milliseconds == 0)
                {
                    seconds = 1;
                    autoSpeedTextBox.Text = $"{seconds}";
                }

                SetAutoTimerSpeed(seconds, milliseconds);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void SimulateTimePassed(object sender=null)
        {
            if(sender is DispatcherTimer)
            {
                //om det är autotimern som kallar på funktionen sätts isAuto till true
                Simulate.OneDay(true);
            }
            else
            {
                Simulate.OneDay(false);
            }
            UpdateLabels();
        }

        private void ManualContinue_Click(object sender, RoutedEventArgs e)
        {
            SimulateTimePassed();
        }


        private void Save()
        {
            SaveFileManager.Save(leftPort.boats, "left.txt");
            SaveFileManager.Save(leftPort.RemovedBoats, "left_removed.txt");
            SaveFileManager.Save(rightPort.boats, "right.txt");
            SaveFileManager.Save(rightPort.RemovedBoats, "right_removed.txt");
            SaveFileManager.Save(waitingBoats, "waiting.txt");


            SaveFileManager.SaveStatistics("stats.txt");

            //if(sender is DispatcherTimer)
            //{
            //    MessageBox.Show("Game saved");
            //}
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
                    newBoatsLabel.Content = "Nya båtar per dag: " + Simulate.BoatsPerDay;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }




        private void AutoSpeed_KeyDOwn(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //om man trycker enter
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SetTimerValue();
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

                leftPort = new Port("LeftPort");
                rightPort = new Port("RightPort");


                harbour.ReplacePort(leftPort);
                harbour.ReplacePort(rightPort);

                Simulate.ClearWaiting();
                Simulate.AddToWaiting(5);
                UpdateWaitingBoats();


                BoatData.ClearTable("LeftPort");
                BoatData.ClearTable("RightPort");
                BoatData.ClearTable("WaitingBoats");


                BoatData.UpdateVisitors(waitingBoats);
                BoatData.Update(leftPort);
                BoatData.Update(rightPort);

                Simulate.DaysPassed = 0;
                Simulate.BoatsRejected = 0;
                Simulate.BoatsAccepted = 0;

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
                e.Column.Width = 55;
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



        private void AutoToggle()
        {

            automatic = !automatic; //toggle
            automaticTimer.IsEnabled = automatic;
            manualContinue.IsEnabled = !automatic;

        }

        private void SetupTimers()
        {
            automaticTimer = new DispatcherTimer();
            automaticTimer.Tick += (sender, eventArgs) =>
            {
                SimulateTimePassed(sender);
            };
            automaticTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);




            saveTimer = new DispatcherTimer();
            saveTimer.Tick += async (sender, eventArgs) =>
            //gör den async så att inte hela programet låser sig när jag vill pausa i 1 sek
            {
                timeSinceSave += 1;
                if (timeSinceSave == saveInterval)
                {
                    Save();
                    timeSinceSaveLabel.Content = "Sparat!";
                    await Task.Delay(1000);
                    timeSinceSave = 0;
                }
                timeSinceSaveLabel.Content = $"Sparar om {(saveInterval-timeSinceSave)} sekunder";
            };
            //varje sekund ska texten uppdateras
            saveTimer.Interval = new TimeSpan(0, 0, 1);
            saveTimer.IsEnabled = true;
        }



        private void SetAutoTimerSpeed(int seconds, int milliseconds)
        {
            automaticTimer.Interval = new TimeSpan(0, 0, 0, seconds, milliseconds);
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