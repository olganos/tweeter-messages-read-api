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
            .ReverseMap();

        CreateMap<Tweet, FullTweetDto>()
           .ReverseMap();

        CreateMap<Reply, ReplyDto>()
            .ReverseMap();

        CreateMap<User, UserDto>()
           .ReverseMap();
    }
}