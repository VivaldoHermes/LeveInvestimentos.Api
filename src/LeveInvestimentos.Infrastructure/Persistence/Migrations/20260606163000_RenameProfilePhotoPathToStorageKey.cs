using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeveInvestimentos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameProfilePhotoPathToStorageKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePhotoPath",
                table: "AspNetUsers",
                newName: "ProfilePhotoStorageKey");

            migrationBuilder.Sql(
                """
                UPDATE AspNetUsers
                SET ProfilePhotoStorageKey = SUBSTRING(ProfilePhotoStorageKey, LEN('/uploads/') + 1, LEN(ProfilePhotoStorageKey))
                WHERE ProfilePhotoStorageKey LIKE '/uploads/%'
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE AspNetUsers
                SET ProfilePhotoStorageKey = '/uploads/' + ProfilePhotoStorageKey
                WHERE ProfilePhotoStorageKey <> ''
                  AND ProfilePhotoStorageKey NOT LIKE '/%'
                """);

            migrationBuilder.RenameColumn(
                name: "ProfilePhotoStorageKey",
                table: "AspNetUsers",
                newName: "ProfilePhotoPath");
        }
    }
}
