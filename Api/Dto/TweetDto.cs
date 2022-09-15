namespace Api.Dto
{
    public class TweetDto
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Tag { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset Created { get; set; }
        public int Likes { get; set; }
        public List<ReplyDto> Replies { get; set; }
    }
}