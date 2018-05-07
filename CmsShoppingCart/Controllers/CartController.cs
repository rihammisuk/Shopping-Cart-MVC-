using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Init the cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Check if cart is empty
            if (cart.Count == 0 ||Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty";
                return View();
            }

            //Calculate total and save the ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            //Return view with list

            return View(cart);
        }


        public ActionResult CartPartial()
        {
            //Init CartVM
            CartVM model = new CartVM();

            //Init Quantity
            int qty = 0;

            //Init Price
            decimal price = 0m;


            //Check for cart session
            if (Session["cart"] != null)
            {

                //Get total quantity and price

                var list = (List<CartVM>)Session["cart"];
                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;

                }

                model.Quantity = qty;
                model.Price = price;

            }
            else
            {

                //Or set quantity and price to 0

                model.Quantity = 0;
                model.Price = 0m;

            }

            //Return PartialView with model


            return PartialView(model);
        }


        public ActionResult AddtoCartPartial(int id)
        {
            //Init CartVM list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();


            //Init CartVM
            CartVM model = new CartVM();


            using (Db db = new Db()) {
                //Get the product
                ProductDTO product = db.Products.Find(id);


                //Check if the product is already in cart
                var productInCart = cart.FirstOrDefault(x=>x.ProductId == id);

                //If not add new
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }

                else
                {
                    //If it is incrament
                    productInCart.Quantity++;

                }
            }

            //Get total quantity and price and add to model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price +=item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //save cart back to session
            Session["cart"] = cart;

                //Return partial view with model


                return PartialView(model);
        }

        // GET: Cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //get CartVM from list
                CartVM model = cart.FirstOrDefault(x=>x.ProductId == productId);

                //Increment qty
                model.Quantity++;

                //Store neede data
                var result = new {qty = model.Quantity,price = model.Price };

                //Return Json with data

                return Json(result,JsonRequestBehavior.AllowGet);

            }
        }



        // GET: Cart/DecrementProduct
        public ActionResult DecrementProduct(int productId)
        {
            //Init cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);


                //Decrement qty
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
                //Store needed data
                var result = new { qty = model.Quantity, price = model.Price };


                //Return Json
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            
        }


        // GET: Cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            //Init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db()) {
                //Get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);


                //Remove model from list
                cart.Remove(model);
            }
        }

        // POST: Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //Get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Get Username
            string username = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                //Init OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                //Get user id
                var q = db.Users.FirstOrDefault(x=>x.Username==username);
                int userId = q.Id;

                //Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                //Get inserted id
                orderId = orderDTO.OrderId;

                //Init OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();


                //Add to OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();

                }

            }

            //Reset Session


            Session["cart"] = null;



        }


        public ActionResult PayOptions()
        {
            return View();
        }

    }
}