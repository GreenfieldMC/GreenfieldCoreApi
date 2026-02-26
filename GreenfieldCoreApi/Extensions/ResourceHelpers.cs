using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Extensions;

public class ResourceHelpers
{
    private const string RedirectScript = """

                                                  <script>
                                                      setTimeout(() => {
                                                          window.location.href = '{{REDIRECT}}';
                                                      }, 1000);
                                                  </script>
                                          """;

    public static ContentResult Redirect(RedirectType type, string? redirectUrl, string message, string? submessage = null)
    {
        var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Redirect.html");
        var htmlContent = File.ReadAllText(htmlPath);
        
        htmlContent = htmlContent.Replace("{{REDIRECT_SCRIPT}}", redirectUrl == null ? string.Empty : RedirectScript.Replace("{{REDIRECT}}", redirectUrl));
        htmlContent = htmlContent.Replace("{{MESSAGE}}", message);
        htmlContent = htmlContent.Replace("{{BODYSTYLE}}", type.ToString().ToLower());
        htmlContent = htmlContent.Replace("{{SUBMESSAGE}}", submessage ?? string.Empty);
        
        return new ContentResult
        {
            ContentType = "text/html",
            Content = htmlContent
        };
        
    }

}

public enum RedirectType 
{
    Error,
    Info
}