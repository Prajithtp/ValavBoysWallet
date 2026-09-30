using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ValavBoysWallet.Data.Migrations;

public partial class AddProfilePhoto : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(name: "ProfilePhoto", table: "AspNetUsers",
            type: "varbinary(max)", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ProfilePhotoContentType", table: "AspNetUsers",
            type: "nvarchar(32)", maxLength: 32, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ProfilePhoto", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "ProfilePhotoContentType", table: "AspNetUsers");
    }
}
