using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ValavBoysWallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBalance",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "GroupWallets",
                columns: table => new
                {
                    WalletId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupWallets", x => x.WalletId);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScreenshotPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ApprovedByUserId",
                table: "WalletTransactions",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CreatedDate",
                table: "WalletTransactions",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_Status",
                table: "WalletTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_UserId",
                table: "WalletTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupWallets");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "CurrentBalance",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");
        }
    }
}
