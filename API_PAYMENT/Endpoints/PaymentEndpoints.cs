using API_PAYMENT.Application.Payment;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace API_PAYMENT.Endpoints
{
    public static class PaymentEndpoints
    {
        public static RouteGroupBuilder MapPayments(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("/payment");

            api.MapPost("/add", async (
                [FromServices] PaymentHandler paymenthandler,
                [FromBody] PaymentDto information
            ) => await paymenthandler.Add(information));

            api.MapGet("/", async (
                [FromServices] PaymentHandler paymenthandler
            ) => await paymenthandler.GetAll());

            return api;
        }
    }

    [JsonSerializable(typeof(PaymentDto))]
    internal partial class PaymentSerializerContext : JsonSerializerContext
    {
    }
}
