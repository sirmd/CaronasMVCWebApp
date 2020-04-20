﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CaronasMVCWebApp.Models;

namespace CaronasMVCWebApp.Data
{
    public class CaronasMVCWebAppContext : DbContext
    {
        public CaronasMVCWebAppContext (DbContextOptions<CaronasMVCWebAppContext> options)
            : base(options)
        {
        }

        public DbSet<CaronasMVCWebApp.Models.Members> Members { get; set; }
    }
}
