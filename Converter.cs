using ImageMagick;

namespace Converter{
public class Converter{
    public readonly string path;
    public readonly string filename;
    public readonly MagickImage image;
    public readonly uint w;
    public readonly uint h;
    public readonly string format;

    public Converter(string filePath){
        path = filePath;
        filename = filePath.Split("\\").Last();

        MagickImageInfo info = new MagickImageInfo(path);
        w = info.Width;
        h = info.Height;
        format = info.Format.ToString().ToLower();
        CheckImageSpec();

        image = new MagickImage(path);
    } 

    private void CheckImageSpec(){
        if(w == 18 && h == 14 && format == "png"){
            return;
        }else{
            throw new FormatException($"Image must be an 18x14 PNG\nCurrent spec: {w}x{h} {format}");
        }
    }

    public string GetBtyeString(){
        string[] sortedPixels = GetSortedPixels();
        string byteString = $"const char* {filename[..^4]} = \""; 
        foreach(string pixel in sortedPixels){
            byteString += pixel;
        }
        byteString += "\";";
        return byteString;
    }

    public string[] GetSortedPixels(){
        string[] pixels = GetRawPixels();
        for (uint row = 0; row < h; row++){
            if (row % 2 == 1){
                uint start = row * w;
                uint end = start + w - 1;
                while (start < end){
                    string temp = pixels[start];
                    pixels[start] = pixels[end];
                    pixels[end] = temp;
                    start++;
                    end--;
                }
            }
        }
        return pixels;
    }

    public string[] GetRawPixels(){
        CheckImageSpec();
        string[] pixels = new string[w*h];
        for (int y = 0; y < h; y++){
            for (int x = 0; x < w; x++){
                IPixel<byte> pixel = image.GetPixels().GetPixel(x, y);
                IMagickColor<byte> color = pixel.ToColor()!;

                byte red = color!.R;
                byte green = color!.G;
                byte blue = color!.B;
                string hexPixel = $"{red:X2}{green:X2}{blue:X2}";
                pixels[y*w + x] = hexPixel;
            }
        }
        return pixels;
    }
}
}