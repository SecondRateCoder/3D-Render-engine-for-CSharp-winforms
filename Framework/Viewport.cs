/// <summary>
///  This class represents the the Scene and what will be viewed by the user.
/// </summary>
static class World{
    public delegate void Orient_(Vector3 position, Vector3 rotation, bool PrivateTranslation = false);
    public static Orient_ WorldOrient;
    public static List<gameObj> worldData = new List<gameObj>();
    public static List<Camera> cams = new List<Camera>(){new Camera()};
    public static int camIndex{get; private set;}
    static void setCam(int index){
        if(index < cams.Count){
            camIndex = index;
        }
    }
}

/// <summary>
///  This class represents the viewport of the user, the Camera property is what will be used to select the viewpoint.
/// </summary> 
static partial class ViewPort{
    /// <summary>
    ///  This property represents the bounds of the form.
    /// </summary>
    ///<remarks>The left(l) and bottom(b) properties are set to 0, 
    /// whilst the right(r) and top(t) properties hold the window width and height, respectively.</remarks>
    public static readonly (float r, float l, float b, float t) boundary = (Camera.form_.DisplayRectangle.Width, 0, 0, Camera.form_.DisplayRectangle.Height);
    /// <summary>
    ///  This property represents the 4x4 Matrix that will convert all viewable vector positions to Perspective projection.
    /// </summary> 
    static float[,] PPMatrix {get;} = new float[4, 4]{
        {(float)(1/(boundary.r/boundary.t)*Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f, 0f},
        {0f, (float)(1/Math.Tan(World.cams[World.camIndex].theta/2)), 0f, 0f},
        {0f, 0f, World.cams[World.camIndex].far/(World.cams[World.camIndex].far-World.cams[World.camIndex].near), -World.cams[World.camIndex].far*World.cams[World.camIndex].near/(World.cams[World.camIndex].far-World.cams[World.camIndex].near)},
        {0f, 0f, 1f, 0f}};

    
    /// <summary>
    /// <summary>
    ///  This overload takes the parameter data from the static World class.
    /// </summary> 
    /// <remarks>If <seealso cref="World.worldData.Count" == 0, then it creats a cube to be rendered./></remarks>
    static (Point p, Color color)[] Convert_(){
        List<(Color[] texture, Polygon poly)> parameter = new List<(Color[] texture, Polygon poly)>();
        if(World.worldData.Count == 0){
            gameObj gO = new gameObj(Vector3.zero, Vector3.zero, Polygon.Mesh(1, 0, 1, 4));
            for(int cc = 0;cc < gO.Children.Count;cc++){
                parameter.Add((gO.Texture(cc), gO.Children[cc]));
            }
        }else{
            for(int cc = 0;cc < World.worldData.Count;cc++){
                for(int cc_ = 0; cc_ < World.worldData[cc].Children.Count;cc_++){
                    parameter.Add((World.worldData[cc].Texture(cc_), World.worldData[cc].Children[cc_]));
                }
            }
        }
        return Convert_(parameter);
    }
    ///  This overload turns Gameobjs into polygons then runs the main overload(Convert_()).
    /// </summary> 
    public static (Point p, Color color)[] Convert_(List<gameObj>? objs = null){
        List<(Color[], Polygon)> parameter = new List<(Color[], Polygon)>();
        if(objs == null){
            return Convert_();
        }
        for(int cc = 0;cc < objs.Count;cc++){
            for(int cc_ = 0; cc_ < objs[cc].Children.Count;cc_++){
                parameter.Add((objs[cc].GetComponent<Texturer>().Texture(objs[cc].Children[cc_].UVPoints), objs[cc].Children[cc_]));
            }
        }
        return Convert_(parameter);
    }
    /// <summary>
    ///  The main Convert_() overload, This function converts the world 3d enviroment into a 2 representation.
    /// </summary> 
    public static (Point p, Color color)[] Convert_(List<(Color[] texture, Polygon poly)> polygons){
        (Point p, Color color)[] result = new (Point p, Color color)[polygons.Count];
        if(polygons == null){
            throw new Exception("Can't be null, you Monkey. \nDo you know how much fricking code I had to write to implement texturing \nDon't fucing do this");
        }else{
            //Apply camera transform and clip each polygon.
            for(int cc = 0; cc < polygons.Count; cc++){
                if(Vector3.GetRotation(polygons[cc].poly.Normal, World.cams[World.camIndex].Position).Magnitude<90){
                    polygons[cc].poly.Translate(World.cams[World.camIndex].Position, 0f-World.cams[World.camIndex].Position, 0f-World.cams[World.camIndex].Rotation);
                    Vector3[] CalcBuffer = Polygon.ToVector3(Multiply(polygons[cc].poly));
                    
                }else{
                    cc++;
                }
                polygons[cc] = (polygons[cc].texture, Polygon.PolyClip(polygons[cc].poly, Vector3.zero, World.cams[World.camIndex].far));
            }
        }
        return result;
    }
    static Polygon Multiply(Polygon polygon){
        Polygon CalcBuffer = new Polygon();
        float wA = 1;
        float wB = 1;
        float wC = 1;
        //For Point A
        CalcBuffer.A = new Vector3(
            (PPMatrix[0, 0]*World.cams[World.camIndex].far/4 * polygon.A.X/World.cams[World.camIndex].far)+(PPMatrix[1, 0]*World.cams[World.camIndex].far/4 * polygon.A.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 0]*World.cams[World.camIndex].far/4 * polygon.A.Z/World.cams[World.camIndex].far)+(PPMatrix[3, 0]),
            (PPMatrix[0, 1]*World.cams[World.camIndex].far/4 * polygon.A.X/World.cams[World.camIndex].far)+(PPMatrix[1, 1]*World.cams[World.camIndex].far/4 * polygon.A.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 1]*World.cams[World.camIndex].far/4 * polygon.A.Z/World.cams[World.camIndex].far)+(PPMatrix[3, 1]),
            (PPMatrix[0, 2]*World.cams[World.camIndex].far/4 * polygon.A.X/World.cams[World.camIndex].far)+(PPMatrix[1, 2]*World.cams[World.camIndex].far/4 * polygon.A.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 2]*World.cams[World.camIndex].far/4 * polygon.A.Z/World.cams[World.camIndex].far)+(PPMatrix[3, 2])
            );
        wA = (PPMatrix[0, 3]*CalcBuffer.A.X)+(PPMatrix[1, 3]*CalcBuffer.A.Y)+(PPMatrix[2, 3]*CalcBuffer.A.Z)+PPMatrix[3, 3];
        CalcBuffer.A /= wA;
        //For Point B
        CalcBuffer.B = new Vector3(
            (PPMatrix[0, 0]*World.cams[World.camIndex].far/4 * polygon.B.X/World.cams[World.camIndex].far)+(PPMatrix[1, 0]*World.cams[World.camIndex].far/4 * polygon.B.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 0]*World.cams[World.camIndex].far/4 * polygon.B.Z/World.cams[World.camIndex].far)+(PPMatrix[3, 0]),
            (PPMatrix[0, 1]*World.cams[World.camIndex].far/4 * polygon.B.X/World.cams[World.camIndex].far)+(PPMatrix[1, 1]*World.cams[World.camIndex].far/4 * polygon.B.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 1]*World.cams[World.camIndex].far/4 * polygon.B.Z/World.cams[World.camIndex].far)+(PPMatrix[3, 1]),
            (PPMatrix[0, 2]*World.cams[World.camIndex].far/4 * polygon.B.X/World.cams[World.camIndex].far)+(PPMatrix[1, 2]*World.cams[World.camIndex].far/4 * polygon.B.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 2]*World.cams[World.camIndex].far/4 * polygon.B.Z/World.cams[World.camIndex].far)+(PPMatrix[3, 2])
            );
        wB = (PPMatrix[0, 3]*CalcBuffer.B.X)+(PPMatrix[1, 3]*CalcBuffer.B.Y)+(PPMatrix[2, 3]*CalcBuffer.B.Z)+(PPMatrix[3, 3]);
        CalcBuffer.B /= wB;
        //For Point C
        CalcBuffer.C = new Vector3(
            (PPMatrix[0, 0]*World.cams[World.camIndex].far/4 * polygon.C.X/World.cams[World.camIndex].far)+(PPMatrix[1, 0]*World.cams[World.camIndex].far/4 * polygon.C.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 0]*World.cams[World.camIndex].far/4 * polygon.C.Z/World.cams[World.camIndex].far)+(PPMatrix[3, 0]),
            (PPMatrix[0, 1]*World.cams[World.camIndex].far/4 * polygon.C.X/World.cams[World.camIndex].far)+(PPMatrix[1, 1]*World.cams[World.camIndex].far/4 * polygon.C.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 1]*World.cams[World.camIndex].far/4 * polygon.C.Z/World.cams[World.camIndex].far)+(PPMatrix[3, 1]),
            (PPMatrix[0, 2]*World.cams[World.camIndex].far/4 * polygon.C.X/World.cams[World.camIndex].far)+(PPMatrix[1, 2]*World.cams[World.camIndex].far/4 * polygon.C.Y/World.cams[World.camIndex].far)+(PPMatrix[2, 2]*World.cams[World.camIndex].far/4 * polygon.C.Z/World.cams[World.camIndex].far)+(PPMatrix[3, 2])
            );
        wC = (PPMatrix[0, 3]*CalcBuffer.C.X)+(PPMatrix[1, 3]*CalcBuffer.C.Y)+(PPMatrix[2, 3]*CalcBuffer.C.Z)+(PPMatrix[3, 3]);
        CalcBuffer.C /= wC;
        return CalcBuffer;
    }
}