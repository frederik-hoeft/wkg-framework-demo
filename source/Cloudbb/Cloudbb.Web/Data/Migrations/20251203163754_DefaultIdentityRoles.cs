using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cloudbb.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class DefaultIdentityRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: ["Id", "ConcurrencyStamp", "Name", "NormalizedName"],
                values: new object[,]
                {
                    { "019ae512-ae98-7973-8de5-7e654298ec4b", "828439b8-c96e-48c1-860d-62076f394c76", "admin", "ADMIN" },
                    { "019ae512-d047-7708-b68e-a0e24a6ced56", "bc86bddf-c568-4f39-91a2-330595964e90", "user", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019ae512-ae98-7973-8de5-7e654298ec4b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019ae512-d047-7708-b68e-a0e24a6ced56");
        }
    }
}
