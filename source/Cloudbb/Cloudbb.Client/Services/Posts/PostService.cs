using Cloudbb.Client.Models;
using Cloudbb.Client.Services.Network;
using System.Text.Json;

namespace Cloudbb.Client.Services.Posts;

internal sealed class PostService(IHttpClientFactory httpClientFactory, JsonSerializerOptions jsonOptions) 
    : ApiService(httpClientFactory, jsonOptions), IPostService
{
    public Task<PostListResponse?> GetPostsAsync(PostListRequest request) => 
        PostAsync<PostListRequest, PostListResponse>("api/v1/posts/list", request);

    public Task<PostReadResponse?> GetPostAsync(PostReadRequest request) => 
        PostAsync<PostReadRequest, PostReadResponse>("api/v1/posts/read", request);

    public Task<PostCreationResponse?> CreatePostAsync(PostCreationRequest request) => 
        PostAsync<PostCreationRequest, PostCreationResponse>("api/v1/posts/create", request);

    public Task<PostEditResponse?> EditPostAsync(PostEditRequest request) => 
        PostAsync<PostEditRequest, PostEditResponse>("api/v1/posts/edit", request);

    public Task<bool> DeletePostAsync(PostDeleteRequest request) =>
        PostAsync("api/v1/posts/delete", request);

    public Task<PostVoteResponse?> VotePostAsync(PostVoteRequest request) => 
        PostAsync<PostVoteRequest, PostVoteResponse>("api/v1/posts/vote", request);
}
