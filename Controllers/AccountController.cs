using AuthenticationWebUI.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.EntityFrameworkCore;
using AuthenticationWebUI.Commons;

namespace AuthenticationWebUI.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Login()
    {        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string emailOrUserName, string password)
    {
        var result = false;

        TempData["emailOrUserName"] = emailOrUserName;

        AppUser user = await _userManager.FindByEmailAsync(emailOrUserName);
        if(user == null)
        {
            user = await _userManager.FindByNameAsync(emailOrUserName);

            if(user == null )
            {
                TempData["error"] = "Kullanıcı adı ya da mail adresi yanlış!";
                return View();
            }

            result = await _userManager.CheckPasswordAsync(user, password);
            if(result == false)
            {
                TempData["error"] = "Kullanıcı şifresi yanlış!";
                return View();
            }

            if (!user.EmailConfirmed)
            {
                TempData["error"] = "Mail adresiniz onaylanmamış! Mail adresinizi onaylamadan giriş yapamazsınız!";
                return View();
            }

            await _signInManager.SignInAsync(user, true);

            return RedirectToAction("Index", "Home");
        }

        result = await _userManager.CheckPasswordAsync(user, password);
        if (result == false)
        {
            TempData["error"] = "Kullanıcı şifresi yanlış!";
            return View();
        }

        if (!user.EmailConfirmed)
        {
            TempData["error"] = "Mail adresiniz onaylanmamış! Mail adresinizi onaylamadan giriş yapamazsınız!";
            return View();
        }

        await _signInManager.SignInAsync(user, true);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string userName, string name, string email, string password)
    {
        AppUser user = await _userManager.FindByNameAsync(userName);
        if(user != null)
        {
            TempData["error"] = "Bu kullanıcı adı daha önce kullanılmış!";
            return View();
        }
        user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            TempData["error"] = "Bu mail adresi daha önce kullanılmış!";
            return View();
        }

    again:;
        Random random = new Random();
        string emailConfirmValue = random.Next(100000, 999999).ToString();

        var checkEmailConfirmValue = await _userManager.Users.Where(p => p.EmailConfirmValue == emailConfirmValue).FirstOrDefaultAsync();

        if(checkEmailConfirmValue != null)
        {
            goto again;
        }

        user = new()
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Name = name,
            EmailConfirmValue = emailConfirmValue,
            UserName = userName,
        };
        await _userManager.CreateAsync(user, password);

        await CommonMethods.SendEmail(user, "ConfirmEmail");

        TempData["success"] = "Kullanıcınız oluşturuldu. Mail adresinizi onayladıktan sonra giriş yapabilirsiniz!";
        return RedirectToAction("Login");
    }

    public async Task<IActionResult> ConfirmEmailValue(string confirmValue)
    {
        AppUser user = await _userManager.Users.Where(p => p.EmailConfirmValue == confirmValue).FirstOrDefaultAsync();

        if(user == null)
        {
            TempData["error"] = "Kullanıcı bulunamadı!";
            return View();
        }

        if (user.EmailConfirmed)
        {
            TempData["error"] = "Kullanıcı maili daha önce onaylanmış!";
            return View();
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        TempData["success"] = "Kullanıcı maili başarıyla onaylandı. Giriş yapabilirsiniz!";
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult SendEmailConfirmValue()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendEmailConfirmValue(string emailOrUserName)
    {
        TempData["emailOrUserName"] = emailOrUserName;

        AppUser user = await _userManager.FindByEmailAsync(emailOrUserName);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(emailOrUserName);

            if (user == null)
            {
                TempData["error"] = "Kullanıcı adı ya da mail adresi yanlış!";
                return View();
            }
        }

        if(user.EmailConfirmed)
        {
            TempData["error"] = "Mail adresi zaten onaylı!";
            return View();
        }

        await CommonMethods.SendEmail(user, "ConfirmEmail");

        TempData["success"] = "Onay maili başarıyla gönderildi!";
        return RedirectToAction("Login");
    }

    public IActionResult SendEmailForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendEmailForgotPassword(string emailOrUserName)
    {
        TempData["emailOrUserName"] = emailOrUserName;

        AppUser user = await _userManager.FindByEmailAsync(emailOrUserName);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(emailOrUserName);

            if (user == null)
            {
                TempData["error"] = "Kullanıcı adı ya da mail adresi yanlış!";
                return View();
            }
        }

        TimeSpan timeSpan = DateTime.Now - Convert.ToDateTime(user.ForgotPasswordSendEmailDate);

        if (timeSpan.TotalMinutes < 5)
        {
            if (user.IsForgotPasswordConfirmValueActive)
            {
                TempData["error"] = "Şifremi unuttum maili 5 dakika da bir gönderebilir!";
                return View();
            }            
        }

    again:;
        Random random = new Random();
        string forgotPasswordConfirmValue = random.Next(100000, 999999).ToString();

        var checkForgotPasswordConfirmValue = await _userManager.Users.Where(p => p.ForgotPasswordConfirmValue == forgotPasswordConfirmValue).FirstOrDefaultAsync();

        if (checkForgotPasswordConfirmValue != null)
        {
            goto again;
        }

        user.ForgotPasswordConfirmValue = forgotPasswordConfirmValue;
        user.ForgotPasswordSendEmailDate = DateTime.Now;
        user.IsForgotPasswordConfirmValueActive = true;
        await _userManager.UpdateAsync(user);

        await CommonMethods.SendEmail(user, "ForgotPassword");
        
        return RedirectToAction("ChangePassword");
    }

    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string confirmValue, string password)
    {
        AppUser user = await _userManager.Users.Where(p=> p.ForgotPasswordConfirmValue == confirmValue).FirstOrDefaultAsync();

        if(user == null)
        {
            TempData["error"] = "Kullanıcı bulunamadı!";
            return View();
        }

        if (!user.IsForgotPasswordConfirmValueActive)
        {
            TempData["error"] = "Şifre değiştirme kodu geçersiz!";
            return View();
        }

        TimeSpan timeSpan = DateTime.Now - Convert.ToDateTime(user.ForgotPasswordSendEmailDate);
        if(timeSpan.TotalMinutes > 5)
        {
            TempData["error"] = "Şifre değiştirme kodu geçersiz!";
            return View();
        }             

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);       
        await _userManager.ResetPasswordAsync(user, token, password);


        user.IsForgotPasswordConfirmValueActive = false;
        await _userManager.UpdateAsync(user);

        TempData["success"] = "Şifreniz başarıyla değiştirildi. Giriş yapabilirsiniz!";
        return RedirectToAction("Login");

    }
}
