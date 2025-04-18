﻿using jotun.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(jotun.Startup))]
namespace jotun
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers();

        }
        // In this method we will create default User roles and Admin user for login   
        private void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("SuperAdmin"))
            {

                // first we create Admin rool   
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "SuperAdmin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "bt-jotun";
                user.Email = "bt-jotun@gmail.com";

                string userPWD = "@Btjotun1234";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "SuperAdmin");

                }
            }
            
            //else
            //{
            //    var user = new ApplicationUser();
            //    user.UserName = "btborith";
            //    user.Email = "borith@gmail.com";

            //    string userPWD = "P@$$w0rd";

            //    var chkUser = UserManager.Create(user, userPWD);

            //    //Add default User to Role Admin   
            //    if (chkUser.Succeeded)
            //    {
            //        var result1 = UserManager.AddToRole(user.Id, "SuperAdmin");

            //    }
            //}

            // creating Creating Manager role    
            if (!roleManager.RoleExists("Sales"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Sales";
                roleManager.Create(role);

            }
       
           

            // creating Creating Employee role    
            if (!roleManager.RoleExists("Administrator"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Administrator";
                roleManager.Create(role);

            }

            if (!roleManager.RoleExists("SuperAdmin"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "SuperAdmin";
                roleManager.Create(role);

            }

        }
    }
}
