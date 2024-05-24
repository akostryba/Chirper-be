namespace final_project_back_end_akostryba;

public class Comment
{
    public int commentId { get; set; }
    public int postId { get; set; }
    public int userId { get; set; }
    public string? text { get; set; }
    public string? createdAt { get; set; }
}