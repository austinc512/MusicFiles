using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicFiles.Core.DTOs.Request;
using MusicFiles.Core.DTOs.Response;
using MusicFiles.Core.DTOs.Shared;
using MusicFiles.Core.ServiceContracts;
using MusicFiles.Core.Services;

namespace MusicFiles.WebAPI.Controllers;

[ApiController]
[AllowAnonymous] // DELETE THIS ASAP
[Route("api/[controller]")]
public class MusicUploadController(IFileUploadService s3Service, IMusicDataService musicDataService) : Controller
{
    // client requests Presigned URL -> client posts to that URL -> client posts back information to the server
    
    // 1. requests Presigned URL
    [HttpPost("[action]")]
    public IActionResult RequestMediaUpload([FromBody]PreSignedUrlDto urlDto)
    {
        // Arguments: (string userPublicId, string fileName, TimeSpan expiration)
        // I'm using placeholder values for:
        // testPublicId (will be obtained through authentication)
        // testTimeSpan (can be dynamically set by client)
        
        var testPublicId = "abfd6d1d-f402-4dc4-b387-0a25b1391059";
        var testTimeSpan = TimeSpan.FromMinutes(1);
        
        var presignedUrl = s3Service.GeneratePreSignedUploadUrl(testPublicId, urlDto.FileName!,testTimeSpan);
        
        return Ok(presignedUrl);
    }
    
    // 2. client posts to that URL -- I need to test this action here or on the front end
    // I should do Auth before creating any front end.
    // 3. client posts back information to the server
    [HttpPost("[action]")]
    public async Task<IActionResult> CompleteMediaUpload([FromBody] MusicInfoDto sheetMusicRequest)
    {
        // again, user's PublicId will be exposed through authentication
        // We can still use the SheetMusicInfoDto class, and check whether the PublicId from the DTO matches the one returned through auth.
        // S3Key format: users/PublicId/FileName
        
        var s3Key = "users/" + sheetMusicRequest.UserPublicId + sheetMusicRequest.FileName!;

        try
        {
            var response = await musicDataService.CreateMusicMediaFile(sheetMusicRequest, s3Key);
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