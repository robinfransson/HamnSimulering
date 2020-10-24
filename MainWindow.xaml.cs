using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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



        bool automatic = false;
        Harbour leftHarbour = new Harbour();
        Harbour rightHarbour = new Harbour();
        WaitingBoats boatsAtSea = new WaitingBoats();
        int daysPassed;
        int boatsRejected;
        DispatcherTimer dispatcherTimer;
        int numberOfNewBoats = 5;
        int timerSeconds = 5;




        public MainWindow()
        {
            InitializeComponent();


            SetupAutomatic();
            BoatData.SetupDataTables();

            leftHarbourGrid.ItemsSource = BoatData.BoatDataViewer("LeftHarbour");
            rightHarbourGrid.ItemsSource = BoatData.BoatDataViewer("RightHarbour");
            waitingBoatsGrid.ItemsSource = BoatData.Info("WaitingBoats").DefaultView;


            leftHarbourGrid.IsReadOnly = true;
            rightHarbourGrid.IsReadOnly = true;
            waitingBoatsGrid.IsReadOnly = true;


            LoadSaveFiles();
            UpdateDataTables();
            ListEmptySpots();
            UpdateLabels();
            leftHarbour.UpdateLargestSpot();
            rightHarbour.UpdateLargestSpot();
        }


        void UpdateLabels()
        {
            waitingBoatsLabel.Content = "Väntande båtar: " + boatsAtSea.Waiting.Count();
            numberOfDaysLabel.Content = "Passerade dagar: " + daysPassed;
            rejectedBoatsLabel.Content = "Avvisade båtar: " + boatsRejected;

            leftHarbourLabel.Content = "Antal båtar i hamnen: " + leftHarbour.Port.Count();
            rightHarbourLabel.Content = "Antal båtar i hamnen: " + rightHarbour.Port.Count();

            leftHarbourSpotsLeftLabel.Content = "Lediga platser: " + leftHarbour.SpotsLeft;
            rightHarbourSpotsLeftLabel.Content = "Lediga platser: " + rightHarbour.SpotsLeft;
        }
        void LoadSaveFiles()
        {
            try
            {
                leftHarbour.Port = SaveFileManager.Load("left.txt");
                rightHarbour.Port = SaveFileManager.Load("right.txt");
                boatsAtSea.Waiting = SaveFileManager.Load("waiting.txt");

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
                if (!boatsAtSea.Waiting.Any())
                {
                    boatsAtSea.AddBoats(numberOfNewBoats);
                }

                BoatData.UpdateVisitors(boatsAtSea.Waiting);

                SaveFileManager.LoadStatistics("stats.txt", out daysPassed, out boatsRejected);
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
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5); // var 5:e sekund ska lambda expression utföras
        }



        private void SimulateTimePassed()
        {
            TryToDock();
            boatsAtSea.AddBoats(numberOfNewBoats);

            BoatData.UpdateVisitors(boatsAtSea.Waiting);

            AddOneDay();
            UpdateDataTables();
            UpdateLabels();
            ListEmptySpots();


            leftHarbour.UpdateLargestSpot();
            rightHarbour.UpdateLargestSpot();

        }

        private void ListEmptySpots()
        {
            BoatData.ListFreeSpots(leftHarbour.SpotsInUse, BoatData.Info("LeftHarbour"));
            BoatData.ListFreeSpots(rightHarbour.SpotsInUse, BoatData.Info("RightHarbour"));
        }

        private void UpdateDataTables(bool clear=false)
        {
            if (clear)
            {

            }
            else
            {

                BoatData.UpdateHarbour(BoatData.Info("LeftHarbour"), leftHarbour.Port);
                BoatData.UpdateHarbour(BoatData.Info("RightHarbour"), rightHarbour.Port);
                BoatData.UpdateVisitors(boatsAtSea.Waiting);
            }
        }

        private void AddOneDay()
        {
            List<Boat> toRemove;


            //först vänstra hamnen
            toRemove = leftHarbour.Port.Where(boat => boat.DaysSpentAtHarbour == boat.MaxDaysAtHarbour).ToList();
            foreach (Boat boat in toRemove)
            {
                leftHarbour.Remove(boat);
                BoatData.RemoveBoat(BoatData.Info("LeftHarbour"), boat);
            }


            //sedan vänstra
            toRemove = rightHarbour.Port.Where(boat => boat.DaysSpentAtHarbour == boat.MaxDaysAtHarbour).ToList();
            foreach (Boat boat in toRemove)
            {
                rightHarbour.Remove(boat);
                BoatData.RemoveBoat(BoatData.Info("RightHarbour"), boat);
            }


            if (leftHarbour.Port.Any())
            {
                foreach (Boat boat in leftHarbour.Port)
                {
                    boat.DaysSpentAtHarbour++;
                }
            }
            if (rightHarbour.Port.Any())
            {
                foreach (Boat boat in rightHarbour.Port)
                {
                    boat.DaysSpentAtHarbour++;
                }
            }

            daysPassed++;

        }


        private void TryToDock()
        {
            foreach (Boat boat in boatsAtSea.Waiting)
            {
                if (leftHarbour.HasFreeSpots(boat, out string assignedSpot))
                {
                    boat.AssignSpot(assignedSpot);
                    leftHarbour.UpdateSpots(boat);
                    leftHarbour.Port.Add(boat);
                }
                else if (rightHarbour.HasFreeSpots(boat, out assignedSpot))
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
            if (boatsAtSea.Waiting.Any())
            {
                boatsAtSea.Waiting.Clear();
            }
        }
        private void ManualContinue_Click(object sender, RoutedEventArgs e)
        {
            SimulateTimePassed();
        }


        private void Save(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveFileManager.Save(leftHarbour.Port, "left.txt");
            SaveFileManager.Save(rightHarbour.Port, "right.txt");
            SaveFileManager.Save(boatsAtSea.Waiting, "waiting.txt");
            SaveFileManager.SaveStatistics("stats.txt", daysPassed, boatsRejected);
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
                    numberOfNewBoats = (int)e.NewValue;
                    newBoatsLabel.Content = "Nya båtar per dag: " + numberOfNewBoats;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void autoSpeedTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if(e.Key == System.Windows.Input.Key.Enter)
            {
                try
                {

                    timerSeconds = Int32.Parse(autoSpeedTextBox.Text);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void clearSave_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete your savefile?", "WARNING!", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                SaveFileManager.DeleteSaves();
                LoadSaveFiles();
                UpdateDataTables();
                ListEmptySpots();
            }
        }
    }
}
