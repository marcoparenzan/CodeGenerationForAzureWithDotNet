using Microsoft.Extensions.Logging;
using MinimalApiCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = builder.Environment.ApplicationName, Version = "v1" });
});

// add services

var app = builder.Build();

// logging

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();

var service = new ServiceImpl();
var impl = new ServiceImplService(service);
Register<DoSomethingRequest, DoSomethingResponse>("DoSomething", impl.HandleDoSomething);
Register<ElseSomethingRequest, ElseSomethingResponse>("ElseSomething", impl.HandleElseSomething);

// swagger
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{builder.Environment.ApplicationName} v1"));

app.Run();

void Register<TRequest, TResponse>(string name, Action<TRequest, TResponse> handler)
{
    app.MapPost($"/api/{name}", async (TRequest request) =>
    {
        var response = Activator.CreateInstance<TResponse>();
        handler.Invoke(request, response);

        return response;
    });
}
