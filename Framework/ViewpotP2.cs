using SharpDX;

class WriteableBitmap{
    Bitmap bmp
    public Bitmap Get(){return this.bmp;}
    public void PutPixel(int x, int y, Color c){bmp.SetPixel(x, y, c);}
    public Color GetPixel(int x, int y){return bmp.GetPixel(x, y);}
    public int pixelHeight{get{return bmp.Height;}}
    public int pixelWidth{get{return bmp.Width;}}
    public int Count{get{return pixelWidth*pixelHeight;}}
    Color this[int x, int y]{
        get{
            if(x > pixelWidth | y > pixelHeight){return bmp.GetPixel(pixelWidth, pixelHeight);}
            return bmp.GetPixel(x, y);
        }
    }
    public WriteableBitmap(IEnumerable<byte> bytes, int Width = 0, int Height = 0){
        bmp = new Bitmap(Width, Height);
        int cc =0;
        for(int y =0;y < Height;y++){
            for(int x =0;x < Width;x++, cc+=4){
                if(cc >= bytes.Count()){
                    bmp.SetPixel(x, y, Color.Black);
                }else{
                    bmp.SetPixel(x, y, new Color((byte)bytes.GetItem(cc), (byte)bytes.GetItem(cc), (byte)bytes.GetItem(cc), (byte)bytes.GetItem(cc)));
                }
            }
        }
    }
    public void Update(byte a, byte r, byte g, byte b, int x, int y){
        bmp.SetPixel(x, y, new Color((int)a, (int)r, (int)g, (int)b));
    }
    public void Update((Point p, Color c)[] TextureData){
        foreach((Point p, Color c) bit in TextureData){
            bmp.SetPixel(p.X, p.Y, c);
        }
    }
    public void Update(byte[] bytes){
        int cc =0;
        for(int y =0;y < Height;y++){
            for(int x =0;x < Width;x++, cc+=4){
                if(cc >= bytes.Count){
                    bmp.SetPixel(x, y, Color.Black);
                }else{
                    bmp.SetPixel(x, y, new Color((byte)bytes[cc], (byte)bytes[cc+1], (byte)bytes[cc+2], (byte)bytes[cc+3]));
                }
            }
        }
    }
    public static explicit operator WriteableBitmap(Bitmap bmp){return new WriteableBitmap(bmp.RawFormat.GUID.ToBytes());}
}

static partial class Viewport{
    static byte[] backBuffer;
    static WriteableBitmap bmp;
    public static Bitmap Get(){return bmp.Get();}
    public static void Update(WriteableBitmap Bmp){
        bmp = Bmp;
        backBuffer = new byte[bmp.pixelWidth * bmp.pixelHeight * 4];
    }
    public static void Clear(Color c){
        for(int cc =0;cc < bmp.Count;cc+=4){
            backBuffer[cc] = c.A;
            backBuffer[cc] = c.R;
            backBuffer[cc] = c.G;
            backBuffer[cc] = c.B;
        }
    }
    public static void Present(){
        bmp.Update(backBuffer);
    }
}