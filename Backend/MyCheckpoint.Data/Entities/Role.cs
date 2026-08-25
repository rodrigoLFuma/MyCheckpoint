using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Entities
{
    public class Role : IdentityRole<int>, IEntity
    {
    }
}
