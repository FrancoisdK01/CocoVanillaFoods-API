using API.Data;
using API.Model;
using iTextSharp.text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Newtonsoft.Json;
using QRCoder;
using System;
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

        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<TicketPurchase>> AddTicket(TicketPurchaseDTO ticketDTO)
        {
            // Lookup the event details and customer based on the DTO data
            var eventDetails = await _context.Events.FirstOrDefaultAsync(e => e.EventID == ticketDTO.EventId);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == ticketDTO.UserEmail);

            if (eventDetails == null || customer == null)
            {
                return BadRequest("Event or customer not found");
            }

            // Populate the TicketPurchase object based on looked-up data
            TicketPurchase ticket = new TicketPurchase
            {
                UserEmail = ticketDTO.UserEmail,
                EventId = ticketDTO.EventId,
                EventDate = eventDetails.EventDate,
                PurchaseDate = DateTime.UtcNow,
                TicketPrice = (decimal)eventDetails.Price,
                EventName = eventDetails.Name,
                Description = eventDetails.Description,
            };

            // Save the TicketPurchase first to generate an ID
            _context.TicketPurchases.Add(ticket);
            await _context.SaveChangesAsync();  // Save the TicketPurchase

            // Now that the TicketPurchase is saved, it should have an Id.
            int ticketPurchaseId = ticket.Id;

            // Create TicketPurchasedStatus and set its TicketPurchaseId
            TicketPurchasedStatus status = new TicketPurchasedStatus
            {
                ScanningToken = Guid.NewGuid().ToString(),
                IsScanned = false,
                EventDeleted = false,
                TicketPurchaseId = ticketPurchaseId  // Set the foreign key explicitly
            };

            _context.TicketPurchasedStatuses.Add(status);
            await _context.SaveChangesAsync();  // Save TicketPurchasedStatus

            // Generate a QR code
            var qrCode = GenerateQRCode($"http://localhost:4200/TicketPurchases/Scan/{status.ScanningToken}");

            QrCode qr = new QrCode
            {
                QrCodeBase64 = qrCode,
                TicketPurchaseId = ticket.Id // Assuming TicketPurchaseId is the foreign key
            };

            _context.QrCodes.Add(qr);
            await _context.SaveChangesAsync();


            // Send the email to the customer
            await SendEmail(customer.Email, qrCode, customer, eventDetails);

            return CreatedAtAction("GetSingleTicketPurchaseEntry", new { id = ticket.Id }, ticket);
        }




        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<TicketPurchase>> GetSingleTicketPurchaseEntry(int id)
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
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<TicketPurchase>>> GetAllPurchasesForUser(string userEmail)
        {
            var ticketPurchases = await _context.TicketPurchases
                .Where(tp => tp.UserEmail == userEmail)
                .Include(tp => tp.QrCode)
                .ToListAsync();

            if (ticketPurchases == null || ticketPurchases.Count == 0)
            {
                return NotFound("Customer tickets not found");
            }

            return ticketPurchases;
        }
        [HttpDelete("CheckAndDelete/{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> ValidateAndDeletePurchasedTicket(int id)
        {
            var ticketPurchase = await _context.TicketPurchases.FindAsync(id);

            if (ticketPurchase == null)
            {
                return NotFound();
            }

            if (ticketPurchase.TicketPurchasedStatus.EventDeleted)
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
        public async Task<IActionResult> ValidateTicket(string token)
        {
            // Debugging lines
            Console.WriteLine($"Scanning Token: {token}");

            // Fetch TicketPurchasedStatus first
            var status = await _context.TicketPurchasedStatuses.FirstOrDefaultAsync(s => s.ScanningToken == token);

            if (status == null)
            {
                return NotFound(new { message = "TicketPurchasedStatus not found." });
            }

            // Fetch associated TicketPurchase object
            var ticket = await _context.TicketPurchases
                                       .Include(tp => tp.TicketPurchasedStatus)
                                       .FirstOrDefaultAsync(tp => tp.Id == status.TicketPurchaseId);

            if (ticket == null || ticket.TicketPurchasedStatus == null)
            {
                return NotFound(new { message = "Ticket or Ticket Status not found." });
            }

            // Convert the current UTC time to South African Standard Time
            DateTime currentLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time"));

            // Check if the scanning is within two hours before the event
            if (ticket.EventDate > currentLocalTime.AddHours(2))
            {
                return BadRequest(new { message = "Ticket can only be scanned two hours before the event." });
            }

            // Check if the ticket is already scanned
            if (status.IsScanned)
            {
                return BadRequest(new { message = $"Ticket already scanned at {status.ScannedAt}." });
            }

            // Update the IsScanned and ScannedAt properties of the status
            status.IsScanned = true;
            status.ScannedAt = currentLocalTime;

            // Save changes
            _context.TicketPurchasedStatuses.Update(status);
            await _context.SaveChangesAsync();

            // Debugging lines
            Console.WriteLine($"Ticket successfully scanned.");

            return Ok(new { message = "Ticket successfully scanned." });
        }


        private async Task SendEmail(string email, string qrCode, Customer customer, Event eventDetails)
        {
            var emailServer = "smtp.gmail.com";
            var emailPort = 587;
            var emailUser = "promenadetestacc2@gmail.com";
            var emailPassword = "kriunnuftrotvecb";

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
            string messageBodyText = $"{greeting}\r\n\r\nThank you for your purchase for the event {eventDetails.Name} on {eventDetails.EventDate}. Please find attached your QR code. Keep it safe and present it at the entrance. We look forward to seeing you!\r\n\r\nWarm regards,\r\nThe Promenade Team";

            string messageBodyHtml = $"<p>{greeting}</p><p>Thank you for your purchase for the event <strong>{eventDetails.Name}</strong> on {eventDetails.EventDate}. Please find below your QR code. Keep it safe and present it at the entrance. We look forward to seeing you!</p><img src=\"data:image/png;base64,{qrCode}\" /><p>Warm regards,<br/>The Promenade Team</p>";


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
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<object>>> GenerateTicketSalesReport(string? startDate, string? endDate)
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
