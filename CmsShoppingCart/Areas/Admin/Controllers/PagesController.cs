using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare the list

            List<PageVM> pageList;

            using (Db db= new Db()) {

                //Init the list
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x=>  new PageVM(x)).ToList();

            }

                //Return view
                return View(pageList);
        }

        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Check model state
            if (!ModelState.IsValid) {
                return View(model);
            }

            using (Db db= new Db()) {
                //Declare slug
                string slug;

                //Init PageDTO
                PageDTO dto = new PageDTO();

                //DTO title
                dto.Title = model.Title;
                //Check for and set slug if need be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {

                    slug = model.Title.Replace(" ", "-").ToLower();

                }
                else {

                    slug = model.Slug.Replace(" ", "-").ToLower();

                }

                //Make sure title and slug are unique
                if (db.Pages.Any(x=>x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug)) {

                    ModelState.AddModelError("","That Title or Slug are already exists");
                    return View(model);
                }

                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }
            //Set TempData message
            TempData["SM"] = "You have added new Pages!";

            //Redirect
            return RedirectToAction("AddPage");
               
        }

        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Decare PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Get the page
                PageDTO dto = db.Pages.Find(id);

                //Confirm Page Exists
                if (dto == null) {
                    return Content("Page doesn't exists");
                }

                //Init PageVM
                model = new PageVM(dto);
            }
                //Return View with model

                return View(model);
        }

        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Check Model state
            if (!ModelState.IsValid) {
                return View(model);
            }

            using (Db db = new Db()) {
                //Get Page Id

                int id = model.Id;

                // Init Slug
                string slug="home";

                // Get the page
                PageDTO dto = db.Pages.Find(id);

                // DTO the title
                dto.Title = model.Title;

                // Check for slug and set it if need be
                if (model.Slug!="home") {

                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }

                }

                //Make sure Title and Slug are unique
                if (db.Pages.Where(x=>x.Id != id).Any(x=>x.Title ==model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("","This title of slug already exists");
                    return View(model);
                }
                // DTO the rest

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                // Save the DTO

                db.SaveChanges();
            }

            // Set TempData Message
            TempData["SM"] = "You have edited the page";

            //Redirect

            return RedirectToAction("EditPage");



              
        }


        public ActionResult PageDetails(int id)
        {

            //Declare PageVM
            PageVM model = new PageVM();
            using (Db db = new Db())
            {
                //Get the page
                PageDTO dto = db.Pages.Find(id);
                //Confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist");
                }

                //Init Page VM
                model = new PageVM(dto);
            }
                //Return page view with model

                return View(model);
        }


        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db()) {
                //Get the page
                PageDTO dto = db.Pages.Find(id);

                // Remove the page
                db.Pages.Remove(dto);

                //Save

                db.SaveChanges();
            }

            return RedirectToAction("Index");
            //Redirect



           
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db= new Db()) {

                //set init count
                int count = 1;

                //declare pageDTO
                PageDTO dto;

                //set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;

                }


            }
        }

        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Declare model
            SidebarVM model;

            using (Db db = new Db()) {
                //Get the DTO

                SidebarDTO dto = db.Sidebar.Find(1);

                //Init model
                model = new SidebarVM(dto);

            }
                //Return view with model


                return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db()) {

                //Get the DTO

                SidebarDTO dto = db.Sidebar.Find(1);

                //DTO the body
                dto.Body = model.Body;

                //Save
                db.SaveChanges();
            }
            //Set Tempdata message
            TempData["SM"] = "You have edited this sidebar";


            //Redirect


            return RedirectToAction("EditSidebar");
        }

    }
}