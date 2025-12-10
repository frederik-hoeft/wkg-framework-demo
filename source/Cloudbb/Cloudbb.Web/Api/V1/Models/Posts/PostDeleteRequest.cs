using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

public sealed record PostDeleteRequest([Required] Guid PostId);