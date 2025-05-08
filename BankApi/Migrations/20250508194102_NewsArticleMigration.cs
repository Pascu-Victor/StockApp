using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Migrations
{
    /// <inheritdoc />
    public partial class NewsArticleMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NEWS_ARTICLE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Source = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsWatchlistRelated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Category = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PublishedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NEWS_ARTICLE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CNP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashedPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsModerator = table.Column<bool>(type: "bit", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    GemBalance = table.Column<int>(type: "int", nullable: false),
                    NumberOfOffenses = table.Column<int>(type: "int", nullable: false),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    ROI = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditScore = table.Column<int>(type: "int", nullable: false),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: false),
                    ZodiacSign = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZodiacAttribute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfBillSharesPaid = table.Column<int>(type: "int", nullable: false),
                    Income = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "USER_ARTICLE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Topic = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Pending"),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PublishedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ARTICLE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_USER_ARTICLE_User_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NEWS_ARTICLE_STOCK",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    UserArticleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NEWS_ARTICLE_STOCK", x => new { x.ArticleId, x.StockId });
                    table.ForeignKey(
                        name: "FK_NEWS_ARTICLE_STOCK_BaseStocks_StockId",
                        column: x => x.StockId,
                        principalTable: "BaseStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NEWS_ARTICLE_STOCK_NEWS_ARTICLE_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "NEWS_ARTICLE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NEWS_ARTICLE_STOCK_USER_ARTICLE_UserArticleId",
                        column: x => x.UserArticleId,
                        principalTable: "USER_ARTICLE",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NEWS_ARTICLE_STOCK_StockId",
                table: "NEWS_ARTICLE_STOCK",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_NEWS_ARTICLE_STOCK_UserArticleId",
                table: "NEWS_ARTICLE_STOCK",
                column: "UserArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ARTICLE_AuthorId",
                table: "USER_ARTICLE",
                column: "AuthorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NEWS_ARTICLE_STOCK");

            migrationBuilder.DropTable(
                name: "NEWS_ARTICLE");

            migrationBuilder.DropTable(
                name: "USER_ARTICLE");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
