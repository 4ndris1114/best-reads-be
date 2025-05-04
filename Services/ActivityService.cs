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

    public async Task<string?> LogBookAddedToShelfAsync(string userId, string bookId, string bookTitle, string coverImage, string shelfName) {
        var activity = new Activity {
            UserId = userId,
            Type = Activity.ActivityType.AddedBookToShelf,
            BookId = bookId,
            CreatedAt = DateTime.UtcNow,
            Payload = new
            {
                BookTitle = bookTitle,
                CoverImage = coverImage,
                ShelfName = shelfName
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
        if (isUpdate) {
            var activityId = await _activityRepository.GetActivityByUserAndBookIdAsync(userId, bookId).ContinueWith(t => t.Result?.Id);
            return await _activityRepository.UpdateActivityAsync(activityId, activity);
        } else {
            return await _activityRepository.AddActivityAsync(activity);
        }
    }
}
