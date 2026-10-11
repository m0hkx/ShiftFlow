using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizations : Migration
    {
        private static readonly Guid DefaultOrgId = new("8f1c2a5e-0000-4000-8000-000000000001");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_Email",
                table: "Employees");

            migrationBuilder.AddColumn<Guid>(
                name: "OrgId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrgId",
                table: "Shifts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrgId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            // Rows created before organizations existed go into one default organization.
            // Existing users become its Managers, since before this change every user
            // could already create, edit and delete everything.
            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "Id", "Name" },
                values: new object[] { DefaultOrgId, "Default Organization" });

            migrationBuilder.Sql($"UPDATE [Users] SET [OrgId] = '{DefaultOrgId}', [Role] = 'Manager';");
            migrationBuilder.Sql($"UPDATE [Employees] SET [OrgId] = '{DefaultOrgId}';");
            migrationBuilder.Sql($"UPDATE [Shifts] SET [OrgId] = '{DefaultOrgId}';");

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(44)", maxLength: 44, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invitations_Organizations_OrgId",
                        column: x => x.OrgId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrgId",
                table: "Users",
                column: "OrgId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_OrgId",
                table: "Shifts",
                column: "OrgId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_OrgId_Email",
                table: "Employees",
                columns: new[] { "OrgId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_OrgId",
                table: "Invitations",
                column: "OrgId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_TokenHash",
                table: "Invitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Organizations_OrgId",
                table: "Employees",
                column: "OrgId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Organizations_OrgId",
                table: "Shifts",
                column: "OrgId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrgId",
                table: "Users",
                column: "OrgId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Organizations_OrgId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Organizations_OrgId",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrgId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrgId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_OrgId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Employees_OrgId_Email",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OrgId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrgId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "OrgId",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);
        }
    }
}
