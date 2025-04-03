namespace Application.Models.Dtos;

public class ChangeSubscriptionDto
{
    public string ClientId { get; set; }
    public List<string> TopicIds { get; set; }
}