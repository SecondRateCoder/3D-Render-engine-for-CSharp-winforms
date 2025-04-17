using System.Collections;
using System.Timers;
class CollisionDatabase : IEnumerable{
	List<(gameObj gameObject, int Mass, Vector3 velocity)>[] collidingObjects;
	public int Length{get{return collidingObjects.Length;}}
	public (gameObj gameObject, int Mass, Vector3 velocity) this[int Row, int Column]{
		get{
			return this.collidingObjects[Row][Column];
		}
		set{
			if(Row >= collidingObjects.Length | Column >= collidingObjects[Row].Count){throw new ArgumentOutOfRangeException("");}
			this.collidingObjects[Row][Column] = value;
		}
	}
    public List<(gameObj gameObject, int Mass, Vector3 velocity)> this[int Row]{
        get{return this.collidingObjects[Row];}
        set{if(Row > collidingObjects.Length){collidingObjects.Append(value);}
        else{collidingObjects[Row] = value;}}
    }
	public void SetOrAppend(Point p, (gameObj gameObject, int Mass, Vector3 velocity) data){
		if(p.X >= collidingObjects.Length){collidingObjects.Append([data]);}else
		if(p.Y >= collidingObjects[p.X].Count){collidingObjects[p.X].Append(data);}else
		{collidingObjects[p.X][p.Y] = data;}
	}
    public void Append(IEnumerable<(gameObj gameObject, int Mass, Vector3 velocity)> data){this.collidingObjects.Append(data);}
	public void Append(int position, (gameObj gameObject, int Mass, Vector3 velocity) data){this.collidingObjects[position].Append(data);}
	public void AddRange(int position, IEnumerable<(gameObj gameObject, int Mass, Vector3 velocity)> data){
        if(position > this.Length){return;}else{
            this.collidingObjects[position].AddRange(data);
        }
    }
    IEnumerator IEnumerable.GetEnumerator(){return (IEnumerator)GetEnumerator();}
	public CollisionDataEnum GetEnumerator(){ return new CollisionDataEnum(collidingObjects);}
}
class CollisionDataEnum : IEnumerator{
	public List<(gameObj gameObject, int Mass, Vector3 velocity)>[] objects;
    Point position= new Point(-1, -1);
    public CollisionDataEnum(List<(gameObj gameObject, int Mass, Vector3 velocity)>[] collidingObjects){
        this.objects = collidingObjects;
    }
    public bool MoveNext(){
        if(position.Y < objects[position.X].Count){
			position.Y++;
		}else if(position.X > objects.Length){
			return true;
		}else{
			position.Y = 0;
			position.X++;
			return true;
		}
		return false;
    }
    public void Reset(){position = new Point(-1, -1);}
    object IEnumerator.Current{
        get{
            return Current;
        }
    }
    public (gameObj gameobject, int mass, Vector3 velocity) Current{
        get{
            try{
                return this.objects[position.X][position.Y];
            }catch(IndexOutOfRangeException){
                throw new InvalidOperationException();
            }
        }
    }
}
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