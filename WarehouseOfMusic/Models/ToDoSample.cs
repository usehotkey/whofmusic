﻿//-----------------------------------------------------------------------
// <copyright file="ToDoSample.cs">
//     Copyright (c) Igor Golopolosov. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WarehouseOfMusic.Models
{
    using System;
    using System.ComponentModel;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Table of tracks are containing in a project
    /// </summary>
    [Table]
    public class ToDoSample : INotifyPropertyChanged, INotifyPropertyChanging
    {
        #region SampleColor
        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (this._isPlaying != value)
                {
                    this.NotifyPropertyChanging("StateBrush");
                    this._isPlaying = value;
                    this.NotifyPropertyChanged("StateBrush");
                }
            }
        }

        /// <summary>
        /// Use to color sample rectangle
        /// </summary>
        public Brush StateBrush
        {
            get
            {
                return _isPlaying
                    ? (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]
                    : (SolidColorBrush)Application.Current.Resources["PhoneBackgroundBrush"];
            }
        } 
        #endregion

        /// <summary>
        /// ID of sample
        /// </summary>
        private int _id;

        /// <summary>
        /// Number of first tact in the sample
        /// </summary>
        private int _initialTact;

        /// <summary>
        /// Name of the sample
        /// </summary>
        private string _name;

        /// <summary>
        /// Entity set for the collection side of the relationship.
        /// </summary>
        private EntitySet<ToDoNote> _notes;

        /// <summary>
        /// Quantity of tacts in the sample
        /// </summary>
        private byte _size;

        /// <summary>
        /// Entity reference, to identify the ToDoProject "storage" table
        /// </summary>
        private EntityRef<ToDoTrack> _trackRef;

        /// <summary>
        /// Version column aids update performance.
        /// </summary>
        [Column(IsVersion = true)]
        private Binary _version;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoTrack" /> class.
        /// Assign handlers for the add and remove operations, respectively.
        /// </summary>
        public ToDoSample()
        {
            this._notes = new EntitySet<ToDoNote>(
                new Action<ToDoNote>(this.AttachToDoNote),
                new Action<ToDoNote>(this.DetachToDoNote));
        }

        /// <summary>
        /// Event of property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event of property changing
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// Gets or sets ID of track
        /// </summary>
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int Id
        {
            get
            {
                return this._id;
            }

            set
            {
                if (this._id != value)
                {
                    this.NotifyPropertyChanging("Id");
                    this._id = value;
                    this.NotifyPropertyChanged("Id");
                }
            }
        }

        /// <summary>
        /// Gets or sets number of first tact in the sample
        /// </summary>
        [Column]
        public int InitialTact
        {
            get
            {
                return this._initialTact;
            }

            set
            {
                if (this._initialTact != value)
                {
                    foreach (var note in Notes)
                    {
                        note.Tact = note.Tact + (value - _initialTact);
                    }
                    this.NotifyPropertyChanging("InitialTact");
                    this.NotifyPropertyChanging("Title");
                    this._initialTact = value;
                    this.NotifyPropertyChanged("InitialTact");
                    this.NotifyPropertyChanged("Title");
                }
            }
        }

        /// <summary>
        /// Gets or sets name of the sample
        /// </summary>
        [Column]
        public string Name
        {
            get
            {
                return this._name;
            }

            set
            {
                if (this._name != value)
                {
                    this.NotifyPropertyChanging("Name");
                    this.NotifyPropertyChanging("Title");
                    this._name = value;
                    this.NotifyPropertyChanged("Name");
                    this.NotifyPropertyChanged("Title");
                }
            }
        }

        /// <summary>
        /// Gets or sets quantity of tacts in sample
        /// </summary>
        [Column]
        public byte Size
        {
            get
            {
                return this._size;
            }

            set
            {
                if (this._size != value)
                {
                    this.NotifyPropertyChanging("Size");
                    this._size = value;
                    this.NotifyPropertyChanged("Size");
                }
            }
        }

        /// <summary>
        /// Show title of sample
        /// </summary>
        public string Title
        {
            get
            {
                if (_size > 1) return _initialTact + " - " + (_initialTact + _size - 1);
                return string.Empty + _initialTact;
            }
        }

        /// <summary>
        /// Gets or sets association, to describe the relationship between this key and that "storage" table
        /// </summary>
        [Association(Storage = "_trackRef", ThisKey = "TrackId", OtherKey = "Id", IsForeignKey = true)]
        public ToDoTrack TrackRef
        {
            get
            {
                return this._trackRef.Entity;
            }

            set
            {
                this.NotifyPropertyChanging("TrackRefRef");
                this._trackRef.Entity = value;

                if (value != null)
                {
                    this.TrackId = value.Id;
                }

                this.NotifyPropertyChanging("TrackRefRef");
            }
        }

        /// <summary>
        /// Gets or sets entity set for the collection side of the relationship.
        /// </summary>
        [Association(Storage = "_notes", OtherKey = "SampleId", ThisKey = "Id")]
        public EntitySet<ToDoNote> Notes
        {
            get { return this._notes; }
            set { this._notes.Assign(value); }
        }

        /// <summary>
        /// Gets or sets internal column for the associated ToDoProject ID value
        /// </summary>
        [Column]
        internal int TrackId { get; set; }

        /// <summary>
        /// Called during an add operation
        /// </summary>
        /// <param name="toDo">Note on adding</param>
        private void AttachToDoNote(ToDoNote toDo)
        {
            this.NotifyPropertyChanging("ToDoNote");
            toDo.SampleRef = this;
        }

        /// <summary>
        ///  Called during a remove operation
        /// </summary>
        /// <param name="toDo">Note on removing</param>
        private void DetachToDoNote(ToDoNote toDo)
        {
            this.NotifyPropertyChanging("ToDoNote");
            toDo.SampleRef = null;
        }

        #region INotifyProperty Members

        /// <summary>
        /// Used to notify that a property changed
        /// </summary>
        /// <param name="propertyName">Property on changed</param>
        private void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Used to notify that a property is about to change
        /// </summary>
        /// <param name="propertyName">Property on changing</param>
        private void NotifyPropertyChanging(string propertyName)
        {
            if (this.PropertyChanging != null)
            {
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }
}
