using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PrintPlatform.Gamification.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gam_achievements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IconSlug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CriteriaJson = table.Column<string>(type: "jsonb", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BonusXP = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gam_challenges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CriteriaJson = table.Column<string>(type: "jsonb", nullable: false),
                    RewardXP = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gam_leaderboards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Period = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_leaderboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gam_levels",
                columns: table => new
                {
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MinXP = table.Column<int>(type: "integer", nullable: false),
                    MaxXP = table.Column<int>(type: "integer", nullable: false),
                    BenefitsJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_levels", x => x.Tier);
                });

            migrationBuilder.CreateTable(
                name: "gam_players",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalXP = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CurrentLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastActivityAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gam_leaderboard_entries",
                columns: table => new
                {
                    LeaderboardId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_leaderboard_entries", x => new { x.LeaderboardId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_gam_leaderboard_entries_gam_leaderboards_LeaderboardId",
                        column: x => x.LeaderboardId,
                        principalTable: "gam_leaderboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gam_leaderboard_entries_gam_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "gam_players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gam_player_achievements",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    AchievementId = table.Column<long>(type: "bigint", nullable: false),
                    UnlockedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_player_achievements", x => new { x.PlayerId, x.AchievementId });
                    table.ForeignKey(
                        name: "FK_gam_player_achievements_gam_achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "gam_achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gam_player_achievements_gam_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "gam_players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gam_player_challenges",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    ChallengeId = table.Column<long>(type: "bigint", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_player_challenges", x => new { x.PlayerId, x.ChallengeId });
                    table.ForeignKey(
                        name: "FK_gam_player_challenges_gam_challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "gam_challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gam_player_challenges_gam_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "gam_players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gam_point_transactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_point_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gam_point_transactions_gam_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "gam_players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gam_streaks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CurrentCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BestCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastActionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gam_streaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gam_streaks_gam_players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "gam_players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gam_achievements_Category",
                table: "gam_achievements",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_gam_challenges_IsActive",
                table: "gam_challenges",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_gam_challenges_StartDate_EndDate",
                table: "gam_challenges",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_gam_leaderboard_entries_LeaderboardId_Rank",
                table: "gam_leaderboard_entries",
                columns: new[] { "LeaderboardId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_gam_leaderboard_entries_PlayerId",
                table: "gam_leaderboard_entries",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_gam_leaderboards_Period_Type",
                table: "gam_leaderboards",
                columns: new[] { "Period", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_gam_player_achievements_AchievementId",
                table: "gam_player_achievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_gam_player_challenges_ChallengeId",
                table: "gam_player_challenges",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_gam_players_ExternalUserId",
                table: "gam_players",
                column: "ExternalUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gam_point_transactions_CreatedAt",
                table: "gam_point_transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_gam_point_transactions_PlayerId",
                table: "gam_point_transactions",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_gam_streaks_PlayerId_ActionType",
                table: "gam_streaks",
                columns: new[] { "PlayerId", "ActionType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gam_leaderboard_entries");

            migrationBuilder.DropTable(
                name: "gam_levels");

            migrationBuilder.DropTable(
                name: "gam_player_achievements");

            migrationBuilder.DropTable(
                name: "gam_player_challenges");

            migrationBuilder.DropTable(
                name: "gam_point_transactions");

            migrationBuilder.DropTable(
                name: "gam_streaks");

            migrationBuilder.DropTable(
                name: "gam_leaderboards");

            migrationBuilder.DropTable(
                name: "gam_achievements");

            migrationBuilder.DropTable(
                name: "gam_challenges");

            migrationBuilder.DropTable(
                name: "gam_players");
        }
    }
}
