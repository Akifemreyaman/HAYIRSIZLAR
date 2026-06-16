using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HayirsizlarApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTweetLikeDislike : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TweetLikeDislikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TweetId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsLike = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetLikeDislikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TweetLikeDislikes_Tweets_TweetId",
                        column: x => x.TweetId,
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TweetLikeDislikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TweetLikeDislikes_TweetId",
                table: "TweetLikeDislikes",
                column: "TweetId");

            migrationBuilder.CreateIndex(
                name: "IX_TweetLikeDislikes_UserId_TweetId",
                table: "TweetLikeDislikes",
                columns: new[] { "UserId", "TweetId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TweetLikeDislikes");
        }
    }
}
