using System.Net.Http.Json;
using System.Text.Json;
using Cloudbb.Client.Models;

namespace Cloudbb.Client.Services.Comments;

internal sealed class CommentService
(
    HttpClient httpClient,
    JsonSerializerOptions jsonOptions
) : ICommentService
{
    public async Task<CommentCreationResponse?> CreateCommentAsync(CommentCreationRequest request)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/comments/create", request, jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<CommentCreationResponse>(jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteCommentAsync(CommentDeleteRequest request)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/comments/delete", request, jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> VoteCommentAsync(CommentVoteRequest request)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/comments/vote", request, jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
