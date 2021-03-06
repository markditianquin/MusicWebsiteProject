﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MusicProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MusicProject.Controllers
{
    
    public class ArtistsController : Controller
    {
        private MusicContext db = new MusicContext();
        ApplicationDbContext context;

        public ArtistsController()
        {
            context = new ApplicationDbContext();
        }

        //check if the user is adminitrator or not
        public Boolean isAdminUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "Admin")
                {
                    return true;
                }
            }
            return false;
        }

        // do a quick search that returns a json
        public ActionResult QuickSearch(string term)
        {
            var artists = GetArtists(term).Select(a => new { value = a.Lname, a.Fname });
            return Json(artists, JsonRequestBehavior.AllowGet);
        }

        //get the Artists in db
        private List<Artist> GetArtists(string searchString)
        {
            return db.Artists
                .Where(a => a.Lname.Contains(searchString) || a.Fname.Contains(searchString))
                .ToList();
        }

        // GET: Artists
        public ActionResult Index(string name, string currentFilter, string searchString)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ViewBag.Name = user.Name;

                ViewBag.displayMenu = "Member";

                if (isAdminUser())
                {
                    ViewBag.displayMenu = "Admin";
                }
            }

            //Set up sorting cases 
            ViewBag.FnameSorting = String.IsNullOrEmpty(name) ? "Fname_DESC" : "";
            ViewBag.LnameSorting = name == "Lname" ? "Lname_DESC" : "Lname";
            ViewBag.GenreSorting = name == "Genres" ? "Genres_DESC" : "Genres";
            ViewBag.CompanySorting = name == "Company" ? "Company_DESC" : "Company";
            ViewBag.DateSorting = name == "Date" ? "Date_DESC" : "Date";
            ViewBag.CreateSorting = name == "Create" ? "Create_DESC" : "Create";

            if (searchString == null)
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var artist = from s in db.Artists select s;

            //Searching the song by song title
            if (!String.IsNullOrEmpty(searchString))
            {
                artist = artist.Where(s => s.Lname.Contains(searchString)
                            || s.Fname.Contains(searchString));
            }

            //Sorting by title, genre, or artist first name
            switch (name)
            {
                case "Fname_DESC":
                    artist = artist.OrderByDescending(s => s.Fname);
                    break;
                case "Lname":
                    artist = artist.OrderBy(s => s.Lname);
                    break;
                case "Lname_DESC":
                    artist = artist.OrderByDescending(s => s.Lname);
                    break;
                case "Genres":
                    artist = artist.OrderBy(s => s.Genres);
                    break;
                case "Genres_DESC":
                    artist = artist.OrderByDescending(s => s.Genres);
                    break;
                case "Company":
                    artist = artist.OrderBy(s => s.Companies.Name);
                    break;
                case "Company_DESC":
                    artist = artist.OrderByDescending(s => s.Companies.Name);
                    break;
                case "Date":
                    artist = artist.OrderBy(s => s.Debut_in);
                    break;
                case "Date_DESC":
                    artist = artist.OrderByDescending(s => s.Debut_in);
                    break;
                case "Create":
                    artist = artist.OrderBy(s => s.CreateOrUpdate);
                    break;
                case "Create_DESC":
                    artist = artist.OrderByDescending(s => s.CreateOrUpdate);
                    break;
                default:
                    artist = artist.OrderBy(s => s.Fname);
                    break;
            }

            return View(artist.ToList());
        }

        // GET: Artists/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (User.Identity.IsAuthenticated)
            {
                ViewBag.displayMenu = "Member";
            }

            Artist artist = db.Artists.Find(id);

            if (artist == null)
            {
                return HttpNotFound();
            }

            getImage(artist);

            var artistModel = db.Artists.Include(s => s.Albums).Include(s => s.Songs).Single(g => g.ArtistID == id);

            return View(artistModel);
        }

        //Convert the image data in the database into image to display on the page
        public void getImage(Artist artist)
        {
            if (artist.Image != null)
            {
                byte[] imageByteData = artist.Image;
                string imageBase64Data = Convert.ToBase64String(imageByteData);
                string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                ViewBag.ImageData = imageDataURL;
            }
        }

        // GET: Artists/Create
        public ActionResult Create()
        {
            ViewBag.CompanyID = new SelectList(db.Companies, "CompanyID", "Name");
            return View();
        }

        // POST: Artists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ArtistID,Fname,Lname,Genres,Debut_in,History,CompanyID,Rewards")] Artist artist, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    using (var reader = new System.IO.BinaryReader(upload.InputStream))
                    {
                        artist.Image = reader.ReadBytes(upload.ContentLength);
                        artist.ImageName = upload.FileName;
                    }
                }
                artist.CreateOrUpdate = System.DateTime.Now;

                db.Artists.Add(artist);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CompanyID = new SelectList(db.Companies, "CompanyID", "Name", artist.CompanyID);
            return View(artist);
        }

        // GET: Artists/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Artist artist = db.Artists.Find(id);

            if (artist == null)
            {
                return HttpNotFound();
            }

            getImage(artist);

            ViewBag.CompanyID = new SelectList(db.Companies, "CompanyID", "Name", artist.CompanyID);
            return View(artist);
        }

        // POST: Artists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, HttpPostedFileBase upload)
        {
            Artist artist = db.Artists.FirstOrDefault(i => i.ArtistID == id);

            if (upload != null && upload.ContentLength > 0)
            {
                using (var reader = new System.IO.BinaryReader(upload.InputStream))
                {
                    artist.Image = reader.ReadBytes(upload.ContentLength);
                    artist.ImageName = upload.FileName;
                }
            }
            artist.CreateOrUpdate = System.DateTime.Now;

            UpdateModel(artist);
            db.SaveChanges();

            ViewBag.CompanyID = new SelectList(db.Companies, "CompanyID", "Name", artist.CompanyID);

            return RedirectToAction("Details", new { id = artist.ArtistID });
        }

        // GET: Artists/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Artist artist = db.Artists.Find(id);

            //var album = from s in db.Albums select s;
            //album = album.Where(s => s.ArtistID == id);
            Album album = db.Albums.FirstOrDefault(a => a.ArtistID == id);
            Song song = db.Songs.FirstOrDefault(a => a.ArtistID == id);

            if (album != null && song != null)
            {
                ViewBag.Delete = "No";
            }

            if (artist == null)
            {
                return HttpNotFound();
            }

            getImage(artist);

            return View(artist);
        }

        // POST: Artists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            Artist artist = db.Artists.Find(id);
            //var album = from s in db.Albums select s;
            //album = album.Where(s => s.ArtistID == id);

            //if (album != null)
            //{
            //    return RedirectToAction("Delete", new { id = artist.ArtistID });
            //}

            db.Artists.Remove(artist);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
