using API.Data;
using API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using QRCoder;
using System.Globalization;
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
            ticket.ScanningToken = Guid.NewGuid().ToString();

            // Generate a QR code
            var qrCode = GenerateQRCode($"http://localhost:4200/Tickets/Scan/{ticket.ScanningToken}");

            //ticket.QRCode = qrCode;
            await _context.SaveChangesAsync();

            var eventDetails = await _context.Events.FirstOrDefaultAsync(e => e.EventID == ticket.EventId);

            // Get the customer associated with the ticket
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == ticket.UserEmail);

            if (customer != null)
            {
                // Send the email to the customer
                await SendEmail(customer.Email, qrCode, customer, eventDetails);
            }




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

            DateTime utcNow = DateTime.UtcNow;
            TimeZoneInfo saTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, saTimeZone);

            ticket.ScannedAt = localTime;

            _context.TicketPurchases.Update(ticket);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ticket successfully scanned." });
        }


        private async Task SendEmail(string email, string qrCode, Customer customer, Event eventDetails)
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
            string messageBodyText = $"{greeting}\r\n\r\nThank you for your purchase for the event {eventDetails.EventName} on {eventDetails.EventDate}. Please find attached your QR code. Keep it safe and present it at the entrance. We look forward to seeing you!\r\n\r\nWarm regards,\r\nThe Promenade Team";

            string messageBodyHtml = $"<p>{greeting}</p><p>Thank you for your purchase for the event <strong>{eventDetails.EventName}</strong> on {eventDetails.EventDate}. Please find below your QR code. Keep it safe and present it at the entrance. We look forward to seeing you!</p><img src=\"data:image/png;base64,{qrCode}\" /><p>Warm regards,<br/>The Promenade Team</p>";


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

        [HttpGet("TicketSalesReport")]
        public async Task<ActionResult<IEnumerable<object>>> GetTicketSalesReport(string? startDate, string? endDate)
        {
            Console.WriteLine($"Received start date: {startDate}, end date: {endDate}"); // Debugging line

            if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                // Return an empty array with a 200 OK status
                return Ok(new List<object>());
            }

            DateTime startDateTime = DateTime.ParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime endDateTime = DateTime.ParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddHours(23).AddMinutes(59).AddSeconds(59);

            var ticketPurchases = await _context.TicketPurchases
                                .Where(tp => tp.PurchaseDate >= startDateTime && tp.PurchaseDate <= endDateTime)
                                .ToListAsync();

            if (ticketPurchases == null || !ticketPurchases.Any())
            {
                // Return an empty array with a 200 OK status
                return Ok(new List<object>());
            }

            // Optionally, you can transform the data as needed to match what you want on the client
            var result = ticketPurchases.GroupBy(tp => tp.PurchaseDate.Date)
                                        .Select(group => new
                                        {
                                            PurchaseDate = group.Key,
                                            TotalAmount = group.Sum(tp => tp.TicketPrice)
                                        })
                                        .OrderBy(item => item.PurchaseDate)
                                        .ToList();

            return Ok(result);
        }



    }
}
