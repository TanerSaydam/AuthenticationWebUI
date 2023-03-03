using AuthenticationWebUI.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationWebUI.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;

    public HomeController(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public IActionResult Index()
    {        
        return View();
    }
    

}