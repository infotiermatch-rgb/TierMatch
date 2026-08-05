using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TierMatch.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkAdoptionRequestsToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AdoptionRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionRequests_UserId",
                table: "AdoptionRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionRequests_UserId_AnimalId_Status",
                table: "AdoptionRequests",
                columns: new[] { "UserId", "AnimalId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdoptionRequests_UserId",
                table: "AdoptionRequests");

            migrationBuilder.DropIndex(
                name: "IX_AdoptionRequests_UserId_AnimalId_Status",
                table: "AdoptionRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AdoptionRequests");
        }
    }
}
