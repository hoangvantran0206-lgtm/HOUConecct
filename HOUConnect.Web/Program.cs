using HOUConnect.Data;
using HOUConnect.Data.Repositories;
using HOUConnect.Business.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache(); // Đăng ký bộ nhớ tạm cho Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session sẽ hết hạn sau 30 phút không hoạt động
    options.Cookie.HttpOnly = true; // Bảo mật: Chỉ Server mới đọc được Cookie này
    options.Cookie.IsEssential = true; // Bắt buộc phải có để ứng dụng chạy đúng
});
// Đăng ký SqlHelper và các lớp DAL/BLL
builder.Services.AddSingleton<HOUConnect.Data.SqlHelper>();
builder.Services.AddScoped<HOUConnect.Data.Repositories.UserDAL>();
builder.Services.AddScoped<HOUConnect.Business.Services.UserService>();
// Đăng ký tầng Data
builder.Services.AddScoped<SongDAL>();

// Đăng ký tầng Business
builder.Services.AddScoped<SongService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();
app.MapRazorPages();
app.Run();
