
﻿using GestionConcours.Models;


using Rotativa.Options;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace GestionConcours.Controllers
{
    public class HomeController : Controller
    {

        GestionConcourDbContext cl = new GestionConcourDbContext();
        private GestionConcourDbContext db = new GestionConcourDbContext();
        private Candidat candidat;
        public bool isNull(Object obj)
        {
            bool isNull = obj.GetType().GetProperties().All(p => p.GetValue(obj,null) != null);
            return isNull;
        }
        public string checkConformity()
        {
            string msg = "";
            var diplome = db.Diplomes.Find(Session["cne"]);
            var anne = db.AnneeUniversitaires.Find(Session["cne"]);
            var bac = db.Baccalaureats.Find(Session["cne"]);
            string type_dip = diplome.Type;
            int k = 0;
            if(!isNull(diplome))
            {
                msg += "Diplôme Info, ";
                k = 1;
            }
            if (!isNull(anne))
            {
                if(type_dip == null)
                {
                    msg += "Année Univertsitaire, ";
                    k = 1;
                }
                else if ( type_dip.Contains("Lic"))
                {
                    msg += "Année Univertsitaire, ";
                    k = 1;
                }
            }
            if (!isNull(bac))
            {
                msg += "Bac Info,";
                k = 1;
            }
            if (k == 1)
            {
                msg += "still need editing";
            }
            return msg;
        }
        public ActionResult Index()
        {
            if (Session["cne"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var candidat = db.Candidats.Find(Session["cne"]);
            if (candidat.Verified == 0)
            {
                return RedirectToAction("Step1", "Auth");
            }
            Session["photo"] = candidat.Photo;
            Session["nom"] = candidat.Nom;
            Session["prenom"] = candidat.Prenom;
            Session["niveau"] = candidat.Niveau;
            string message = checkConformity();
            ViewData["error"] = message;
            string cne = Session["cne"].ToString();
            Candidat c1 = db.Candidats.Where(p => p.Cne == cne).SingleOrDefault();
            return View(c1);
        }

        public ActionResult Profil()
        {
            if (Session["cne"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            GestionConcourDbContext db = new GestionConcourDbContext();
            
            Candidat c1 = db.Candidats.Where(p => p.Cne == Session["cne"].ToString()).SingleOrDefault();
            if (c1.Verified == 0)
            {
                return RedirectToAction("Step1", "Auth");
            }
            return View(c1);
        }

        [HttpGet]
        public ActionResult ModifierPersonel()
        {
            if (Session["cne"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            Candidat candidat = db.Candidats.Find(Session["cne"]);
            if (candidat.Verified == 0)
            {
                return RedirectToAction("Step1", "Auth");
            }
            Session["photo"] = candidat.Photo;
            return View(candidat);
        }
        [HttpPost]
        public ActionResult ModifierPersonel(Candidat candidat)
        {
            var originalCandiat = (from c in db.Candidats where c.Cne == candidat.Cne select c).First();
            originalCandiat.Nom = candidat.Nom;
            originalCandiat.Prenom = candidat.Prenom;
            originalCandiat.Password = candidat.Password;
            originalCandiat.Cin = candidat.Cin;
            originalCandiat.DateNaissance = candidat.DateNaissance;
            originalCandiat.LieuNaissance = candidat.LieuNaissance;
            originalCandiat.Nationalite = candidat.Nationalite;
            originalCandiat.Gsm = candidat.Gsm;
            originalCandiat.Telephone = candidat.Telephone;
            originalCandiat.Adresse = candidat.Adresse;
            originalCandiat.Ville = candidat.Ville;
            originalCandiat.Email = candidat.Email;
            originalCandiat.Sexe = candidat.Sexe;
            db.SaveChanges();
            TempData["message"] = "Profil Personel Modified succefully";
            return RedirectToAction("Index");
        }

        public ActionResult ModifierDiplome()
        {
            if (Session["cne"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var candidat = db.Candidats.Find(Session["cne"]);
            if (candidat.Verified == 0)
            {
                return RedirectToAction("Step1", "Auth");
            }
            var diplome = db.Diplomes.Find(Session["cne"]);
            var anne = db.AnneeUniversitaires.Find(Session["cne"]);
            DiplomeNote dipNote = new DiplomeNote()
            {
                Type = diplome.Type,
                Etablissement = diplome.Etablissement,
                VilleObtention = diplome.VilleObtention,
                NoteDiplome = diplome.NoteDiplome,
                Specialite = diplome.Specialite,
                Semestre1 = anne.Semestre1,
                Semestre2 = anne.Semestre2,
                Semestre3 = anne.Semestre3,
                Semestre4 = anne.Semestre4,
                Semestre5 = anne.Semestre5,
                Semestre6 = anne.Semestre6,
                Redoublant1 = anne.Redoublant1,
                Redoublant2 = anne.Redoublant2,
                Redoublant3 = anne.Redoublant3,
                AnneUni1 = anne.AnneUni1,
                AnneUni2 = anne.AnneUni2,
                AnneUni3 = anne.AnneUni3
            };
            return View(dipNote);			
        }

        public ActionResult FichierScanne()
        {
            if (Session["cne"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var candidat = db.Candidats.Find(Session["cne"]);
            if (candidat.Verified == 0)
            {
                return RedirectToAction("Step1", "Auth");
            }
            return View();
        }

        [HttpPost]
        public ActionResult FichierScanne(HttpPostedFileBase[] files)
        {
            string cne = Session["cne"].ToString();
            string globalName="";
            if (ModelState.IsValid)
            {   //iterating through multiple file collection   
                foreach (HttpPostedFileBase file in files)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        var InputFileName = Path.GetFileName(file.FileName);
                        globalName += InputFileName + "|";
                        var ServerSavePath = Path.Combine(Server.MapPath("~/DiplomeScanné/") + InputFileName);
                        //Save file to server folder  
                        file.SaveAs(ServerSavePath);
                        //assigning file uploaded status to ViewBag for showing message to user.  
                    }

                }
                ViewBag.UploadStatus = files.Count().ToString() + " files uploaded successfully.";
                var y = db.Fichiers.Where(f => f.Cne == cne).SingleOrDefault();
                if (y == null)
                {
                    Fichier fichier = new Fichier();
                    fichier.Cne = cne;
                    fichier.nom = globalName;
                    db.Fichiers.Add(fichier);
                    db.SaveChanges();
                }
                else
                {
                    y.nom = globalName;
                    db.SaveChanges();
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult ModifierDiplome(DiplomeNote diplome)
        {
            string cne = Session["cne"].ToString();
            if (ModelState.IsValid)
            {
                var x = db.Diplomes.Where(c => c.Cne == cne).SingleOrDefault();
                x.Type = diplome.Type;
                x.Etablissement = diplome.Etablissement;
                x.VilleObtention = diplome.VilleObtention;
                x.NoteDiplome = diplome.NoteDiplome;
                x.Specialite = diplome.Specialite;
                db.SaveChanges();

                var y = db.AnneeUniversitaires.Where(a => a.Cne == cne).SingleOrDefault();
                y.Semestre1 = diplome.Semestre1;
                y.Semestre2 = diplome.Semestre2;
                y.Semestre3 = diplome.Semestre3;
                y.Semestre4 = diplome.Semestre4;
                y.Semestre5 = diplome.Semestre5;
                y.Semestre6 = diplome.Semestre6;
                y.Redoublant1 = diplome.Redoublant1;
                y.Redoublant2 = diplome.Redoublant2;
                y.Redoublant3 = diplome.Redoublant3;
                y.AnneUni1 = diplome.AnneUni1;
                y.AnneUni2 = diplome.AnneUni2;
                y.AnneUni3 = diplome.AnneUni3;

                db.SaveChanges();
                TempData["message"] = "Diplome Modified succefully";
                return RedirectToAction("Index");
            }
            return View(diplome);
            
        }

        public ActionResult ModifierBac()
        {
            if (Session["cne"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var candidat = db.Candidats.Find(Session["cne"]);
            if (candidat.Verified == 0)
            {
                return RedirectToAction("Step1", "Auth");
            }
            Baccalaureat bac = db.Baccalaureats.Find(Session["cne"]);

            List<SelectListItem> listTypeBac = new List<SelectListItem>
            {
                new SelectListItem{Text="SMA", Value="SMA"},
                new SelectListItem{Text="SMB", Value="SMB"},
                new SelectListItem{Text="SVT", Value="SVT"},
                new SelectListItem{Text="PC", Value="PC"}
            };
            List<SelectListItem> listMention = new List<SelectListItem>
            {
                new SelectListItem{Text="Très Bien", Value="Très Bien"},
                new SelectListItem{Text="Bien", Value="Bien"},
                new SelectListItem{Text="Assez Bien", Value="Assez Bien"},
                new SelectListItem{Text="Passable", Value="Passable"}
            };

            //ViewBag.typeBac = listTypeBac;
            //ViewBag.mention = listMention;

            return View(bac);
        }

        [HttpPost]
        public ActionResult ModifierBac([Bind(Include = "Cne,TypeBac,DateObtentionBac,NoteBac,MentionBac")]Baccalaureat bac)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bac).State = EntityState.Modified;
                db.SaveChanges();
                TempData["message"] = "Bac Modified succefully";
                return RedirectToAction("Index");
            }
            return View(bac);


        }


        public ActionResult ModifierFiliere()
        {
            if (Session["cne"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var candidat = db.Candidats.Find(Session["cne"]);
            if (candidat.Verified == 0)
            {
                return RedirectToAction("Step1", "Auth");
            }
            string cne = Session["cne"].ToString();
            var x = db.Candidats.Where(c => c.Cne == cne).SingleOrDefault();
            var y = db.Filieres.Where(f => f.ID == x.ID).SingleOrDefault();
            ViewData["filiere"] = y.Nom;
            return View();
        }

        [HttpPost]
        public ActionResult ModifierFiliere(string ID)
        {
            if (Session["cne"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            string cne = Session["cne"].ToString();
            var x = db.Candidats.Where(c => c.Cne == cne).SingleOrDefault();
            x.ID = Convert.ToInt32(ID);
            db.SaveChanges();
            var y = db.Filieres.Where(f => f.ID == x.ID).SingleOrDefault();
            TempData["message"] = "Filiere Modified succefully";
            return RedirectToAction("Index");
        }


        public JsonResult Image(HttpPostedFileBase file)
        {
            string response=" ";
            if (file != null && file.ContentLength > 0)
                try
                {
                    String extension = Path.GetExtension(file.FileName);
                    Random r = new Random();
                    int rInt = r.Next(0, 10000);
                    string fileName = rInt.ToString() + extension.ToLower();
                    string path = Path.Combine(Server.MapPath("~/Pictures/userPic"), fileName);
                    file.SaveAs(path);
                    string cne = Session["cne"].ToString();
                    GestionConcourDbContext db = new GestionConcourDbContext();
                    var x = db.Candidats.Where(c => c.Cne == cne).SingleOrDefault();
                    x.Photo = fileName;
                    db.SaveChanges();
                    Session["photo"] = fileName;
                    response = fileName;
                }
                catch (Exception ex)
                {
                    response = "icon.png";
                }
            else
            {
                response = "icon.png";
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        
        // une fois on clique sur le lien download on appelle cette action qui va telecharger le fichier 
        // que son nom est passé en parametre depuis le dossier Epreuves
        public FileResult Download(string fichier)
        {
            string fullName = Server.MapPath("~/Epreuves/" + fichier);
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullName);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fichier);
        }


       // affiche tous les donnes extstantes dans la table épreuve
        public ActionResult Epreuve()
        {
            if (Session["cne"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var candidat = db.Candidats.Find(Session["cne"]);
            if (candidat.Verified == 0)
            {
                return RedirectToAction("Step1", "Auth");
            }
            var data = cl.Epreuves;
            return View(data);
        }

        // Afficher le contenue de la convocation 
        public ActionResult Fiche(string id,string click="empty")
        {
            if (Session["cne"] == null && id==null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var candidat = db.Candidats.Find(Session["cne"]);
            if (candidat.Verified == 0)
            {
                return RedirectToAction("Step1", "Auth");
            }
            // Pour supprimer le header de la page de la convocation
            if (click.Equals("imprimer"))
            {
                ViewBag.Imprimer = "imprimer";
            }
        
            if(id==null)
            {
                id = Session["cne"].ToString();
            }
             
            Candidat data = GetCandidat(id);
            if(data.Diplome.Type==null || data.Diplome.VilleObtention==null)
            {
                return RedirectToAction("Index");
            }
            return View(data);
        }

       

        public Candidat GetCandidat(string cne)
        {
            var candidat = cl.Candidats.Include("Filiere").Include("Diplome").Where(c => c.Cne == cne).SingleOrDefault();
            return candidat as Candidat;
        }


        // Responsable d'imprimer le fiche mais les sessions ne marchent pas lors de l'appel de Fiche()
         public ActionResult ImprimerConvocation(string cne)
         {
           return new Rotativa.ActionAsPdf("Fiche", new { id = cne, click = "imprimer" })

           {
               PageSize = Size.A4,
               CustomSwitches = "--disable-smart-shrinking",
               PageOrientation = Orientation.Portrait,
               PageMargins = new Margins(0, 0, 0, 0),
               PageWidth = 210,
               PageHeight = 220
           };
         }

        public ActionResult Deconnexion()
        {
            Session["cne"] = null;
            Session["photo"] = null;
            Session["nom"] = null;
            Session["prenom"] = null;
            Session["niveau"] = null;
            return RedirectToAction("Login", "Auth");

        }

    }
}