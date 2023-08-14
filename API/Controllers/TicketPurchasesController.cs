using API.Data;
using API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using QRCoder;
using static QRCoder.PayloadGenerator;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketPurchasesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public TicketPurchasesController(MyDbContext context)
        {
            _context = context;
        }

        // POST: api/Tickets
        [HttpPost]
        public async Task<ActionResult<TicketPurchase>> PostTicket(TicketPurchase ticket)
        {
            _context.TicketPurchases.Add(ticket);
            await _context.SaveChangesAsync();

            // Generate a QR code
            var qrCode = GenerateQRCode($"http://localhost:4200/Tickets/Scan/{ticket.ScanningToken}");

            //ticket.QRCode = qrCode;
            await _context.SaveChangesAsync();

            // Get the customer associated with the ticket
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == ticket.UserEmail);

            if (customer != null)
            {
                // Send the email to the customer
                await SendEmail(customer.Email, qrCode, customer);
            }

            ticket.ScanningToken = Guid.NewGuid().ToString();


            return CreatedAtAction("GetTicket", new { id = ticket.UserEmail }, ticket);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<TicketPurchase>> GetTicketPurchase(int id)
        {
            var ticketPurchase = await _context.TicketPurchases.FindAsync(id);

            if (ticketPurchase == null)
            {
                return NotFound();
            }

            return ticketPurchase;
        }

        // New method to get all purchases for a user
        [HttpGet("User/{userEmail}")]
        public async Task<ActionResult<IEnumerable<TicketPurchase>>> GetPurchasesForUser(string userEmail)
        {
            var ticketPurchases = await _context.TicketPurchases
                .Where(tp => tp.UserEmail == userEmail)
                .ToListAsync();

            if (ticketPurchases == null || ticketPurchases.Count == 0)
            {
                return NotFound();
            }

            return ticketPurchases;
        }
        [HttpDelete("CheckAndDelete/{id}")]
        public async Task<IActionResult> CheckAndDeletePurchasedTicket(int id)
        {
            var ticketPurchase = await _context.TicketPurchases.FindAsync(id);

            if (ticketPurchase == null)
            {
                return NotFound();
            }

            if (ticketPurchase.EventDeleted)
            {
                _context.TicketPurchases.Remove(ticketPurchase);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            return BadRequest(new { message = "The ticket purchase cannot be deleted as the event has not been deleted." });
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

        // POST: api/Tickets/Scan/{token}
        [HttpPost("Scan/{token}")]
        public async Task<IActionResult> ScanTicket(string token)
        {
            var ticket = await _context.TicketPurchases.FirstOrDefaultAsync(t => t.ScanningToken == token);

            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found." });
            }

            if (ticket.IsScanned)
            {
                return BadRequest(new { message = $"Ticket already scanned at {ticket.ScannedAt}." });
            }

            ticket.IsScanned = true;
            ticket.ScannedAt = DateTime.UtcNow;

            _context.TicketPurchases.Update(ticket);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ticket successfully scanned." });
        }


        private async Task SendEmail(string email, string qrCode, Customer customer)
        {
            var emailServer = "smtp.gmail.com";
            var emailPort = 587;
            var emailUser = "pfpfrancois2001@gmail.com";
            var emailPassword = "wupastwcxrbihgfa";

            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("The Promenade", emailUser));
            message.To.Add(new MailboxAddress($"{customer.First_Name} {customer.Last_Name}", email));
            message.Subject = "Your Event Ticket";

            // Create multipart/mixed parent to hold both the body and the attachment.
            var multipart = new Multipart("mixed");
            var body = new MultipartAlternative();

            // Constructing a friendly greeting
            string greeting = $"Dear {customer.Title} {customer.First_Name} {customer.Last_Name},";

            // Constructing the personalized message body
            string messageBodyText = $"{greeting}\r\n\r\nThank you for your purchase! Please find attached your QR code for the event. Keep it safe and present it at the entrance. We look forward to seeing you!\r\n\r\nWarm regards,\r\nThe Promenade Team";

            string messageBodyHtml = $"<p>{greeting}</p><p>Thank you for your purchase! Please find below your QR code for the event. Keep it safe and present it at the entrance. We look forward to seeing you!</p><img src=\"data:image/png;base64,{qrCode}\" /><p>Warm regards,<br/>The Promenade Team</p>";

            body.Add(new TextPart("plain")
            {
                Text = messageBodyText
            });

            body.Add(new TextPart("html")
            {
                Text = messageBodyHtml
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
