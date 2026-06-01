using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningPlatform.API.Migrations;

/// <inheritdoc />
public partial class AddUserV2Fields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PreferredCategory",
            table: "Users",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "TwoFactorEnabled",
            table: "Users",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "TwoFactorCode",
            table: "Users",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "TwoFactorCodeExpiry",
            table: "Users",
            type: "TEXT",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PreferredCategory", table: "Users");
        migrationBuilder.DropColumn(name: "TwoFactorEnabled", table: "Users");
        migrationBuilder.DropColumn(name: "TwoFactorCode", table: "Users");
        migrationBuilder.DropColumn(name: "TwoFactorCodeExpiry", table: "Users");
    }
}
