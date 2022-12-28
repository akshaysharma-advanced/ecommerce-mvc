using ecommerce.Common;
using ecommerce.DAL;
using ecommerce.Models;
using ecommerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ecommerce.Controllers
{
    public class RegistrationController : Controller
    {
        ecommerceEntities db = new ecommerceEntities();
        // GET: Registration
        public ActionResult Registration()
        {
            return View();
        }

        public ActionResult Register(RegisrationModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Where(u => u.UserName == model.UserName).Any())
                {
                    ViewBag.Message = "UserName is Already Taken, try with another UserName";
                    return View("~/Views/Users/Index.cshtml");
                }
                //RegistrationDataAccessLayer registrationDataAccessLayer = new RegistrationDataAccessLayer();
                //string message = registrationDataAccessLayer.SignUpUser(model);
                Password encryptPassword = new Password();
                //RegisterDataLayer dal = new RegisterDataLayer();
                //string message = dal.SignUpUser(model);
                User user = new User();
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Mobile = model.Mobile;
                user.Password = encryptPassword.EncryptPassword(model.Password);

                db.Users.Add(user);
                db.SaveChanges();
                return View("Register");
            }
        
            else
            {
                return View("~/Views/Registration/Registration.cshtml");
            }

            return RedirectToAction("Login","Login");
        }

        public JsonResult doesUserNameExist(string UserName)
        {
            ecommerceEntities db = new ecommerceEntities ();
            return Json(!db.Users.Any(x => x.UserName == UserName),JsonRequestBehavior.AllowGet);
        }
    }
}