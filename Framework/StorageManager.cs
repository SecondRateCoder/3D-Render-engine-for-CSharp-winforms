static class StorageManager{
    static string PathProp = "";
    public static string filePath{set{
        PathProp = value + @"\Cache\StorageAttempt";
    }}
    static string fileSpecific = ".bin";
	/// <summary>
	///  This saves the World.worldData to the Binary file in StorageAttempt<seealso cref="fileSpecific"/>
	/// </summary>
	/// <remarks>This algorithm saves date in the pattern:	
	/// each gameObj's Position then their Rotation, then an number detailing the number of it's children,
	/// after which each child's A, B and C position is then stored.
	/// </remarks>
    public static void Save(string Method = ".bin"){
		fileSpecific = Method;
		List<byte> fileContent = [.. BitConverter.GetBytes(World.worldData.Count)];
		foreach(gameObj gO in World.worldData){
			fileContent.AddRange(BitConverter.GetBytes(gO.Size));
			fileContent.AddRange(gO.Position.ToBytes());
			fileContent.AddRange(gO.Rotation.ToBytes());
			fileContent.AddRange(BitConverter.GetBytes(gO.Children.Count));
			int cc = 0;
			//The children are encoded.
			foreach(Polygon pO in gO.Children){
				fileContent.AddRange(gO.Children[cc].A.ToBytes());
				fileContent.AddRange(gO.Children[cc].B.ToBytes());
				fileContent.AddRange(gO.Children[cc].C.ToBytes());
				cc++;
			}
			/*
			for(int cc =0;cc < gO.compLength;cc++){
				fileContent.AddRange(BitConverter.GetBytes(sizeof(char)*gO.GetComponentType(cc).Name.Count));
				foreach(char c in gO.GetComponentType(cc).Name){
					fileContent.AddRange(BitConverter.GetByte(c));
				}
			fileContent.AddRange(BitConverter.GetBytes(gO.GetComponentType(cc).Name));
			fileContent.AddRange(gO.GetComponent(cc).ToBytes());
			*/
			}
		}
	static gameObj[] Load(){
		gameObj[]? World = null;
		using(BinaryReader sR = new BinaryReader(File.Open(PathProp+fileSpecific, FileMode.Open))){
			int worldSize = GetFloat(sR);
			World = new gameObj[worldSize];
			for(int cc=0;cc<worldSize;cc++){
				int childSize = GetFloat(sR)-(sizeof(float)*6);
				Vector3 Pos = new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR));
				Vector3 Rot = new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR));
				List<Polygon> polys = new List<Polygon>();
				for(int i = 0;i < childSize;i++){
					polys.Append(new Polygon(new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR)), 
					new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR)), 
					new Vector3(GetFloat(sR), GetFloat(sR), GetFloat(sR))));
				}
				World[cc] = new gameObj(Pos, Rot, polys);
				string typeName = "";
				int siZe = GetFloat(sR);
				List<byte> content = new List<byte>();
				for(int i =0;i < siZe;i++){
					content.Append(sR.ReadByte());
				}
			}
		}
		return World;
	}
	static int GetFloat(BinaryReader sR){
		byte[] buffer = new byte[sizeof(float)];
		for(int cc=0;cc < sizeof(float);cc++){
			buffer[cc] = sR.ReadByte();
		}
		return BitConverter.ToInt32(buffer);
	}
	static List<string> typeNames = ["Rndrcomponent", "RigidBdy", "Vector", "Polygon", "gameObj", "Camera"];
	static string GetTypeName(byte[] content, ref int Mem){
		string result = "";
		for(int cc = Mem; cc < content.Length;cc++){
			byte[] buffer = new byte[sizeof(char)];
			for(int i = 0; i < buffer.Length;i++){
				buffer[i] = content[i+(cc*sizeof(char))];
			}
			result += BitConverter.ToChar(buffer);
			if(typeNames.Contains(result)){
				return result;
			}
			Mem = cc;
		}
		throw new Exception("CorruptedSaveFile", new FileFormatException("The file was been saved in a specific format, if this pattern has been broken then the "));
	}
}