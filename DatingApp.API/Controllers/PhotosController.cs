using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {            
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;

            Account account = new Account{
                Cloud = _cloudinaryConfig.Value.CloudName,
                ApiKey = _cloudinaryConfig.Value.ApiKey,
                ApiSecret = _cloudinaryConfig.Value.ApiSecret
            };

            _cloudinary = new Cloudinary(account);

        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photo = await _repo.GetPhoto(id);

            var result = _mapper.Map<PhotoForReturnDto>(photo);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm] PhotoForCreationDto photoForCreationDto)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(userId != claimId) {
                return Unauthorized();
            }

            var userFromRepo = await _repo.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                            .Width(500).Height(500)
                            .Crop("fill")
                            .Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo =_mapper.Map<Photo>(photoForCreationDto);

            photo.IsMain = !userFromRepo.Photos.Any(p => p.IsMain);

            userFromRepo.Photos.Add(photo);            

            if(await _repo.SaveAll())
            {
                var result = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new{ id = photo.Id}, result);
            }

            return BadRequest("Could not add photo.");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(userId != claimId) {
                return Unauthorized();
            }

            var userFromRepo = await _repo.GetUser(userId);
            
            if (!userFromRepo.Photos.Any(p => p.Id == id))
                return BadRequest("Photo does not exist");

            var photoFromRepo = await _repo.GetPhoto(id);
            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;
            photoFromRepo.IsMain = true;

            if(await _repo.SaveAll())
            {                
                return NoContent();
            }

            return BadRequest("Could modify photo.");
        }

        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            var claimId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(userId != claimId) {
                return Unauthorized();
            }

            var userFromRepo = await _repo.GetUser(userId);
            
            if (!userFromRepo.Photos.Any(p => p.Id == id))
                return BadRequest("Photo does not exist");

            var photoFromRepo = await _repo.GetPhoto(id);
            
            if(photoFromRepo.IsMain)
                return BadRequest("Cannot delete main photo.");

            if(photoFromRepo.PublicId != null)
            {
                var result = _cloudinary.Destroy(new DeletionParams(photoFromRepo.PublicId));    

                if (result.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }        
            }

            if(photoFromRepo.PublicId == null)
            {
                _repo.Delete(photoFromRepo);
            }            

            if(await _repo.SaveAll())
            {                
                return Ok();
            }
                
            return BadRequest("Could not delete photo.");
        }

    }
}