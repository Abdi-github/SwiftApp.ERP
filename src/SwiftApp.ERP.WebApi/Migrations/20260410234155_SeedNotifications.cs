using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwiftApp.ERP.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Notification seeding moved to runtime NotificationSeedData.SeedAsync
            // to use the actual admin user ID instead of a hardcoded one.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
