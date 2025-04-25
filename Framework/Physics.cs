using System.Collections;
using System.Timers;

class ColliderShape{
    public TrueCollider Type{get; private set;}
    public ColliderShape(TrueCollider tC){
        this.Type = tC;
    }














    public static implicit operator ColliderShape?(int tyPe){return new ColliderShape((TrueCollider)tyPe);}
    public static explicit operator ColliderShape(TrueCollider tC){return new ColliderShape(tC);}
    public static readonly ColliderShape Cube = (ColliderShape)TrueCollider.Cube;
    public static readonly ColliderShape Capsule = (ColliderShape)TrueCollider.Capsule;
    public class TrueCollider{
        //public static readonly TrueCollider Sphere = 0;
        public static TrueCollider Cube{get{_cube.Dimensions = (5, 5); return _cube;}}
        static readonly TrueCollider _cube = (TrueCollider)1;
        public static TrueCollider Capsule{get{_capsule.Dimensions = (5, 3); return _capsule;}}
        static readonly TrueCollider _capsule = (TrueCollider)1;

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
        public static explicit operator TrueCollider?(int type){if(IsCollider(type)){return new TrueCollider(type);}else{return null;}}
        public static explicit operator int(TrueCollider tC){
            return tC.type;
        }
        public static Mesh GetCollider(TrueCollider tC){
            int bevel;
            switch(tC.type){
                case 1: bevel = 0;  break;
                case 2: bevel = 2;  break;
                default: bevel = 0; break;
            }
            return (Mesh)Polygon.Mesh(tC.Dimensions.height, tC.Dimensions.width, bevel);
        }
        public static void Assign(TrueCollider tC1, TrueCollider tC2){tC1 = tC2;}
    }
}


class PhysicsMaterial{
    public static readonly PhysicsMaterial SandPaper = new PhysicsMaterial(10, 0);
    public static readonly PhysicsMaterial Rubber = new PhysicsMaterial(5, 7);
    public static readonly PhysicsMaterial GlazedWood = new PhysicsMaterial(3, 1);

    public int Friction{get; private set;}
    public int Bounciness{get; private set;}
    public PhysicsMaterial(int Friction, int Bounciness){
        this.Friction = Friction;
        this.Bounciness = Bounciness;
    }
}



static class CollisionManager{
    static CollisionManager(){
        //Entry.TUpdate += Collider;
    }
    ///<summary>A property with a nested iterative function.</summary>
    ///<returns>A CollisionDatabase describing the collisions occuring in the program at the current frame.</returns>
    ///<remarks>This property runs an internal function when getting is attempted, assigning the return to a CollisionDatabase variable is best practice.</remarks>
    public static CollisionDatabase LooseCollisions{
        get{
            CollisionDatabase list = new CollisionDatabase();
            gameObj scope = World.worldData[0];
            for(int cc = 1;cc < World.worldData.Count;cc++, scope = World.worldData[cc]){
                if(!scope.HasComponent<RigidBdy>()){continue;}else{
                    list.Append(new List<(gameObj, int, Vector3)>());
                    list[cc].AddRange(SubSearch(scope));
                }
            }
            return list;
    }}
    ///<summary>Searches in the World.worldData list for any objects colliding with gameObj</summary>
    static (gameObj gO, int Mass, Vector3 velocity)[] SubSearch(gameObj scope){
        (gameObj gO, int Mass, Vector3 velocity)[] result = [];
        foreach(gameObj gO in World.worldData){
            if(Vector3.GetDistance(scope.Position, gO.Position) < gO.CollisionRange+scope.CollisionRange){
                RigidBdy? rG = gO.GetComponent<RigidBdy>();
                if(rG == null){continue;}else{
                    result.Append((gO, rG.Mass, rG.velocity));
                }
            }
        }
        return result;
    }

    public static CollisionDatabase FineCollisions{
        get{
            return new CollisionDatabase();
        }
    }
    public static void Collider(object? sender, ElapsedEventArgs e){
        CollisionDatabase cD = LooseCollisions;
        //For now just apply the Normal between the colliding polygons as a force.
        for(int y =0; y < cD.Length;y++){
            for(int x =1; x < cD[y].Count;x++){
                cD[y][0].gameObject.GetComponent<RigidBdy>().velocity = 
                    Vector3.CProduct(cD[y][0].gameObject.Position, 
                        cD[y][x].gameObject.Position * cD[y][x].gameObject.GetComponent<RigidBdy>().GetEnergy())/10;
            }
        }
    }
}