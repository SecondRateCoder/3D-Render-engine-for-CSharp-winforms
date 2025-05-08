public class TextureStyles{
    public static TextureStyles Empty = (TextureStyles)10;
    public static TextureStyles StretchToFit = (TextureStyles)0;
    public static TextureStyles EdgeFillBlack = (TextureStyles)1;
    public static TextureStyles EdgeFillWhite = (TextureStyles)2;
    public static TextureStyles ClipToFit = (TextureStyles)3;
    public static explicit operator TextureStyles(int i){if(IsStyle(i)){return new TextureStyles(i);}else{return TextureStyles.Empty;}}
    public static implicit operator int(TextureStyles t){return t.type;}
    public static bool IsStyle(int type){
        switch(type){
            case 0:
                return true;
            case 1:
                return true;
            case 10:
                return true;
            case 2:
                return true;
            case 3:
                return true;
            default:
                return false;
        }
    }
    int type;
    internal TextureStyles(int i){this.type = i;}

    /// <summary>Take the TextureData and it's corresponding Mesh, stretching and squashing the colors to ensure they fit</summary>
    static TextureDatabase StretchToFit_(TextureDatabase tD, Mesh m, Point _12mid, Equation p0_p1, Equation p1_p12mid, Equation p1_p2){
        int point = 0;
        m.Foreach((p) => {
            //This will manipulate use p's UVPoint data to manipulate tD
            for(int cc =0; cc < p.UVPoints.Length;cc++){
                //TextureDatabase stores a Point and the corresponding color, 
                //so simply find the bounding X-Coordinates at each Y-Coordinate.
                //And for all the Points on that Y, find an increment and blend the colors together
                //With the increment as a reference point.
                Equation perpendicular = Equation.FromPoints(p.UVPoints[0], 
                //Midpoint between p.UVPoints[1] and p.UVPoints[2]
                new Point((p.UVPoints[1].Y + p.UVPoints[2].Y)/2, (p.UVPoints[1].X + p.UVPoints[2].X)/2));
                int MinY = Texturer.Min([p.UVPoints[0].Y, p.UVPoints[1].Y, p.UVPoints[2].Y]);
                int MaxY = Texturer.Max([p.UVPoints[0].Y, p.UVPoints[1].Y, p.UVPoints[2].Y]);
                for(int y =MinY; y < MaxY - MinY;y++){
                        float increment = tD.AllThat(((Point point, Color color) item) => {
                            if((item.point.X > p0_p1.SolveX(y) & item.point.X < p1_p12mid.SolveX(y)) &&
                            item.point.Y == y){return true;}else{return false;}});
                        increment /= p1_p12mid.SolveX(y) - p0_p1.SolveX(y);
                        //Ive now got the multiplier the shrink or stretch the textureData.
                }
            }
            return p;
        });
    return tD;
    }
}
/// <summary>
/// The true shape of a Collider
/// </summary>
class TrueCollider{
    //public static readonly TrueCollider Sphere = 0;
    public static TrueCollider Cube{get{_cube.Dimensions = (5, 5); return _cube;}}
#pragma warning disable CS8601 // Possible null reference assignment.
    static readonly TrueCollider _cube = (TrueCollider?)1;
    public static TrueCollider Capsule{get{_capsule.Dimensions = (5, 3); return _capsule;}}
    static readonly TrueCollider _capsule = (TrueCollider?)2;
#pragma warning restore CS8601 // Possible null reference assignment.

    internal string Name{get; private set;}
    internal int type;

    public (int height, int width) Dimensions{get; private set;}
    public void UpdateDimensions(int height, int width){this.Dimensions = (height, width);}

    internal TrueCollider(int tyPe, int Height = 10, int Width = 3){
        if(IsCollider(tyPe)){
            this.Name = TrueCollider.GetCollider(tyPe);
            this.type = tyPe;
            this.Dimensions = (Height, Width);
        }else{
            this.Name = "Cube";
            this.type = TrueCollider.Cube.type;
            this.Dimensions = TrueCollider.Cube.Dimensions;
        }
    }

    public static bool IsCollider(int type){
        switch(type){
            case 1: return true;
            case 2: return true;
            default: return false;
        }
    }
    public static string GetCollider(int type){
        if(!IsCollider(type)){return "";}else{
            switch(type){
                case 1: return  "Cube";
                case 2: return "Cylinder";
                default: return "";
            }
        }
    }
    public static explicit operator TrueCollider(int type){if(IsCollider(type)){return new TrueCollider(type);}else{return TrueCollider.Cube;}}
    public static explicit operator int(TrueCollider tC){
        return tC.type;
    }
    public static Polygon[] GetMesh(TrueCollider? tC){
        int bevel = 0;
        if(tC == null){return Polygon.Mesh(10, 10, bevel);}
        bevel = tC.type switch{
            1 => 0,
            2 => 2,
            _ => 0,
        };
        return Polygon.Mesh(tC.Dimensions.height, tC.Dimensions.width, bevel);
    }
    public static void Assign(TrueCollider tC1, TrueCollider tC2){tC1 = tC2;}
}

/// <summary>
/// This will affect how physics interacts with the attached object.
/// </summary>
class PhysicsMaterial{
    public static readonly PhysicsMaterial SandPaper = new PhysicsMaterial(10, 0);
    public static readonly PhysicsMaterial Rubber = new PhysicsMaterial(5, 7);
    public static readonly PhysicsMaterial GlazedWood = new PhysicsMaterial(3, 1);
    public static explicit operator PhysicsMaterial((int a, int b) tC){return new PhysicsMaterial(tC);}
    public static bool IsPhysicsMaterial((int f, int b) item){
        switch(item){
            case (10, 0): return true;
            case (5, 7): return true;
            case (3, 1): return true;
            default: return false;
        }
    }

    public int Friction{get; private set;}
    public int Bounciness{get; private set;}
    public PhysicsMaterial(int Friction, int Bounciness){
        this.Friction = Friction;
        this.Bounciness = Bounciness;
    }
    public PhysicsMaterial((int f, int b) item){
        this.Friction = item.f;
        this.Bounciness = item.b;
    }
}

/// <summary>
/// Affects how velocity is Applied to the attached object.
/// </summary>
class ForceMode{
#pragma warning disable CS8601 // Possible null reference assignment.
    static ForceMode(){
        Impulse = (ForceMode?)0;
        Acceleration = (ForceMode?)1;
        VelocityChange = (ForceMode?)2;
#pragma warning restore CS8601 // Possible null reference assignment.
    }
    ///<summary>Apply a force that is affected by the gameObjects mass and is multiplied by <see cref"ExternalControl.deltaTime"></summary>
    /// <remarks>This is also the default ForceMode value.</remarks>
    public static readonly ForceMode Impulse;
    ///<summary>Apply a force that is affected by the gameObjects mass.</summary>
    public static readonly ForceMode Acceleration;
    ///<summary>Apply a force that ignores the object's mass.</summary>
    public static readonly ForceMode VelocityChange;
    public static explicit operator ForceMode?(int type){if(IsForceMode(type)){return new ForceMode(type);}else{return null;}}
    public static bool IsForceMode(int type){
        switch(type){
            case 0: return true;
            case 1: return true;
            case 2: return true;
            default: return false;
        }
    }

    public int type;
    internal ForceMode(int type){this.type = type;}
    public void Apply(RigidBdy rB, Vector3 v){
        if(!ForceMode.IsForceMode(rB.MetaData.fM.type)){return;}else{
            switch(this.type){
                case 0:
                    //Is Impulse.
                    rB.velocity += new Vector3((float)Math.Sqrt(v.X/(rB.Mass/2)), (float)Math.Sqrt(v.Y/(rB.Mass/2)), (float)Math.Sqrt(v.Z/(rB.Mass/2))) * ExternalControl.deltaTime;
                    break;
                case 1:
                    //Is Acceleration.
                    rB.velocity += new Vector3((float)Math.Sqrt(v.X/(rB.Mass/2)), (float)Math.Sqrt(v.Y/(rB.Mass/2)), (float)Math.Sqrt(v.Z/(rB.Mass/2)));
                    break;
                case 2:
                    //Is VelocityChange.
                    rB.velocity = v;
                    break;
                default:    return;
            }
        }
    }
}
