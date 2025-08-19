var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ✅ Add this
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("https://web-sockets-5x7i.onrender.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});
builder.Services.AddSingleton<RoomManager>();

var app = builder.Build();


app.UseWebSockets();
app.MapControllers();

// app.Map("/ws", () =>
// {
//     if ()
// });

app.UseCors("AllowReactApp");
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
app.Run();
