using Microsoft.AspNetCore.Mvc;
using Examination_System.Repos;

namespace Examination_System.Controllers
{
    public class UserController : Controller
    {
        readonly IUserRepo userRepo; //user repository

        public UserController(IUserRepo _userRepo) //constructor
        {
            userRepo = _userRepo;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string UserName, string UserPass)
        {
            if (userRepo.IsUserCredentialsValid(UserName, UserPass).Result) //check if the user credentials are valid
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Message = "Invalid Credentials";
                return PartialView("ErrorInLogin");
            }
        }
    }
}
