using FacturasSRI.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;

namespace FacturasSRI.Web.Endpoints
{
    public static class ReportEndpoints
    {
        public static void MapReportEndpoints(this IEndpointRouteBuilder app)
        {
            var reportGroup = app.MapGroup("/api/reports").WithTags("Reports");

            reportGroup.MapGet("/sales/by-period", async (IReportService reportService, DateTime? startDate, DateTime? endDate) =>
            {
                var finalStartDate = (startDate ?? DateTime.Now.AddMonths(-1)).ToUniversalTime();
                var finalEndDate = (endDate ?? DateTime.Now).ToUniversalTime();

                var result = await reportService.GetVentasPorPeriodoAsync(finalStartDate, finalEndDate);
                return Results.Ok(result);
            })
            .WithName("GetSalesByPeriodReport")
            .Produces(200, typeof(IEnumerable<FacturasSRI.Application.Dtos.Reports.VentasPorPeriodoDto>));

            reportGroup.MapGet("/sales/by-product", async (IReportService reportService, DateTime? startDate, DateTime? endDate) =>
            {
                var finalStartDate = (startDate ?? DateTime.Now.AddMonths(-1)).ToUniversalTime();
                var finalEndDate = (endDate ?? DateTime.Now).ToUniversalTime();

                var result = await reportService.GetVentasPorProductoAsync(finalStartDate, finalEndDate);
                return Results.Ok(result);
            })
            .WithName("GetSalesByProductReport")
            .Produces(200, typeof(IEnumerable<FacturasSRI.Application.Dtos.Reports.VentasPorProductoDto>));

            reportGroup.MapGet("/sales/customer-activity", async (IReportService reportService, DateTime? startDate, DateTime? endDate) =>
            {
                var finalStartDate = (startDate ?? DateTime.Now.AddMonths(-1)).ToUniversalTime();
                var finalEndDate = (endDate ?? DateTime.Now).ToUniversalTime();

                var result = await reportService.GetActividadClientesAsync(finalStartDate, finalEndDate);
                return Results.Ok(result);
            })
            .WithName("GetCustomerActivityReport")
            .Produces(200, typeof(IEnumerable<FacturasSRI.Application.Dtos.Reports.ClienteActividadDto>));

            reportGroup.MapGet("/sales/accounts-receivable", async (IReportService reportService) =>
            {
                var result = await reportService.GetCuentasPorCobrarAsync();
                return Results.Ok(result);
            })
            .WithName("GetAccountsReceivableReport")
            .Produces(200, typeof(IEnumerable<FacturasSRI.Application.Dtos.Reports.CuentasPorCobrarDto>));

            reportGroup.MapGet("/sales/credit-notes", async (IReportService reportService, DateTime? startDate, DateTime? endDate) =>
            {
                var finalStartDate = (startDate ?? DateTime.Now.AddMonths(-1)).ToUniversalTime();
                var finalEndDate = (endDate ?? DateTime.Now).ToUniversalTime();

                var result = await reportService.GetNotasDeCreditoAsync(finalStartDate, finalEndDate);
                return Results.Ok(result);
            })
            .WithName("GetCreditNotesReport")
            .Produces(200, typeof(IEnumerable<FacturasSRI.Application.Dtos.Reports.NotasDeCreditoReportDto>));
        }
    }
}
