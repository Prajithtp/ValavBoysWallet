using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ValavBoysWallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletTransactionRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WalletTransactions",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WalletTransactions");
        }
    }
}
