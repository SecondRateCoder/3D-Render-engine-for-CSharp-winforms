using System.Drawing.Imaging;
using SharpDX;

class WriteableBitmap{
    Bitmap bmp;
    public Bitmap Get(){return this.bmp;}
    public int pixelHeight{get; private set;}
    public int pixelWidth{get; private set;}
    public int Count{get{return pixelWidth*pixelHeight;}}
    Color this[int x, int y]{
        get{
            if(x > pixelWidth | y > pixelHeight){return bmp.GetPixel(pixelWidth, pixelHeight);}else{
                return bmp.GetPixel(x, y);
            }
        }
        set{
            if(x > pixelWidth | y > pixelHeight){throw new ArgumentOutOfRangeException();}else{
                this.bmp.SetPixel(x, y, value);
            }
        }
    }
    public WriteableBitmap(Bitmap bmp){

    }
    public WriteableBitmap(int Width = 200, int Height = 200){
        this.pixelHeight = Height;
        this.pixelWidth = Width;
        this.bmp = new Bitmap(Width, Height);
    }
    public WriteableBitmap(IEnumerable<byte> bytes, int Width = 200, int Height = 200){
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
    public void Set(byte a, byte r, byte g, byte b, int x, int y){
        bmp.SetPixel(x, y, Color.FromArgb((int)a, (int)r, (int)g, (int)b));
    }
    public void Set(TextureDatabase TextureData){
        int cc =0;
        for((Point p, Color c) bit = TextureData[cc]; cc < TextureData.Count;cc++, bit = TextureData[cc]){
            bmp.SetPixel(bit.p.X, bit.p.Y, bit.c);
        }
    }
    public void Dispose(bool disposing = true){
        if(disposing && this.bmp != null){
            this.bmp.Dispose();
        }
    }
    public void Initialise(IEnumerable<byte> bytes, int Width, int Height){
        this.Dispose(true);
        this.pixelHeight = Height;
        this.pixelWidth = Width;
        if(!(bytes.Count() == Width * Height * 4)){throw new ArgumentOutOfRangeException();}else{
            this.bmp = new Bitmap(Width, Height);
            this.Update(bytes);
        }
    }
    public void Update(IEnumerable<byte> bytes){
        int cc =0;
		if(bytes.Count() > this.pixelHeight * this.pixelWidth * 4){throw new ArgumentOutOfRangeException();}
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
		return new WriteableBitmap();
	}
}