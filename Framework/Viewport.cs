
/// <summary>
///  This class represents the the Scene and what will be viewed by the user.
/// </summary>
static class World{
    static World(){
        worldData = [];
        cams = [new()];
        lights = [new Light(Vector3.Zero, Color.White, 15)];
        activeLights = [0];
    }
    /// <summary>Stores the entire 3d world.</summary>
    public static List<gameObj> worldData;
    public static void Add(gameObj gO){
        worldData ??= new List<gameObj>(5);
        worldData.Add(gO);
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
        foreach(gameObj gO in worldData){
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
        foreach(gameObj gO in World.worldData){
            Vector3[] PolygonOrigins = new Vector3[gO.GetComponent<Mesh>().Count];
            for(int cc =0; cc < PolygonOrigins.Length;cc++){PolygonOrigins[cc] = gO.GetComponent<Mesh>()[cc].origin;}
            bmp.Set(Convert_(gO.Texture(TextureStyles.Styles.StretchToFit), PolygonOrigins), (int)(255/2));
        }
        return bmp;
    }
    static WriteableBitmap Convert_(TextureDatabase tD, Vector3[] Origins){
		WriteableBitmap bmp = new(Entry.f.Width, Entry.f.Height);
		if(tD.Count % Origins.Length != 0 && tD.Count > Origins.Length){throw new ArgumentOutOfRangeException();}
        for(int cc = 0; cc < tD.Count;cc++){
			float increment = Origins.Length/tD.Count;
			TextureDatabase buffer = tD.Slice_PerSectionRanges(cc);
			for(int cc_ = 0; cc_ < buffer.Count; cc_++){
                buffer.AddRangeAt(cc_, (IEnumerable<TextureDatabase.TexturePoint>)DrawBLine(buffer[cc_], buffer[cc_+1]));
				buffer[cc_] = new TextureDatabase.TexturePoint(
                new PointF(buffer[cc].p.X * ((PointF)Origins[cc]).X, buffer[cc].p.Y * ((PointF)Origins[cc]).Y), 
                buffer[cc_].c);
			}
            tD.AssignRangeAt(cc, buffer.GetIEnumerable(), false);
		}
		bmp.Set(tD);
        return bmp;
    }


    /// <summary>
    /// Will return a polygon whose vector co-ordinaates can be directly converted to points.
    /// </summary>
    /// <returns>A polygon whose vector co-ordinaates can be directly converted to points.</returns>
    public static Polygon Multiply(Polygon polygon){
        return new Polygon(Multiply(polygon.A), Multiply(polygon.B), Multiply(polygon.C));
    }
    static Vector3 Multiply(Vector3 v){
        v = new Vector3(
            (PPMatrix[0, 0]*World.cams[World.camIndex].far/4 * v.X/World.cams[World.camIndex].far)+(PPMatrix[1, 0]*World.cams[World.camIndex].far/4 * v.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 0]*World.cams[World.camIndex].far/4 * v.Z/World.cams[World.camIndex].far)+PPMatrix[3, 0],
            (PPMatrix[0, 1]*World.cams[World.camIndex].far/4 * v.X/World.cams[World.camIndex].far)+(PPMatrix[1, 1]*World.cams[World.camIndex].far/4 * v.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 1]*World.cams[World.camIndex].far/4 * v.Z/World.cams[World.camIndex].far)+PPMatrix[3, 1],
            (PPMatrix[0, 2]*World.cams[World.camIndex].far/4 * v.X/World.cams[World.camIndex].far)+(PPMatrix[1, 2]*World.cams[World.camIndex].far/4 * v.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 2]*World.cams[World.camIndex].far/4 * v.Z/World.cams[World.camIndex].far)+PPMatrix[3, 2]
            );
        float wC = PPMatrix[0, 3] * v.X + PPMatrix[1, 3] * v.Y + PPMatrix[2, 3] * v.Z + PPMatrix[3, 3];
        v /= wC;
        //Normalise then multiply by the form dimensions to scale it.
        v.Normalise();
        v= new Vector3(v.X/v.Z * Entry.f.Bounds.Width, v.Y/v.Z  * Entry.f.Bounds.Height, 0);
        return v;
    }
}