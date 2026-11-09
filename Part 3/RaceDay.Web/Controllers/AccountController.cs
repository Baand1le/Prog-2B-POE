using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiService _api;
    private readonly LocalImageService _images;

    public AccountController(ApiService api, LocalImageService images)
    {
        _api = api;
        _images = images;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId")))
            return RedirectToRoleHome();
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.LoginAsync(new LoginRequest
        {
            Email = model.Email,
            Password = model.Password
        });

        if (!result.IsSuccess || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        HttpContext.Session.SetString("UserId", result.Data.UserID);
        HttpContext.Session.SetString("UserRole", result.Data.Role);
        HttpContext.Session.SetString("UserEmail", model.Email);

        var profile = await _api.GetAsync<ApiProfile>("api/users/me");
        if (profile.IsSuccess && profile.Data != null)
        {
            HttpContext.Session.SetString("UserName", profile.Data.FirstName + " " + profile.Data.Surname);
            HttpContext.Session.SetString("UserEmail", profile.Data.Email);
        }
        else
        {
            HttpContext.Session.SetString("UserName", model.Email.Split('@')[0]);
        }

        TempData["Success"] = "Welcome back to RaceDay.";
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToRoleHome();
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId")))
            return RedirectToRoleHome();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.RoleType is not ("Participant" or "Organiser"))
            ModelState.AddModelError(nameof(model.RoleType), "Choose a valid account type.");

        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PostAsync<RegisterRequest, ApiMessage>("api/auth/register", new RegisterRequest
        {
            FirstName = model.FirstName.Trim(),
            Surname = model.Surname.Trim(),
            Email = model.Email.Trim(),
            Password = model.Password,
            RoleType = model.RoleType
        });

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = "Your RaceDay account has been created. You can now sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [RequireSession]
    public async Task<IActionResult> Logout()
    {
        await _api.LogoutAsync();
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been signed out.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [RequireSession]
    public async Task<IActionResult> Profile()
    {
        var result = await _api.GetAsync<ApiProfile>("api/users/me");
        if (!result.IsSuccess || result.Data == null)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction("Index", "Home");
        }

        var userId = HttpContext.Session.GetString("UserId")!;
        var model = new ProfileViewModel
        {
            FirstName = result.Data.FirstName,
            Surname = result.Data.Surname,
            Email = result.Data.Email,
            Role = result.Data.Role,
            ProfileImageUrl = _images.Find("profiles", userId)
        };
        return View(model);
    }

    [HttpPost]
    [RequireSession]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        ModelState.Remove(nameof(model.ProfileImageUrl));
        ModelState.Remove(nameof(model.Role));
        if (!ModelState.IsValid)
        {
            model.ProfileImageUrl = _images.Find("profiles", HttpContext.Session.GetString("UserId")!);
            model.Role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            return View(model);
        }

        var update = await _api.PutAsync<UpdateProfileRequest, ApiMessage>("api/users/me", new UpdateProfileRequest
        {
            FirstName = model.FirstName.Trim(),
            Surname = model.Surname.Trim(),
            Email = model.Email.Trim()
        });

        if (!update.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, update.Message);
            model.ProfileImageUrl = _images.Find("profiles", HttpContext.Session.GetString("UserId")!);
            model.Role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            return View(model);
        }

        var userId = HttpContext.Session.GetString("UserId")!;
        try
        {
            if (model.ProfilePicture != null)
                model.ProfileImageUrl = await _images.SaveAsync(model.ProfilePicture, "profiles", userId);
            else
                model.ProfileImageUrl = _images.Find("profiles", userId);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ProfilePicture), ex.Message);
            model.ProfileImageUrl = _images.Find("profiles", userId);
            model.Role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            return View(model);
        }

        HttpContext.Session.SetString("UserName", model.FirstName + " " + model.Surname);
        HttpContext.Session.SetString("UserEmail", model.Email);
        model.Role = HttpContext.Session.GetString("UserRole") ?? string.Empty;
        TempData["Success"] = "Your profile has been updated.";
        return View(model);
    }

    private IActionResult RedirectToRoleHome()
    {
        return string.Equals(HttpContext.Session.GetString("UserRole"), "Organiser", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "Organiser")
            : RedirectToAction("Index", "Participant");
    }
}
