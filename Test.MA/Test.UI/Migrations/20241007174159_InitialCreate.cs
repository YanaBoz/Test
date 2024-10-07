using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test.UI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Oper_Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oper_Classes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turnover",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Group_ID = table.Column<int>(type: "int", nullable: true),
                    Class_ID = table.Column<int>(type: "int", nullable: true),
                    Account = table.Column<int>(type: "int", nullable: false),
                    Start_Active = table.Column<double>(type: "float", nullable: false),
                    Start_Passive = table.Column<double>(type: "float", nullable: false),
                    Turn_Debit = table.Column<double>(type: "float", nullable: false),
                    Turn_Credit = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnover", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turnover_Groups_Group_ID",
                        column: x => x.Group_ID,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Turnover_Oper_Classes_Class_ID",
                        column: x => x.Class_ID,
                        principalTable: "Oper_Classes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Turnover_Class_ID",
                table: "Turnover",
                column: "Class_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Turnover_Group_ID",
                table: "Turnover",
                column: "Group_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Turnover");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Oper_Classes");
        }
    }
}
