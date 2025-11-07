namespace NOTIFICATIONSAPP.Dtos
{
    public class DeliveryAttemptReadDto
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public string? TargetUrl { get; set; }
        public string? Status { get; set; }
        public int? HttpStatus { get; set; }
        public string? ResponseBody { get; set; }
        public DateTimeOffset AttemptedAt { get; set; }
    }
}
