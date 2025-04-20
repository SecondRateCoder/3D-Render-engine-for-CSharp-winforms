using System.Collections;
using System.Timers;
static class CollisionManager{
    static CollisionManager(){
        //Entry.TUpdate += Collider;
    }
    ///<summary>
    /// This returns a 2d list that contains the data of colliding gameObjs, 
    ///</summary>
    ///<returns>The list is in the structure where for every gameObj(the 1st item on the 2nd dimension), 
    /// all the recorded collisions are recorded on every entry below the 1st.</returns>
    ///<remarks>This property runs an internal function when getting is attempted, saving the return to a list is suggested.</remarks>
    public static CollisionDatabase BoundaryCollisions{
        get{
            CollisionDatabase list = new CollisionDatabase();
            gameObj scope = World.worldData[0];
            for(int cc = 1;cc < World.worldData.Count;cc++, scope = World.worldData[cc]){
                RigidBdy? pM = scope.GetComponent<RigidBdy>();
                if(pM == null){continue;}else{
                    list.Append(new List<(gameObj, int, Vector3)>());
                    list.AddRange(list.Length, SubSearch(scope));
                }
            }
            return list;
    }}
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
        //Much of the properties in this tuple list is for the physics itself.
        /*Next is to make a function that gets the angle(as Vector3) of the colliding polygon
        Ill do this by finding the polygon closest to the colliding object then ill invert the velocity with that polygon as the normal
		get Vector3 v (position of the colliding obj)
		new float array a[children.Length]
		for Polygon p in Children{
			a[cc] = p.GetDistance(v)
		}
		foreach(float f in a){
			float scope;
			if (f > scope){
				scope = f
			}
			when completed{
				p = Children[cc]
			}
		}
		this.invert Velocity(Vector3 angle = p.GetAngle)
        */
    }
}