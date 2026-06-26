using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext với chuỗi kết nối
builder.Services.AddDbContext<parking_booking_backend.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 1. Tạo dữ liệu giả lập (Mock Data) lưu trong bộ nhớ
var todos = new List<TodoItem>
{
    new TodoItem(1, "Học .NET 10 trên VS Code xanh", true),
    new TodoItem(2, "Viết API đầu tiên", false)
};

// 2. API GET: Lấy toàn bộ danh sách Todo
app.MapGet("/api/todos", () => todos);

// 3. API GET (chứa tham số): Lấy Todo theo ID
app.MapGet("/api/todos/{id}", (int id) => 
    todos.FirstOrDefault(t => t.Id == id) is TodoItem todo 
        ? Results.Ok(todo) 
        : Results.NotFound("Không tìm thấy việc này!"));

// 4. API POST: Thêm một Todo mới
app.MapPost("/api/todos", (TodoItem newTodo) => {
    todos.Add(newTodo);
    return Results.Created($"/api/todos/{newTodo.Id}", newTodo);
});

// API Test: Kiểm tra kết nối Database
app.MapGet("/api/test-db", async (parking_booking_backend.Data.ApplicationDbContext dbContext) =>
{
    try
    {
        // CanConnectAsync sẽ trả về true nếu chuỗi kết nối hợp lệ VÀ Database đã tồn tại
        bool canConnect = await dbContext.Database.CanConnectAsync();
        
        if (canConnect)
        {
            return Results.Ok(new { Message = "Kết nối Database thành công và Database đã tồn tại!" });
        }
        else
        {
            return Results.Ok(new { Message = "Kết nối SQL Server thành công NHƯNG Database chưa được tạo. Hãy chạy Migration (Update-Database) nhé!" });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "Lỗi kết nối Database", detail: ex.Message);
    }
});

app.Run();

// Định nghĩa cấu trúc dữ liệu Todo
record TodoItem(int Id, string Title, bool IsCompleted);