using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Extensions;

public class ResourceHelpers
{

    public static ContentResult Redirect(RedirectType type, string redirectUrl, string message)
    {
        var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Redirect.html");
        var htmlContent = File.ReadAllText(htmlPath);
        htmlContent = htmlContent.Replace("{{REDIRECT_URL}}", redirectUrl);
        htmlContent = htmlContent.Replace("{{MESSAGE}}", message);
        htmlContent = htmlContent.Replace("{{BODYSTYLE}}", type.ToString().ToLower());
        
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