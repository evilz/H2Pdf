namespace RazorPdf.Sample.Models;

public class InvoiceData
{
    public CompanyInfo Company { get; set; } = new();
    public ClientInfo Client { get; set; } = new();
    public InvoiceDetails Details { get; set; } = new();
    public PaymentInfo Payment { get; set; } = new();
    public List<InvoiceItem> Items { get; set; } = new();
    public FinancialSummary Summary { get; set; } = new();
    public string TermsAndConditions { get; set; } = string.Empty;
    public SignatureInfo Signature { get; set; } = new();
}

public class CompanyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string LogoText { get; set; } = "LOGO";
    public string Address { get; set; } = string.Empty;
    public string Phone1 { get; set; } = string.Empty;
    public string Phone2 { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ClientInfo
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class InvoiceDetails
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime IssueDate { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
}

public class PaymentInfo
{
    public string Method { get; set; } = string.Empty;
    public string PayPalEmail { get; set; } = string.Empty;
    public List<string> AcceptedCards { get; set; } = new();
}

public class InvoiceItem
{
    public string Description { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    
    /// <summary>
    /// Gets the line item total calculated as UnitPrice × Quantity
    /// </summary>
    public decimal Total => UnitPrice * Quantity;
}

public class FinancialSummary
{
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal DiscountRate { get; set; }
    
    /// <summary>
    /// Gets the tax amount calculated as Subtotal × (TaxRate / 100)
    /// </summary>
    public decimal TaxAmount => Subtotal * (TaxRate / 100);
    
    /// <summary>
    /// Gets the discount amount calculated as Subtotal × (DiscountRate / 100)
    /// </summary>
    public decimal DiscountAmount => Subtotal * (DiscountRate / 100);
    
    /// <summary>
    /// Gets the grand total calculated as Subtotal + TaxAmount - DiscountAmount
    /// </summary>
    public decimal GrandTotal => Subtotal + TaxAmount - DiscountAmount;
}

public class SignatureInfo
{
    public string SignatureText { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
