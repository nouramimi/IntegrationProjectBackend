namespace NOTIFICATIONSAPP.Dtos
{
    public class UserNotificationReadDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid NotificationId { get; set; }
        public bool IsRead { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
