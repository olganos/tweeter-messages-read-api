using Api.Dto;

using AutoMapper;

using Core.Entities;

namespace AutoMapperProfiles;

public class TweetProfile : Profile
{
    public TweetProfile()
    {
        AllowNullCollections = true;
        CreateMap<Tweet, TweetDto>()
             .ForMember(d => d.Likes, opt => opt.ConvertUsing(new CurrencyFormatter(), src => src.Likes));

        CreateMap<TweetDto, Tweet>();

        CreateMap<Reply, ReplyDto>()
            .ReverseMap();

        CreateMap<User, UserDto>()
           .ReverseMap();


    }

    public class CurrencyFormatter : IValueConverter<List<Like>, int>
    {
        public int Convert(List<Like> source, ResolutionContext context)
            => source.Count;
    }
}