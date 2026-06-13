using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HayirsizlarApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTweetRepliesAndRemoveLengthLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Tweets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<int>(
                name: "ParentTweetId",
                table: "Tweets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_ParentTweetId",
                table: "Tweets",
                column: "ParentTweetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tweets_Tweets_ParentTweetId",
                table: "Tweets",
                column: "ParentTweetId",
                principalTable: "Tweets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tweets_Tweets_ParentTweetId",
                table: "Tweets");

            migrationBuilder.DropIndex(
                name: "IX_Tweets_ParentTweetId",
                table: "Tweets");

            migrationBuilder.DropColumn(
                name: "ParentTweetId",
                table: "Tweets");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Tweets",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
