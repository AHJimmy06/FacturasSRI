using FacturasSRI.Application.Dtos.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting; // Needed for IWebHostEnvironment
using System.Globalization; // For CultureInfo

namespace FacturasSRI.Infrastructure.Services
{
    public class ReportPdfGeneratorService
    {
        private readonly IWebHostEnvironment _env;
        private readonly CultureInfo esEcCulture = new CultureInfo("es-EC");

        public ReportPdfGeneratorService(IWebHostEnvironment env)
        {
            _env = env;
            QuestPDF.Settings.License = LicenseType.Community; // Ensure license is set
        }

        public byte[] GenerateVentasPorPeriodoPdf(IEnumerable<VentasPorPeriodoDto> data, DateTime startDate, DateTime endDate)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                    page.Header().Element(compose => ComposeHeader(compose, "Reporte de Ventas por Período", startDate, endDate));
                    page.Content().Element(compose => ComposeContent(compose, data));
                    page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            }).GeneratePdf();
        }

        private void ComposeHeader(IContainer container, string reportTitle, DateTime startDate, DateTime endDate)
        {
            container.PaddingBottom(10).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).Column(col => // Increased relative size for logo/company name
                    {
                        var logoPath = Path.Combine(_env.WebRootPath, "logo.png");
                        if (File.Exists(logoPath))
                        {
                            col.Item().Height(50).Width(150).Image(logoPath).FitArea();
                        }
                        col.Item().Text("Aether Tecnologías").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                    });

                    row.RelativeItem(3).AlignRight().Column(col => // Report title and dates on the right
                    {
                        col.Item().Text(reportTitle).Bold().FontSize(16).FontColor(Colors.Blue.Darken2); // Changed color from Red to Blue
                        col.Item().Text($"Período: {startDate.ToString("dd/MM/yyyy")} - {endDate.ToString("dd/MM/yyyy")}").FontSize(10);
                        col.Item().Text($"Fecha de Generación: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}").FontSize(8);
                    });
                });
                column.Item().PaddingTop(5).LineHorizontal(1); // Slightly thicker line below header
            });
        }

        private void ComposeContent(IContainer container, IEnumerable<VentasPorPeriodoDto> data)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.5f); // Fecha
                    columns.RelativeColumn(1.5f); // CantidadFacturas
                    columns.RelativeColumn(2); // Subtotal
                    columns.RelativeColumn(1.5f); // TotalIva
                    columns.RelativeColumn(2); // Total
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Fecha");
                    header.Cell().Element(HeaderCellStyle).Text("Facturas Emitidas");
                    header.Cell().Element(HeaderCellStyle).Text("Subtotal");
                    header.Cell().Element(HeaderCellStyle).Text("Total IVA");
                    header.Cell().Element(HeaderCellStyle).Text("Total Vendido");

                    // No need for LineHorizontal here, HeaderCellStyle takes care of bottom border
                });

                foreach (var item in data)
                {
                    table.Cell().Element(DataCellStyle).Text(item.Fecha.ToString("dd/MM/yyyy"));
                    table.Cell().Element(DataCellStyle).AlignRight().Text(item.CantidadFacturas.ToString());
                    table.Cell().Element(DataCellStyle).AlignRight().Text(item.Subtotal.ToString("C", esEcCulture));
                    table.Cell().Element(DataCellStyle).AlignRight().Text(item.TotalIva.ToString("C", esEcCulture));
                    table.Cell().Element(DataCellStyle).AlignRight().Text(item.Total.ToString("C", esEcCulture));
                }

                table.Cell().ColumnSpan(5).Element(TotalsRowStyle).Row(row =>
                {
                    row.RelativeItem(1.5f).AlignRight().Text("Totales:").Bold();
                    row.RelativeItem(1.5f).AlignRight().Text(data.Sum(x => x.CantidadFacturas).ToString()).Bold();
                    row.RelativeItem(2).AlignRight().Text(data.Sum(x => x.Subtotal).ToString("C", esEcCulture)).Bold();
                    row.RelativeItem(1.5f).AlignRight().Text(data.Sum(x => x.TotalIva).ToString("C", esEcCulture)).Bold();
                    row.RelativeItem(2).AlignRight().Text(data.Sum(x => x.Total).ToString("C", esEcCulture)).Bold();
                });
            });
        }

        // Helper styles
        static IContainer HeaderCellStyle(IContainer container) => 
            container.DefaultTextStyle(x => x.Bold().FontSize(10)).BorderBottom(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(5).AlignCenter();
        
        static IContainer DataCellStyle(IContainer container) => 
            container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).PaddingHorizontal(5); // Lighter border for data rows

        static IContainer TotalsRowStyle(IContainer container) => 
            container.BorderTop(1).BorderBottom(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4).PaddingVertical(5); // Stronger borders for totals row
    }
}
