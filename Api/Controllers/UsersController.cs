using Api.Dto;

using AutoMapper;

using Core;

using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/v1.0/tweets")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet("users/all")]
        public async Task<List<UserDto>> All(CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<List<UserDto>>(users);
        }

        [HttpGet("user/search/{username}")]
        public async Task<List<UserDto>> SearchByPartialUsernamel(string partialUsername, CancellationToken cancellationToken)
        {
            var users = await _userRepository.SearchByPartialUsernameAsync(partialUsername, cancellationToken);
            // todo: check the user
            return _mapper.Map<List<UserDto>>(users);
        }
    }
}
