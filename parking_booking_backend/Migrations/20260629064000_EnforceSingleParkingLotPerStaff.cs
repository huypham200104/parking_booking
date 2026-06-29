using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using parking_booking_backend.Data;

#nullable disable

namespace parking_booking.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260629064000_EnforceSingleParkingLotPerStaff")]
    public partial class EnforceSingleParkingLotPerStaff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLotStaffs_UserId_ParkingLotId' AND object_id = OBJECT_ID('ParkingLotStaffs'))
                    DROP INDEX IX_ParkingLotStaffs_UserId_ParkingLotId ON ParkingLotStaffs;

                IF OBJECT_ID('ParkingLotStaffs', 'U') IS NOT NULL
                BEGIN
                    WITH DuplicateAssignments AS (
                        SELECT Id, ROW_NUMBER() OVER (PARTITION BY UserId ORDER BY CreatedAt, Id) AS RowNumber
                        FROM ParkingLotStaffs
                    )
                    DELETE FROM ParkingLotStaffs
                    WHERE Id IN (SELECT Id FROM DuplicateAssignments WHERE RowNumber > 1);

                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLotStaffs_UserId' AND object_id = OBJECT_ID('ParkingLotStaffs'))
                        CREATE UNIQUE INDEX IX_ParkingLotStaffs_UserId ON ParkingLotStaffs(UserId);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ParkingLotStaffs_UserId' AND object_id = OBJECT_ID('ParkingLotStaffs'))
                    DROP INDEX IX_ParkingLotStaffs_UserId ON ParkingLotStaffs;

                IF OBJECT_ID('ParkingLotStaffs', 'U') IS NOT NULL
                    CREATE UNIQUE INDEX IX_ParkingLotStaffs_UserId_ParkingLotId ON ParkingLotStaffs(UserId, ParkingLotId);
                """);
        }
    }
}
