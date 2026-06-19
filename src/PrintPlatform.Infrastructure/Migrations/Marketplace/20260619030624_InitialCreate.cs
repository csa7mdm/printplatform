using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrintPlatform.Infrastructure.Migrations.Marketplace
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mkt_material_options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ColorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ColorHex = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    PricePerGram = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    OwnerCostPerGram = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    PropertiesJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mkt_material_options", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "mkt_printers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrinterOwnerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnologyType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BuildVolumeX = table.Column<int>(type: "integer", nullable: false),
                    BuildVolumeY = table.Column<int>(type: "integer", nullable: false),
                    BuildVolumeZ = table.Column<int>(type: "integer", nullable: false),
                    NozzleDiameter = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    SupportedMaterialsJson = table.Column<string>(type: "jsonb", nullable: false),
                    MaxLayerHeight = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    MinLayerHeight = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    AreaDistrict = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AreaCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDeliverable = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CertificationLevel = table.Column<int>(type: "integer", nullable: false),
                    MaxConcurrentJobs = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mkt_printers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "mkt_printer_materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrinterId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxQuality = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mkt_printer_materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mkt_printer_materials_mkt_material_options_MaterialOptionId",
                        column: x => x.MaterialOptionId,
                        principalTable: "mkt_material_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mkt_printer_materials_mkt_printers_PrinterId",
                        column: x => x.PrinterId,
                        principalTable: "mkt_printers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mkt_material_options_IsActive",
                table: "mkt_material_options",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_mkt_material_options_MaterialType",
                table: "mkt_material_options",
                column: "MaterialType");

            migrationBuilder.CreateIndex(
                name: "IX_mkt_printer_materials_MaterialOptionId",
                table: "mkt_printer_materials",
                column: "MaterialOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_mkt_printer_materials_PrinterId_MaterialOptionId",
                table: "mkt_printer_materials",
                columns: new[] { "PrinterId", "MaterialOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mkt_printers_AreaDistrict",
                table: "mkt_printers",
                column: "AreaDistrict");

            migrationBuilder.CreateIndex(
                name: "IX_mkt_printers_PrinterOwnerProfileId",
                table: "mkt_printers",
                column: "PrinterOwnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_mkt_printers_Status",
                table: "mkt_printers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mkt_printer_materials");

            migrationBuilder.DropTable(
                name: "mkt_material_options");

            migrationBuilder.DropTable(
                name: "mkt_printers");
        }
    }
}
