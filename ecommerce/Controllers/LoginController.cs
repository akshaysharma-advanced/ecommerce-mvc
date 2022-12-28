using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Microsoft.Ajax.Utilities;
using PagedList;
using ecommerce.Common;
using ecommerce.Models;
using ecommerce;

namespace ecommerce.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            Session.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel user)
        {
            if (ModelState.IsValid)
            {
                Password decryptPassword = new Password();
                //PasswordBase64 decryptPassword = new PasswordBase64();
                using (ecommerceEntities db = new ecommerceEntities())
                {
                    var obj = db.Users.ToList().Where(model => model.UserName.Equals(user.UserName) && decryptPassword.DecryptPassword(model.Password).Equals(user.Password)).FirstOrDefault();
                    if (obj != null && decryptPassword.DecryptPassword(obj.Password) == user.Password)
                    {
                        Session["UserID"] = obj.UserId.ToString();
                        Session["UserName"] = obj.UserName.ToString();
                        return RedirectToAction("Product");
                    }
                    else
                    {
                        ViewBag.Message = "User Name or Password is incorrect";
                    }
                }
            }
            return View(user);
        }


        public ActionResult Logout()
        {
            Session["UserId"] = null;
            return RedirectToAction("Login");
        }

        public ActionResult Error()
        {
            return View();
        }


        private SelectListItem[] GetCategoryTypesList()
        {
            SelectListItem[] categoryTypes = null;

            ecommerceEntities db = new ecommerceEntities();

            var items = db.Categories.Select(a => new SelectListItem()
            {
                Text = a.CategoryName,
                Value = a.CategoryId.ToString()
            }).ToList();

            categoryTypes = items.ToArray();


            // Have to create new instances via projection
            // to avoid ModelBinding updates to affect this
            // globally
            return categoryTypes
                .Select(d => new SelectListItem()
                {
                    Value = d.Value,
                    Text = d.Text
                })
             .ToArray();
        }



        public ActionResult Product(int? page)
        {
            if (Session["UserID"] == null)
            {
                return View("Error");
            }
            ecommerceEntities db = new ecommerceEntities();
            //return View(db.Products.Where(model => model.Status == true).ToList());
            ViewBag.categoryTypes = GetCategoryTypesList();
            ViewBag.Action = "Product";
            var product = db.Products.ToList().ToPagedList(page ?? 1, 5);
            return View(product);
        }


        public ActionResult Add()
        {
            if (Session["UserID"] != null)
            {
                ViewBag.categoryTypes = GetCategoryTypesList();
                return View();
            }
            return View("Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product product)
        {
            if (ModelState.IsValid)
            {
                ecommerceEntities db = new ecommerceEntities();
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Product");
            }
            return View(product);
        }


        public ActionResult Edit(int? id)
        {
            if (Session["UserID"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                ecommerceEntities db = new ecommerceEntities();
                Product product = db.Products.Find(id);
                if (product == null)
                {
                    return HttpNotFound();
                }
                ViewBag.categoryTypes = GetCategoryTypesList();
                return View(product);
            }
            return View("Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,ProductName,ProductDescription,ProductPrice,CategoryId,Status,CreateDate")] Product product)
        {
            if (ModelState.IsValid)
            {
                ecommerceEntities db = new ecommerceEntities();
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Product");
            }
            return View(product);
        }


        public ActionResult Inactivate(int? id)
        {
            if (Session["UserID"] != null)
            {
                ecommerceEntities db = new ecommerceEntities();
                var Product = db.Products.Find(id);
                Product.Status = false;
                db.SaveChanges();
                return RedirectToAction("Product");
            }
            return View("Error");
        }


        public ActionResult Activate(int? id)
        {
            if (Session["UserID"] != null)
            {
                ecommerceEntities db = new ecommerceEntities();
                var Product = db.Products.Find(id);
                Product.Status = true;
                db.SaveChanges();
                return RedirectToAction("Product");
            }
            return View("Error");
        }


        public ActionResult CategorySearch(int? CategoryName, int? page)
        {
            ecommerceEntities db = new ecommerceEntities();
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Error");
            }
            ViewBag.categoryTypes = GetCategoryTypesList();
            ViewBag.Action = "CategorySearch";
            if (CategoryName == null)
            {
                return RedirectToAction("Product");
            }
            var product = db.Products.Where(s => s.CategoryId == CategoryName).ToList().ToPagedList(page ?? 1, 5);
            return View("Product",product);
        }


        public ActionResult NameSearch(string searchName, int? page)
        {
            ecommerceEntities db = new ecommerceEntities();
            if (Session["UserID"] != null)
            {
                if (searchName == "")
                {
                    //return View(db.Products.Where(model => model.Status == true).ToList());
                    return RedirectToAction("Product");
                }
                else if (ModelState.IsValid)
                {
                    
                    if (!String.IsNullOrEmpty(searchName))
                    {
                        ViewBag.categoryTypes = GetCategoryTypesList();
                        ViewBag.Action = "NameSearch";
                        var search = db.Products.Where(model => model.ProductName.Contains(searchName)).ToList().ToPagedList(page ?? 1, 5);
                        return View("Product",search);

                    }
                }
                return View("Product");
            }
            return View("Error");
        }
    }
}