namespace CraigsClone.Web.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public CategoryGroup Group { get; set; }
}
