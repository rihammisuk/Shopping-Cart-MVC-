using CmsShoppingCart.Areas.Admin.Models.ViewModels.Shop;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //Declare a list of model
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //Init the model
                categoryVMList = db.Categories
                                .ToArray()
                                .OrderBy(x => x.Sorting)
                                .Select(x => new CategoryVM(x))
                                .ToList();

            }
                //Return view with list
            
                return View(categoryVMList);
        }

        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Declare id
            string id;

            using (Db db = new Db())
            {

                //check that the category name is unique
                if (db.Categories.Any(x=>x.Name == catName)) 
                    return "titletaken";
                
                //Init DTO

                CategoryDTO dto = new CategoryDTO();

                //Add to DTO
                dto.Name = catName;
               // dto.Slug = catName.Replace(" ","-").ToLower();
                dto.Sorting = 100;

                //Save DTO
                db.Categories.Add(dto);
                db.SaveChanges();

                //Get the id
                id = dto.Id.ToString();
            }

            //Return id

            return id;
        }



        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {

                //set init count
                int count = 1;

                //declare pageDTO
                CategoryDTO dto;

                //set sorting for each page
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;

                }


            }
        }



        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Get the Categories
                CategoryDTO dto = db.Categories.Find(id);

                // Remove the Categories
                db.Categories.Remove(dto);

                //Save

                db.SaveChanges();
            }

            return RedirectToAction("Categories");
            //Redirect



           
        }



        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //check category name is unique

                if (db.Categories.Any(x=>x.Name == newCatName)) {
                    return "titletaken";
                }

                //Get DTO

                CategoryDTO dto = db.Categories.Find(id);

                //Edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower(); ;


                //Save
                db.SaveChanges();

            }
            //Return

            return "ok";

        }


        [HttpGet]
        public ActionResult AddProduct()
        {

            //Init model
            ProductVM model = new ProductVM();


            //Add SelectList of categories to model
            using (Db db= new Db()) {

                model.Categories = new SelectList(db.Categories.ToList(),"Id","Name");

            }

                //return view with model

                return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductVM model,HttpPostedFileBase file ) {

            //Check Model State
            if (!ModelState.IsValid)
            {
                using (Db db= new Db()) {
                    model.Categories = new SelectList(db.Categories.ToList(),"Id","Name");
                    return View(model);
                }
            }


            //Make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x=>x.Name == model.Name)) {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("","That product name is taken!");
                    return View(model);
                }
            }
            //Declare ProductId
            int id;

            //Init and save ProductDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);

                product.CategoryName = catDTO.Name;
                db.Products.Add(product);

                db.SaveChanges();

                //Get Inserted Id
                id = product.Id;

            }
            //Set TempData Message

            TempData["SM"] = "You have added a product";

            #region Upload Image

            //Create necessary directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1)) {
                Directory.CreateDirectory(pathString1);
            }
            
            if (!Directory.Exists(pathString2))
            {
                Directory.CreateDirectory(pathString2);
            }

            if (!Directory.Exists(pathString3))
            {
                Directory.CreateDirectory(pathString3);
            }

            if (!Directory.Exists(pathString4))
            {
                Directory.CreateDirectory(pathString4);
            }

            if (!Directory.Exists(pathString5))
            {
                Directory.CreateDirectory(pathString5);
            }

            //check if a file was uploaded

            if (file!=null && file.ContentLength>0) {

                //get file extension
                string ext = file.ContentType.ToLower();

                //verify extention
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png" )
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("","This image was not uploaded - Wrong image extention.");
                        return View(model);
                    }

                }

                //init image name
                string imageName = file.FileName;

                //save image name dto
                using (Db db = new Db()) {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }


                //set original and thumbs image paths
                var path = string.Format("{0}\\{1}",pathString2,imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //save original
                file.SaveAs(path);

                //crate ans save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200,200);
                img.Save(path2);

            }

            #endregion

            //Redirect


            return RedirectToAction("AddProduct");
        }



        public ActionResult Products( int? page ,int ? catId)
        {

            //Declare a list of ProductVM
            List<ProductVM> listOfProductVM;


            //Set page number
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                // Init the list
                listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x=> new ProductVM(x))
                                    .ToList();
                //Populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(),"Id","Name");



                //set selected categories

                ViewBag.SelectedCat = catId.ToString();

            }
            //set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //Return view with list


            return View(listOfProductVM);
        }

        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Declare ProductVM
            ProductVM model;

            using (Db db= new Db())
            {

                //get the product
                ProductDTO dto = db.Products.Find(id);

                //make sure product exists
                if (dto == null) {
                    return Content("The product does not exist.");
                }

                //Init model
                model = new ProductVM(dto);

                //Make a select list
                model.Categories = new SelectList(db.Categories.ToList(),"Id","Name");

                //Get all galary images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" +id+"/Gallery/Thumbs"))
                                                                               .Select(fn=>Path.GetFileName(fn));

            }
            //Return view with model


            return View(model);
        }

        
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Get product id
            int id = model.Id;


            //populate categories selectlist and gallery images

            using (Db db= new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(),"Id","Name");

            }

            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                                                           .Select(fn => Path.GetFileName(fn));
            //check model state
            if (!ModelState.IsValid) {
                return View(model);
            }


            //make sure product name is unique
            using (Db db = new Db())
            {
                if ( db.Products.Where(x=>x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("","That product name is taken");
                    return View(model);
                }
            }

            //update product

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);

                dto.CategoryName = catDTO.Name;
            

                db.SaveChanges();


            }

            //set tempdata message
            TempData["SM"] = "You have edited the product!";

            #region image upload

            //check for file upload
            if (file != null && file.ContentLength > 0) {

                //get extension
                string ext = file.ContentType.ToLower();

                //verify extension
                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "This image was not uploaded - Wrong image extention.");
                        return View(model);
                    }

                }


                //set upload directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //delete file from directoris
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles()) {
                    file2.Delete();
                }

                foreach (FileInfo file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                //save image name
                string imageName = file.FileName;

                using (Db db= new Db()) {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                //save original and images
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

               
                file.SaveAs(path);

                
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }

            #endregion


            //redirect

            return RedirectToAction("EditProduct");

        }




        public ActionResult DeleteProduct(int id)
        {
            using (Db db = new Db())
            {
                //Get the page
                ProductDTO dto = db.Products.Find(id);

                // Remove the page
                db.Products.Remove(dto);

                //Save

                db.SaveChanges();
            }

            //Delete product Folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads",Server.MapPath(@"\")));
            string pathString = Path.Combine(originalDirectory.ToString(),"Products\\"+id.ToString());

            if (Directory.Exists(pathString)) {
                Directory.Delete(pathString,true);
            }



            //Redirect

            return RedirectToAction("Products");




        }


        public void SaveGalleryImages(int id)
        {

            //Lopp through files
            foreach (string fileName in Request.Files) {

                //Init the file
                HttpPostedFileBase file = Request.Files[fileName];

                //check it's not null
                if (file != null && file.ContentLength > 0) {

                    //set directory paths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads",Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(),"Products\\"+id.ToString()+"\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //set image paths
                    var path = string.Format("{0}\\{1}",pathString1,file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    //save original and thumb
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200,200);
                    img.Save(path2);

                }
            }
        }



        public void DeleteImage(int id, string imageName)
        {

            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/"+id.ToString()+"/Gallery/"+imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);


            if (System.IO.File.Exists(fullPath1)) {
                System.IO.File.Delete(fullPath1);
            }

            if (System.IO.File.Exists(fullPath2))
            {
                System.IO.File.Delete(fullPath2);
            }

        }

        //GET Admin/Shop/Orders
        public ActionResult Orders()
        {
            //Init list for OrdersForAdminVM
            List<OrdersForAdminVM> orderForAdmin = new List<OrdersForAdminVM>();

            using (Db db= new Db())
            {

                //Init list of OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();


                //Loop through list of OrderVM
                foreach (var order in orders)
                {
                    //Init product dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //Declare total
                    decimal total = 0m;

                    //Init list of OrderDetaailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x=>x.OrderId ==order.OrderId).ToList();


                    //Get username
                    UserDTO user = db.Users.Where(x=>x.Id ==order.UserId).FirstOrDefault();
                    string username = user.Username;

                    //Loop through list OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        //Get product price
                        decimal price = product.Price;

                        //Get product name
                        string productName = product.Name;

                        //Add to product dict
                        productsAndQty.Add(productName,orderDetails.Quantity);

                        //Get total
                        total += orderDetails.Quantity * price;

                    }
                    //Add to ordersForAdminVM list
                    orderForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber=order.OrderId,
                        UserName= username,
                        Total=total,
                        ProductsAndQty=productsAndQty,
                        CreatedAt=order.CreatedAT
                    });


                }



            }
                return View(orderForAdmin);
        }

    }
}




