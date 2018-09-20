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
    [Route("api/users/{userId}/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name="GetMessage")]
        public async Task<IActionResult> GetMessage(int id)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(id != claimId) {
                return Unauthorized();
            }

            var message = await _repo.GetMessage(id);

            if(message ==  null)
                return NotFound();

            return Ok(message);
        }

        [HttpGet("thread/{recipientId}", Name="GetMessageThread")]
        public async Task<IActionResult> GetMessagesForConversation(int userId, int recipientId)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(userId != claimId) {
                return Unauthorized();
            }

            var messagesFromRepo = await _repo.GetMessageThread(userId, recipientId);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            return Ok(messages);

        }

        [HttpGet(Name="GetMessages")]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(userId != claimId) {
                return Unauthorized();
            }         

            messageParams.UserId = userId;  

            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage,
                messagesFromRepo.PageSize, 
                messagesFromRepo.TotalCount, 
                messagesFromRepo.TotalPages);           

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, [FromBody]MessageForCreationDto model)
        {
            var sender = await _repo.GetUser(userId);

            if(sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            model.SenderId = userId;

            var recipient = await _repo.GetUser(model.RecipientId);

            if(recipient ==  null)
                return BadRequest("Recipient doesn't exist.");

            var messageToCreate = _mapper.Map<Message>(model);
                
            _repo.Add<Message>(messageToCreate);

            if(await _repo.SaveAll())
            {
                var message = _mapper.Map<MessageToReturnDto>(messageToCreate);
                return CreatedAtRoute("GetMessage", new{ id = messageToCreate.Id}, message);
            }
            
            throw new Exception("Failed to create message");
        }   

        [HttpPost("{id}")]     
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(userId != claimId) {
                return Unauthorized();
            }     

            var messageFromRepo = await _repo.GetMessage(id);

            if(messageFromRepo.SenderId == userId)
            {
                messageFromRepo.SenderDeleted = true;
            }

            if(messageFromRepo.RecipientId == userId)
            {
                messageFromRepo.RecipientDeleted = true;
            }

            if(messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
            {
                _repo.Delete(messageFromRepo);                
            }

            if(await _repo.SaveAll())
            {
                return NoContent();    
            }
            
            throw new Exception("Failed to delete message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id, int userId)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(userId != claimId) {
                return Unauthorized();
            }     

            var messageFromRepo = await _repo.GetMessage(id);
            messageFromRepo.IsRead = true;
            messageFromRepo.DateRead = DateTime.Now;

            if(await _repo.SaveAll())
            {
                return NoContent();    
            }
            
            throw new Exception("Failed to delete message");
        }
    }
}