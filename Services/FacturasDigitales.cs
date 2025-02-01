using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;

namespace SODERIA_I.Services
{
    public class FacturasDigitales
    {
        public byte[] GenerarFactura(string cliente, List<(string tipoCompra, int cantidad, decimal precio)> compras, decimal total)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var pdfDocument = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .AlignCenter()
                        .Text("Factura Digital")
                        .FontSize(20)
                        .Bold();

                    page.Content()
                        .Column(column =>
                        {
                            column.Item().Text($"Cliente: {cliente}").FontSize(14).Bold();
                            column.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(12);

                            column.Item().LineHorizontal(1);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(200); // Tipo de Compra
                                    columns.ConstantColumn(100); // Cantidad
                                    columns.RelativeColumn();    // Precio
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Tipo de Compra").Bold();
                                    header.Cell().Text("Cantidad").Bold();
                                    header.Cell().Text("Precio").Bold();
                                });

                                foreach (var compra in compras)
                                {
                                    table.Cell().Text(compra.tipoCompra);
                                    table.Cell().Text(compra.cantidad.ToString());
                                    table.Cell().Text($"{compra.precio:C}");
                                }
                            });

                            column.Item().LineHorizontal(1);
                            column.Item().AlignRight().Text($"Total: {total:C}").FontSize(14).Bold();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Gracias por su compra. - SODERIA_I");
                });
            });

            using var stream = new MemoryStream();
            pdfDocument.GeneratePdf(stream);
            return stream.ToArray();
        }
    }
}
