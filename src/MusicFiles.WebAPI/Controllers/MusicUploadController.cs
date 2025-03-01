using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicFiles.Core.DTOs.Request;
using MusicFiles.Core.DTOs.Response;
using MusicFiles.Core.DTOs.Shared;
using MusicFiles.Core.ServiceContracts;
using MusicFiles.Core.Services;

namespace MusicFiles.WebAPI.Controllers;

[ApiController]
// [AllowAnonymous] // note: this endpoint works through authentication now
// I'll keep this commented out as a reminder for when shit goes wrong (inevitably). 
[Route("api/[controller]/[action]")]
public class MusicUploadController : Controller
{
    
    // TO-DO: IMPLEMENT LOGGER CLASS AND ERROR HANDLING MIDDLEWARE
    
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileUploadService _s3Service;
    private readonly IMusicDataService _musicDataService;

    public MusicUploadController(IFileUploadService s3Service, IMusicDataService musicDataService,ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
        _s3Service = s3Service;
        _musicDataService = musicDataService;
    }
    // client requests Presigned URL -> client posts to that URL -> client posts back information to the server
    
    // 1. requests Presigned URL
    [HttpPost]
    public IActionResult RequestMediaUpload([FromBody]PreSignedUrlDto urlDto)
    {
        // Arguments: (string userPublicId, string fileName, TimeSpan expiration)
        // testTimeSpan (could be dynamically set by client, but is currently a placeholder value)
        
        var publicUserId = _currentUserService.PublicUserId;
        if (publicUserId is null)
        {
            // need consistent API for error handling, similar to JwtService.
            // we're getting to the point where this consistent API could be another middleware
            // instead of implementing a private method for error handling in each controller class.
            return BadRequest("User id is null, you idiot!");
        }
        
        var testTimeSpan = TimeSpan.FromMinutes(1);
        
        var presignedUrl = _s3Service.GeneratePreSignedUploadUrl(publicUserId, urlDto.FileName!,testTimeSpan);

        Console.WriteLine($"Presigned URL generated from {publicUserId}, {urlDto.FileName}, and {testTimeSpan}"
             + "\n"
             + $"presignedUrl: {presignedUrl}"
            );
        
        // does this type of response need to be JSON, or can we just yeet a URL? Haven't decided yet.
        return Ok(presignedUrl);
    }
    
    // 2. client posts to that URL -- I need to test this action here or on the front end
    // I should do Auth before creating any front end.
    // 3. client posts back information to the server
    [HttpPost]
    public async Task<IActionResult> CompleteMediaUpload([FromBody] MusicInfoDto sheetMusicRequest)
    {
        // again, user's PublicId will be exposed through authentication
        // We can still use the SheetMusicInfoDto class, and check whether the PublicId from the DTO matches the one returned through auth.
        // S3Key format: users/PublicId/FileName
        
        var s3Key = "users/" + sheetMusicRequest.UserPublicId + sheetMusicRequest.FileName!;

        try
        {
            var response = await _musicDataService.CreateMusicMediaFile(sheetMusicRequest, s3Key);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    // Step 3 should be replaced:
    // file upload completes -> trigger from S3 posts back to a lambda that will queue responses 
    // -> queue notifies clients somehow (through my primary server or directly from the lambda?)
    // this is all much easier said than done
}