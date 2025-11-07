using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationsApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIntegrationAndChannelTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Integrations_Organizations_OrganizationId",
                table: "Integrations");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Integrations_OrganizationId",
                table: "Integrations");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationChannels_IntegrationId",
                table: "IntegrationChannels");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Integrations");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "new",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderEventId",
                table: "Notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ExternalChannelId",
                table: "Notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalUserId",
                table: "Notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Integrations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Integrations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalAccountId",
                table: "Integrations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CredentialType",
                table: "IntegrationCredentials",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "IntegrationChannels",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalChannelId",
                table: "IntegrationChannels",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "IntegrationChannels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "DeliveryAttempts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Filename",
                table: "Attachments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Attachments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "member"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUsers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChannels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntegrationChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsMember = table.Column<bool>(type: "boolean", nullable: false),
                    ReceiveNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserChannels_IntegrationChannels_IntegrationChannelId",
                        column: x => x.IntegrationChannelId,
                        principalTable: "IntegrationChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChannels_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserIntegrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntegrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalUserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExternalUsername = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Settings = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    ReceiveNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIntegrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIntegrations_Integrations_IntegrationId",
                        column: x => x.IntegrationId,
                        principalTable: "Integrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserIntegrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_CreatedAt",
                table: "UserNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId_IsRead",
                table: "UserNotifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId_NotificationId",
                table: "UserNotifications",
                columns: new[] { "UserId", "NotificationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ExternalChannelId",
                table: "Notifications",
                column: "ExternalChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ExternalUserId",
                table: "Notifications",
                column: "ExternalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ProviderEventId",
                table: "Notifications",
                column: "ProviderEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status",
                table: "Notifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_OrgId",
                table: "Integrations",
                column: "OrgId");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_Provider_ExternalAccountId",
                table: "Integrations",
                columns: new[] { "Provider", "ExternalAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationChannels_IntegrationId_ExternalChannelId",
                table: "IntegrationChannels",
                columns: new[] { "IntegrationId", "ExternalChannelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAttempts_AttemptedAt",
                table: "DeliveryAttempts",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_OrganizationId_UserId",
                table: "OrganizationUsers",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUsers_UserId",
                table: "OrganizationUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannels_IntegrationChannelId",
                table: "UserChannels",
                column: "IntegrationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChannels_UserId_IntegrationChannelId",
                table: "UserChannels",
                columns: new[] { "UserId", "IntegrationChannelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserIntegrations_ExternalUserId",
                table: "UserIntegrations",
                column: "ExternalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIntegrations_IntegrationId",
                table: "UserIntegrations",
                column: "IntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIntegrations_UserId_IntegrationId",
                table: "UserIntegrations",
                columns: new[] { "UserId", "IntegrationId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Integrations_Organizations_OrgId",
                table: "Integrations",
                column: "OrgId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Integrations_Organizations_OrgId",
                table: "Integrations");

            migrationBuilder.DropTable(
                name: "OrganizationUsers");

            migrationBuilder.DropTable(
                name: "UserChannels");

            migrationBuilder.DropTable(
                name: "UserIntegrations");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_CreatedAt",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId_IsRead",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId_NotificationId",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ExternalChannelId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ExternalUserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ProviderEventId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_Status",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Integrations_OrgId",
                table: "Integrations");

            migrationBuilder.DropIndex(
                name: "IX_Integrations_Provider_ExternalAccountId",
                table: "Integrations");

            migrationBuilder.DropIndex(
                name: "IX_IntegrationChannels_IntegrationId_ExternalChannelId",
                table: "IntegrationChannels");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryAttempts_AttemptedAt",
                table: "DeliveryAttempts");

            migrationBuilder.DropColumn(
                name: "ExternalChannelId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ExternalUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "IntegrationChannels");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Notifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "new");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderEventId",
                table: "Notifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Notifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Integrations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Integrations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalAccountId",
                table: "Integrations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Integrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CredentialType",
                table: "IntegrationCredentials",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "IntegrationChannels",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalChannelId",
                table: "IntegrationChannels",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "DeliveryAttempts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Filename",
                table: "Attachments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "Attachments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_OrganizationId",
                table: "Integrations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationChannels_IntegrationId",
                table: "IntegrationChannels",
                column: "IntegrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Integrations_Organizations_OrganizationId",
                table: "Integrations",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }
    }
}
