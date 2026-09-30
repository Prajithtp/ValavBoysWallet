using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ValavBoysWallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletLastResetAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastResetAt",
                table: "GroupWallets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastResetAt",
                table: "GroupWallets");
        }
    }
}
