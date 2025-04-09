using SharpDX;
class TrueColor{
    public int A{get{return (int)a;}}
    byte a;
    public int R{get{return (int)r;}}
    byte b;
    public int G{get{return (int)g;}}
    byte r;
    public int B{get{return (int)b;}}
    byte g;
    public TrueColor(byte a, byte r, byte g, byte b){
        this.a = a;
        this.r = r;
        this.g = g;
        this.b = b;
    }
    public TrueColor(int a, int r, int g, int b){
        this.a = BitConverter.GetBytes(a)[0];
        this.r = BitConverter.GetBytes(r)[0];
        this.g = BitConverter.GetBytes(g)[0];
        this.b = BitConverter.GetBytes(b)[0];
    }
    public static explicit operator TrueColor(byte[] bytes){return new TrueColor(bytes[0], bytes[1], bytes[2], bytes[3]);}
    public static explicit operator TrueColor(List<byte> bytes){return new TrueColor(bytes[0], bytes[1], bytes[2], bytes[3]);}
}
class WriteableBitmap{
    TrueColor[] colors;
    public int pixelHeight;
    public int pixelWidth;
    public int Count{get{return pixelWidth*pixelHeight;}}
    TrueColor this[int x, int y]{
        get{
            if(x+(y*pixelWidth) > pixelHeight*pixelWidth){return colors[colors.Length-1];}
            return colors[x+(y*pixelWidth)];
        }
    }
    public TrueColor this[int index]{
        get{
            if(index > pixelHeight*pixelWidth){return colors[colors.Length-1];}
            return this.colors[index];
        }
    }
    public WriteableBitmap(){
        colors = new TrueColor[0];
        this.pixelHeight = 0;
        this.pixelWidth = 0;
    }
    public WriteableBitmap(List<byte> bytes, int Width, int Height){
        for(int cc = 0;cc < bytes.Count;cc+=4){
            colors[cc] = (TrueColor)bytes;
            foreach(int i in new int[]{0, 1, 2, 3}){
                bytes.RemoveAt(0);
            }
        }
        this.pixelWidth = Width;
        this.pixelHeight = Height;
    }
    public void Update(TrueColor tC, int index){
        this.colors[index] = tC;
    }
    public void Update(byte a, byte r, byte g, byte b, int index){
        this.colors[index] = new TrueColor(a, r, g, b);
    }
    public void Update(byte[] bytes){
        this.colors = new TrueColor[bytes.Length/4];
        for(int cc =0;cc < bytes.Length;cc+=4){
            colors[cc] = new TrueColor(bytes[cc], bytes[cc+1], bytes[cc+2], bytes[cc+3]);
        }
    }
    public static explicit operator WriteableBitmap?(Bitmap bmp){
        ImageConverter converter = new ImageConverter();
        byte[]? bytes = (byte[]?)converter.ConvertTo(bmp, typeof(byte[]));
        if(bytes != null){
            return new WriteableBitmap(bytes.ToList(), bmp.Width, bmp.Height);
        }else{
            return null;
        }
    }
}
static partial class Viewport{
    static byte[] backBuffer;
    static WriteableBitmap bmp;
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
    public static void PutPixel(int x, int y, Color color){
        int index = (x + (y * bmp.pixelWidth)) * 4;
        backBuffer[index] = (byte)(color.A * 255);
        backBuffer[index+1] = (byte)(color.R * 255);
        backBuffer[index+2] = (byte)(color.G * 255);
        backBuffer[index+3] = (byte)(color.B * 255);
    }


}