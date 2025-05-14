using System.Collections;

class TextureDatabase : IEnumerable{
    public static TextureDatabase Empty{get{return [];}}
    public delegate TexturePoint ForEachDelegate(TexturePoint item);
    public delegate bool ForEachDelegateConditional((PointF point, Color color) item);
    List<TexturePoint> td;
    public IEnumerable<TexturePoint> GetIEnumerable(){return this.td;}
    /// <summary>This serves to define sections of TextureData within the td List.</summary>
    /// <remarks>This property's main purpose is for applying <see cref="TextureStyles"/>, especially for more complex ones such as <see cref="TextureStyles.StretchToFit"/> 
    /// as well as for Rendering.</remarks>
    List<(int Start, int End)> Sections{get; set;} = [];
    /// <summary>True if the TextureData is sorted.</summary>
    public bool isSorted{get; private set;}
    public int Count{get{return td.Count;}}
    public TexturePoint this[int index]{
        get{return td[index];}
        set{td[index] = value;}
    }
    /// <summary>Interaction with the Bounding points of a section's TextureData.</summary>
    /// <param name="index">The index of the section's bounding data.</param>
    /// <param name="b">Interacting with it won't make a difference, 
    /// serves as a placeholder to distinguish between The TextureData and the Section Bounding Data</param>
    /// <returns>The bounding points of the section's TextureData.</returns>
    public (int Start, int End) this[int index, bool b]{
        get{return Sections[index];}
        set{Sections[index] = value;}
    }
    /// <summary>Retrive the TextureData of a singular Section within this TextureDatabase.</summary>
    /// <param name="index">The index of PerSectionRanges that contains the selected Section's bounding TextureData.</param>
    /// <returns>The TextureData of a singular Section.</returns>
    public TextureDatabase Slice_PerSectionRanges(int index){return new TextureDatabase(new Span<TexturePoint>([.. this.td], this.Sections[index].Start, this.Sections[index].End).ToArray());}
    /// <summary>
    /// Slice a section of this TextureDatabase, from <see cref="Start"/> to <see cref="End"/>.
    /// </summary>
    /// <returns>A section of thisd Database.</returns>
    public TextureDatabase Slice(int Start, int End){
        ArgumentOutOfRangeException.ThrowIfLessThan(Start, End, $"TextureDatabase.RetrieveTexture_PerSectionBasis(Start: {Start}, End: {End})");
        return new TextureDatabase(new Span<TexturePoint>([.. this.td], Start, End).ToArray());}
    /// <summary>
    /// Define the bounds of a singular Section's TextureData within this TextureDatabase.
    /// </summary>
    /// <param name="Start">The index of this TextureDatabase where this Section's TextureData starts.</param>
    /// <param name="UVArea">The UVArea of the Section, can be retrieved with X.UVArea.</param>
    /// <returns>Was this Section's BoundingData successfully Defined</returns>
    public bool AttachSectionBounds(int Start, float UVArea){
        bool function((int, float) x){
            lock(Sections){
                if(Start > 0 && Start + UVArea < Count){
                    Sections.Add((Start, Start + (int)UVArea));
                    return true;
                }
                else { return false; }
            }
        }
        return LockJob<(int Start, float UVArea), bool>.
            LockJobHandler.
                PassJob(function, (Start, UVArea), null, 1000, null, nameof(TextureDatabase)).Result;
    }
    public bool AttachSectionBounds((int a, int b) item){
        bool function((int a, int b) item_){
            lock(this.Sections){
                if(item.b > item.a && (item.a | item.b) > 0){
                    this.Sections.Add(item_);
                    return true;
                }else{return false;}
            }
        }
        return LockJob<(int a, int b), bool>.
            LockJobHandler.PassJob(function, item).Result;
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
            lock(Sections){
                if(Start > 0 && (Start + UVArea) < this.Count){
                    Sections.Add((Start, Start + (int)UVArea));
                    return true;
                }else{return false;}
            }
        }
        return LockJob<(int Start, float UVArea), bool>.
            LockJobHandler.
                PassJob((LockJob<(int Start, float UVArea), bool>.LockJobDelegate<(int, float), bool>)function, (Start, UVArea), null , 1000, null, nameof(TextureDatabase)).Result;

    }
    public TextureDatabase(){this.td = [];}
    public TextureDatabase(int count = 0){this.td = new List<TexturePoint>();}
    public TextureDatabase(IEnumerable<TexturePoint> data){
        this.td = new List<TexturePoint>();
        foreach(TexturePoint item in data){this.td.Add(item);} 
        this.isSorted = false;
    }
    public TextureDatabase(IEnumerable<Point> points, Color color){
        this.td = [];
        foreach(Point p in points){td.Add((TexturePoint)(p, color));}
    }
    public TextureDatabase(WriteableBitmap bmp){
        this.td = new List<TexturePoint>(bmp.pixelWidth * bmp.pixelHeight);
        int cc =0;
        for(int y = 0;y < bmp.pixelHeight;y++){
            for(int x = 0;x < bmp.pixelWidth;x++){
                td[cc] = (TexturePoint)bmp[x, y];
                cc++;
            }
        }
        
    }
    public int AllThat(ForEachDelegateConditional fDC){
        int inc = 0;
        foreach((PointF point, Color color) item in this.td){
            inc = fDC(item)? inc+1: inc;
        }
        return inc;
    }
    public void Foreach(ForEachDelegate fE){
        for(int cc = 0;cc < this.Count;cc++){
            this[cc] = fE(this[cc]);
        }
    }
    public void AddAt(int index, TexturePoint item){
        if(index > this.Count){this.Append(item);}else{
            td.Insert(index, item);
            this.isSorted = this[index].p.X<item.p.X && this[index].p.Y<item.p.Y?true:false;
        }
    }
    public void AddRangeAt(int index, IEnumerable<TexturePoint> data, bool AddSection = true){
        (int a, int b) Section = (0, 0);
        if(index > this.Count){this.Append(data.ToList());}else{
            if(AddSection == true){Section.a = this.td.Count;}
            int cc =index;
            foreach(TexturePoint item in data){
                td.Insert(cc, item);
                this.isSorted = this[cc].p.X<item.p.X && this[cc].p.Y<item.p.Y?true:false;
                cc++;
            }
            if(AddSection == true){
                Section.b = this.td.Count;
                Sections.Add(Section);}

        }
    }
    public void AssignRangeAt(int index, IEnumerable<TexturePoint> data, bool DefineRanges = true){
        this.Sections.Add((index, data.Count()));
        if(index + data.Count() > this.Count){throw new ArgumentOutOfRangeException("The range of data to be assigned is out of bounds.");}
        for(int cc =0; cc < data.Count();cc++){
            td[index + cc] = data.ElementAt(cc);
            if(isSorted == true){this.isSorted = this[index + cc].p.X<data.ElementAt(cc).p.X && this[index + cc].p.Y<data.ElementAt(cc).p.Y?true:false;}
        }
    }
    public void Append(TexturePoint item){
        td.Add(item);
        this.isSorted = this[Count-1].p.X<item.p.X && this[Count-1].p.Y<item.p.Y?true:false;
    }
    public void Append(List<TexturePoint> data){foreach(TexturePoint item in data){this.Append(item);}}
    public static WriteableBitmap ToWriteableBitmap(TextureDatabase tD, int width, int height){
        InconsistentDimensionException.ThrowIf(tD.Count != width * height, "The TextureData's dimensions do not match the Bitmap's dimensions.");
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
    public static implicit operator List<TexturePoint>(TextureDatabase tD){return tD.td;}
    public static explicit operator TextureDatabase(List<TexturePoint> data){return new TextureDatabase(data);}
    public IEnumerator GetEnumerator() => td.GetEnumerator();





    public struct TexturePoint{
        public PointF p;
        public Color c;
        public TexturePoint(PointF p, Color c){
            this.p = p;
            this.c = c;
        }
        public static implicit operator (PointF p, Color c)(TexturePoint tP){return (tP.p, tP.c);}
        public static explicit operator TexturePoint((PointF p, Color c) item){return new TexturePoint(item.p, item.c);}
    }
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
