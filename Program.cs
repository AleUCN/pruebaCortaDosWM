using chairs_dotnet7_api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("chairlist"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

var chairs = app.MapGroup("api/chair");

//TODO: ASIGNACION DE RUTAS A LOS ENDPOINTS
chairs.MapPost("/",CreateChair);
chairs.MapGet("/", GetChairs);
chairs.MapGet("/{nombre}",GetChairsByName);
chairs.MapGet("/{id}",UpdateChair);
chairs.MapGet("/{id}/stock",IncrementChairStock);
chairs.MapGet("/purchase", PurchaseChair);
chairs.MapGet("/{id}",DeleteChair);

app.Run();

//TODO: ENDPOINTS SOLICITADOS
static async Task<IResult> CreateChair(Chair chair,DataContext db)
{
    var aux = await db.Chairs.FirstOrDefaultAsync(c => c.Nombre.Equals(chair.Nombre));
    if(aux is null){
        db.Chairs.Add(chair);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/{chair.Id}", chair);
    }
    return TypedResults.BadRequest();
}

static IResult GetChairs(DataContext db)
{
    return TypedResults.Ok();
}

static async Task<IResult> GetChairsByName(string nombre, DataContext db)
{
    var chair = await db.Chairs.FirstOrDefaultAsync(c => c.Nombre.Equals(nombre));
    if( chair is null){
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(chair);
}

static async Task<IResult> UpdateChair(int id, Chair inputChair, DataContext db){
    var chair = await db.Chairs.FindAsync(id);
    if (chair is null) {
        return TypedResults.NotFound();
    }
    chair.Nombre = inputChair.Nombre;
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}

static async Task<IResult> IncrementChairStock(int id, int stock, DataContext db){
    var chair = await db.Chairs.FindAsync(id);
    if( chair is null){
        return TypedResults.NotFound();
    }
    chair.Stock = chair.Stock + stock;
    await db.SaveChangesAsync();
    return TypedResults.Ok(chair);
}

static async Task<IResult> PurchaseChair(int id, int cantidad, int pago, DataContext db ){
    var chair = await db.Chairs.FindAsync(id);
    if( chair is null){
        return TypedResults.NotFound();
    }
    if ( chair.Stock < cantidad){
        return TypedResults.BadRequest("No hay esa cantidad de productos");
    }
    var total = chair.Precio*cantidad;
    if ( total != pago){
        return TypedResults.BadRequest("No cumple con el pago");
    }

    chair.Stock = chair.Stock - cantidad;
    await db.SaveChangesAsync();
    return TypedResults.Ok();
}

static async Task<IResult> DeleteChair(int id, DataContext db){
    var chair = await db.Chairs.FindAsync(id);
    if(chair == null){
        return TypedResults.NotFound();
    }
    db.Chairs.Remove(chair);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}