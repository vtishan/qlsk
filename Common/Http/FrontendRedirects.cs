using Microsoft.AspNetCore.Rewrite;

namespace QLSK.Common.Http;

internal static class FrontendRedirects
{
    // Older bookmarks, catalog image URLs and cached shells still use these paths.
    public static RewriteOptions FromPreviousLayout() => new RewriteOptions()
        .AddRedirect(@"^giaodienmau\.html$", "giaodientest/giaodienmau.html", 301)
        .AddRedirect(@"^(?:giaodientest/)?images/([\w-]+\.webp)$", "assets/images/$1", 301)
        .AddRedirect(@"^(?:giaodientest/)?(venue-[\w-]+\.js)$", "giaodientest/js/$1", 301)
        .AddRedirect(@"^(?:giaodientest/)?venue\.css$", "giaodientest/css/venue.css", 301)
        .AddRedirect(@"^views/(event|venue)\.html$", "features/$1s/$1.html", 301)
        .AddRedirect(@"^views/export\.html$", "features/reports/export.html", 301)
        .AddRedirect(@"^views/work-overview\.html$", "features/work/work-overview.html", 301)
        .AddRedirect(@"^views/([a-z]+)\.html$", "features/$1/$1.html", 301)
        .AddRedirect(@"^js/core/(app|api)\.js$", "app/$1.js", 301)
        .AddRedirect(@"^js/components/([a-z]+)\.js$", "shared/ui/$1.js", 301)
        .AddRedirect(@"^js/utils/date\.js$", "shared/date.js", 301)
        .AddRedirect(@"^js/modules/(work-context|work-sections)\.js$", "features/work/$1.js", 301)
        .AddRedirect(@"^js/modules/([a-z]+)\.js$", "features/$1/$1.js", 301)
        .AddRedirect(@"^css/pages/catalog\.css$", "shared/styles/catalog.css", 301)
        .AddRedirect(@"^css/pages/([a-z]+)\.css$", "features/$1/$1.css", 301)
        .AddRedirect(@"^css/(base|layout|components)\.css$", "shared/styles/$1.css", 301);
}
