using Api.Dto;

using AutoMapper;

using Core;
using Core.Entities;

using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/v1.0/tweets")]
    [ApiController]
    public class TweetsController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public TweetsController(
            IMessageRepository messageRepository,
            IMapper mapper)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<TweetDto>> All(CancellationToken cancellationToken)
        {
            var tweets = await _messageRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<List<TweetDto>>(tweets);
        }

        [HttpGet("{username}")]
        public async Task<List<TweetDto>> All(string username, CancellationToken cancellationToken)
        {
            var tweets = await _messageRepository.GetByUsernameAsync(username, cancellationToken);
            // todo: check the user
            return _mapper.Map<List<TweetDto>>(tweets);
        }
    }
}
