using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    public async Task<HistoryResponse> GetHistoryAsync(long carId)
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.Id == carId);
        if (car is null) throw new KeyNotFoundException();

        var policies = await _db.Policies.AsNoTracking()
            .Where(p => p.CarId == carId)
            .ToListAsync();

        var claims = await _db.Claims.AsNoTracking()
            .Where(c => c.CarId == carId)
            .ToListAsync();

        var items = new List<HistoryItem>(policies.Count + claims.Count);

        foreach (var p in policies)
        {
            items.Add(new HistoryItem(
                type: "policy",
                date: p.StartDate.ToString("yyyy-MM-dd"),
                endDate: p.EndDate.ToString("yyyy-MM-dd"),
                provider: p.Provider
            ));
        }

        foreach (var c in claims)
        {
            items.Add(new HistoryItem(
                type: "claim",
                date: c.ClaimDate.ToString("yyyy-MM-dd"),
                description: c.Description,
                amount: c.Amount
            ));
        }

        var ordered = items.OrderBy(i => i.date).ThenBy(i => i.type).ToList();
        return new HistoryResponse(carId, ordered);
    }


    public static DateOnly ParseStrictDate(string dateStr)
    {
        return DateOnly.ParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public async Task<ClaimResponse> RegisterClaimAsync(long carId, string claimDate, string description, decimal amount)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == carId);
        if (car is null) throw new KeyNotFoundException();

        var date = ParseStrictDate(claimDate);

        var claim = new Claim
        {
            CarId = carId,
            ClaimDate = date,
            Description = description,
            Amount = amount
        };
        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();

        return new ClaimResponse(claim.Id, carId, date.ToString("yyyy-MM-dd"), claim.Description, claim.Amount);
    }

    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            date <= p.EndDate
        );
    }
}
