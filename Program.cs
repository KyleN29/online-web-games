var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ✅ Add this
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // your React dev server
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

app.Run();
