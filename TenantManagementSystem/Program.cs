using Microsoft.EntityFrameworkCore;
using TenantManagement.DataAccessLibrary.Data;
using TenentManagement.Common.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext with in-memory database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map CRUD operations for Bills
app.MapGet("/", () => "Hello World!").WithTags("A Test");

app.MapGet("/bills", async (ApplicationDbContext db) =>
	await db.Bills.ToListAsync()).WithTags("Basic Bills Operations:");

app.MapGet("/bills/{id}", async (int id, ApplicationDbContext db) =>
	await db.Bills.FindAsync(id)
		is Bill bill
			? Results.Ok(bill)
			: Results.NotFound()).WithTags("Basic Bills Operations:");

app.MapPost("/bills", async (Bill bill, ApplicationDbContext db) =>
{
	db.Bills.Add(bill);
	await db.SaveChangesAsync();

	return Results.Created($"/bills/{bill.Id}", bill);
}).WithTags("Basic Bills Operations:");

app.MapPut("/bills/{id}", async (int id, Bill inputBill, ApplicationDbContext db) =>
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

app.MapDelete("/bills/{id}", async (int id, ApplicationDbContext db) =>
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

app.MapGet("/tenants/{tenantId}/bills", async (int tenantId, ApplicationDbContext db) =>
	await db.Bills.Where(b => b.TenantId == tenantId).ToListAsync()).WithTags("Tenants Info :");

app.MapPost("/bills/{id}/pay", async (int id, ApplicationDbContext db) =>
{
	var bill = await db.Bills.FindAsync(id);

	if (bill is null) return Results.NotFound();

	bill.IsPaid = true;
	await db.SaveChangesAsync();

	return Results.Ok(bill);
}).WithTags("Basic Bills Operations:");

app.MapGet("/tenants/{tenantId}/bills/unpaid", async (int tenantId, ApplicationDbContext db) =>
	await db.Bills.Where(b => b.TenantId == tenantId && !b.IsPaid).ToListAsync()).WithTags("Bill Payment Info:");

app.MapGet("/bills/overdue", async (ApplicationDbContext db) =>
	await db.Bills.Where(b => b.DueDate < DateTime.UtcNow && !b.IsPaid).ToListAsync()).WithTags("Bill Payment Info:");

app.MapGet("/tenants/{tenantId}/bills/totaldue", async (int tenantId, ApplicationDbContext db) =>
{
	var totalDue = await db.Bills
		.Where(b => b.TenantId == tenantId && !b.IsPaid)
		.SumAsync(b => b.MonthlyFee + b.Water + b.Electricity + b.Waste);

	return Results.Ok(totalDue);
}).WithTags("Bill Payment Info:");

// Map CRUD operations for Tenants
app.MapGet("/tenants", async (ApplicationDbContext db) =>
	await db.Tenants.ToListAsync()).WithTags("Tenants Info :");

app.MapGet("/tenants/{id}", async (int id, ApplicationDbContext db) =>
	await db.Tenants.FindAsync(id)
		is Tenant tenant
			? Results.Ok(tenant)
			: Results.NotFound()).WithTags("Tenants Info :");

app.MapPost("/tenants", async (Tenant tenant, ApplicationDbContext db) =>
{
	db.Tenants.Add(tenant);
	await db.SaveChangesAsync();

	return Results.Created($"/tenants/{tenant.Id}", tenant);
}).WithTags("Tenants Management:");

app.MapPut("/tenants/{id}", async (int id, Tenant inputTenant, ApplicationDbContext db) =>
{
	var tenant = await db.Tenants.FindAsync(id);

	if (tenant is null) return Results.NotFound();

	tenant.Name = inputTenant.Name;
	tenant.Email = inputTenant.Email;

	await db.SaveChangesAsync();

	return Results.NoContent();
}).WithTags("Tenants Management:");

app.MapDelete("/tenants/{id}", async (int id, ApplicationDbContext db) =>
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
