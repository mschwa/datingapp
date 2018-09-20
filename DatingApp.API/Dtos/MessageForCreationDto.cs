using System;

namespace DatingApp.API.Dtos
{
    public class MessageForCreationDto
    {
        public MessageForCreationDto()
        {
            MessageSent = DateTime.UtcNow;
        }

        public string SenderPhotoUrl { get; set; }

        public int SenderId { get; set; }

        public int RecipientId { get; set; }

        public string Content { get; set; }

        public DateTime MessageSent { get; set; }
    }
}