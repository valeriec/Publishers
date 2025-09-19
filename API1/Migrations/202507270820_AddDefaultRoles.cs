using Microsoft.EntityFrameworkCore.Migrations;

namespace API1.Migrations
{
    public partial class AddDefaultRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "AspNetRoles",
        columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
        values: new object[,]
        {
            { "c7d8d5e3-8b8a-4b7c-9d4e-111111111111", "Admin", "ADMIN", System.Guid.NewGuid().ToString() },
            { "d2f9b8e3-9a9a-4c7c-8d8e-222222222222", "User", "USER", System.Guid.NewGuid().ToString() }
        }
    );
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DeleteData(
        table: "AspNetRoles",
        keyColumn: "Id",
        keyValues: new object[] { "c7d8d5e3-8b8a-4b7c-9d4e-111111111111", "d2f9b8e3-9a9a-4c7c-8d8e-222222222222" }
    );
}
    }
}
