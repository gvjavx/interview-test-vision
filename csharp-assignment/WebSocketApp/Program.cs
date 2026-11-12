using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddSingleton<ChatService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
};
app.UseWebSockets(webSocketOptions);

app.MapHealthChecks("/health");

app.Map("/ws", async (HttpContext context, ChatService chatService) =>
{
    if (context.WebSockets.IsWebSocketRequest == false)
    {
        context.Response.StatusCode = 400;
        return;
    }

    using WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
    await chatService.HandleClient(socket, context.RequestAborted);
});

app.Run();
