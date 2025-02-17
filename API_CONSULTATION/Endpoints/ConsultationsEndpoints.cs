using API_CONSULTATION.Application.Consultation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace API_CONSULTATION.Endpoints
{
    public static class ConsultationsEndpoints
    {
        public static RouteGroupBuilder MapConsultations(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("/consultations");

            api.MapGet("/", async (
                [FromServices] ConsultationHandler consultationhandler
            ) => await consultationhandler.GetAll());

            return api;
        }
    }

    [JsonSerializable(typeof(ConsultationDto))]
    internal partial class ConsultationSerializerContext : JsonSerializerContext
    {
    }
}
