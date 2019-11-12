using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Route("api/users/{userId}/photos")]
    [Authorize]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private IDatingRepository _repo;
        private IMapper _mapper;
        private IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IOptions<CloudinarySettings> cloudinaryConfig, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;



            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }


        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int Id)
        {

            var photoFromRepo = await _repo.GetPhoto(Id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }


        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {

            // .... User from token mathces user from request route so I can't update photo of turd to sameone else's profile .....
            //...... User is controller property, NOT my User !!!! ....
            // ... NameIdentifier is a claim optional property assigned in Startup -> ConfigureSerrvices -> JWT initialization: new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()),
            //.... getting User id from claims
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            //... get my account
            var userFromRepo = await _repo.GetUser(userId);

            //... IFileForm represents a physical file sent with the HttpRequest (with lenth in bytes etc...)
            var file = photoForCreationDto.File;

            //... prepare pointer and instance  of RESULT for storing the result from cloud after upload
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    //... ImageUploadParams - Cloudinary libraries
                    var uploadParams = new ImageUploadParams()
                    {
                        //... FileDescription - Cloudinary libraries
                        File = new FileDescription(file.FileName, stream),
                        //... OPTIONALY if photo is too big Cloudinary will transform it and crop arounf the face and shit and store it .
                        Transformation = new Transformation()
                        .Width(500)
                        .Height(500)
                        .Crop("fill")
                        .Gravity("face")

                    };

                    //... UPLOADING the photo ! ...
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            //... POPULATE DTO by upload result
            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            //... and populate domain model
            var photo = _mapper.Map<Photo>(photoForCreationDto);

            //... find if there is MAIN PHOTO set already in User in Repostory we are pointed to,
            //... if not then set uploaded photo as MAIN
            if (!userFromRepo.Photos.Any(p => p.IsMain))
                photo.IsMain = true;

            //... add photo into photo-collection in User model ...
            userFromRepo.Photos.Add(photo);

            if (await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);

                //... result is f.e.: "http://res.cloudinary.com/cembo/image/upload/v1557112028/au7cvae5hyzvqbmrbgw1.jpg"
                var result = CreatedAtRoute("GetPhoto", new { id = photoToReturn.Id }, photoToReturn);

                return result;
            };

            return BadRequest("Could not add the photo.");
        }



        //.................................... Neil's approach ..................................
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("Photo is already Main");

            var currentMainPhoto = await _repo.GetMainPhoto(userId);

            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _repo.SaveAll())
                return NoContent(); // OR: Ok("Photo updated")

            return BadRequest("Photo couldn't be updated.");
        }

        //.................................... my approach .................
        //[HttpPost("{id}/setMain")]
        //public async Task<IActionResult> SetMainPhoto(int userId, int id)
        //{

        //    if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        //        return Unauthorized();

        //    var photo = await _repo.GetPhoto(id);
        //    if (photo == null)
        //        return BadRequest();

        //    if (photo.UserId != userId)
        //        return Unauthorized();

        //    var mainPhoto = await _repo.GetMainPhoto(userId);
        //    if (mainPhoto == null)
        //        return BadRequest();

        //    mainPhoto.IsMain = false;

        //    photo.IsMain = true;

        //    if (await _repo.SaveAll())
        //        return NoContent();

        //    return BadRequest("Photo couldn't be updated.");
        //}

          

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("You can't delete the main photo.");

            //... I haven't included delete from randomuser for clearer demonstration ...

            var deleteParam = new DeletionParams(photoFromRepo.PublicId);
            var result = _cloudinary.Destroy(deleteParam);

            if (result.Result == "ok")
                _repo.Delete(photoFromRepo);

            if (await _repo.SaveAll())
                return Ok(); //  ... string in 'return Ok("Photo was deleted !")' was causing strange server error response in Angular ...

            return BadRequest("Failed to delete the photo !");

        }

    }
}