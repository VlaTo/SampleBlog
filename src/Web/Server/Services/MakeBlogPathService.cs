﻿using SampleBlog.Core.Application.Services;

namespace SampleBlog.Web.Server.Services;

internal sealed class MakeBlogPathService : IMakeBlogPathService
{
    public MakeBlogPathService()
    {
    }

    public ValueTask<string> BuildBlogPathAsync(string blogId)
    {
        throw new NotImplementedException();
    }
}