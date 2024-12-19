using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace music_manager_start.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Artist = table.Column<string>(type: "TEXT", nullable: false),
                    Album = table.Column<string>(type: "TEXT", nullable: false),
                    Genre = table.Column<string>(type: "TEXT", nullable: false),
                    AlbumArtUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Songs",
                columns: new[] { "Id", "Album", "AlbumArtUrl", "Artist", "Genre", "Title" },
                values: new object[,]
                {
                    { new Guid("22aa6f84-06d8-4a0e-bdad-3000b35b5b5f"), "Twelve Carat Toothache", null, "Post Malone", "Hip Hop", "Something Real" },
                    { new Guid("2a76a0b1-b3e1-4ff0-9aa5-5f5e4c81bc45"), "Canyon", null, "Pony Bradshaw", "Folk", "Notes on a River Town" },
                    { new Guid("42e4b4d5-93bb-4e46-bb6e-c57de62e7f6e"), "When We All Fall Asleep, Where Do We Go?", null, "Billie Eilish", "Pop", "When the Party's Over" },
                    { new Guid("6f47c84f-4a7d-4e83-8b8f-1829f0eafca3"), "Spiritbox", null, "Spiritbox", "Metal", "Circle With Me" },
                    { new Guid("b7cc1c82-77e2-40d0-8bc2-d7e05962c0e3"), "The Great Escape", null, "French Cassettes", "Indie", "Utah" },
                    { new Guid("d94aa1d4-75ee-4f7a-a89f-f77de7050c8d"), "I Love You.", null, "The Neighbourhood", "Alternative", "Sweater Weather" },
                    { new Guid("fa38a0ed-4f00-48e2-b9c5-5d68f9c0ef41"), "Single", null, "Morgan Allen", "Country", "Flower Shops" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Songs");
        }
    }
}
