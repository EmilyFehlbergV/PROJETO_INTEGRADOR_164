using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VistoPorMim.Data;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços do MVC
builder.Services.AddControllersWithViews();

// Adiciona os serviços do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura a autenticação por cookies
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.AccessDeniedPath = "/Usuarios/Login";
        options.Cookie.Name = "VistoPorMim.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// Recupera a string de conexão
var conexao = builder.Configuration.GetConnectionString("ConexaoMySql")
              ?? throw new InvalidOperationException(
                  "A string de conexão 'ConexaoMySql' não foi configurada."
              );

// Configura o DbContext para usar o MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(conexao)
);

var app = builder.Build();

// Configurações do pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Habilita o Swagger somente em desenvolvimento
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// A autenticação precisa vir antes da autorização
app.UseAuthentication();
app.UseAuthorization();

// Registra as rotas dos controllers de API
app.MapControllers();

// Registra a rota padrão do MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();