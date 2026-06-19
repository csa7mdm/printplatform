using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PrintPlatform.Loyalty.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loy_referrals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferrerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReferredUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BonusPoints = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConvertedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loy_referrals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loy_rewards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PointsCost = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loy_rewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loy_tiers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MinPoints = table.Column<int>(type: "integer", nullable: false),
                    Multiplier = table.Column<double>(type: "double precision", nullable: false),
                    BenefitsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loy_tiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loy_members",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CurrentTierId = table.Column<long>(type: "bigint", nullable: true),
                    LifetimePoints = table.Column<int>(type: "integer", nullable: false),
                    ActivePoints = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedOrderCount = table.Column<int>(type: "integer", nullable: false),
                    ReferralCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loy_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_loy_members_loy_tiers_CurrentTierId",
                        column: x => x.CurrentTierId,
                        principalTable: "loy_tiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "loy_ledger",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<long>(type: "bigint", nullable: false),
                    Delta = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrderReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Expiry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loy_ledger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_loy_ledger_loy_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "loy_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loy_redemptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<long>(type: "bigint", nullable: false),
                    RewardId = table.Column<long>(type: "bigint", nullable: false),
                    PointsUsed = table.Column<int>(type: "integer", nullable: false),
                    OrderReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LoyaltyMemberId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loy_redemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_loy_redemptions_loy_members_LoyaltyMemberId",
                        column: x => x.LoyaltyMemberId,
                        principalTable: "loy_members",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_loy_redemptions_loy_members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "loy_members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_loy_redemptions_loy_rewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "loy_rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_loy_ledger_MemberId",
                table: "loy_ledger",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_loy_members_CurrentTierId",
                table: "loy_members",
                column: "CurrentTierId");

            migrationBuilder.CreateIndex(
                name: "IX_loy_members_ExternalUserId",
                table: "loy_members",
                column: "ExternalUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loy_members_ReferralCode",
                table: "loy_members",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loy_redemptions_LoyaltyMemberId",
                table: "loy_redemptions",
                column: "LoyaltyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_loy_redemptions_MemberId",
                table: "loy_redemptions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_loy_redemptions_RewardId",
                table: "loy_redemptions",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_loy_referrals_Code",
                table: "loy_referrals",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_loy_referrals_ReferredUserId",
                table: "loy_referrals",
                column: "ReferredUserId");

            migrationBuilder.CreateIndex(
                name: "IX_loy_referrals_ReferrerId",
                table: "loy_referrals",
                column: "ReferrerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loy_ledger");

            migrationBuilder.DropTable(
                name: "loy_redemptions");

            migrationBuilder.DropTable(
                name: "loy_referrals");

            migrationBuilder.DropTable(
                name: "loy_members");

            migrationBuilder.DropTable(
                name: "loy_rewards");

            migrationBuilder.DropTable(
                name: "loy_tiers");
        }
    }
}
