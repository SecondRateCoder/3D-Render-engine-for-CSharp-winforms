
/// <summary>
///  This class represents the the Scene and what will be viewed by the user.
/// </summary>
static class World{
    static World(){
        world = new List<gameObj>(10);
        cams = [new(15f, Vector3.Zero, Vector3.Forward, 10)]; //Error here.
        lights = [new Light(Vector3.Zero, Color.White, 15)]; //Error here.
        activeLights = [0];
    }
    /// <summary>Stores the entire 3d world.</summary>
    public static List<gameObj> world;
    public static void Add(gameObj gO){
        try{world.Add(gO);}
        catch(TypeInitializationException){
            world = new List<gameObj>(10);
            world.Add(gO);
        }
    }
    /// <summary>Stores all the cams in the world.</summary>
    public static List<Camera> cams;
    /// <summary>Stores the index of the selected cam.</summary>
    public static int camIndex{get; private set;}
    /// <summary>Stores all the lights in the world.</summary>
    public static List<Light> lights;
    /// <summary>Stores the indexes of all the lights that are active in the world.</summary>
    public static List<int> activeLights{get; private set;}
    /// <summary>Add a <see cref="Camera"/> to the world.</summary>
    /// <param name="c">The <see cref="Camera"/> to be added.</param>
    public static void AddCam(Camera c){cams.Add(c);}
    /// <summary>Try to set a new <see cref="Camera"/> to be the Main in the world.</summary>
    /// <param name="index">The index of the <see cref="Camera"/> to be selected.</param>
    /// <returns>Was the <see cref="Camera"/> successfully selected?</returns>
    public static bool TrySetCam(int index){
        if(index < cams.Count && index >= 0){
            camIndex = index;
            return true;
        }
        return false;
    }
    /// <summary>Add a new Light to <see cref="activeLights"/></summary>
    /// <param name="light">The Light to be added.</param>
    public static void AddLight(int light){activeLights.Add(light);}
    public static bool TrySetLight(int index){
        if(!activeLights.Contains(index)){
            activeLights.Add(index);
            return true;
        }else{return false;}
    }
    public static void Call(){
        Vector3 camPos = 0f-cams[camIndex].Position;
        Vector3 camRot = 0f-cams[camIndex].Rotation;
        foreach(gameObj gO in world){
            gO.Translate(camPos, camRot, false);
        }
    }
}

/// <summary>
///  This class represents the viewport of the user, the Camera property is what will be used to select the viewpoint.
/// </summary> 
static class ViewPort{
    static byte[] backBuffer= [];
    static WriteableBitmap bmp = WriteableBitmap.Empty;
    public static Bitmap Get(){return bmp.Get();}
    public static void Update(WriteableBitmap Bmp){
        bmp = Bmp;
        backBuffer = new byte[bmp.pixelWidth * bmp.pixelHeight * 4];
    }
    public static void Clear(Color c){
        for(int cc =0;cc < bmp.Count;cc+=4){
            backBuffer[cc] = c.A;
            backBuffer[cc+1] = c.R;
            backBuffer[cc+2] = c.G;
            backBuffer[cc+3] = c.B;
        }
    }
    public static void Present(){
        bmp.Update(backBuffer);
    }
    public static TextureDatabase DrawBLine(TextureDatabase.TexturePoint a, TextureDatabase.TexturePoint b){
        Point[] points = DrawBLine(new Point((int)a.p.X, (int)a.p.Y), new Point((int)b.p.X, (int)b.p.Y));
        TextureDatabase result = [];
        float colorBlend = 0;
        float Colorincrement = (float)(points.Length/Math.Sqrt((int)(Math.Abs(b.p.X - a.p.X) * Math.Abs(b.p.X - a.p.X)) + (int)(Math.Abs(b.p.Y - a.p.Y) * Math.Abs(b.p.Y - a.p.Y))));
        for(int i = 0; i< points.Length;i++){
            Color c = Color.FromArgb(
                (int)Math.Clamp((a.c.A/colorBlend) + (b.c.A*colorBlend), 0, byte.MaxValue), 
                (int)Math.Clamp((a.c.R/colorBlend) + (b.c.R*colorBlend), 0, byte.MaxValue), 
                (int)Math.Clamp((a.c.G/colorBlend) + (b.c.G*colorBlend), 0, byte.MaxValue), 
                (int)Math.Clamp((a.c.B/colorBlend) + (b.c.B*colorBlend), 0, byte.MaxValue));
            result.Append(new TextureDatabase.TexturePoint(new PointF(points[i].X, points[i].Y), c));
            colorBlend += Colorincrement;
        }
        return result;
    }
    public static Point[] DrawBLine(Point a, Point b){
        int dx = Math.Abs(b.X - a.X);
        int dy = Math.Abs(b.Y - a.Y);
        int sx = (a.X < b.X)? 1 : -1;
        int sy = (a.Y < b.Y)? 1: -1;
        int err = dx - dy;
        List<Point> points =[];
        while(true){
            points.Add(a);
            if((a.X == b.X) && (a.Y == b.Y)){break;}
            int e2 = err*2;
            if(e2 > -dy){err -= dy;     a.X += sx;}
            if(e2 < dx){err += dx;      a.Y += sy;}
        }
        return [.. points];
    }
    /// <summary>
    ///  This property represents the bounds of the form.
    /// </summary>
    ///<remarks>The left(l) and bottom(b) properties are set to 0, 
    /// whilst the right(r) and top(t) properties hold the window width and height, respectively.</remarks>
    public static readonly (float r, float l, float b, float t) boundary = (Entry.f.DisplayRectangle.Width, 0, 0, Entry.f.DisplayRectangle.Height);
    /// <summary>
    ///  This property represents the 4x4 Matrix that will convert all viewable vector positions to Perspective projection.
    /// </summary>
    static float[,] PPMatrix {get{
        if(_ppMatrixDirty){
            _ppMatrix = new float[4, 4]{
                {(float)(1/(boundary.r/boundary.t)*Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f, 0f},
                {0f, (float)(1/Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f},
                {0f, 0f, World.cams[World.camIndex].far/(World.cams[World.camIndex].far-World.cams[World.camIndex].near), -World.cams[World.camIndex].far*World.cams[World.camIndex].near/(World.cams[World.camIndex].far-World.cams[World.camIndex].near)},
                {0f, 0f, 1f, 0f}};
            _ppMatrixDirty = false;
        }
        return _ppMatrix;
    }}
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    static float[,] _ppMatrix;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    static bool _ppMatrixDirty;
    public static void MarkPPMatrixDirty(){
        _ppMatrixDirty = true;
    }
    public static WriteableBitmap Convert(int width, int height){
        WriteableBitmap bmp = new(width, height);
        foreach(gameObj gO in World.world){
            // Vector3[] PolygonOrigins = new Vector3[gO.GetComponent<Mesh>().Count];
            // for(int cc =0; cc < PolygonOrigins.Length;cc++){PolygonOrigins[cc] = gO.GetComponent<Mesh>()[cc].origin;}
            // //Texture Style pre-applied.
            // bmp.Set(Convert_(gO.Texture(TextureStyles.Styles.StretchToFit), PolygonOrigins, new Size(width, height)), (int)(255/2));
            if(!gO.HasComponent<Mesh>()){continue;}
            foreach(Polygon p in gO.GetComponent<Mesh>()){
                Polygon p_ = Multiply(p);
                p_.A.Normalise();
                p_.A *= new PointF(width, height);
                bmp.Set(Color.Black, (Point)p_.A);
                p_.B.Normalise();
                p_.B *= new PointF(width, height);
                bmp.Set(Color.Black, (Point)p_.B);
                p_.C.Normalise();
                p_.C *= new PointF(width, height);
                bmp.Set(Color.Black, (Point)p_.C);
            }
        }
        return bmp;
    }
    static WriteableBitmap Convert_(TextureDatabase tD, Vector3[] Origins, Size s){
		WriteableBitmap bmp = new(s.Width, s.Height);
		if(tD.Count % Origins.Length != 0 && tD.Count > Origins.Length){throw new ArgumentOutOfRangeException();}
        int cc =0;
        int Length = Origins.Length;
        while(cc < Length){
            bmp.Set(tD[cc].c.A, tD[cc].c.R, tD[cc].c.G, tD[cc].c.B, tD[cc].p.X, tD[cc].p.Y, 255);
            ++cc;
        }
        // for(int cc = 0; cc < tD.Count;cc++){
        // 	float increment = Origins.Length/tD.Count;
        // 	TextureDatabase buffer = tD.Slice_PerSectionRanges(cc);
        // 	for(int cc_ = 0; cc_ < buffer.Count; cc_++){
        //         buffer.AddRangeAt(cc_, (IEnumerable<TextureDatabase.TexturePoint>)DrawBLine(buffer[cc_], buffer[cc_+1]));
        // 		buffer[cc_] = new TextureDatabase.TexturePoint(
        //         new PointF(buffer[cc].p.X * ((PointF)Origins[cc]).X, buffer[cc].p.Y * ((PointF)Origins[cc]).Y), 
        //         buffer[cc_].c);
        // 	}
        //     tD.AssignRangeAt(cc, buffer.GetIEnumerable(), false);
        // }
        // bmp.Set(tD);
        return bmp;
    }
    /*
    void DrawTexturedTriangle(
    Bitmap target, 
    Vector3 v0, Vector3 v1, Vector3 v2, 
    PointF uv0, PointF uv1, PointF uv2, 
    Bitmap texture)
{
    // Convert 3D to 2D screen points
    PointF p0 = new PointF(v0.X, v0.Y);
    PointF p1 = new PointF(v1.X, v1.Y);
    PointF p2 = new PointF(v2.X, v2.Y);

    // Find bounding box for the triangle
    int minX = (int)MathF.Min(p0.X, MathF.Min(p1.X, p2.X));
    int maxX = (int)MathF.Max(p0.X, MathF.Max(p1.X, p2.X));
    int minY = (int)MathF.Min(p0.Y, MathF.Min(p1.Y, p2.Y));
    int maxY = (int)MathF.Max(p0.Y, MathF.Max(p1.Y, p2.Y));

    for (int y = minY; y <= maxY; y++)
    {
        for (int x = minX; x <= maxX; x++)
        {
            // Barycentric coordinates
            float[] bary = ComputeBarycentric(p0, p1, p2, new PointF(x, y));
            float u = bary[0] * uv0.X + bary[1] * uv1.X + bary[2] * uv2.X;
            float v = bary[0] * uv0.Y + bary[1] * uv1.Y + bary[2] * uv2.Y;

            // Check if the point is inside the triangle
            if (bary[0] >= 0 && bary[1] >= 0 && bary[2] >= 0)
            {
                // Sample the texture
                int texX = (int)(u * (texture.Width - 1));
                int texY = (int)(v * (texture.Height - 1));
                Color color = texture.GetPixel(texX, texY);

                // Set the pixel on the target bitmap
                target.SetPixel(x, y, color);
            }
        }
    }
}

// Helper: Compute barycentric coordinates
float[] ComputeBarycentric(PointF a, PointF b, PointF c, PointF p)
{
    float det = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
    float l0 = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / det;
    float l1 = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / det;
    float l2 = 1.0f - l0 - l1;
    return new float[] { l0, l1, l2 };
}
    */

    /// <summary>
    /// Will return a polygon whose vector co-ordinaates can be directly converted to points.
    /// </summary>
    /// <returns>A polygon whose vector co-ordinaates can be directly converted to points.</returns>
    public static Polygon Multiply(Polygon polygon){
        return new Polygon(Vector3.MatrixMultiply(polygon.A, PPMatrix), Vector3.MatrixMultiply(polygon.B, PPMatrix), Vector3.MatrixMultiply(polygon.C, PPMatrix));
    }
}