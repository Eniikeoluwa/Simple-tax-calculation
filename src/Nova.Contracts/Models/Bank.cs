namespace Nova.Contracts.Models;

public class CreateBankRequest
{
    public string Name { get; set; } = string.Empty;
    public string SortCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class UpdateBankRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SortCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class BankResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SortCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class GetBanksResponse
{
    public List<BankResponse> Banks { get; set; } = new();
}
