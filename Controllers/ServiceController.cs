using jotun.Entities;
using jotun.Models;
using jotun.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;


namespace jotun.Controllers
{
    public class ServiceController : Controller
    {
		private readonly PermissionService _permissionService;

		public ServiceController(PermissionService permissionService)
		{
			_permissionService = permissionService;
		}
		public ActionResult Index()
        {
			using (var db = new jotunDBEntities())
			{
				var services = db.tblServices
					.Where(s => s.IsActive)
					.Select(s => new CreateServiceViewModel
					{
						ServiceId = s.ServiceId,
						Name = s.Name,
						Price = s.Price,
						Description = s.Description,
						ServiceTypeName = s.tblServiceType.Name,
						CreatedAt = s.CreatedAt,
						Products = s.tblServiceProducts.Select(sp => new ServiceProductViewModel
						{
							ProductName = sp.tblProduct.ProductName,
							Quantity = sp.Quantity,
							Unit = sp.Unit,
						}).ToList()
					}).ToList();
				return View(services);  
			}
		}
		public ActionResult DetailService(Guid id)
		{
			using (var db = new jotunDBEntities())
			{
				var service = db.tblServices
					.Where(s => s.ServiceId == id)
					.Select(s => new CreateServiceViewModel
					{
						ServiceId = s.ServiceId,
						Name = s.Name,
						Price = s.Price,
						Description = s.Description,
						ServiceTypeId = s.ServiceTypeId,
						ServiceTypeName = s.tblServiceType.Name,
						CreatedAt = s.CreatedAt,
						Products = s.tblServiceProducts.Select(sp => new ServiceProductViewModel
						{
							ProductId = sp.ProductId,
							ProductName = sp.tblProduct.ProductName,
							Quantity = sp.Quantity,
							Unit = sp.Unit
						}).ToList(),
						ServiceTypes = db.tblServiceTypes
							.Where(st => st.IsActive)
							.Select(st => new SelectListItem
							{
								Value = st.ServiceTypeId.ToString(),
								Text = st.Name
							})
							.ToList()
					})
					.FirstOrDefault();

				if (service == null)
				{
					return HttpNotFound();
				}
				service.AvailableProducts = db.tblProducts
					.Where(p => p.CreatedDate.Value.Year == 2025)
					.Select(p => new SelectListItem
					{
						Value = p.Id.ToString(),
						Text = p.ProductName
					})
					.ToList();
				return View(service);
			}
		}
		private class FromBodyAttribute : Attribute
		{
		}
		[HttpPost]
		public JsonResult SaveServiceType([FromBody] tblServiceType model)
		{

			if (model != null && !string.IsNullOrEmpty(model.Name))
			{
				using (var db = new jotunDBEntities())
				{
					var newServiceType = new tblServiceType
					{
						ServiceTypeId = Guid.NewGuid(),
						Name = model.Name,
						Description = model.Description,
						IsActive = true,
						CreatedAt = DateTime.Now,
					};

					db.tblServiceTypes.Add(newServiceType);
					db.SaveChanges();
				}

				return Json(new { success = true, message = "Service type saved successfully." });
			}
			return Json(new { success = false, message = "Invalid data." });
		}
		public ActionResult CreateService()
		{
			using (var db = new jotunDBEntities())
			{
				var model = new CreateServiceViewModel
				{
					ServiceTypes = db.tblServiceTypes
						.Where(st => st.IsActive)
						.Select(st => new SelectListItem
						{
							Value = st.ServiceTypeId.ToString(),
							Text = st.Name
						})
						.ToList(),
					AvailableProducts = db.tblProducts
						.Where(p => p.CreatedDate.Value.Year == 2025)
						.Select(p => new SelectListItem
						{
							Value = p.Id.ToString(),
							Text = p.ProductName
						})
						.ToList(),
					Products = new List<ServiceProductViewModel> { new ServiceProductViewModel() }
				};
				return View(model);
			}
		}
		public ActionResult GetAvailableUnits(string productId)
		{
			using (var db = new jotunDBEntities())
			{
				var product = db.tblProducts
					.Where(p => p.Id.ToString() == productId)
					.FirstOrDefault();
				if (product != null)
				{
					var unit = db.tblUnits
						.Where(u => u.Id == product.Unitid)
						.Select(u => new SelectListItem
						{
							Value = u.Id.ToString(),
							Text = u.UnitNameKh
						})
						.ToList();
					return Json(unit, JsonRequestBehavior.AllowGet);
				}
				return Json(new List<SelectListItem>(), JsonRequestBehavior.AllowGet);
			}
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreateService(CreateServiceViewModel model)
		{
			if (!ModelState.IsValid)
			{
				using (var db = new jotunDBEntities())
				{
					model.ServiceTypes = db.tblServiceTypes
						.Where(st => st.IsActive)
						.Select(st => new SelectListItem
						{
							Value = st.ServiceTypeId.ToString(),
							Text = st.Name
						})
						.ToList();
				}
				return View(model);
			}
			using (var db = new jotunDBEntities())
			{
				var service = new tblService
				{
					ServiceId = Guid.NewGuid(),
					Name = model.Name,
					Price = model.Price,
					Description = model.Description,
					ServiceTypeId = model.ServiceTypeId,
					CreatedAt = DateTime.Now,
					IsActive = true,
				};
				db.tblServices.Add(service);
				db.SaveChanges();

				if (model.Products != null && model.Products.Any())
				{
					foreach (var product in model.Products)
					{
						var unitName = db.tblUnits
							.Where(u => u.Id == product.Unit)
							.Select(u => u.UnitNameKh)
							.FirstOrDefault();
						var serviceProduct = new tblServiceProduct
						{
							ServiceProductId = Guid.NewGuid(),
							ServiceId = service.ServiceId,
							ProductId = product.ProductId.ToString(),
							Unit = unitName,
							Quantity = product.Quantity,
						};

						db.tblServiceProducts.Add(serviceProduct);
					}
					db.SaveChanges();
				}
			}
			return RedirectToAction("Index");
		}
		public ActionResult EditService(Guid id)
		{
			using (var db = new jotunDBEntities())
			{
				var service = db.tblServices
					.Where(s => s.ServiceId == id)
					.Select(s => new CreateServiceViewModel
					{
						ServiceId = s.ServiceId,
						Name = s.Name,
						Price = s.Price,
						Description = s.Description,
						ServiceTypeId = s.ServiceTypeId,
						ServiceTypeName = s.tblServiceType.Name,
						CreatedAt = s.CreatedAt,
						Products = s.tblServiceProducts.Select(sp => new ServiceProductViewModel
						{
							ProductId = sp.ProductId,
							ProductName = sp.tblProduct.ProductName,
							Quantity = sp.Quantity,
							Quality = sp.Quality
						}).ToList(),
						ServiceTypes = db.tblServiceTypes
							.Where(st => st.IsActive)
							.Select(st => new SelectListItem
							{
								Value = st.ServiceTypeId.ToString(),
								Text = st.Name
							})
							.ToList()
					})
					.FirstOrDefault();

				if (service == null)
				{
					return HttpNotFound();
				}
				service.AvailableProducts = db.tblProducts
					.Where(p => p.CreatedDate.Value.Year == 2025)
					.Select(p => new SelectListItem
					{
						Value = p.Id.ToString(),
						Text = p.ProductName
					})
					.ToList();
				return View(service);
			}
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditService(CreateServiceViewModel model)
		{
			if (!ModelState.IsValid)
			{
				using (var db = new jotunDBEntities())
				{
					model.ServiceTypes = db.tblServiceTypes
						.Where(st => st.IsActive)
						.Select(st => new SelectListItem
						{
							Value = st.ServiceTypeId.ToString(),
							Text = st.Name
						})
						.ToList();

					model.AvailableProducts = db.tblProducts
						.Where(p => p.CreatedDate.Value.Year == 2025)
						.Select(p => new SelectListItem
						{
							Value = p.Id.ToString(),
							Text = p.ProductName
						})
						.ToList();
				}

				return View(model);
			}
			using (var db = new jotunDBEntities())
			{
				var service = db.tblServices
					.Include(s => s.tblServiceProducts)
					.FirstOrDefault(s => s.ServiceId == model.ServiceId);

				if (service == null)
				{
					return HttpNotFound();
				}
				service.Name = model.Name;
				service.Price = model.Price;
				service.Description = model.Description;
				service.ServiceTypeId = model.ServiceTypeId;
				var existingProductIds = service.tblServiceProducts.Select(sp => sp.ProductId).ToList();
				if (model.Products == null || !model.Products.Any())
				{
					db.tblServiceProducts.RemoveRange(service.tblServiceProducts);
				}
				else
				{
					foreach (var productId in existingProductIds)
					{
						if (!model.Products.Any(p => p.ProductId == productId))
						{
							var serviceProduct = db.tblServiceProducts
								.FirstOrDefault(sp => sp.ProductId == productId && sp.ServiceId == model.ServiceId);
							if (serviceProduct != null)
							{
								db.tblServiceProducts.Remove(serviceProduct);
							}
						}
					}
					foreach (var product in model.Products)
					{
						if (!string.IsNullOrEmpty(product.ProductId))
						{
							var existingProduct = service.tblServiceProducts
								.FirstOrDefault(sp => sp.ProductId == product.ProductId);

							if (existingProduct != null)
							{
								existingProduct.Quantity = product.Quantity;
								existingProduct.Quality = product.Quality;
							}
							else
							{								
								var serviceProduct = new tblServiceProduct
								{
									ServiceProductId = Guid.NewGuid(),
									ServiceId = service.ServiceId,
									ProductId = product.ProductId,
									Quantity = product.Quantity,
									Quality = product.Quality
								};
								db.tblServiceProducts.Add(serviceProduct);
							}
						}
					}
				}
				db.SaveChanges();
			}

			return RedirectToAction("Index");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteService(Guid id)
		{
			using (var db = new jotunDBEntities())
			{
				var service = db.tblServices
					.Include(s => s.tblServiceProducts)
					.FirstOrDefault(s => s.ServiceId == id);

				if (service == null)
				{
					return HttpNotFound();
				}
				db.tblServiceProducts.RemoveRange(service.tblServiceProducts);
				db.tblServices.Remove(service);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
		}
		[PermissionAuthorize(moduleId: 1, permissionTypes: "view")]
		public ActionResult CreatePackage()
		{
			var userId = User.Identity.GetUserId();
			var hasViewPermission = _permissionService.HasPermission(userId, 1, "view");
			ViewBag.HasViewPermission = hasViewPermission;
			using (var db = new jotunDBEntities())
			{
				var model = new CreatePackageViewModel
				{					
					AvailableServices = db.tblServices
						.Where(s => s.IsActive)
						.Select(s => new SelectListItem
						{
							Value = s.ServiceId.ToString(),
							Text = s.Name
						})
						.ToList(),					
					AvailableProducts = db.tblProducts
						.Where(p => p.CreatedDate.Value.Year == 2025)
						.Select(p => new SelectListItem
						{
							Value = p.Id.ToString(),
							Text = p.ProductName
						})
						.ToList(),					
					SelectedServices = new List<ServiceSelectionViewModel> { new ServiceSelectionViewModel() },
					SelectedProducts = new List<ProductSelectionViewModel> { new ProductSelectionViewModel() }
				};
				return View(model);
			}
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreatePackage(CreatePackageViewModel model)
		{
			if (!ModelState.IsValid)
			{
				var selectedServices = model.SelectedServices;
				var selectedProducts = model.SelectedProducts;
				using (var db = new jotunDBEntities())
				{
					model.AvailableServices = db.tblServices
						.Where(s => s.IsActive)
						.Select(s => new SelectListItem
						{
							Value = s.ServiceId.ToString(),
							Text = s.Name
						})
						.ToList();

					model.AvailableProducts = db.tblProducts
						.Where(p => p.CreatedDate.Value.Year == 2025)
						.Select(p => new SelectListItem
						{
							Value = p.Id.ToString(),
							Text = p.ProductName
						})
						.ToList();
				}
				return View(model);
			}

			using (var db = new jotunDBEntities())
			{
				var package = new Package
				{
					PackageName = model.Name,
					Price = model.Price,
					Description = model.Description,
					CreatedAt = DateTime.Now,
					/*CreatedBy = */
					Status = 1,
				};
				db.Packages.Add(package);
				db.SaveChanges();			
				if (model.SelectedServices != null && model.SelectedServices.Any())
				{
					foreach (var service in model.SelectedServices)
					{
						var packageService = new PackageService
						{
							PackageId = package.Id,
							ServiceId = service.Id,
							Quantity = service.Quantity,
						};
						db.PackageServices.Add(packageService);
					}
					db.SaveChanges();
				}				
				if (model.SelectedProducts != null && model.SelectedProducts.Any())
				{
					foreach (var product in model.SelectedProducts)
					{
						var packageProduct = new PackageProduct
						{
							PackageId = package.Id,
							ProductId = product.Id,
							Quantity = product.Quantity,
							Unit = product.Unit,
							Qty = product.Qty,
						};
						db.PackageProducts.Add(packageProduct);
					}
					db.SaveChanges();
				}
			}
			TempData["CreateSuccess"] = "Update Successful!";
			return RedirectToAction("ListPackage");
		}
		public ActionResult ListPackage()
		{
			using (var db = new jotunDBEntities())
			{
				var packages = db.Packages.Select(p => new PackageViewModel
				{
					Id = p.Id,
					PackageName = p.PackageName,
					Price = (decimal)p.Price,
					Description = p.Description,
					CreatedAt = (DateTime)p.CreatedAt,
					Status = (int)p.Status,
					Services = db.PackageServices
								.Where(ps => ps.PackageId == p.Id)
								.Select(ps => new ServiceModel
								{
									ServiceId = ps.ServiceId.ToString(),
									Name = ps.tblService.Name,
									/*Quantity = ps.Quantity*/
								}).ToList(),
					Products = db.PackageProducts
								.Where(pp => pp.PackageId == p.Id)
								.Select(pp => new ProductViewModels
								{
									Id = pp.ProductId.ToString(),
									ProductName = pp.tblProduct.ProductName,
									ProductCode = pp.tblProduct.ProductCode,
									ProductNo = pp.tblProduct.ProductNo,
									ProductImage = pp.tblProduct.ProductImage,
									QuantityInStock = pp.tblProduct.QuantityInStock.ToString(),
									UnitQuant = pp.Unit
								}).ToList()
				}).ToList();

				return View(packages);
			}
		}
		public ActionResult SearchPackage(string searchTerm = "")
		{
			using (var db = new jotunDBEntities())
			{
				var packagesQuery = db.Packages.AsQueryable();

				if (!string.IsNullOrEmpty(searchTerm))
				{
					packagesQuery = packagesQuery.Where(p => p.PackageName.Contains(searchTerm) ||
															  p.PackageServices.Any(ps => ps.tblService.Name.Contains(searchTerm)) ||
															  p.PackageProducts.Any(pp => pp.tblProduct.ProductName.Contains(searchTerm) ||
																						   pp.tblProduct.ProductCode.Contains(searchTerm)));
				}
				var packages = packagesQuery.Select(p => new PackageViewModel
				{
					Id = p.Id,
					PackageName = p.PackageName,
					Price = (decimal)p.Price,
					Description = p.Description,
					CreatedAt = (DateTime)p.CreatedAt,
					Status = (int)p.Status,
					Services = db.PackageServices
								.Where(ps => ps.PackageId == p.Id)
								.Select(ps => new ServiceModel
								{
									ServiceId = ps.ServiceId.ToString(),
									Name = ps.tblService.Name,
									/*Quantity = ps.Quantity*/
								}).ToList(),
					Products = db.PackageProducts
								.Where(pp => pp.PackageId == p.Id)
								.Select(pp => new ProductViewModels
								{
									Id = pp.ProductId.ToString(),
									ProductName = pp.tblProduct.ProductName,
									ProductCode = pp.tblProduct.ProductCode,
									ProductNo = pp.tblProduct.ProductNo,
									ProductImage = pp.tblProduct.ProductImage,
									QuantityInStock = pp.tblProduct.QuantityInStock.ToString(),
									UnitQuant = pp.Unit
								}).ToList()
				}).ToList();
				return View("ListPackage", packages);
			}
		}
		public ActionResult DetailPackage(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var package = db.Packages
					.Include(p => p.PackageServices.Select(s => s.tblService))
					.Include(p => p.PackageProducts.Select(e => e.tblProduct))
					.FirstOrDefault(p => p.Id == id);

				if (package == null)
				{
					return HttpNotFound();
				}

				var selectedServiceIds = package.PackageServices.Select(s => s.ServiceId).ToList();
				var selectedProductIds = package.PackageProducts.Select(p => p.ProductId).ToList();

				var model = new CreatePackageViewModel
				{
					Id = package.Id,
					Name = package.PackageName,
					Price = (decimal)package.Price,
					Description = package.Description,
					CreatedAt = (DateTime)package.CreatedAt,
					CreateBy = package.CreatedBy,
					// Available Services with pre-selection
					AvailableServices = db.tblServices
						.Where(s => s.IsActive)
						.Select(s => new SelectListItem
						{
							Value = s.ServiceId.ToString(),
							Text = s.Name,
							Selected = selectedServiceIds.Contains(s.ServiceId)
						})
						.ToList(),

					// Available Products with pre-selection
					AvailableProducts = db.tblProducts
						.Where(p => p.CreatedDate.HasValue && p.CreatedDate.Value.Year == 2025)
						.Select(p => new SelectListItem
						{
							Value = p.Id.ToString(),
							Text = p.ProductName,
							Selected = selectedProductIds.Contains(p.Id)
						})
						.ToList(),					
					SelectedServices = package.PackageServices?
						.Select(s => new ServiceSelectionViewModel
						{
							Id = s.ServiceId,
							Quantity = s.Quantity ?? 0
						}).ToList() ?? new List<ServiceSelectionViewModel>(),				
					SelectedProducts = package.PackageProducts?
						.Join(db.tblProducts, 
						p => p.ProductId,      
						prod => prod.Id,
						(p, prod) => new ProductSelectionViewModel
						{
							Id = p.ProductId,
							Quantity = p.Quantity ?? 0,
							Unit = p.Unit,
							Qty = p.Qty ?? 0,
							ProductImage = prod.ProductImage,
						})
						.ToList() ?? new List<ProductSelectionViewModel>()
				};
				return PartialView("_DetailPackage", model);
			}
		}
		// GET: EditPackage
		public ActionResult EditPackage(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var package = db.Packages
					.Include(p => p.PackageServices.Select(s => s.tblService))
					.Include(p => p.PackageProducts.Select(e => e.tblProduct))
					.FirstOrDefault(p => p.Id == id);

				if (package == null)
				{
					return HttpNotFound();
				}

				var selectedServiceIds = package.PackageServices.Select(s => s.ServiceId).ToList();
				var selectedProductIds = package.PackageProducts.Select(p => p.ProductId).ToList();

				var model = new CreatePackageViewModel
				{
					Id = package.Id,
					Name = package.PackageName,
					Price = (decimal)package.Price,
					Description = package.Description,

					// Available Services with pre-selection
					AvailableServices = db.tblServices
						.Where(s => s.IsActive)
						.Select(s => new SelectListItem
						{
							Value = s.ServiceId.ToString(),
							Text = s.Name,
							Selected = selectedServiceIds.Contains(s.ServiceId)
						})
						.ToList(),

					// Available Products with pre-selection
					AvailableProducts = db.tblProducts
						.Where(p => p.CreatedDate.HasValue && p.CreatedDate.Value.Year == 2025)
						.Select(p => new SelectListItem
						{
							Value = p.Id.ToString(),
							Text = p.ProductName,
							Selected = selectedProductIds.Contains(p.Id)
						})
						.ToList(),

					// Selected Services mapped to ViewModel
					SelectedServices = package.PackageServices?
						.Select(s => new ServiceSelectionViewModel
						{
							Id = s.ServiceId,
							Quantity = s.Quantity ?? 0
						}).ToList() ?? new List<ServiceSelectionViewModel>(),

					// Selected Products mapped to ViewModel
					SelectedProducts = package.PackageProducts?
						.Select(p => new ProductSelectionViewModel
						{
							Id = p.ProductId,
							Quantity = p.Quantity ?? 0,
							Unit = p.Unit,
							Qty = p.Qty ?? 0
						}).ToList() ?? new List<ProductSelectionViewModel>()
				};
				return View(model);
			}
		}
		[HttpPost]	
		[ValidateAntiForgeryToken]
		public ActionResult EditPackage(CreatePackageViewModel model)
		{
			if (!ModelState.IsValid)
			{
				using (var db = new jotunDBEntities())
				{
					model.AvailableServices = db.tblServices
						.Where(s => s.IsActive)
						.Select(s => new SelectListItem
						{
							Value = s.ServiceId.ToString(),
							Text = s.Name
						})
						.ToList();

					model.AvailableProducts = db.tblProducts
						.Where(p => p.CreatedDate.Value.Year == 2025)
						.Select(p => new SelectListItem
						{
							Value = p.Id.ToString(),
							Text = p.ProductName
						})
						.ToList();
				}
				return View(model);
			}

			using (var db = new jotunDBEntities())
			{
				var package = db.Packages.FirstOrDefault(p => p.Id == model.Id);
				if (package == null)
				{
					return HttpNotFound();
				}

				package.PackageName = model.Name;
				package.Price = model.Price;
				package.Description = model.Description;
				package.UpdateDate = DateTime.Now;
				package.CreatedBy = User.Identity.Name;
				db.SaveChanges();

				// Update selected services
				var existingPackageServices = db.PackageServices.Where(ps => ps.PackageId == package.Id).ToList();
				db.PackageServices.RemoveRange(existingPackageServices);
				db.SaveChanges();

				if (model.SelectedServices != null && model.SelectedServices.Any())
				{
					foreach (var service in model.SelectedServices)
					{
						var packageService = new PackageService
						{
							PackageId = package.Id,
							ServiceId = service.Id,
							Quantity = service.Quantity,
						};
						db.PackageServices.Add(packageService);
					}
					db.SaveChanges();
				}				
				var existingPackageProducts = db.PackageProducts.Where(pp => pp.PackageId == package.Id).ToList();
				db.PackageProducts.RemoveRange(existingPackageProducts);
				db.SaveChanges();

				if (model.SelectedProducts != null && model.SelectedProducts.Any())
				{
					foreach (var product in model.SelectedProducts)
					{
						var packageProduct = new PackageProduct
						{
							PackageId = package.Id,
							ProductId = product.Id,
							Quantity = product.Quantity,
							Unit = product.Unit,
							Qty = product.Qty,
						};
						db.PackageProducts.Add(packageProduct);
					}
					db.SaveChanges();
				}
			}
			TempData["UpdateSuccess"] = "Update Successful!";
			return RedirectToAction("ListPackage");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeletePackage(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var package = db.Packages.FirstOrDefault(p => p.Id == id);
				if (package == null)
				{
					return HttpNotFound();
				}		
				var packageServices = db.PackageServices.Where(ps => ps.PackageId == id).ToList();
				db.PackageServices.RemoveRange(packageServices);		
				var packageProducts = db.PackageProducts.Where(pp => pp.PackageId == id).ToList();
				db.PackageProducts.RemoveRange(packageProducts);
				db.Packages.Remove(package);
				db.SaveChanges();
			}
			TempData["DeleteSuccess"] = "Delete Successful!";
			return RedirectToAction("ListPackage");
		}


	}
}
