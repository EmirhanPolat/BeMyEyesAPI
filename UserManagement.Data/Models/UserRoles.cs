﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class UserRoles : IdentityRole<Guid>
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}