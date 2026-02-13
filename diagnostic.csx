using AngleSharp.Html.Parser;
using RazorPdf;
using RazorPdf.Parsing;

var html = File.ReadAllText(@"e:\PROJECTS\GITHUB\RazorPdf\samples\invoice-sample.html");
var parser = new HtmlParser();
var doc = parser.ParseDocument(html);

var resolver = new CssStyleResolver();
foreach (var s in doc.QuerySelectorAll("style"))
    resolver.Parse(s.TextContent);

var visitor = new MigraDocVisitor(resolver, @"e:\PROJECTS\GITHUB\RazorPdf\samples", 18.88);
HtmlDocumentWalker.Walk(doc.Body!, visitor);
var model = visitor.GetResult();

foreach (var section in model.Sections)
{
    Console.WriteLine($"Section: {section.Blocks.Count} blocks");
    for (int i = 0; i < section.Blocks.Count; i++)
    {
        var block = section.Blocks[i];
        var typeName = block.GetType().Name;
        switch (block)
        {
            case PdfTableModel t:
                Console.WriteLine($"  [{i}] {typeName} Layout={t.IsLayoutTable} Rows={t.Rows.Count} Cols={t.Rows.FirstOrDefault()?.Cells.Count} LeftIndent={t.LeftIndentCm} ColWidths={string.Join(",", t.ColumnWidthsCm ?? [])}");
                foreach (var row in t.Rows)
                    foreach (var cell in row.Cells)
                    {
                        Console.WriteLine($"       Cell: {cell.Blocks.Count} blocks, {cell.Paragraphs.Count} paras");
                        foreach (var cb in cell.Blocks)
                            Console.WriteLine($"         > {cb.GetType().Name}");
                    }
                break;
            case PdfHeadingModel h:
                Console.WriteLine($"  [{i}] {typeName} L{h.Level} '{h.Text}'");
                break;
            case PdfParagraphModel p:
                var text = string.Join("", p.Inlines.OfType<PdfTextRunModel>().Select(tr => tr.Text));
                Console.WriteLine($"  [{i}] {typeName} '{text.Substring(0, Math.Min(50, text.Length))}' style={p.Style?.Alignment}");
                break;
            case PdfDividerModel d:
                Console.WriteLine($"  [{i}] {typeName} Thick={d.Thickness} SpBefore={d.SpaceBefore} SpAfter={d.SpaceAfter}");
                break;
            default:
                Console.WriteLine($"  [{i}] {typeName}");
                break;
        }
    }
}
