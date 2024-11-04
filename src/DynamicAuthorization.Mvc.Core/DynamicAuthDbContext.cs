using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DynamicAuthorization.Mvc.Core;

internal class DynamicAuthDbContext : IdentityDbContext
{
    public DynamicAuthDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityDbContext" /> class.
    /// </summary>
    protected DynamicAuthDbContext()
    { }
}

/// <summary>
///   Represents a dynamic authorization filter for a specific DbContext and User type.
/// </summary>
/// <typeparam name="TUser">The type of the User.</typeparam>
internal class DynamicAuthDbContext<TUser> : IdentityDbContext<TUser>
    where TUser : IdentityUser
{
    public DynamicAuthDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityDbContext" /> class.
    /// </summary>
    protected DynamicAuthDbContext()
    { }
}

/// <summary>
///   Represents a dynamic authorization filter for a specific DbContext, User, Role, and Key type.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
/// <typeparam name="TUser">The type of the User.</typeparam>
/// <typeparam name="TRole">The type of the Role.</typeparam>
/// <typeparam name="TKey">The type of the Key.</typeparam>
internal class DynamicAuthDbContext<TDbContext, TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    public DynamicAuthDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityDbContext" /> class.
    /// </summary>
    protected DynamicAuthDbContext()
    { }
}

/// <summary>
///   Represents a dynamic authorization filter for a specific DbContext, User, Role, Key,
///   UserClaim, UserRole, UserLogin, RoleClaim, and UserToken type.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
/// <typeparam name="TUser">The type of the User.</typeparam>
/// <typeparam name="TRole">The type of the Role.</typeparam>
/// <typeparam name="TKey">The type of the Key.</typeparam>
/// <typeparam name="TUserClaim">The type of the UserClaim.</typeparam>
/// <typeparam name="TUserRole">The type of the UserRole.</typeparam>
/// <typeparam name="TUserLogin">The type of the UserLogin.</typeparam>
/// <typeparam name="TRoleClaim">The type of the RoleClaim.</typeparam>
/// <typeparam name="TUserToken">The type of the UserToken.</typeparam>
internal class DynamicAuthDbContext<TDbContext, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
    : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
{
#if NET6
    private static JsonSerializerOptions s_DefaultSerializerOptions = new() { WriteIndented = false };
#endif

    public DynamicAuthDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    protected DynamicAuthDbContext()
    { }

    public virtual DbSet<RoleAccess> RoleAccesses { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
#if NET6
        modelBuilder.Entity<RoleAccess>(b =>
        {
            b.Property(ra => ra.RoleId).IsUnicode(false).IsRequired().HasMaxLength(128);
            b.Property(e => e.Controllers).HasConversion(
                v => JsonSerializer.Serialize(v, s_DefaultSerializerOptions),
                v => JsonSerializer.Deserialize<IEnumerable<MvcControllerInfo>>(v, s_DefaultSerializerOptions) ?? Enumerable.Empty<MvcControllerInfo>());
            b.HasOne<IdentityRole>().WithMany().HasForeignKey(r => r.RoleId);
        });
#elif NET8
        modelBuilder.Entity<RoleAccess>(b =>
        {
            b.Property(ra => ra.RoleId).IsUnicode(false).IsRequired().HasMaxLength(128);
            b.OwnsMany(ra => ra.Controllers, builder => { builder.ToJson(); });
            b.HasOne<IdentityRole>().WithMany().HasForeignKey(r => r.RoleId);
        });
#endif
    }
}
