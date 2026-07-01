using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authentication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Passkey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HubPasskeyCredentials",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "bytea", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    UserHandle = table.Column<byte[]>(type: "bytea", nullable: false),
                    SignCount = table.Column<long>(type: "bigint", nullable: false),
                    AttestationFormat = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RegDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AaGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    AttestationObject = table.Column<byte[]>(type: "bytea", nullable: true),
                    AttestationClientDataJson = table.Column<byte[]>(type: "bytea", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HubPasskeyCredentials", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_HubPasskeyCredentials_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HubPasskeyCredentials_UserId",
                table: "HubPasskeyCredentials",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HubPasskeyCredentials");
        }
    }
}
