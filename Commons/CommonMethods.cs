using SendGrid.Helpers.Mail;
using SendGrid;
using AuthenticationWebUI.Models.Identity;
using GenericEmailService;
using AuthenticationWebUI.Context;
using AuthenticationWebUI.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationWebUI.Commons
{
    public static class CommonMethods
    {
        public static async Task SendEmailWithSendGrid(AppUser user, string type)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("tanersaydam@gmail.com", "Taner Saydam");            
            var to = new EmailAddress(user.Email, user.Name);

            var subject = "";
            var plainTextContent = "";
            var htmlContent = "";

            if(type == "ConfirmEmail") {
            subject = "AuthenticationMVC Mail Onayı!";
            plainTextContent = $"Mail adresinizi aşağıdaki linkle tıklayarak onaylayabilirsiniz. <a href='http://localhost:7014/account/confirmEmailValue?confirmValue={user.EmailConfirmValue}' target='_blank'>Maili Onayla</a>";
            htmlContent = $"<strong>Mail adresinizi aşağıdaki linkle tıklayarak onaylayabilirsiniz.</strong><br /> <a href='https://localhost:7221/Account/ConfirmEmailValue?confirmValue={user.EmailConfirmValue}' target='_blank'>Maili Onayla</a>";
            }
            else if(type == "ForgotPassword")
            {
                subject = "AuthenticationMVC Şifremi Unuttum!";
                plainTextContent = $"Aşağıdaki kodu sisteme girerek şifrenizi değiştirebilirsiniz. {user.ForgotPasswordConfirmValue}";
                htmlContent = $"<strong>Aşağıdaki kodu sisteme girerek şifrenizi değiştirebilirsiniz.</strong><br /> <h1>{user.ForgotPasswordConfirmValue}</h1>";
            }            


            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        public static async Task SendEmailWithHotmail(AppUser user, string type, AppDbContext context)
        {
            SendEmailModel sendEmailModel = new(
                emails: null,
                email: "authenticationtest001@hotmail.com",
                password: "Password12**",
                subject: "",
                body: "",
                smtp: "smtp.office365.com",
                html: true,
                ssl: true,
                port: 587,
                attachments: null);

            var subject = "";
            var plainTextContent = "";
            var htmlContent = "";

            if (type == "ConfirmEmail")
            {
                subject = "AuthenticationMVC Mail Onayı!";
                plainTextContent = $"Mail adresinizi aşağıdaki linkle tıklayarak onaylayabilirsiniz. <a href='http://localhost:7014/account/confirmEmailValue?confirmValue={user.EmailConfirmValue}' target='_blank'>Maili Onayla</a>";
                htmlContent = $"<strong>Mail adresinizi aşağıdaki linkle tıklayarak onaylayabilirsiniz.</strong><br /> <a href='https://localhost:7221/Account/ConfirmEmailValue?confirmValue={user.EmailConfirmValue}' target='_blank'>Maili Onayla</a>";
            }
            else if (type == "ForgotPassword")
            {
                subject = "AuthenticationMVC Şifremi Unuttum!";

                var emailTemplate = await context.Set<EmailTemplate>().Where(p => p.Title == "ForgotPassword").FirstOrDefaultAsync();
                htmlContent = emailTemplate.Body.Replace("$code", user.ForgotPasswordConfirmValue);

                //htmlContent = $"<strong>Aşağıdaki kodu sisteme girerek şifrenizi değiştirebilirsiniz.</strong><br /> <h1>{user.ForgotPasswordConfirmValue}</h1>";
            }

            sendEmailModel.Subject = subject;
            sendEmailModel.Body = htmlContent;
            sendEmailModel.Emails = new() { user.Email };

            await EmailService.SendEmailAsync(sendEmailModel);
        }
    }
}
