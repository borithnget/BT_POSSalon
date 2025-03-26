using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using jotun.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace jotun.Models
{
    public class ApplicationUser : IdentityUser
    {
		public int? ShopId { get; set; } 
		public virtual tblShop Shop { get; set; }

		public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);          
            return userIdentity;
        }
    }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
		public virtual DbSet<tblShop> TblShops { get; set; }
		public virtual DbSet<tblShopModulePermission> TblShopModulePermissions { get; set; }

	/*	public virtual DbSet<tblModule> TblModules { get; set; }*/

		public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}