﻿namespace WarehouseOfMusic
{
    using System;
    using ViewModels;

    public class NoteEventArgs : EventArgs
    {
        public int Id;
        /// <summary>
        /// Key
        /// </summary>
        public Key Key { get; set; }
        /// <summary>
        /// Position note at the tact
        /// </summary>
        public byte TactPosition { get; set; }
    }
}