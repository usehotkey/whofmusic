﻿//-----------------------------------------------------------------------
// <copyright file="MainPageContext.cs">
//     Copyright (c) Igor Golopolosov. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WarehouseOfMusic.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Models;
    using Resources;

    /// <summary>
    /// Class to realize access to database and represent information to application pages.
    /// </summary>
    public class MainPageContext : Context
    {
        /// <summary>
        /// Project that must be deleted
        /// </summary>
        private ToDoProject _onDeleteProject;

        /// <summary>
        /// Project that must be renamed
        /// </summary>
        private ToDoProject _onRenameProject;

        /// <summary>
        /// A list of all projects
        /// </summary>
        private ObservableCollection<ToDoProject> _projectsList;

        /// <summary>
        /// Gets or sets project on rename
        /// </summary>
        public ToDoProject OnRenameProject
        {
            get
            {
                return this._onRenameProject;
            }

            set
            {
                this._onRenameProject = value;
                this.NotifyPropertyChanged("OnRenameProject");
            }
        }

        /// <summary>
        /// Gets or sets project on delete
        /// </summary>
        public ToDoProject OnDeleteProject
        {
            get
            {
                return this._onDeleteProject;
            }

            set
            {
                this._onDeleteProject = value;
                this.NotifyPropertyChanged("OnDeleteProject");
            }
        }

        /// <summary>
        /// Gets or sets a list of all projects
        /// </summary>
        public ObservableCollection<ToDoProject> ProjectsList
        {
            get
            {
                return this._projectsList;
            }

            set
            {
                this._projectsList = value;
                this.NotifyPropertyChanged("ProjectsList");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPageContext" /> class.
        /// Class constructor, create the data context object.
        /// </summary>
        /// <param name="connectionString">Path to connect to database</param>
        public MainPageContext(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Query database and load the list of projects
        /// </summary>
        public override void LoadData(int id)
        {
            //// Load a list of all projects. 
            this._projectsList = this.DataBaseContext.Projects.Any()
                ? new ObservableCollection<ToDoProject>(this.DataBaseContext.Projects)
                : new ObservableCollection<ToDoProject>();
        }

        /// <summary>
        /// Add a project with specific name to the database and collections.
        /// </summary>
        /// <param name="projectName">Name of project</param>
        /// <returns>New project</returns>
        public ToDoProject CreateProject(string projectName)
        {
            var newProject = new ToDoProject()
            {
                Name = projectName,
                CreationTime = DateTime.Now,
                Tempo = 120
            };
            this.DataBaseContext.Projects.InsertOnSubmit(newProject);
            this.DataBaseContext.SubmitChanges();
            this._projectsList.Add(newProject);

            var trackName = AppResources.TrackString + " 1";
            var newTrack = new ToDoTrack
            {
                Name = trackName,
                ProjectRef = newProject,
                Instrument = this.DataBaseContext.Instruments.First()
            };
            this.DataBaseContext.Tracks.InsertOnSubmit(newTrack);
            this.DataBaseContext.SubmitChanges();
            newProject.Tracks.Add(newTrack);

            var sample = new ToDoSample
            {
                InitialTact = 1,
                Size = 4,
                TrackRef = newTrack,
                Name = newTrack.Name + "_" + newTrack.Samples.Count
            };
            this.DataBaseContext.Samples.InsertOnSubmit(sample);
            this.DataBaseContext.SubmitChanges();
            newTrack.Samples.Add(sample);

            return newProject;
        }

        /// <summary>
        /// Remove project and data linked with him from collections and database.
        /// </summary>
        internal void DeleteProject()
        {
            foreach (var track in _onDeleteProject.Tracks)
            {
                foreach (var sample in track.Samples)
                {
                    foreach (var note in sample.Notes)
                    {
                        this.DataBaseContext.Notes.DeleteOnSubmit(note);
                    }
                    this.DataBaseContext.Samples.DeleteOnSubmit(sample);
                }
                this.DataBaseContext.Tracks.DeleteOnSubmit(track);
            }

            this._projectsList.Remove(_onDeleteProject);
            this.DataBaseContext.Projects.DeleteOnSubmit(_onDeleteProject);
            this.DataBaseContext.SubmitChanges();
        }

        /// <summary>
        /// Give the project new name
        /// </summary>
        /// <param name="newName">New name of the project</param>
        internal void RenameProjectTo(string newName)
        {
            _onRenameProject.Name = newName;
            this.DataBaseContext.SubmitChanges();
        }
    }
}
