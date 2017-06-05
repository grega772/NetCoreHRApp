using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HrApp.Migrations
{
    public partial class addDocumentLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CVString",
                table: "AspNetUsers",
                newName: "DocumentLink");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocumentLink",
                table: "AspNetUsers",
                newName: "CVString");
        }
    }
}
