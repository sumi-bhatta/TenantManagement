using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TenantManagement.DataAccessLibrary.Data;
using TenentManagement.Common.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tenant Management", Version = "v1" });

	var securityScheme = new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter your JWT token in the format 'Bearer {your token}'",

		Reference = new OpenApiReference
		{
			Type = ReferenceType.SecurityScheme,
			Id = "Bearer"
		}
	};

	c.AddSecurityDefinition("Bearer", securityScheme);

	var securityRequirement = new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	};

	c.AddSecurityRequirement(securityRequirement);
});
// Add DbContext with in-memory database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var key = Encoding.ASCII.GetBytes("Your_32_Byte_Secure_Key_1234567890"); // Use a secure key here

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = false,
		ValidateAudience = false
	};
});

var app = builder.Build();
 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "TenantManagementSystem");
	});
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Use authentication middleware
app.UseAuthorization();

//login and signup
app.MapPost("/login", (UserLogin userLogin) =>
{
	if (userLogin.Username == "test" && userLogin.Password == "password") // Replace with your own validation logic
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new Claim[]
			{
				new Claim(ClaimTypes.Name, userLogin.Username)
			}),
			Expires = DateTime.UtcNow.AddHours(1),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};
		var token = tokenHandler.CreateToken(tokenDescriptor);
		var tokenString = tokenHandler.WriteToken(token);

		return Results.Ok(new { Token = tokenString });
	}

	return Results.Unauthorized();
}).WithTags("2. Login");


// Map CRUD operations for Bills
app.MapGet("/", [Authorize] () => "Hello World!").WithTags("1. Test");

app.MapGet("/bills", async (ApplicationDbContext db) =>
	await db.Bills.ToListAsync()).WithTags("Basic Bills Operations:");

app.MapGet("/bills/{id}", async (int id, ApplicationDbContext db) =>
	await db.Bills.FindAsync(id)
		is Bill bill
			? Results.Ok(bill)
			: Results.NotFound()).WithTags("Basic Bills Operations:");

app.MapPost("/bills", [Authorize] async (Bill bill, ApplicationDbContext db) =>
{
	db.Bills.Add(bill);
	await db.SaveChangesAsync();

	return Results.Created($"/bills/{bill.Id}", bill);
}).WithTags("Basic Bills Operations:");

app.MapPut("/bills/{id}", [Authorize] async (int id, Bill inputBill, ApplicationDbContext db) =>
{
	var bill = await db.Bills.FindAsync(id);

	if (bill is null) return Results.NotFound();

	bill.TenantId = inputBill.TenantId;
	bill.MonthlyFee = inputBill.MonthlyFee;
	bill.Water = inputBill.Water;
	bill.Electricity = inputBill.Electricity;
	bill.Waste = inputBill.Waste;
	bill.DueDate = inputBill.DueDate;
	bill.IsPaid = inputBill.IsPaid;

	await db.SaveChangesAsync();

	return Results.NoContent();
}).WithTags("Basic Bills Operations:");

app.MapDelete("/bills/{id}", [Authorize] async (int id, ApplicationDbContext db) =>
{
	if (await db.Bills.FindAsync(id) is Bill bill)
	{
		db.Bills.Remove(bill);
		await db.SaveChangesAsync();
		return Results.Ok(bill);
	}

	return Results.NotFound();
}).WithTags("Basic Bills Operations:");

// Additional Endpoints

app.MapGet("/tenants/{tenantId}/bills", [Authorize] async (int tenantId, ApplicationDbContext db) =>
	await db.Bills.Where(b => b.TenantId == tenantId).ToListAsync()).WithTags("Tenants Info :");

app.MapPost("/bills/{id}/pay", [Authorize] async (int id, ApplicationDbContext db) =>
{
	var bill = await db.Bills.FindAsync(id);

	if (bill is null) return Results.NotFound();

	bill.IsPaid = true;
	await db.SaveChangesAsync();

	return Results.Ok(bill);
}).WithTags("Basic Bills Operations:");

app.MapGet("/tenants/{tenantId}/bills/unpaid", [Authorize] async (int tenantId, ApplicationDbContext db) =>
	await db.Bills.Where(b => b.TenantId == tenantId && !b.IsPaid).ToListAsync()).WithTags("Bill Payment Info:");

app.MapGet("/bills/overdue", [Authorize] async (ApplicationDbContext db) =>
	await db.Bills.Where(b => b.DueDate < DateTime.UtcNow && !b.IsPaid).ToListAsync()).WithTags("Bill Payment Info:");

app.MapGet("/tenants/{tenantId}/bills/totaldue", [Authorize] async (int tenantId, ApplicationDbContext db) =>
{
	var totalDue = await db.Bills
		.Where(b => b.TenantId == tenantId && !b.IsPaid)
		.SumAsync(b => b.MonthlyFee + b.Water + b.Electricity + b.Waste);

	return Results.Ok(totalDue);
}).WithTags("Bill Payment Info:");

// Map CRUD operations for Tenants
app.MapGet("/tenants", [Authorize] async (ApplicationDbContext db) =>
	await db.Tenants.ToListAsync()).WithTags("Tenants Info :");

app.MapGet("/tenants/{id}", [Authorize] async (int id, ApplicationDbContext db) =>
	await db.Tenants.FindAsync(id)
		is Tenant tenant
			? Results.Ok(tenant)
			: Results.NotFound()).WithTags("Tenants Info :");

app.MapPost("/tenants", [Authorize] async (Tenant tenant, ApplicationDbContext db) =>
{
	db.Tenants.Add(tenant);
	await db.SaveChangesAsync();

	return Results.Created($"/tenants/{tenant.Id}", tenant);
}).WithTags("Tenants Management:");

app.MapPut("/tenants/{id}", [Authorize] async (int id, Tenant inputTenant, ApplicationDbContext db) =>
{
	var tenant = await db.Tenants.FindAsync(id);

	if (tenant is null) return Results.NotFound();

	tenant.Name = inputTenant.Name;
	tenant.Email = inputTenant.Email;

	await db.SaveChangesAsync();

	return Results.NoContent();
}).WithTags("Tenants Management:");

app.MapDelete("/tenants/{id}", [Authorize] async (int id, ApplicationDbContext db) =>
{
	if (await db.Tenants.FindAsync(id) is Tenant tenant)
	{
		db.Tenants.Remove(tenant);
		await db.SaveChangesAsync();
		return Results.Ok(tenant);
	}

	return Results.NotFound();
}).WithTags("Tenants Management:");

app.MapControllers();

app.Run();
