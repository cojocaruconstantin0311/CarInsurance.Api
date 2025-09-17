namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);

public record CreateClaimRequest(string claimDate, string description, decimal amount);
public record ClaimResponse(long id, long carId, string claimDate, string description, decimal amount);

public record HistoryItem(
    string type,         
    string date,         
    string? endDate = null,      
    string? provider = null,    
    string? description = null,  
    decimal? amount = null       
);

public record HistoryResponse(long carId, IReadOnlyList<HistoryItem> items);


