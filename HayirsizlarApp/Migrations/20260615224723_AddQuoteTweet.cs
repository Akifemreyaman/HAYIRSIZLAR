using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HayirsizlarApp.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteTweet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuoteTweetId",
                table: "Tweets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_QuoteTweetId",
                table: "Tweets",
                column: "QuoteTweetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tweets_Tweets_QuoteTweetId",
                table: "Tweets",
                column: "QuoteTweetId",
                principalTable: "Tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tweets_Tweets_QuoteTweetId",
                table: "Tweets");

            migrationBuilder.DropIndex(
                name: "IX_Tweets_QuoteTweetId",
                table: "Tweets");

            migrationBuilder.DropColumn(
                name: "QuoteTweetId",
                table: "Tweets");
        }
    }
}
