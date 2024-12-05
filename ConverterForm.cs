using ImageMagick;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace AppForm{
public class ConverterForm : Form {
    private GroupBox imgBox;
    private PictureBox imgPicture;
    private TableLayoutPanel imgPanel;
    private TextBox byteStringOut;
    public ConverterForm(){
        Text = "Tv Head Image Converter v1.0";
        ClientSize = new Size(425, 282);
        Stream ico = LoadDLL();
        Icon = new Icon(ico);

        TableLayoutPanel top = new TableLayoutPanel{Dock = DockStyle.Fill, ColumnCount=1, RowCount=2};
        top.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
        top.RowStyles.Add(new RowStyle(SizeType.Percent, 65f));
        Controls.Add(top);

        GroupBox convertBox = new GroupBox{Dock = DockStyle.Fill, Text = "Convert / Copy"};
        top.Controls.Add(convertBox, 0, 0);

        TableLayoutPanel convertPanel = new TableLayoutPanel{Dock = DockStyle.Fill, ColumnCount=2, RowCount=2};
        convertBox.Controls.Add(convertPanel);

        Button convertButton = new Button{Dock = DockStyle.Fill, Text = "Open"};
        convertButton.Click += convertClick;
        convertPanel.SetRowSpan(convertButton, 2);
        convertPanel.Controls.Add(convertButton, 0, 0);

        byteStringOut = new TextBox{Dock = DockStyle.Fill, Text = "", ReadOnly = true};
        convertPanel.Controls.Add(byteStringOut, 1, 0);

        Button copyButton = new Button{Dock = DockStyle.Fill, Text = "Copy To Clipboard"};
        copyButton.Click += copyClick;
        convertPanel.Controls.Add(copyButton, 1, 1);

        TableLayoutPanel imagePanel = new TableLayoutPanel{Dock = DockStyle.Fill, ColumnCount=2, RowCount=1};
        top.Controls.Add(imagePanel, 0, 1);

        imgBox = new GroupBox{Dock = DockStyle.Fill, Text = "File Name", Width = 202, Height = 176};
        imagePanel.Controls.Add(imgBox, 0, 0);

        imgPicture = new PictureBox{Dock = DockStyle.Fill, Width = 192, Height = 151, SizeMode = PictureBoxSizeMode.Zoom};
        imgBox.Controls.Add(imgPicture);

        GroupBox tvImgBox = new GroupBox{Dock = DockStyle.Fill, Text = "Tv Head Display", Width = 202, Height = 176};
        imagePanel.Controls.Add(tvImgBox, 1, 0);

        imgPanel = new TableLayoutPanel{Dock = DockStyle.Fill, ColumnCount=18, RowCount=14, Width = 192, Height = 151, AutoSize = false};
        imgPanel.BackColor = Color.Black;
        populateEmptyImageGrid(imgPanel);
        tvImgBox.Controls.Add(imgPanel);

    }

    private void convertClick(object? sender, EventArgs e){
        string filePath = openDialog();
        if(filePath == ""){return;}
        try{
            Converter.Converter converter = new Converter.Converter(filePath);
            loadImagePreview(converter);
            populateImageGrid(imgPanel, converter.GetSortedPixels());
            byteStringOut.Text = converter.GetBtyeString();
        } catch (FormatException ex){
            MessageBox.Show($"Failed to load image {filePath}\n\n{ex.Message}", "Image Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string openDialog(){
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        openFileDialog.Filter = "PNG Files (*.png)|*.png";
        if (openFileDialog.ShowDialog() == DialogResult.OK){
            return openFileDialog.FileName;
        }else{
            return "";
        }
    }
    
    private void loadImagePreview(Converter.Converter converter){
        imgBox.Text = converter.filename;
        using (MemoryStream memoryStream = new MemoryStream()){
            converter.image.Write(memoryStream, MagickFormat.Bmp); 
            memoryStream.Seek(0, SeekOrigin.Begin);
            Bitmap bitmap = new Bitmap(memoryStream);
            imgPicture.Paint += (sender, e) =>{     // override default paint
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.DrawImage(bitmap, new Rectangle(0, 0, imgPicture.Width, imgPicture.Height));
            };
            imgPicture.Invalidate();
        }
    }

    private void copyClick(object? sender, EventArgs e){
        try{
        Clipboard.SetText(byteStringOut.Text);
        } catch (Exception ex){
            MessageBox.Show($"Failed to copy to clipboard\n\n{ex}", "Copy Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void populateEmptyImageGrid(TableLayoutPanel p){
        p.Controls.Clear();
        for(int y = 0; y < 14; y++){
            for(int x = 0; x < 18; x++){
                Panel panel = new Panel{Size = new Size(5, 5)};
                panel.BackColor = Color.Black;
                p.Controls.Add(panel, x, y);
            }
        }
    }

    public static void populateImageGrid(TableLayoutPanel p, string[] pixels){
        p.Controls.Clear();
        int pixelIdx = 0;
        for(int y = 0; y < 14; y++){
            if(y % 2 == 0){
                for(int x = 0; x < 18; x++){
                    Panel panel = new Panel{Size = new Size(5, 5)};
                    panel.BackColor = ColorTranslator.FromHtml($"#{pixels[pixelIdx]}");
                    p.Controls.Add(panel, x, y);
                    pixelIdx++;
                }
            }else{
                for(int x = 17; x >= 0; x--){
                    Panel panel = new Panel{Size = new Size(5, 5)};
                    panel.BackColor = ColorTranslator.FromHtml($"#{pixels[pixelIdx]}");
                    p.Controls.Add(panel, x, y);
                    pixelIdx++;
                }
            }
        }
    }

    private Stream LoadDLL(){
        string ico_resource = "ImageToTvHeadBMP.resources.icon.ico";
        Assembly assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(ico_resource)!;
    }

    [STAThread]
    public static void Main(string[] args){
        try{
            ConverterForm form = new ConverterForm();
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
            Application.EnableVisualStyles();
            Application.Run(form);
        } catch (Exception e){
            MessageBox.Show(e.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
}
