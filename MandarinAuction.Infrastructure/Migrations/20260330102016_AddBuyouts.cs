using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MandarinAuction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBuyouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BuyoutPrice",
                table: "Auctions",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyoutPrice",
                table: "Auctions");
        }
    }
}
