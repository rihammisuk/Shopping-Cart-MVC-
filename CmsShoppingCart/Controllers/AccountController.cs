using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Account;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShoppingCart.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: account/login
        [HttpGet]
        public ActionResult Login()
        {
            //Confirm user is not logged In
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("user-profile");
            }

            //Return view
            return View();
        }



        // POST: account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //Check ModelState
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Check User is valid
            bool isValid = false;

            using (Db db= new Db())
            {
                if (db.Users.Any(x=>x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid Username or Password.");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username,model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe)); 
            }
        }

       
        // GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }


        // Post: account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Check model state
            if (!ModelState.IsValid) {
                return View("CreateAccount", model);
            }

            //Check if password match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("","Password do not match");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //Make sure username is unique
                if (db.Users.Any(x=>x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("","Username "+model.Username+" is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //Create UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };


                //Add the DTO
                db.Users.Add(userDTO);

                //Save
                db.SaveChanges();

                //Add to UserRolesDTO
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }
            //Create a TempData Message
            TempData["SM"] = "You are now registered and can login";

            //Redirect
            return Redirect("~/account/login");

        }

        // POST: account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }



        [Authorize]
        public ActionResult UserNavPartial()
        {
            //Get Username
            string username = User.Identity.Name;

            //Declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //Get the user
                UserDTO dto = db.Users.FirstOrDefault(x=>x.Username == username);

                //Build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName

                };


            }
                //Return partial view with model

                return PartialView(model);
        }

        // GET: account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            //Get username
            string username = User.Identity.Name;

            //Declare model
            UserProfileVM model;


            using (Db db = new Db())
            {
                //Get user
                UserDTO dto = db.Users.FirstOrDefault(x=>x.Username == username);

                //Build model
                model = new UserProfileVM(dto);
            }
                //Return view with model
                return View("UserProfile", model);
        }


        // POST: account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            //Check ModelState
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //Check if passwords match if need be
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("","Password do not match");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //Get username
                string username = User.Identity.Name;

                //make sure username is unique
                if (db.Users.Where(x=>x.Id !=model.Id).Any(x=>x.Username == username))
                {
                    ModelState.AddModelError("","Username "+model.Username+" is already exists");
                    model.Username = "";
                    return View("UserProfile", model);
                }


                //Edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //Save
                db.SaveChanges();
            }
            //Set TempData message
            Session["SM"] = "You have edited your profile";


            //Redirect
            return Redirect("~/account/user-profile");
            return View();
        }



        // GET: account/Orders
        [Authorize(Roles ="User")]
        public ActionResult Orders()
        {
            //Init list for OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {

                //Get User id
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                //Init list of OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                //Loop through list of OrderVM
                foreach(var order in orders)
                {
                    //Init product dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //Declare total
                    decimal total = 0m;

                    //Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();


                    //Loop through list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        //Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        //Get product Price
                        decimal price = product.Price;

                        //Get product name
                        string productName = product.Name;

                        //Add to product dict
                        productsAndQty.Add(productName,orderDetails.Quantity);

                        //Get total
                        total += orderDetails.Quantity * price;
                    }

                    //Add to OrderForUserVM
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAT
                    });

                }


            }

            //Return view with list of OrderForUserVM
                return View(ordersForUser);
        }
    }

}







