using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Kontructo.Data;
using Kontructo.Models;
using Kontructo.Logic;
using Kontructo.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kontructo.Controllers
{
    [Authorize(Roles = Roles.Master + "," + Roles.SuperAdmin + "," + Roles.Admin)]
    public class AdminController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<AdminController> logger;

        public AdminController(ILogger<AdminController> _logger,ApplicationDbContext _db, 
            UserManager<AppUser> _userManager, RoleManager<IdentityRole> _roleManager, 
            SignInManager<AppUser> _signInManager)
        {
            db = _db;
            userManager = _userManager;
            roleManager = _roleManager;
            signInManager = _signInManager;
            logger = _logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Product

        [Authorize(Roles = Roles.Master)]
        [HttpGet]
        public async Task<IActionResult> MasterProductList()
        {
            var product = db.Products.Include(pd => pd.ProductType);
            return View(await product.ToListAsync());
        }

        [Authorize(Roles = Roles.Master + "," + Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpGet]
        public async Task<IActionResult> UserProductList(AppUser user)
        {
            user = await userManager.GetUserAsync(User);
            var userId = user.UserName;
            var product = db.Products.Where(pd => pd.Vendor.Contains(userId))
                                                    .Include(pd => pd.ProductType);
            return View(await product.ToListAsync());
        }

        [Authorize(Roles = Roles.Master + "," + Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpGet]
        public IActionResult CreateProduct()
        {
            ViewBag.returnUrl = Request.Headers["Referer"].ToString();
            ViewData["ProductTypeId"] = new SelectList(db.ProductTypes, "Id", "Tag");
            return View();
        }

        [Authorize(Roles = Roles.Master + "," + Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Request.Form.Files.Count > 0)
                {
                    byte[]? imageData = null;
                    foreach (var imagefile in Request.Form.Files)
                    {
                        MemoryStream ms = new MemoryStream();
                        imagefile.CopyTo(ms);
                        if (ms.Length < 512000)
                        {
                            imageData = ms.ToArray();
                            product.ProductImage = imageData;
                        }
                        else
                        {
                            ModelState.AddModelError("File", "The file is too large.");
                            return View(product);
                        }
                    }
                }
                product.Vendor = userManager.GetUserName(this.User);
                db.Add(product);
                await db.SaveChangesAsync();
                return Redirect(returnUrl);
            }
            ViewBag.returnUrl = Request.Headers["Referer"].ToString();
            ViewData["ProductTypeId"] = new SelectList(db.ProductTypes, "Id", "Tag", product.ProductTypeId);
            return View(product);
        }

        [Authorize(Roles = Roles.Master + "," + Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpGet]
        public IActionResult EditProduct(int? id)
        {
            ViewBag.returnUrl = Request.Headers["Referer"].ToString();
            var product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            string imageBase64Data = Convert.ToBase64String(product.ProductImage);
            string imageDataUrl = string.Format("data:image/jpg;base64,{0}", imageBase64Data);
            ViewBag.ImageDataUrl = imageDataUrl;
            ViewData["ProductTypeId"] = new SelectList(db.ProductTypes, "Id", "Tag", product.ProductTypeId);
            return View(product);
        }

        [Authorize(Roles = Roles.Master + "," + Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> EditPost(int? id, string returnUrl)
        {
            var product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            if (await TryUpdateModelAsync<Product>(product, "", p => p.Name,  p => p.Description, p => p.ProductTypeId,
                           p => p.Price, p => p.Url))
            {
                try
                {
                    if (Request.Form.Files.Count > 0)
                    {
                        var file = Request.Form.Files.FirstOrDefault();
                        using (var dataStream = new MemoryStream())
                        {
                            await file.CopyToAsync(dataStream);
                            if (dataStream.Length < 512000)
                            {
                                product.ProductImage = dataStream.ToArray();
                            }
                            else
                            {
                                ModelState.AddModelError("", "The picture file is too large");
                                return View(product);
                            }
                        }
                    }
                    await db.SaveChangesAsync();
                    return Redirect(returnUrl);
                }
                catch (DbUpdateException)
                {

                }
            }
            ViewBag.returnUrl = Request.Headers["Referer"].ToString();
            string imageBase64Data = Convert.ToBase64String(product.ProductImage);
            string imageDataUrl = string.Format("data:image/jpg;base64,{0}", imageBase64Data);
            ViewBag.ImageDataUrl = imageDataUrl;
            ViewData["ProductTypeId"] = new SelectList(db.ProductTypes, "Id", "Tag", product.ProductTypeId);
            return View(product);
        }


        [Authorize(Roles = Roles.Master)]
        [HttpPost]
        public async Task<IActionResult> MasterDelete(int? id)
        {
            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(MasterProductList));
        }

        [Authorize(Roles = Roles.Master + "," + Roles.SuperAdmin + "," + Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(UserProductList));
        }

        #endregion

        #region ProductType

        [Authorize(Roles = Roles.Master)]
        public IActionResult TagList()
        {
            return View(db.ProductTypes.ToList());
        }

        [Authorize(Roles = Roles.Master)]
        public IActionResult CreateTag()
        {
            return View();
        }

        [Authorize(Roles = Roles.Master)]
        [HttpPost]
        public IActionResult CreateTag(ProductType ptype)
        {
            if (ModelState.IsValid)
            {
                db.ProductTypes.Add(ptype);
                db.SaveChanges();
                return RedirectToAction(nameof(TagList));
            }
            return View(ptype);
        }

        [Authorize(Roles = Roles.Master)]
        [HttpGet]
        public IActionResult EditTag(int? id)
        {
            var ptype = db.ProductTypes.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return View(ptype);
        }

        [Authorize(Roles = Roles.Master)]
        [HttpPost]
        public IActionResult EditTag(ProductType ptype)
        {
            if (ModelState.IsValid)
            {
                db.Update(ptype);
                db.SaveChanges();
                return RedirectToAction(nameof(TagList));
            }
            return View(ptype);
        }

        [Authorize(Roles = Roles.Master)]
        [HttpPost]
        public IActionResult DeleteTag(int? id)
        {
            var ptype = db.ProductTypes.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            db.ProductTypes.Remove(ptype);
            db.SaveChanges();
            return RedirectToAction(nameof(TagList));
        }

        #endregion 

        #region AppUsers

        [Authorize(Roles = Roles.Master)]
        public IActionResult UsersList()
        {
            var users = userManager.Users.ToList();
            return View(users);
        }

        [Authorize(Roles = Roles.Master)]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
            }
            IdentityResult result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{id}'.");
            }

            logger.LogInformation("User with ID '{UserId}' was deleted.", id);

            return RedirectToAction(nameof(UsersList));
        }

        #endregion

        #region Roles

        [Authorize(Roles = Roles.Master)]
        public async Task<IActionResult> UserRoles()
        {
            var users = await userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (AppUser user in users)
            {
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.UserName = user.UserName;
                thisViewModel.Roles = await GetUserRoles(user);
                userRolesViewModel.Add(thisViewModel);
            }
            return View(userRolesViewModel);
        }

        [Authorize(Roles = Roles.Master)]
        [HttpGet]
        public async Task<IActionResult> ManageRoles(string userId)
        {
            ViewBag.userId = userId;
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }

        [Authorize(Roles = Roles.Master)]
        [HttpPost]
        public async Task<IActionResult> ManageRoles(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction(nameof(UserRoles));
        }

        private async Task<List<string>> GetUserRoles(AppUser user)
        {
            return new List<string>(await userManager.GetRolesAsync(user));
        }

        #endregion
    }
}
