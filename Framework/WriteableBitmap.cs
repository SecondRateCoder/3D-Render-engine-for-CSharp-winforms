using System.Drawing.Imaging;
using SharpDX;

class WriteableBitmap{
    public static WriteableBitmap Empty = new WriteableBitmap(0, 0);
    Bitmap bmp;
    public Bitmap Get(){return this.bmp;}
    public int pixelHeight{get; private set;}
    public int pixelWidth{get; private set;}
    public int Count{get{return (pixelHeight == null | pixelWidth == null? 0: pixelWidth*pixelHeight);}}
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
        this.bmp = bmp;
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
    /// <summary>
    /// Releases all resources used by this instance.
    /// </summary>
    /// <param name="disposing">If true then this instance fully disposes, 
    /// if false this instance saves it's bitmap to a file named (number).png</param>
    public void Dispose(bool disposing = true){
        if(this.bmp != null){
            if(disposing){
                this.bmp.Dispose();
                this.bmp = new Bitmap(0, 0);
            }else{
                string filePath = AppDomain.CurrentDomain.BaseDirectory + 
                @"Cache\Images\" + 
                $"({Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + 
                @"Cache\Images\").Count()}).png";
                File.Create(filePath);
                this.bmp.Save(filePath);
                this.bmp.Dispose();
                this.bmp = new Bitmap(0, 0);
            }
        }
    }
    public void Initialise_Save(TextureDatabase tD, int Width, int Height, bool ExceedClear = true){
        this.Dispose(false);
        this.Initialise(tD, Width, Height, ExceedClear);
    }
    public void Initialise_Save(IEnumerable<byte> bytes, int Width, int Height){
        this.Dispose(false);
        this.Initialise(bytes, Width, Height);
    }
    /// <summary>
    /// Re-initialise this WriteableBitmap, filling it with new TextureData.
    /// </summary>
    /// <param name="tD">The data to be writtem to this bitmap.</param>
    /// <param name="Width">The width of the new Bitmap.</param>
    /// <param name="Height">The height of the new Bitmap.</param>
    /// <param name="ExceedClear">Should this function write still write to the Bitmap if Points in TextureData exceeds Bitmap dimensions.</param>
    public void Initialise(TextureDatabase tD, int Width, int Height, bool ExceedClear = true){
        this.bmp = new Bitmap(Width, Height);
        for(int cc =0; cc < tD.Count;cc++){
            if(tD[cc].p.X > pixelWidth | tD[cc].p.Y > pixelHeight && ExceedClear){continue;}else{
                this.bmp.SetPixel(tD[cc].p.X > Width? Width :tD[cc].p.X, tD[cc].p.Y > Height? Height: tD[cc].p.Y, tD[cc].c);
            }
        }
    }
    /// <summary>
    /// Re-initialise this WriteableBitmap, filling it with new TextureData.
    /// </summary>
    /// <param name="bytes">The array of data.</param>
    /// <param name="Width">The Width of the Bitmap.</param>
    /// <param name="Height">The Height of the Bitmap.</param>
    /// <exception cref="ArgumentOutOfRangeException">If bytes.Count exceeds the specified Bitmap area.</exception>
    /// <remarks>This function unlike "Initialise(TextureDatabase tD, int Width, int Height, [bool ExceedClear = true])" will assign the Color linearly, from left to right
    /// and cannot call an error</remarks>
    public void Initialise(IEnumerable<byte> bytes, int Width, int Height){
        this.Dispose(true);
        this.pixelHeight = Height;
        this.pixelWidth = Width;
        if((bytes.Count() != Width * Height * 4) | bytes.Count() % 4 != 0){throw new ArgumentOutOfRangeException();}else{
            this.bmp = new Bitmap(Width, Height);
            this.Update(bytes);
        }
    }
    public void Update(IEnumerable<byte> bytes){
		if((bytes.Count() > this.pixelHeight * this.pixelWidth * 4)){throw new ArgumentOutOfRangeException(nameof(bytes), "Parameter cannot have a length that exceeds the bounds of this bitmap");}
        for(int y =0; y < this.pixelHeight;y++){
            for(int x =0; x < this.pixelWidth;x++){
                int index = (x + (y * pixelWidth) * 4);
                this.bmp.SetPixel(x, y, 
                Color.FromArgb(bytes.ElementAt(index), bytes.ElementAt(index+1), bytes.ElementAt(index+2), bytes.ElementAt(index+3)));
            }
        }
    }
    public static explicit operator WriteableBitmap(Bitmap bmp){
		return new WriteableBitmap(bmp);
	}
    public static explicit operator Bitmap(WriteableBitmap bmp){
		return bmp.bmp;
	}
    public static unsafe explicit operator byte[](WriteableBitmap bmp){
        byte[] result = new byte[bmp.pixelWidth * bmp.pixelHeight * sizeof(Color)];
        int index =0;
        for(int y =0; y < bmp.pixelHeight; y++){
            for(int x =0; x < bmp.pixelWidth;x++, index += sizeof(Color)){
                result[index] = bmp[x, y].A;
                result[index+1] = bmp[x, y].R;
                result[index+2] = bmp[x, y].G;
                result[index+3] = bmp[x, y].B;
            }
        }
        return result;
    }
}