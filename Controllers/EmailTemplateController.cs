using AuthenticationWebUI.Context;
using AuthenticationWebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationWebUI.Controllers
{
    public class EmailTemplateController : Controller
    {
        private readonly AppDbContext _context;

        public EmailTemplateController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(string title, string body)
        {
            EmailTemplate emailTemplate = await _context.Set<EmailTemplate>().Where(p => p.Title == title).FirstOrDefaultAsync();

            if(emailTemplate == null)
            {
                emailTemplate = new EmailTemplate();
                emailTemplate.Id = Guid.NewGuid();
                emailTemplate.Title = title;
                emailTemplate.Body = body;

                await _context.Set<EmailTemplate>().AddAsync(emailTemplate);
                await _context.SaveChangesAsync();
            }
            else
            {
                emailTemplate.Title = title;
                emailTemplate.Body = body;

                await _context.SaveChangesAsync();
            }
            return View();
        }
    }
}
