using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DynamicAuthorization.Mvc.Core.Extensions;

/// <summary>
/// Provides extension methods for configuring Entity Framework model builder.
/// </summary>
public static class EntityFrameworkModelBuilderExtensions
{
#if NET6
    private static JsonSerializerOptions s_DefaultSerializerOptions = new JsonSerializerOptions { WriteIndented = false };

    /// <summary>
    /// Configures the model builder to enable dynamic authorization for .NET 6.0.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    /// <param name="serializerOptions">Optional JSON serializer options.</param>
    /// <returns>The configured model builder.</returns>
    public static ModelBuilder EnableDynamicAuthorization(this ModelBuilder modelBuilder, JsonSerializerOptions? serializerOptions = null)
    {
        serializerOptions ??= s_DefaultSerializerOptions;

        modelBuilder.Entity<RoleAccess>(b =>
        {
            b.Property(ra => ra.RoleId).IsUnicode(false).IsRequired().HasMaxLength(128);
            b.Property(e => e.Controllers).HasConversion(
                v => JsonSerializer.Serialize(v, serializerOptions),
                v => JsonSerializer.Deserialize<IEnumerable<MvcControllerInfo>>(v, serializerOptions) ?? Enumerable.Empty<MvcControllerInfo>());
            b.HasOne<IdentityRole>().WithMany().HasForeignKey(r => r.RoleId);
        });

        return modelBuilder;
    }

#elif NET8

    /// <summary>
    /// Configures the model builder to enable dynamic authorization for .NET 8.0.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    /// <returns>The configured model builder.</returns>
    public static ModelBuilder EnableDynamicAuthorization(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleAccess>(b =>
        {
            b.Property(ra => ra.RoleId).IsUnicode(false).IsRequired().HasMaxLength(128);
            b.OwnsMany(ra => ra.Controllers, builder => { builder.ToJson(); });
            b.HasOne<IdentityRole>().WithMany().HasForeignKey(r => r.RoleId);
        });

        return modelBuilder;
    }

#endif
}
