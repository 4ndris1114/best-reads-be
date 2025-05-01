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

    public async Task LogBookAddedToShelfAsync(string userId, string bookId, string bookTitle, string coverImage, string shelfName) {
        var activity = new Activity {
            UserId = userId,
            Type = Activity.ActivityType.AddedBookToShelf,
            CreatedAt = DateTime.UtcNow,
            Payload = new
            {
                BookId = bookId,
                BookTitle = bookTitle,
                CoverImage = coverImage,
                ShelfName = shelfName
            }
        };

        await _activityRepository.AddActivityAsync(activity);
    }

    public async Task LogBookReviewedAsync(string userId, string bookId, string bookTitle, string coverImage, double rating, string? reviewText, bool isUpdate) {
        var activity = new Activity {
            UserId = userId,
            Type = Activity.ActivityType.RatedBook,
            CreatedAt = DateTime.UtcNow,
            Payload = new {
                BookId = bookId,
                BookTitle = bookTitle,
                CoverImage = coverImage,
                Rating = rating,
                ReviewText = reviewText,
                IsUpdate = isUpdate
            }
        };

        await _activityRepository.AddActivityAsync(activity);
    }
}
