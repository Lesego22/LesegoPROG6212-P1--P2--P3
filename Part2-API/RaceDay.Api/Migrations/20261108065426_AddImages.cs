using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_EventCategory_EventCategoryId",
                table: "Enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_User_ParticipantId",
                table: "Enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_Result_Enrollment_EnrolmentId",
                table: "Result");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment");

            migrationBuilder.RenameTable(
                name: "Enrollment",
                newName: "Enrolment");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollment_ParticipantId",
                table: "Enrolment",
                newName: "IX_Enrolment_ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollment_EventCategoryId",
                table: "Enrolment",
                newName: "IX_Enrolment_EventCategoryId");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicture",
                table: "User",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BannerImage",
                table: "Event",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrolment",
                table: "Enrolment",
                column: "EnrolmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolment_EventCategory_EventCategoryId",
                table: "Enrolment",
                column: "EventCategoryId",
                principalTable: "EventCategory",
                principalColumn: "EventCategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolment_User_ParticipantId",
                table: "Enrolment",
                column: "ParticipantId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Result_Enrolment_EnrolmentId",
                table: "Result",
                column: "EnrolmentId",
                principalTable: "Enrolment",
                principalColumn: "EnrolmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolment_EventCategory_EventCategoryId",
                table: "Enrolment");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolment_User_ParticipantId",
                table: "Enrolment");

            migrationBuilder.DropForeignKey(
                name: "FK_Result_Enrolment_EnrolmentId",
                table: "Result");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrolment",
                table: "Enrolment");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "User");

            migrationBuilder.DropColumn(
                name: "BannerImage",
                table: "Event");

            migrationBuilder.RenameTable(
                name: "Enrolment",
                newName: "Enrollment");

            migrationBuilder.RenameIndex(
                name: "IX_Enrolment_ParticipantId",
                table: "Enrollment",
                newName: "IX_Enrollment_ParticipantId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrolment_EventCategoryId",
                table: "Enrollment",
                newName: "IX_Enrollment_EventCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment",
                column: "EnrolmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_EventCategory_EventCategoryId",
                table: "Enrollment",
                column: "EventCategoryId",
                principalTable: "EventCategory",
                principalColumn: "EventCategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_User_ParticipantId",
                table: "Enrollment",
                column: "ParticipantId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Result_Enrollment_EnrolmentId",
                table: "Result",
                column: "EnrolmentId",
                principalTable: "Enrollment",
                principalColumn: "EnrolmentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
