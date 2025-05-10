using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NUnit.Framework;
using SharpDX;

class WriteableBitmap{
    bool disposed = false;
    public static WriteableBitmap Empty = new WriteableBitmap(0, 0);
    Bitmap bmp;
    public int bytesPerPixel{get{lock(this.bmp){return Image.GetPixelFormatSize(this.bmp.PixelFormat) / 8;}}}
    public Bitmap Get(){return this.bmp;}
    public int pixelHeight{get; private set;}
    public int pixelWidth{get; private set;}
    public int Count{get{return (pixelHeight == null | pixelWidth == null? 0: pixelWidth*pixelHeight);}}
    (Point p, Color c) this[int x, int y]{
        get{
            lock (this){
                
            }
        }
        set{
            this.Set(value.c.A, value.c.R, value.c.G, value.c.B, x, y);
        }
    }
    public WriteableBitmap(Bitmap bmp){
        this.bmp = bmp;
    }
    private void ValidateBounds(int x, int y){
        if (x < 0 || x >= pixelWidth || y < 0 || y >= pixelHeight){
            throw new ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are out of bounds.");
        }
    }
    public WriteableBitmap(int Width = 200, int Height = 200){
        this.pixelHeight = Height;
        this.pixelWidth = Width;
        this.bmp = new Bitmap(Width, Height);
    }
    public WriteableBitmap(IEnumerable<byte> bytes, int Width = 200, int Height = 200){
        this.Initialise(bytes, Width, Height);
		/*
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
        */
    }
    public void Set(byte a, byte r, byte g, byte b, int x, int y){
        lock(this){
            Rectangle rect = new Rectangle(Point.Empty, new Size(bmp.Width, bmp.Height));
            BitmapData bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr ptr = bitmapData.Scan0;
            int stride = bitmapData.Stride;
            unsafe{
                byte* pixel = (byte*)ptr + (y * stride) + (x * bytesPerPixel);
                pixel[0] = b;
                pixel[1] = g;
                pixel[2] = r;
                if(this.bytesPerPixel == 4){
                    pixel[3] = a;
                }
            }
            bmp.UnlockBits(bitmapData);
        }
        //bmp.SetPixel(x, y, Color.FromArgb((int)a, (int)r, (int)g, (int)b));
    }
    public void Set(TextureDatabase TextureData){
        int cc =0;
        for((Point p, Color c) bit = TextureData[cc]; cc < TextureData.Count;cc++, bit = TextureData[cc]){
            Set(bit.c.A, bit.c.R, bit.c.G, bit.c.B, bit.p.X, bit.p.Y);
        }
    }

    public (Point p, Color c) Get(int x, int y){
        ValidateBounds(x, y);

        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try{
            IntPtr ptr = bitmapData.Scan0;
            int stride = bitmapData.Stride;
            unsafe{
                byte* pixel = (byte*)ptr + (y * stride) + (x * bytesPerPixel);
                return (new Point(x, y), Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]));
            }
        }finally{
            bmp.UnlockBits(bitmapData);
        }
    }
    /// <summary>
    /// Releases all resources used by this instance.
    /// </summary>
    /// <param name="disposing">If true then this instance fully disposes, 
    /// if false this instance saves it's bitmap to a file named (number).png</param>
    public void Dispose(bool disposing = true){
        if(this.bmp != null && !this.disposed){
            if(disposing){
                Dispoze();
                GC.SuppressFinalize(this);
            }else{
                string filePath = AppDomain.CurrentDomain.BaseDirectory + 
                @"Cache\Images\" + 
                $"({Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + 
                @"Cache\Images\").Count()}).png";
                Directory.CreateDirectory(filePath);
                File.Create(filePath);
                this.bmp.Save(filePath);
                this.bmp.Dispose();
                this.bmp = new Bitmap(0, 0);
            }
        }
    }
    protected virtual void Dispoze(){
        if(!this.disposed){
            this.bmp.Dispose();
            this.bmp = null;
            this.disposed = true;
        }
    }
    ~WriteableBitmap(){
        this.Dispose(false);
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
    /// <remarks>If ExceedClear is false, this method WILL throw exceptions when it faces invalid arguments.</remarks>
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
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentOutOfRangeException.ThrowIfLessThan<int>(Width, 0, "Width must be positive.");
        ArgumentOutOfRangeException.ThrowIfLessThan<int>(Height, 0, "Height must be positive.");
        if((bytes.Count() != Width * Height * 4) | bytes.Count() % 4 != 0){throw new ArgumentOutOfRangeException("Initialise(IEnumerable<byte> bytes, int Width, int Height)");}else{
            this.bmp = new Bitmap(Width, Height);
            this.Update(bytes);
        }
    }
    public void Update(IEnumerable<byte> Bytes){
        Span<byte> bytes = Bytes.ToArray();
        if(bytes.Length > this.pixelHeight * this.pixelWidth * 4){
            throw new ArgumentOutOfRangeException(nameof(bytes), "Parameter cannot exceed the bounds of this bitmap.");
        }
		if(bytes.Length > this.pixelHeight * this.pixelWidth * 4){throw new ArgumentOutOfRangeException(nameof(bytes), "Parameter cannot have a length that exceeds the bounds of this bitmap");}
        lock(this){
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bitmapData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

            IntPtr ptr = bitmapData.Scan0;
            Marshal.Copy(bytes.ToArray(), 0, ptr, bytes.Length);

            bmp.UnlockBits(bitmapData);
        }
    }
    [Test]
    public void TestWriteableBitmapInitialization() {
        var bitmap = new WriteableBitmap(100, 100);
        Assert.That(bitmap.pixelWidth, Is.EqualTo(100));
        Assert.That(bitmap.pixelHeight, Is.EqualTo(100));
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