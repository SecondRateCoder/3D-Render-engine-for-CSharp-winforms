using SharpDX;
partial static class Viewport{
    byte[] backBuffer;
    WriteableBitmap bmp;
    public void Update(WriteableBitmap bmp){
        this.bmp = bmp;
        backBuffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
    }
    public void Clear(Color c){
        for(int cc =0;cc < bmp.Length;cc+=4){
            backBuffer[cc] = c.a;
            backBuffer[cc] = c.r;
            backBuffer[cc] = c.g;
            backBuffer[cc] = c.b;
        }
    }
    public void Present(){
        using(Stream stream = bmp.PixelBuffer.AsStream()){
            // writing our byte[] back buffer into our WriteableBitmap stream
            stream.Write(backBuffer, 0, backBuffer.Length);
        }
        // request a redraw of the entire bitmap
        bmp.Invalidate();
    }
    public void PutPixel(int x, int y, Color color){
        int index = (x + (y * bmp.PixelWidth)) * 4;
        backBuffer[index] = (byte)(color.a * 255);
        backBuffer[index+1] = (byte)(color.r * 255);
        backBuffer[index+2] = (byte)(color.g * 255);
        backBuffer[index+3] = (byte)(color.b * 255);
    }

    public Vector2 Project(Vector3 coord, Matrix transMat)
    {
        // transforming the coordinates
        var point = Vector3.TransformCoordinate(coord, transMat);
        // The transformed coordinates will be based on coordinate system
        // starting on the center of the screen. But drawing on screen normally starts
        // from top left. We then need to transform them again to have x:0, y:0 on top left.
        var x = point.X * bmp.PixelWidth + bmp.PixelWidth / 2.0f;
        var y = -point.Y * bmp.PixelHeight + bmp.PixelHeight / 2.0f;
        return (new Vector2(x, y));
    }
}