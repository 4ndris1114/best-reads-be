using BestReads.Models;
using BestReads.Repositories;
using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using Microsoft.AspNetCore.SignalR;
using BestReads.Hubs;

namespace BestReads.Services;

public class ActivityService {
    private readonly ActivityRepository _activityRepository;
    private readonly IHubContext<ActivityHub> _hubContext;

    public ActivityService(ActivityRepository activityRepository, IHubContext<ActivityHub> hubContext) {
        _activityRepository = activityRepository;
        _hubContext = hubContext;
    }

    public async Task<string?> LogBookAddedToShelfAsync(string userId, string bookId, string bookTitle, string coverImage, bool isUpdate, string? sourceShelfName, string? targetShelfName) {
        var activity = new Activity {
            UserId = userId,
            Type = Activity.ActivityType.AddedBookToShelf,
            BookId = bookId,
            CreatedAt = DateTime.UtcNow,
            Payload = new
            {
                BookTitle = bookTitle,
                CoverImage = coverImage,
                TargetShelfName = targetShelfName,
                SourceShelfName = isUpdate ? sourceShelfName : null,
                IsUpdate = isUpdate
            }
        };

        try {
            var id = await _activityRepository.AddActivityAsync(activity);

            if (id != null) {
                // Send the full activity to all clients
                await _hubContext.Clients.AllExcept(Context.ConnectionId).SendAsync("ReceiveActivity", activity);
            }
            return id;
        } catch (Exception ex) {
            Console.WriteLine($"Error logging activity: {ex}");
            return null;
        }
    }

    public async Task<string?> LogBookReviewedAsync(string userId, string bookId, string bookTitle, string coverImage, double rating, string? reviewText, bool isUpdate) {
        var activity = new Activity {            
            UserId = userId,
            Type = Activity.ActivityType.RatedBook,
            BookId = bookId,
            CreatedAt = DateTime.UtcNow,
            Payload = new {
                BookTitle = bookTitle,
                CoverImage = coverImage,
                Rating = rating,
                ReviewText = reviewText,
                IsUpdate = isUpdate
            }
        };

        return await _activityRepository.AddActivityAsync(activity);
    }
}
