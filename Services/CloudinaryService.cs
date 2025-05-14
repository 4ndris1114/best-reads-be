using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System;

namespace BestReads.Services;

public class CloudinaryService {
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration) {
        var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    // Method to generate signature and timestamp
    public (string signature, string timestamp) GenerateSignature(string userId) {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        // Create the parameters dictionary with object type values
        var parameters = new System.Collections.Generic.Dictionary<string, object> {
            { "overwrite", "true" },
            { "public_id", userId },
            { "timestamp", timestamp },
        };

        // Generate the signature using the Cloudinary API
        var signature = _cloudinary.Api.SignParameters(parameters);

        return (signature, timestamp);
    }
}