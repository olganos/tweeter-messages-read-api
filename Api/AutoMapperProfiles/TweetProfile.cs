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

        CreateMap<Reply, ReplyDto>()
            .ReverseMap();
    }
}