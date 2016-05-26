using System.Drawing;
using System.Drawing.Imaging;
using System.Web;

namespace WebApplication.Config
{
    public class ImageConfig
    {
        public Size NewImageSize(Size imageSize, Size newSize)
        {
            Size finalSize;
            if (imageSize.Height > newSize.Height || imageSize.Width > newSize.Width)
            {
                double tempval;
                if (imageSize.Height > imageSize.Width)
                    tempval = newSize.Height / (imageSize.Height * 1.0);
                else
                    tempval = newSize.Width / (imageSize.Width * 1.0);

                finalSize = new Size((int)(tempval * imageSize.Width), (int)(tempval * imageSize.Height));
            }
            else
                finalSize = imageSize;

            return finalSize;
        }

        public void SaveToFolder(Image img, string fileName, string extension, Size newSize, string pathToSave)
        {
            Size imgSize = NewImageSize(img.Size, newSize);
            using (Image newImg = new Bitmap(img, imgSize.Width, imgSize.Height))
            {
                newImg.Save(HttpContext.Current.Server.MapPath("~/Files"), ImageFormat.Jpeg);
            }
        }
    }
}