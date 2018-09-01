using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var user = await _repo.GetUser(userId);

            userParams.UserId = userId;

            userParams.Gender = string.IsNullOrEmpty(userParams.Gender) ? 
                (user.Gender.ToLower() == "male" ? "female" : "male") :
                userParams.Gender;

            var users = await _repo.GetUsers(userParams);

            var results = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage, users.TotalPages, users.PageSize, users.TotalCount);

            return Ok(results);
        }

        [HttpGet("{id}", Name="GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);

            var results = _mapper.Map<UserForDetailsDto>(user);

            return Ok(results);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserForUpdateDto userForUpdateDto)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(id != claimId) {
                return Unauthorized();
            }

            var userFromRepo = await _repo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if(await _repo.SaveAll())
            {
                return NoContent();
            }

            throw new Exception("Failed on save.");                            
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(id != claimId) {
                return Unauthorized();
            }

            var like = await _repo.GetLike(id, recipientId);

            if(like != null)
            {
                return BadRequest("Already liked.");
            }

            if(await _repo.GetUser(recipientId) == null)
                return NotFound();

            like = new Like{LikerId = id, LikeeId = recipientId};

            _repo.Add<Like>(like);

            if(await _repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Could not like.");
        }
    }
}