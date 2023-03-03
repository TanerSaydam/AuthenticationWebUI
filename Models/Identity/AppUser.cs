using Microsoft.AspNetCore.Identity;

namespace AuthenticationWebUI.Models.Identity;

public sealed class AppUser : IdentityUser<string>
{
    public string Name { get; set; }
    public string EmailConfirmValue { get; set; }    
    public bool IsForgotPasswordConfirmValueActive { get; set; }
    public string? ForgotPasswordConfirmValue { get; set; }
    public DateTime? ForgotPasswordSendEmailDate { get; set; }
}
