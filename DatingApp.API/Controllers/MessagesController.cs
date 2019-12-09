using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [ApiController]
    [Route("api/users/{userId}/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }


        //... 'userId' is from Controller ROUTE, 'id' is from this action QUERY STRING.
        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _repo.GetMessage(id);

            if (message == null)
                return NotFound();

            return Ok(message);
        }



        //... [HttpGet("{id}")] - would conflict with GetMessage signature
        //... 'userId' is from Controller ROUTE, 'messageParams' is from action QUERY STRING
        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            //... passing 'userId' from Controller's ROUTE into 'messageParams' IF action QUERY string is MISSING
            messageParams.UserId = userId;

            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

            var messagesToReturn = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

            return Ok(messagesToReturn);

        }


        //... 'thread' in 'thread/{recipientId}' prevents conflict with the REQUEST signature '{id}'
        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messages = await _repo.GetMessageThread(userId, recipientId);

            var messagesToReturn = _mapper.Map<IEnumerable<MessageToReturnDto>>(messages);

            return Ok(messagesToReturn);
        }





        // ... no paramater in attribute, sender's id is retrieved from controller's route "api/users/{userId}/[controller]"
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {

            var sender = await _repo.GetUser(userId);

            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // ... copying senderId from query to model in body ???
            messageForCreationDto.SenderId = userId;

            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
                return NotFound("Recipient not found !");


            var message = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add(message);


            if (await _repo.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);
            }

            throw new Exception("Creating the message failed on Save !");

        }


        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _repo.GetMessage(id);

            if (message.SenderId == userId)
                message.DeletedBySender = true;

            if (message.RecipientId == userId)
                message.DeletedByRecipient = true;

            if (message.DeletedBySender && message.DeletedByRecipient )
                _repo.Delete(message);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception("Message was not deleted !");
        }


        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int id, int userId)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _repo.GetMessage(id);

            if (message.RecipientId != userId)
                return Unauthorized();


            message.IsRead = true;
            message.MessageRead = DateTime.Now;

            await _repo.SaveAll();

            return NoContent();
        
        }


    }
}
