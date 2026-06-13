using AutoMapper;
using Pharmacy_API.Dtos.Question;
using Pharmacy_API.Models.Question;

namespace Pharmacy_API.MapperProfiles.Question
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            // Map ProductQuestion -> QuestionDto
            CreateMap<ProductQuestion, QuestionDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd HH:mm")))
                .ForMember(dest => dest.Tag, opt => opt.Ignore()); // Tag sẽ được set trong service

            // Map ProductAnswer -> AnswerDto
            CreateMap<ProductAnswer, AnswerDto>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd HH:mm")));
        }
    }
}