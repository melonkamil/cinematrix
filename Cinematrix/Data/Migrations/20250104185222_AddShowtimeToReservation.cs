using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinematrix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShowtimeToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ShowtimeId",
                table: "Reservations",
                column: "ShowtimeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Showtimes_ShowtimeId",
                table: "Reservations",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Showtimes_ShowtimeId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ShowtimeId",
                table: "Reservations");
        }
    }
}
