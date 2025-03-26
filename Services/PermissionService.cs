using System.Linq;
using jotun.Entities;
using jotun.Models;
public class PermissionService
{
	private readonly jotunDBEntities _dbContext;
	private readonly ApplicationDbContext _identityContext;

	public PermissionService(jotunDBEntities dbContext, ApplicationDbContext identityContext)
	{
		_dbContext = dbContext;
		_identityContext = identityContext;
	}

	public bool HasPermission(string userId, int moduleId, string permissionType)
	{
		var user = _identityContext.Users.FirstOrDefault(u => u.Id == userId);
		if (user == null) return false;


		var permission = _dbContext.tblShopModulePermissions
			 .FirstOrDefault(p => p.ShopId == user.ShopId && p.ModuleId == moduleId);

		if (permission == null) return false;
		switch (permissionType.ToLower())
		{
			case "view":
				return permission.CanView == true;
			case "edit":
				return permission.CanEdit == true;
			case "delete":
				return permission.CanDelete == true;
			default:
				return false;
		}
	}
}



