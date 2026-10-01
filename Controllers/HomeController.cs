using EduConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduConnect.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (User.IsInRole("Teacher"))
                {
                    return RedirectToAction("Index", "Teacher");
                }

                if (User.IsInRole("Parent"))
                {
                    return RedirectToAction("Index", "Parent");
                }

                if (User.IsInRole("Student"))
                {
                    return RedirectToAction("Index", "Student");
                }
            }

            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
        
    }
}
