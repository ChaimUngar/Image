using ImageUpload.Data;

namespace ImageUpload.Web.Models
{
    public class ImageViewVM
    {
        public Image Image { get; set; }
        public bool ShowPassword { get; set; }
        public string Message { get; set; }
    }
}
