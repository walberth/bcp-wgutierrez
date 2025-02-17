using API_ORDER.Application.Order;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace API_ORDER.Endpoints
{
    public static class OrderEndpoints
    {
        public static RouteGroupBuilder MapOrders(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("/order");

            api.MapPost("/procesa", async (
                [FromServices] OrderHandler orderhandler,
                [FromBody] RequestDto information
            ) => await orderhandler.Process(information));

            return api;
        }
    }

    [JsonSerializable(typeof(RequestDto))]
    internal partial class OrderSerializerContext : JsonSerializerContext
    {
    }
}
