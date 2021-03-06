﻿//-----------------------------------------------------------------------
// <copyright file="ProjectEditorPage.xaml.cs">
//     Copyright (c) Igor Golopolosov. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WarehouseOfMusic.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using Coding4Fun.Toolkit.Controls;
    using Enums;
    using EventArgs;
    using Managers;
    using Models;
    using Resources;
    using ViewModels;
    
    /// <summary>
    /// Page of editing projects
    /// </summary>
    public partial class ProjectEditorPage : PhoneApplicationPage
    {
        /// <summary>
        /// ViewModel for this page
        /// </summary>
        private ProjectEditorContext _viewModel;

        /// <summary>
        /// Manager of track 
        /// </summary>
        private PlayerManager _playerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectEditorPage" /> class.
        /// </summary>
        public ProjectEditorPage()
        {
            this.InitializeComponent();
            this.BuildLocalizedAppBar();
        }

        #region Navigation control
        /// <summary>
        /// Called when the page is activated
        /// </summary>
        /// <param name="e">Navigation event</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.InitialiazeDataContext();
        }

        /// <summary>
        /// Loads new state of ViewModel
        /// </summary>
        private void InitialiazeDataContext()
        {
            this._viewModel = new ProjectEditorContext(App.DbConnectionString);
            this._viewModel.LoadData((int)IsoSettingsManager.LoadRecord("CurrentProjectId"));
            this.DataContext = this._viewModel;
            _playerManager = new PlayerManager(_viewModel.CurrentProject.Tempo);
            _playerManager.StateChangedEvent += _playerManager_StateChangeEvent;
        }

        /// <summary>
        /// Called when the page is deactivated
        /// </summary>
        /// <param name="e">Navigation event</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _viewModel.SaveChangesToDb();
        }
        #endregion
        
        #region Rename and delete track dialogs

        /// <summary>
        /// Show dialog to create or rename project
        /// </summary>
        /// <param name="trackId">-1 = for create project dialog, n - rename project dialog </param>
        private void ShowRenameTrackDialog()
        {
            var trackName = _viewModel.OnRenameTrack.Name;
            var renameTrackDialog = new InputPromptOveride()
            {
                IsSubmitOnEnterKey = false,
                Title = AppResources.RenameTrack,
                Value = trackName
            };
            renameTrackDialog.LostFocus += RenameTrackDialog_OnLostFocus;
            renameTrackDialog.KeyUp += RenameTrackDialog_OnKeyUp;
            renameTrackDialog.Completed += RenameTrackDialog_OnCompleted;
            renameTrackDialog.Show();
        }

        /// <summary>
        /// Show or hide 'empty name' error message
        /// </summary>
        void RenameTrackDialog_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var input = sender as InputPrompt;
            if (input == null) return;
            input.Message = input.Value == string.Empty ? AppResources.ErrorEmptyName : string.Empty;
        }

        /// <summary>
        /// Detect the end of input text
        /// </summary>
        private void RenameTrackDialog_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;
            var input = sender as InputPrompt;
            if (input == null) return;
            input.Focus();
        }

        private void RenameTrackDialog_OnCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            var input = sender as InputPrompt;
            if (input == null) return;
            if (e.PopUpResult == PopUpResult.Ok) this._viewModel.RenameTrackTo(input.Value);
            this._viewModel.OnRenameTrack = null;
        }

        /// <summary>
        /// Show dialog to delete track
        /// </summary>
        private void ShowDeleteTrackDialog()
        {
            var deleteTrackDialog = new MessagePrompt()
            {
                Message = AppResources.MessageDeleteTrack + " " + _viewModel.OnDeleteTrack.Name + "?"
            };
            deleteTrackDialog.Completed += DeleteTrackDialog_OnCompleted;
            deleteTrackDialog.Show();
        }

        private void DeleteTrackDialog_OnCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult == PopUpResult.Ok) this._viewModel.DeleteTrack();
            _viewModel.OnDeleteTrack = null;
        }
        #endregion

        #region For application bar
        /// <summary>
        /// Build Localized application bar
        /// </summary>
        private void BuildLocalizedAppBar()
        {
            this.ApplicationBar = new ApplicationBar();

            //// Add menu item linked with help page
            var helpMenuItem = new ApplicationBarMenuItem(AppResources.AppBarHelp);
            helpMenuItem.Click += (sender, args) => NavigationService.Navigate(new Uri("/Views/HelpPage.xaml", UriKind.Relative));
            this.ApplicationBar.MenuItems.Add(helpMenuItem);

            //// Add play button for player
            var addButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.png", UriKind.Relative))
            {
                Text = AppResources.AppBarAddTrack,
            };
            addButton.Click += (sender, args) => this._viewModel.AddTrack();
            this.ApplicationBar.Buttons.Add(addButton);

            //// Add play button for player
            var playButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.control.play.png", UriKind.Relative))
            {
                Text = AppResources.AppBarPlay,
            };
            playButton.Click += (sender, args) =>
            {
                switch (_playerManager.State)
                {
                    case PlayerState.Stopped: _playerManager.Play(_viewModel.CurrentProject);
                        break;
                    case PlayerState.Playing: _playerManager.Pause();
                        break;
                    case PlayerState.Paused: _playerManager.Resume();
                        break;
                }
            };
            this.ApplicationBar.Buttons.Add(playButton);

            //// Add stop button for player
            var stopButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.control.stop.png", UriKind.Relative))
            {
                Text = AppResources.AppBarStop,
            };
            stopButton.Click += (sender, args) => _playerManager.Stop();
            this.ApplicationBar.Buttons.Add(stopButton);
        }

        /// <summary>
        /// Dispatcher used to avoid threading exeptions
        /// </summary>
        private void _playerManager_StateChangeEvent(object sender, PlayerEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                var button = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                if (button == null) return;
                switch (e.State)
                {
                    case PlayerState.Stopped:
                        {
                            button.IconUri = new Uri("/Assets/AppBar/appbar.control.play.png", UriKind.Relative);
                            button.Text = AppResources.AppBarPlay;
                        }
                        break;
                    case PlayerState.Playing:
                        {
                            button.IconUri = new Uri("/Assets/AppBar/appbar.control.pause.png", UriKind.Relative);
                            button.Text = AppResources.AppBarPause;
                        }
                        break;
                    case PlayerState.Paused:
                        {
                            button.IconUri = new Uri("/Assets/AppBar/appbar.control.resume.png", UriKind.Relative);
                            button.Text = AppResources.AppBarResume;
                        }
                        break;
                }
            });
        }

        #endregion

        #region Track Mode
        private void SoloCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;
            if (checkBox == null) return;
            var track = checkBox.DataContext as ToDoTrack;
            if (track != null) track.Mute = false;
        }

        private void MuteCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;
            if (checkBox == null) return;
            var track = checkBox.DataContext as ToDoTrack;
            if (track != null) track.Solo = false;
        }
        #endregion

        #region Manipulations with track

        /// <summary>
        /// Chose project for editing
        /// </summary>
        private void TrackListBoxItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var grid = sender as TextBlock;
            if (grid == null) return;
            var chosenTrack = grid.DataContext as ToDoTrack;
            if (chosenTrack == null) return;
            IsoSettingsManager.SaveRecord("CurrentTrackId", chosenTrack.Id);
            NavigationService.Navigate(new Uri("/Views/TrackEditorPage.xaml", UriKind.Relative));
        }

        private void RenameTrack_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var contextMenuItem = sender as MenuItem;
            if (contextMenuItem == null) return;
            _viewModel.OnRenameTrack = contextMenuItem.DataContext as ToDoTrack;
            ShowRenameTrackDialog();
        }

        private void DeleteTrack_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var contextMenuItem = sender as MenuItem;
            if (contextMenuItem == null) return;
            this._viewModel.OnDeleteTrack = contextMenuItem.DataContext as ToDoTrack;
            ShowDeleteTrackDialog();
        }
        #endregion

        #region Tempo change
        private void TempoGrid_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var tempoChangeDialog = new InputPromptOveride()
            {
                IsSubmitOnEnterKey = false,
                Title = AppResources.TitleChangeTempo,
                Message = AppResources.MessageChangeTempo,
                Value = string.Empty + _viewModel.CurrentProject.Tempo
            };
            tempoChangeDialog.LostFocus += TempoChangeDialog_OnLostFocus;
            tempoChangeDialog.KeyUp += TempoChangeDialog_OnKeyUp;
            tempoChangeDialog.Completed += TempoChangeDialog_OnCompleted;
            tempoChangeDialog.Show();
        }

        /// <summary>
        /// Show or hide 'empty name' error message
        /// </summary>
        void TempoChangeDialog_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var input = sender as InputPrompt;
            if (input == null) return;
            try
            {
                var newValue = Convert.ToInt32(input.Value);
                if (30 <= newValue && newValue <= 240) input.Message = AppResources.MessageChangeTempo;
                else input.Message = AppResources.MessageIncorrectValue + " " + AppResources.MessageChangeTempo;
            }
            catch (Exception)
            {
                input.Message = AppResources.MessageIncorrectValue + " " + AppResources.MessageChangeTempo;
            }
        }

        /// <summary>
        /// Detect the end of input text
        /// </summary>
        private void TempoChangeDialog_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;
            var input = sender as InputPrompt;
            if (input == null) return;
            input.Focus();
        }

        private void TempoChangeDialog_OnCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            var input = sender as InputPrompt;
            if (input == null) return;
            if (e.PopUpResult == PopUpResult.Ok)
            {
                try
                {
                    var newValue = Convert.ToInt32(input.Value);
                    if (30 <= newValue && newValue <= 240)
                    {
                        _viewModel.CurrentProject.Tempo = newValue;
                        _viewModel.SaveChangesToDb();
                        _playerManager = new PlayerManager(newValue);
                    }
                }
                catch (Exception)
                {
                    input.Message = AppResources.MessageIncorrectValue + " " + AppResources.MessageChangeTempo;
                }
            }
        }
        #endregion

        #region Choose instrument

        /// <summary>
        /// Handle multiply calls of selection changed
        /// </summary>
        private bool _handled;

        private void InstrumentList_OnLoaded(object sender, RoutedEventArgs e)
        {
            var list = sender as ListPicker;
            if (list == null) return;
            var track = list.DataContext as ToDoTrack;
            if (track == null) return;
            list.SelectedItem = track.Instrument;
            _handled = false;
        }

        private void InstrumentList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_handled) return;
            if (e.RemovedItems == null || e.RemovedItems.Count <= 0) return;
            var list = sender as ListPicker;
            if (list == null) return;
            var track = list.DataContext as ToDoTrack;
            if (track == null) return;
            var instrument = list.SelectedItem as ToDoInstrument;
            if (instrument == null) return;
            _viewModel.SelectInstrument(track, instrument);
            _handled = true;
        }
        #endregion
    }
}