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
                    page.Size(PageSizes.A5);
                    page.Margin(30);

                    // Aplicar fuente global para el documento
                    page.DefaultTextStyle(x => x.FontFamily("Nunito Sans").FontSize(12));

                    // Aplicar "Josefin Sans" al título
                    page.Header()
                        .AlignCenter()
                        .Text("Soderia Crystal")
                        .FontSize(35)
                        .FontFamily("Josefin Sans")
                        .Bold();

                    page.Content()
                        .Column(column =>
                        {
                            column.Item().PaddingTop(15);
                            column.Item().Text("Factura digital").FontSize(10);
                            column.Item().PaddingBottom(5);

                            column.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(12).Bold();
                            column.Item().Text($"Cliente: {cliente}").FontSize(12).Bold();
                            column.Item().PaddingBottom(5);

                            column.Item().LineHorizontal(1);
                            column.Item().PaddingBottom(5);

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
                                    header.Cell().Text("Precio u/").Bold();
                                });
                                column.Item().PaddingBottom(5);

                                foreach (var compra in compras)
                                {
                                    table.Cell().Text(compra.tipoCompra);
                                    table.Cell().Text(compra.cantidad.ToString());
                                    table.Cell().Text($"{compra.precio:C}");
                                }
                            });

                            column.Item().PaddingBottom(10);
                            column.Item().LineHorizontal(1);
                            column.Item().PaddingTop(10);
                            column.Item().AlignRight().Text($"Total: {total:C}").FontSize(16).Bold();
                            column.Item().PaddingBottom(10);
                            column.Item().Text("MercadoPago ALIAS:    SODA.CRYSTAL").FontSize(12);
                            column.Item().Text("Titular:              Gustavo José Olocco").FontSize(12);
                            column.Item().PaddingBottom(10);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Muchas gracias por su compra. Soderia Crystal");
                });
            });

            using var stream = new MemoryStream();
            pdfDocument.GeneratePdf(stream);
            return stream.ToArray();
        }
    }
}
