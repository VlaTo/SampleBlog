using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleBlog.Infrastructure.Models.Identity;

[Table("Blogs", Schema = "SampleBlog")]
public class Blog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id
    {
        get;
        set;
    }

    public string Title
    {
        get;
        set;
    }

    public string Slug
    {
        get;
        set;
    }

    public string AuthorId
    {
        get;
        set;
    }

    [ForeignKey(nameof(AuthorId))]
    public BlogUser Author
    {
        get;
        set;
    }
}