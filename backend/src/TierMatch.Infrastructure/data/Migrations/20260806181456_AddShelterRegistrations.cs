using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TierMatch.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShelterRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShelterRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShelterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HouseNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, defaultValue: "DE"),
                    ShelterPhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ShelterEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ContactFirstName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactLastName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContactPhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RejectionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShelterId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShelterRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShelterRegistrations_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShelterRegistrations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShelterRegistrations_Shelters_ShelterId",
                        column: x => x.ShelterId,
                        principalTable: "Shelters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShelterRegistrations_ContactEmail",
                table: "ShelterRegistrations",
                column: "ContactEmail");

            migrationBuilder.CreateIndex(
                name: "IX_ShelterRegistrations_CreatedAt",
                table: "ShelterRegistrations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ShelterRegistrations_ReviewedByUserId",
                table: "ShelterRegistrations",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelterRegistrations_ShelterEmail",
                table: "ShelterRegistrations",
                column: "ShelterEmail");

            migrationBuilder.CreateIndex(
                name: "IX_ShelterRegistrations_ShelterId",
                table: "ShelterRegistrations",
                column: "ShelterId");

            migrationBuilder.CreateIndex(
                name: "IX_ShelterRegistrations_Status",
                table: "ShelterRegistrations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ShelterRegistrations_UserId",
                table: "ShelterRegistrations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShelterRegistrations");
        }
    }
}
