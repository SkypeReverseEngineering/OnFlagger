public static class StaticHTML
{
    public const string SERVER_BRANDING = $"<hr/><p>vlOd's SkypeFlagsServer v{SkypeFlagsServer.SERVER_VERSION}</p>";
    public const string HTML_INFO_PAGE = @$"
        <!Doctype HTML>
        <html>
            <head>
                <title>Hello, world!</title>
            </head>
            <body>
                <h1>Hello, world!</h1>
                <p>If you see this message, that means you have configured the server correctly!</p>
                {SERVER_BRANDING}
            </body>
        </html>
    ";
    public const string HTML_404 = @$"
        <!Doctype HTML>
        <html>
            <head>
                <title>Page not found</title>
            </head>
            <body>
                <h1>Page not found</h1>
                <p>The requested page could not be found</p>
                {SERVER_BRANDING}
            </body>
        </html>
    ";
}