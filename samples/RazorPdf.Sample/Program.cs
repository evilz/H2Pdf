using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RazorPdf;
using RazorPdf.Sample.Components;
using RazorPdf.Sample.Models;

// Set up dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddRazorPdf();

using var serviceProvider = services.BuildServiceProvider();

// Get the PDF renderer
var pdfRenderer = serviceProvider.GetRequiredService<PdfRenderer>();

Console.WriteLine("RazorPdf Sample - Generating PDF from Razor Components");
Console.WriteLine("=====================================================");
Console.WriteLine();

try
{
    // Example 1: Simple HelloWorld component
    Console.WriteLine("Generating HelloWorld sample...");
    var parameters = new Dictionary<string, object?>
    {
        { "Name", "Developer" },
        { "AdditionalMessage", "This is a real .razor file component!" }
    };

    var document = await pdfRenderer.RenderToDocumentAsync<HelloWorld>(parameters);
    
    var outputPath = "sample-output.pdf";
    pdfRenderer.SaveToPdf(document, outputPath);
    
    Console.WriteLine($"✓ HelloWorld PDF generated: {Path.GetFullPath(outputPath)}");
    Console.WriteLine();

    // Example 2: Complex Invoice component
    Console.WriteLine("Generating Invoice sample...");
    var invoiceData = CreateSampleInvoiceData();
    var invoiceParameters = new Dictionary<string, object?>
    {
        { "Data", invoiceData }
    };

    var invoiceDocument = await pdfRenderer.RenderToDocumentAsync<Invoice>(invoiceParameters);
    
    var invoiceOutputPath = "invoice-sample.pdf";
    pdfRenderer.SaveToPdf(invoiceDocument, invoiceOutputPath);
    
    Console.WriteLine($"✓ Invoice PDF generated: {Path.GetFullPath(invoiceOutputPath)}");
    Console.WriteLine();
    Console.WriteLine("All samples generated successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error generating PDF: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}

static InvoiceData CreateSampleInvoiceData()
{
    return new InvoiceData
    {
        Company = new CompanyInfo
        {
            Name = "Tech Solutions Inc.",
            Tagline = "Your Technology Partner",
            LogoText = "TECHSOL",
            Address = "123 Business Avenue, Suite 456, San Francisco, CA 94102",
            Phone1 = "+1 (555) 123-4567",
            Phone2 = "+1 (555) 765-4321",
            Website = "www.techsolutions.com",
            Email = "contact@techsolutions.com"
        },
        Client = new ClientInfo
        {
            Name = "Devie Willson",
            Role = "Director",
            Phone = "+1 (555) 987-6543",
            Email = "devie.willson@example.com",
            Website = "www.example.com",
            Address = "789 Client Street, New York, NY 10001"
        },
        Details = new InvoiceDetails
        {
            InvoiceNumber = "INV-2024-001",
            InvoiceDate = DateTime.Now,
            IssueDate = DateTime.Now.AddDays(-1),
            AccountNumber = "ACC-98765"
        },
        Payment = new PaymentInfo
        {
            Method = "Bank Transfer / PayPal",
            PayPalEmail = "payments@techsolutions.com",
            AcceptedCards = new List<string> { "Visa", "Mastercard", "Payoneer" }
        },
        Items = new List<InvoiceItem>
        {
            new InvoiceItem
            {
                Description = "Creative Suite License",
                Details = "Annual subscription for Adobe Creative Cloud - Full package including Photoshop, Illustrator, and Premiere Pro",
                UnitPrice = 599.00m,
                Quantity = 10
            },
            new InvoiceItem
            {
                Description = "Cloud Storage",
                Details = "Enterprise cloud storage solution with 10TB capacity and advanced security features",
                UnitPrice = 299.00m,
                Quantity = 5
            },
            new InvoiceItem
            {
                Description = "Technical Support",
                Details = "Premium 24/7 technical support with dedicated account manager",
                UnitPrice = 1500.00m,
                Quantity = 12
            },
            new InvoiceItem
            {
                Description = "Website Development",
                Details = "Custom responsive website design and development with CMS integration",
                UnitPrice = 8500.00m,
                Quantity = 1
            },
            new InvoiceItem
            {
                Description = "SEO Optimization",
                Details = "Search engine optimization services including keyword research and content optimization",
                UnitPrice = 750.00m,
                Quantity = 6
            }
        },
        Summary = new FinancialSummary
        {
            Subtotal = 35985.00m,
            TaxRate = 20m,
            DiscountRate = 3m
        },
        TermsAndConditions = "Payment is due within 30 days of invoice date. Late payments may incur a 1.5% monthly interest charge. All services are provided according to our standard terms and conditions. Please make checks payable to Tech Solutions Inc.",
        Signature = new SignatureInfo
        {
            SignatureText = "Smako Atson",
            Name = "Smako Atson",
            Title = "Manager"
        }
    };
}
