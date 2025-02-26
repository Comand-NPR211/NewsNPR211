namespace WebAPI.Data.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Навігаційна властивість для зв'язку з новинами
        public List<NewsEntity> News { get; set; } = new List<NewsEntity>();


    }
}
