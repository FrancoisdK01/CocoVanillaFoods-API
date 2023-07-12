using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using MimeKit;
using QRCoder;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public TicketsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Tickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            return await _context.Tickets.ToListAsync();
        }

        // GET: api/Tickets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            return ticket;
        }

        // PUT: api/Tickets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket(int id, Ticket ticket)
        {
            if (id != ticket.TicketID)
            {
                return BadRequest();
            }

            _context.Entry(ticket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tickets
        [HttpPost]
        public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Generate a QR code
            var qrCode = GenerateQRCode($"http://yourwebsite.com/api/Tickets/{ticket.TicketID}");
            ticket.QRCode = qrCode;
            await _context.SaveChangesAsync();

            // Send the email
            await SendEmail("mclarenmarco998@gmail.com", qrCode);
            await SendEmail("u20444550@tuks.co.za", qrCode);
           

            return CreatedAtAction("GetTicket", new { id = ticket.TicketID }, ticket);
        }

        // DELETE: api/Tickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketID == id);
        }

        private string GenerateQRCode(string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using var bitmap = qrCode.GetGraphic(20);
            using var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            return Convert.ToBase64String(stream.ToArray());
        }

        private async Task SendEmail(string email, string qrCode)
        {
            var emailServer = "smtp.gmail.com";
            var emailPort = 587;
            var emailUser = "pfpfrancois2001@gmail.com";
            var emailPassword = "wupastwcxrbihgfa";

            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("The Promenade", emailUser));
            message.To.Add(new MailboxAddress("Ticket Recipient", email));
            message.Subject = "Your Ticket";

            // Create multipart/mixed parent to hold both the body and the attachment.
            var multipart = new Multipart("mixed");
            var body = new MultipartAlternative();

            body.Add(new TextPart("plain")
            {
                Text = $"Here is your ticket QR Code:\r\n\r\n{qrCode}"
            });

            body.Add(new TextPart("html")
            {
                Text = $"Here is your ticket QR Code:<br /><br /><img src=\"data:image/png;base64,{qrCode}\" />"
            });

            multipart.Add(body);

            // Create the attachment part.
            var qrCodeBytes = Convert.FromBase64String(qrCode);
            var attachment = new MimePart("image", "png")
            {
                Content = new MimeContent(new MemoryStream(qrCodeBytes), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "PromenadeTicket.png"
            };

            multipart.Add(attachment);

            message.Body = multipart;

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(emailServer, emailPort);
            await client.AuthenticateAsync(emailUser, emailPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

    }
}
