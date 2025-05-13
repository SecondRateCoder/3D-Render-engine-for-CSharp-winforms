using System.Collections;

class TextureDatabase : IEnumerable{
    public static TextureDatabase Empty{get{return [];}}
    public delegate (Point point, Color color) ForEachDelegate((Point point, Color color) item);
    public delegate bool ForEachDelegateConditional((Point point, Color color) item);
    List<(Point point, Color color)> td;
    /// <summary>This serves to define sections of TextureData within the td List.</summary>
    List<(int Start, int End)> PerSectionRanges{get; set;} = [];
    /// <summary>True if the TextureData is sorted.</summary>
    public bool isSorted{get; private set;}
    public int Count{get{return td.Count;}}
    public (Point p, Color c) this[int index]{
        get{return td[index];}
        set{td[index] = value;}
    }
    /// <summary>Interaction with the Bounding points of a section's TextureData.</summary>
    /// <param name="index">The index of the section's bounding data.</param>
    /// <param name="b">Interacting with it won't make a difference, 
    /// serves as a placeholder to distinguish between The TextureData and the Section Bounding Data</param>
    /// <returns>The bounding points of the section's TextureData.</returns>
    public (int Start, int End) this[int index, bool b]{
        get{return PerSectionRanges[index];}
        set{PerSectionRanges[index] = value;}
    }
    /// <summary>Retrive the TextureData of a singular Section within this TextureDatabase.</summary>
    /// <param name="index">The index of PerSectionRanges that contains the selected Section's bounding TextureData.</param>
    /// <returns>The TextureData of a singular Section.</returns>
    public TextureDatabase RetrieveTexture_PerSectionBasis(int index){
        Span<(Point point, Color color)> Data = new([.. this.td], this.PerSectionRanges[index].Start, this.PerSectionRanges[index].End);
        return new TextureDatabase(Data.ToArray());
    }
    /// <summary>
    /// Define the bounds of a singular Section's TextureData within this TextureDatabase.
    /// </summary>
    /// <param name="Start">The index of this TextureDatabase where this Section's TextureData starts.</param>
    /// <param name="UVArea">The UVArea of the Section, can be retrieved with X.UVArea.</param>
    /// <returns>Was this Section's BoundingData successfully Defined</returns>
    public bool DefineSectionBounds(int Start, float UVArea){
        bool function((int, float) x){
            lock(PerSectionRanges){
                if (Start > 0 && Start + UVArea < Count){
                    PerSectionRanges.Add((Start, Start + (int)UVArea));
                    return true;
                }
                else { return false; }
            }
        }
        return LockJob<(int Start, float UVArea), bool>.
            LockJobHandler.
                PassJob((LockJob<(int Start, float UVArea), bool>.LockJobDelegate<(int, float), bool>)function, (Start, UVArea), null, 1000, null, nameof(TextureDatabase)).Result;
    }



    /// <summary>
    /// Re-define the bounds of a singular Section's TextureData within this TextureDatabase.
    /// </summary>
    /// <param name="index">The index of PerSectionRanges that contains the selected Section's bounding TextureData.</param>
    /// <param name="Start">The index of this TextureDatabase where this Section's TextureData starts.</param>
    /// <param name="UVArea">The UVArea of the Section, can be retrieved with X.UVArea.</param>
    /// <returns>Was this Section's BoundingData successfully Re-defined</returns>
    public bool ReDefineSectionBounds(int index, int Start, float UVArea){

        bool function((int, float) x){
            lock(PerSectionRanges){
                if(Start > 0 && (Start + UVArea) < this.Count){
                    PerSectionRanges.Add((Start, Start + (int)UVArea));
                    return true;
                }else{return false;}
            }
        }
        return LockJob<(int Start, float UVArea), bool>.
            LockJobHandler.
                PassJob((LockJob<(int Start, float UVArea), bool>.LockJobDelegate<(int, float), bool>)function, (Start, UVArea), null , 1000, null, nameof(TextureDatabase)).Result;

    }
    public TextureDatabase(){this.td = [];}
    public TextureDatabase(int count = 0){this.td = new List<(Point point, Color color)>();}
    public TextureDatabase(IEnumerable<(Point p, Color c)> data){this.td = [.. data]; this.isSorted = false;}
    public TextureDatabase(IEnumerable<Point> points, Color color){
        this.td = [];
        foreach(Point p in points){td.Add((p, color));}
    }
    public TextureDatabase(WriteableBitmap bmp){
        this.td = new List<(Point point, Color color)>(bmp.pixelWidth * bmp.pixelHeight);
        int cc =0;
        for(int y = 0;y < bmp.pixelHeight;y++){
            for(int x = 0;x < bmp.pixelWidth;x++){
                td[cc] = bmp[x, y];
                cc++;
            }
        }
        
    }
    public int AllThat(ForEachDelegateConditional fDC){
        int inc = 0;
        foreach((Point point, Color color) item in this.td){
            inc = fDC(item)? inc+1: inc;
        }
        return inc;
    }
    public void Foreach(ForEachDelegate fE){
        for(int cc = 0;cc < this.Count;cc++){
            this[cc] = fE(this[cc]);
        }
    }
    public void AddAt(int index, (Point p, Color c) item){
        if(index > this.Count){this.Append(item);}else{
            td.Insert(index, item);
            this.isSorted = this[index].p.X<item.p.X && this[index].p.Y<item.p.Y?true:false;
        }
    }
    public void AddRangeAt(int index, IEnumerable<(Point p, Color c)> data){
        if(index > this.Count){this.Append(data.ToList());}else{
            foreach((Point p, Color c) item in data){
                td.Insert(index, item);
                this.isSorted = this[index].p.X<item.p.X && this[index].p.Y<item.p.Y?true:false;
            }
        }
    }
    public void AssignRangeAt(int index, IEnumerable<(Point p, Color c)> data){
        if(index + data.Count() > this.Count){throw new ArgumentOutOfRangeException("The range of data to be assigned is out of bounds.");}
        for(int cc =0; cc < data.Count();cc++){
            td[index + cc] = data.ElementAt(cc);
            this.isSorted = this[index + cc].p.X<data.ElementAt(cc).p.X && this[index + cc].p.Y<data.ElementAt(cc).p.Y?true:false;
        }
    }
    public void Append((Point p, Color c) item){
        td.Add(item);
        this.isSorted = this[Count-1].p.X<item.p.X && this[Count-1].p.Y<item.p.Y?true:false;
    }
    public void Append(List<(Point p, Color c)> data){foreach((Point p, Color c) item in data){this.Append(item);}}
    public static WriteableBitmap ToWriteableBitmap(TextureDatabase tD, int width, int height){
        new InconsistentDimensionException().ThrowIf(tD.Count != width * height, "The TextureData's dimensions do not match the Bitmap's dimensions.");
        WriteableBitmap wB = new(width, height);
        int cc= 0;
        for(int y= 0;y < height;y++){
            for(int x = 0;x < width;x++){
                wB[x, y] = tD[cc];
                cc++;
            }
        }
        return wB;
    }
    public static implicit operator List<(Point point, Color color)>(TextureDatabase tD){return tD.td;}
    public static explicit operator TextureDatabase(List<(Point point, Color color)> data){return new TextureDatabase(data);}
    public IEnumerator GetEnumerator() => td.GetEnumerator();
}


class CollisionDatabase : IEnumerable{
    public delegate List<(gameObj gameObject, int Mass, Vector3 velocity)> ForEachDelegateExpandable (List<(gameObj gameObject, int Mass, Vector3 velocity)> item);
	List<List<(gameObj gameObject, int Mass, Vector3 velocity)>> collidingObjects = [];
	public int Length{get{return collidingObjects.Count;}}
    public void Foreach(ForEachDelegateExpandable fE){
        for(int cc = 0;cc < this.Length;cc++){
            this[cc] = fE(this[cc]);
        }
    }
	public (gameObj gameObject, int Mass, Vector3 velocity) this[int Row, int Column]{
		get{
			return this.collidingObjects[Row][Column];
		}
		set{
			if(Row >= collidingObjects.Count | Column >= collidingObjects[Row].Count){throw new ArgumentOutOfRangeException("");}
			this.collidingObjects[Row][Column] = value;
		}
	}
    public List<(gameObj gameObject, int Mass, Vector3 velocity)> this[int Row]{
        get{return this.collidingObjects[Row];}
        set{if(Row > collidingObjects.Count){collidingObjects.Append(value);}
        else{collidingObjects[Row] = value;}}
    }
    public CollisionDatabase(){this.collidingObjects = [];}
	public void SetOrAppend(Point p, (gameObj gameObject, int Mass, Vector3 velocity) data){
		if(p.X >= collidingObjects.Count){collidingObjects.Append([data]);}else
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
	public IEnumerator GetEnumerator() => collidingObjects.GetEnumerator();
}
