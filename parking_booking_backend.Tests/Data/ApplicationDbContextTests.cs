using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Models;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Data;

public sealed class ApplicationDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_sets_audit_fields_for_added_and_modified_entities()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var user = TestData.User();

        await using (var context = database.CreateContext())
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var reloaded = await context.Users.SingleAsync(u => u.Id == user.Id);
            var originalCreatedAt = reloaded.CreatedAt;
            reloaded.FullName = "Updated";
            await Task.Delay(5);
            await context.SaveChangesAsync();

            Assert.Equal(originalCreatedAt, reloaded.CreatedAt);
            Assert.True(reloaded.UpdatedAt >= originalCreatedAt);
        }
    }

    [Fact]
    public async Task Query_filters_hide_soft_deleted_entities()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var active = TestData.User(phone: "0900000001");
        var deleted = TestData.User(phone: "0900000002");
        deleted.IsDeleted = true;

        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(active, deleted);
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var visibleUsers = await queryContext.Users.ToListAsync();
        var allUsers = await queryContext.Users.IgnoreQueryFilters().ToListAsync();

        Assert.Single(visibleUsers);
        Assert.Equal(2, allUsers.Count);
        Assert.Equal(active.Id, visibleUsers.Single().Id);
    }
}
