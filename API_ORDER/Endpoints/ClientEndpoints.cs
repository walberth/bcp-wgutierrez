using API_ORDER.Application.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace API_ORDER.Endpoints
{
    public static class ClientEndpoints
    {
        public static RouteGroupBuilder MapClients(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("/client");

            api.MapGet("/", async (
                [FromServices] ClientHandler clientHandler
            ) => await clientHandler.GetAll());

            return api;
        }
    }

    [JsonSerializable(typeof(ClientDto))]
    internal partial class ClientSerializerContext : JsonSerializerContext
    {
    }
}
