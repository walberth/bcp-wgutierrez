using API_CONSULTATION.Domain.Consultation;
using MapsterMapper;

namespace API_CONSULTATION.Application.Consultation
{
    public class ConsultationHandler
    {
        private readonly IMapper _mapper;
        private readonly IConsultationRepository _consultationRepository;

        public ConsultationHandler(
            IMapper mapper,
            IConsultationRepository consultationRepository)
        {
            _mapper = mapper;
            _consultationRepository = consultationRepository;
        }

        public async Task<IEnumerable<ConsultationDto>> GetAll()
        {
            var payments = await _consultationRepository.GetAll();

            return _mapper.Map<IEnumerable<ConsultationDto>>(payments);
        }
    }
}
