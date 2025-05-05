using BestReads.Models;
using BestReads.Repositories;
using System;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace BestReads.Services;

public class ActivityService {
    private readonly ActivityRepository _activityRepository;

    public ActivityService(ActivityRepository activityRepository) {
        _activityRepository = activityRepository;
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

        return await _activityRepository.AddActivityAsync(activity);
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
