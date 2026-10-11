using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SafeTransfer_API.Models;

namespace SafeTransfer_API.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options);