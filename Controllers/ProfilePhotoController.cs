using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValavBoysWallet.Data;
using ValavBoysWallet.Models;

namespace ValavBoysWallet.Controllers;

[Authorize]
public class ProfilePhotoController(ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index()
    {
        var id = userManager.GetUserId(User);
        var photo = await context.Users.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.ProfilePhoto, x.ProfilePhotoContentType })
            .SingleOrDefaultAsync();
        if (photo?.ProfilePhoto == null ||
            photo.ProfilePhotoContentType is not ("image/jpeg" or "image/png"))
            return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(photo.ProfilePhoto, photo.ProfilePhotoContentType);
    }
}
