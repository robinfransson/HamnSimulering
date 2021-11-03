using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        readonly int saveInterval = 30;
        int timeSinceSave = 0;
        bool superMergeBoats = false;

        Harbour harbour;
        Port leftPort;
        Port rightPort;
        List<Boat> waitingBoats;
        DispatcherTimer automaticTimer;
        DispatcherTimer saveTimer;

        //public object labelContent
        //{
        //    get { return acceptedBoatsLabel.Content; }
        //    set { acceptedBoatsLabel.Content = value; }
        //}




        public MainWindow()
        {
            InitializeComponent();


            SetupTimers();

            SetupHarbour();
            LoadSaveFiles();
            SetupGridsAndTables();

            UpdateLabels();


            newBoatsSlider.Value = Simulate.BoatsPerDay;

            if (!waitingBoats.Any())
            {
                Simulate.AddBoats();
            }


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

            waitingBoatsLabel.ToolTip = BoatData.PortStats(waitingBoats);
            waitingBoatsLabel.Content = "Väntande båtar: " + waitingBoats.Count();
            newBoatsLabel.Content = "Nya båtar per dag: " + Simulate.BoatsPerDay;
        }


        private void UpdateStatsLabels()
        {
            var percentRejected = BoatData.CalculatePercentage(Simulate.BoatsAccepted, Simulate.BoatsRejected);
            var percentAccepted = BoatData.CalculatePercentage(Simulate.BoatsRejected, Simulate.BoatsAccepted);


            numberOfDaysLabel.Content = "Passerade dagar: " + Simulate.DaysPassed;
            rejectedBoatsLabel.Content = "Avvisade båtar: " + Simulate.BoatsRejected + $"  ({percentRejected}%)";
            acceptedBoatsLabel.Content = "Antagna båtar: " + Simulate.BoatsAccepted + $"  ({percentAccepted}%)";
        }

        private void UpdateLeftPortLabels()
        {

            leftPortBoatsLabel.Content = "Antal båtar vid kajen: " + leftPort.Boats.Count();
            leftPortSpotsRemainingLabel.Content = "Lediga platser: " + leftPort.SpotsLeft;

            leftPortBoatsLabel.ToolTip = BoatData.PortStats(leftPort.Boats);
            leftPortSpotsRemainingLabel.ToolTip = BoatData.FreeSpotsInPort(leftPort);
        }
        private void UpdateRightPortLabels()
        {

            rightPortBoatsLabel.Content = "Antal båtar vid kajen: " + rightPort.Boats.Count();
            rightPortSpotsRemainingLabel.Content = "Lediga platser: " + rightPort.SpotsLeft;

            rightPortBoatsLabel.ToolTip = BoatData.PortStats(rightPort.Boats);
            rightPortSpotsRemainingLabel.ToolTip = BoatData.FreeSpotsInPort(rightPort);
        }


        /// <summary>
        /// Uppdaterar alla labels.
        /// </summary>
        private void UpdateLabels()
        {


            UpdateWaitingBoats();
            UpdateStatsLabels();
            UpdateLeftPortLabels();
            UpdateRightPortLabels();

        }


        void LoadSaveFiles()
        {
            Func<List<Boat>, string, List<Boat>> loadBoats = (boats, port) => boats.Where(boat => boat.IsInPort == port).ToList();
            try
            {
                List<Boat> boatsFromFile = SaveFileManager.Load("boats.txt");
                leftPort.Boats = loadBoats(boatsFromFile, leftPort.PortName);
                leftPort.RemovedBoats = loadBoats(boatsFromFile, leftPort.PortName + "-removed");


                rightPort.Boats = loadBoats(boatsFromFile, rightPort.PortName);
                rightPort.RemovedBoats = loadBoats(boatsFromFile, rightPort.PortName + "-removed");

                Simulate.waitingBoats = boatsFromFile.Where(boat => boat.IsInPort == "?").ToList();

                //Simulate.waitingBoats = SaveFileManager.Load("waiting.txt");
                //leftPort.Boats = SaveFileManager.Load("left.txt");
                //leftPort.RemovedBoats = SaveFileManager.Load("left_removed.txt");


                //rightPort.Boats = SaveFileManager.Load("right.txt");
                //rightPort.RemovedBoats = SaveFileManager.Load("right_removed.txt");



                waitingBoats = Simulate.waitingBoats;



                if (leftPort.Boats.Any())
                {
                    leftPort.UpdateSpots();
                }



                if (rightPort.Boats.Any())
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
                int seconds, milliseconds;

                if (splitTime.Length > 2)
                {

                    throw new Exception(message: "För många punkter inmatade");
                }
                else if(splitTime.Length == 1)
                {
                    seconds = int.Parse(splitTime[0]);
                    milliseconds = 0;
                }
                else
                {

                    seconds = int.Parse(splitTime[0]);
                    milliseconds = int.Parse(splitTime[1]);
                        //skriver användaren in t.ex 1.1 så ska det bli 100ms, 1.20 ska bli 200ms osv
                    switch (splitTime[1].Length)
                    {
                        case 1:
                            milliseconds *= 100;
                            break;
                        case 2:
                            milliseconds *= 10;
                            break;
                        default:
                            break;
                    }
                }

                //om båda är noll ska sekunderna bli 1
                if (seconds == 0 && milliseconds == 0)
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
                Simulate.OneDay(true, superMergeBoats);
            }
            else
            {
                Simulate.OneDay(false, superMergeBoats);
            }
            UpdateLabels();
        }

        private void ManualContinue_Click(object sender, RoutedEventArgs e)
        {
            SimulateTimePassed();
        }


        private void Save()
        {
            List<Boat> boatsToSave = new List<Boat>();
            boatsToSave.AddRange(leftPort.Boats);
            boatsToSave.AddRange(leftPort.RemovedBoats);
            boatsToSave.AddRange(rightPort.Boats);
            boatsToSave.AddRange(rightPort.RemovedBoats);
            boatsToSave.AddRange(waitingBoats);
            SaveFileManager.Save(boatsToSave, "boats.txt");


            SaveFileManager.SaveStatistics("stats.txt");

            //SaveFileManager.Save(leftPort.Boats, "left.txt");
            //SaveFileManager.Save(leftPort.RemovedBoats, "left_removed.txt");
            //SaveFileManager.Save(rightPort.Boats, "right.txt");
            //SaveFileManager.Save(rightPort.RemovedBoats, "right_removed.txt");
            //SaveFileManager.Save(waitingBoats, "waiting.txt");



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
            if(newBoatsLabel != null)
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




        private void AutoSpeed_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //om man trycker enter
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SetTimerValue();
                //rensar fokusen från textboxen
                System.Windows.Input.Keyboard.ClearFocus();
            }

        }

        private void ClearSave_Clicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Är du säker?", "WARNING!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                SaveFileManager.DeleteSaves();

                leftPort = new Port("LeftPort");
                rightPort = new Port("RightPort");


                harbour.ReplacePort(leftPort);
                harbour.ReplacePort(rightPort);

                waitingBoats.Clear();
                Simulate.AddToWaiting(5);
                UpdateWaitingBoats();


                BoatData.ClearTable(leftPort.PortName);
                BoatData.ClearTable(rightPort.PortName);
                BoatData.ClearTable("WaitingBoats");


                BoatData.UpdateVisitors(waitingBoats);
                BoatData.Update(leftPort);
                BoatData.Update(rightPort);

                Simulate.DaysPassed = 0;
                Simulate.BoatsRejected = 0;
                Simulate.BoatsAccepted = 0;

                UpdateLabels();
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
            automaticTimer.Tick += (sender, eventArgs) => SimulateTimePassed(sender);
            automaticTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);




            saveTimer = new DispatcherTimer();
            saveTimer.Tick += (sender, eventArgs) => SaveTimerTick();
            //varje sekund ska texten uppdateras
            saveTimer.Interval = new TimeSpan(0, 0, 1);
            saveTimer.IsEnabled = true;
        }


        private void SaveTimerTick()
        {
            timeSinceSave += 1;
            if (timeSinceSave == saveInterval)
            {
                Save();
                timeSinceSaveLabel.Content = "Sparat!";
                timeSinceSave = 0;
            }
            timeSinceSaveLabel.Content = $"Sparar om {(saveInterval - timeSinceSave)} sekunder";
        }


        private void SetAutoTimerSpeed(int seconds, int milliseconds)
        {
            automaticTimer.Interval = new TimeSpan(0, 0, 0, seconds, milliseconds);
        }

        private void SuperMerge_StateChanged(object sender, RoutedEventArgs e)
        {
            superMergeBoats = (bool)this.superMerge.IsChecked;
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