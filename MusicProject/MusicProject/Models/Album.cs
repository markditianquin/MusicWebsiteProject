﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicProject.Models
{
    public class Album
    {
        [Key]
        public int AlbumID { get; set; }
        [Display(Name = "Album's Title")]
        public string Title { get; set; }

        [Display(Name = "Artist")]
        [ForeignKey("Artists")]
        public int? ArtistID { get; set; }   //Foreign key
        public virtual Artist Artists { get; set; } //Navigation

        public string Genres { get; set; }
        public string Producers { get; set; }

        [Display(Name = "Labels By")]
        [ForeignKey("Companies")]
        public int? CompanyID { get; set; }  //Foreign key
        public virtual Company Companies { get; set; }  //Navigation

        [Display(Name = "Release In")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Release { get; set; }

        public ICollection<Song> Songs { get; set; }
    }
}