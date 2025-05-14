using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NUnit.Framework;
using SharpDX;

class WriteableBitmap{
    bool disposed = false;
    public static WriteableBitmap Empty = new WriteableBitmap(0, 0);
    Bitmap bmp;
    public int bytesPerPixel{
        get{
            lock(this.bmp){
                return Image.GetPixelFormatSize(this.bmp.PixelFormat) / 8;
            }
        }
    }
    public Bitmap Get(){return this.bmp;}
    public int pixelHeight{get; private set;}
    public int pixelWidth{get; private set;}
    public int Count{get{return pixelWidth*pixelHeight;}}
    public (PointF p, Color c) this[int x, int y]{
        get{
            return this.Get(x, y);
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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public WriteableBitmap(IEnumerable<byte> bytes, int Width = 200, int Height = 200){
        this.Initialise(bytes, Width, Height);
		
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public void Set(byte a, byte r, byte g, byte b, float x, float y, int Transparency = 0){
        bool function((byte a, byte r, byte g, byte b, float x, float y) args){
            lock(this){Rectangle rect = new Rectangle(Point.Empty, new Size(bmp.Width, bmp.Height));
            BitmapData bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            IntPtr ptr = bitmapData.Scan0;
            int stride = bitmapData.Stride;
            unsafe{
                byte* pixel = (byte*)ptr + ((int)y * stride) + ((int)x * bytesPerPixel);
                pixel[0] = b;
                pixel[1] = g;
                pixel[2] = r;
                if(this.bytesPerPixel == 4){
                    pixel[3] = (byte)Transparency;
                }
            }
            bmp.UnlockBits(bitmapData);
            return true;}
        }

        _ = LockJob<(byte a, byte r, byte g, byte b, float x, float y), bool>.
            LockJobHandler.PassJob(function, (a, r, g, b, x, y), null, 1000, null, nameof(WriteableBitmap));
        /*
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
        */
        //bmp.SetPixel(x, y, Color.FromArgb((int)a, (int)r, (int)g, (int)b));
    }
    public void Set(TextureDatabase TextureData, int Transperancy = 255){
        int cc =0;
        Math.Clamp(Transperancy, 0, 255);
        for((PointF p, Color c) bit = TextureData[cc]; cc < TextureData.Count;cc++, bit = TextureData[cc]){
            Set(bit.c.A, bit.c.R, bit.c.G, bit.c.B, bit.p.X, bit.p.Y, Transperancy);
        }
    }
    public void Set(WriteableBitmap bmp, int TransparencyOverride = 0){
        if(TransparencyOverride == 0 || (this.pixelWidth * this.pixelHeight) != (bmp.pixelWidth * bmp.pixelHeight)){return;}else{
            for(int y =0; y < this.pixelHeight; y++){
                for(int x =0; x < this.pixelHeight; x++){
                    this.Set(
                        (byte)Math.Clamp((this.Get(x, y).c.A + bmp.Get(x, y).c.A)/2, 0, byte.MaxValue), 
                        (byte)Math.Clamp((this.Get(x, y).c.R + bmp.Get(x, y).c.R)/2, 0, byte.MaxValue), 
                        (byte)Math.Clamp((this.Get(x, y).c.G + bmp.Get(x, y).c.G)/2, 0, byte.MaxValue), 
                        (byte)Math.Clamp((this.Get(x, y).c.B + bmp.Get(x, y).c.B)/2, 0, byte.MaxValue), x, y);
                }
            }
        }
    }

    public TextureDatabase.TexturePoint Get(int x, int y){
        ValidateBounds(x, y);

        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        try{
            IntPtr ptr = bitmapData.Scan0;
            int stride = bitmapData.Stride;
            unsafe{
                byte* pixel = (byte*)ptr + (y * stride) + (x * bytesPerPixel);
                return (TextureDatabase.TexturePoint)(new PointF(x, y), Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]));
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
                this.bmp.Dispose();
                this.disposed = true;
                GC.SuppressFinalize(this);
            }else{
                string filePath = Directory.GetCurrentDirectory() + 
                @"Cache\Images\" + 
                $"({Directory.EnumerateFiles(Directory.GetCurrentDirectory() + 
                @"Cache\Images\").Count()}).png";
                Directory.CreateDirectory(filePath);
                File.Create(filePath);
                this.bmp.Save(filePath);
                this.bmp.Dispose();
                this.bmp = new Bitmap(0, 0);
            }
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
                this.bmp.SetPixel(tD[cc].p.X > Width? Width :(int)tD[cc].p.X, tD[cc].p.Y > Height? Height: (int)tD[cc].p.Y, tD[cc].c);
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
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    public static unsafe explicit operator byte[](WriteableBitmap bmp){
        byte[] result = new byte[bmp.pixelWidth * bmp.pixelHeight * sizeof(Color)];
        int index =0;
        for(int y =0; y < bmp.pixelHeight; y++){
            for(int x =0; x < bmp.pixelWidth;x++, index += sizeof(Color)){
                result[index] = bmp[x, y].c.A;
                result[index+1] = bmp[x, y].c.R;
                result[index+2] = bmp[x, y].c.G;
                result[index+3] = bmp[x, y].c.B;
            }
        }
        return result;
    }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
}