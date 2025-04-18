using System.Drawing.Imaging;
using SharpDX;

class WriteableBitmap{
    Bitmap bmp;
    public Bitmap Get(){return this.bmp;}
    public void PutPixel(int x, int y, Color c){bmp.SetPixel(x, y, c);}
    public Color GetPixel(int x, int y){return bmp.GetPixel(x, y);}
    public int pixelHeight{get; private set;}
    public int pixelWidth{get; private set;}
    public int Count{get{return pixelWidth*pixelHeight;}}
    Color this[int x, int y]{
        get{
            if(x > pixelWidth | y > pixelHeight){return bmp.GetPixel(pixelWidth, pixelHeight);}
            return bmp.GetPixel(x, y);
        }
    }
    public WriteableBitmap(IEnumerable<byte> bytes, int Width = 0, int Height = 0){
        bmp = new Bitmap(Width, Height);
		this.pixelHeight = Height;
		this.pixelWidth = Width;
        int cc =0;
        for(int y =0;y < Height;y++){
            for(int x =0;x < Width;x++, cc+=4){
                if(cc >= bytes.Count()){
                    bmp.SetPixel(x, y, Color.Black);
                }else{
                    bmp.SetPixel(x, y, Color.FromArgb((byte)bytes.ElementAt(cc), (byte)bytes.ElementAt(cc), (byte)bytes.ElementAt(cc), (byte)bytes.ElementAt(cc)));
                }
            }
        }
    }
    public void set(byte a, byte r, byte g, byte b, int x, int y){
        bmp.SetPixel(x, y, Color.FromArgb((int)a, (int)r, (int)g, (int)b));
    }
    public void Set((Point p, Color c)[] TextureData){
        foreach((Point p, Color c) bit in TextureData){bmp.SetPixel(bit.p.X, bit.p.Y, bit.c);}
    }
    public void Update(IEnumerable<byte> bytes){
        int cc =0;
		if(bytes.Count() > this.pixelHeight * this.pixelWidth){throw new ArgumentOutOfRangeException();}
        for(int y =0;y < this.pixelHeight;y++){
            for(int x =0;x < this.pixelWidth;x++, cc+=4){
                if(cc >= bytes.Count()){
                    bmp.SetPixel(x, y, Color.Black);
                }else{
                    bmp.SetPixel(x, y, Color.FromArgb((byte)bytes.ElementAt(cc), (byte)bytes.ElementAt(cc++), (byte)bytes.ElementAt(cc++), (byte)bytes.ElementAt(cc++)));
                }
            }
        }
    }
    public static explicit operator WriteableBitmap(Bitmap bmp){
		using(MemoryStream mS = new MemoryStream()){
			bmp.Save(mS, ImageFormat.Bmp);
			return new WriteableBitmap(mS.ToArray());
		}
	}
}

static partial class Viewport{
}