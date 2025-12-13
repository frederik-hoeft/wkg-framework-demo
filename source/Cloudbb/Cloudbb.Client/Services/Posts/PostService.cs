using System.Net.Http.Json;
using System.Text.Json;
using Cloudbb.Client.Models;

namespace Cloudbb.Client.Services.Posts;

internal sealed class PostService
(
    HttpClient httpClient,
    JsonSerializerOptions jsonOptions
) : IPostService
{
    public async Task<PostListResponse?> GetPostsAsync(PostListRequest request)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/posts/list", request, jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<PostListResponse>(jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PostReadResponse?> GetPostAsync(PostReadRequest request)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/posts/read", request, jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<PostReadResponse>(jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PostCreationResponse?> CreatePostAsync(PostCreationRequest request)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/posts/create", request, jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<PostCreationResponse>(jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<PostEditResponse?> EditPostAsync(PostEditRequest request)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/posts/edit", request, jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<PostEditResponse>(jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeletePostAsync(PostDeleteRequest request)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/posts/delete", request, jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> VotePostAsync(PostVoteRequest request)
    {
        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/posts/vote", request, jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
