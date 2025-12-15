using Cloudbb.Client.Models;
using Cloudbb.Client.Services.Network;
using System.Text.Json;

namespace Cloudbb.Client.Services.Comments;

internal sealed class CommentService(IHttpClientFactory httpClientFactory, JsonSerializerOptions jsonOptions) 
    : ApiService(httpClientFactory, jsonOptions), ICommentService
{
    public Task<CommentCreationResponse?> CreateCommentAsync(CommentCreationRequest request) =>
        PostAsync<CommentCreationRequest, CommentCreationResponse>("api/v1/comments/create", request);

    public Task<bool> DeleteCommentAsync(CommentDeleteRequest request) =>
        PostAsync("api/v1/comments/delete", request);

    public Task<CommentVoteResponse?> VoteCommentAsync(CommentVoteRequest request) =>
        PostAsync<CommentVoteRequest, CommentVoteResponse>("api/v1/comments/vote", request);
}