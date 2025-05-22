using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text;
using Microsoft.CodeAnalysis;

static class ExtensionHandler{
	static readonly string Start = "SetUp";
	static readonly string TUpdate = "TimedUpdate";
	static readonly string Update = "Update";
	static readonly string Closing = "Closing";
	/// <summary>This is simply for the sake of allowng the process to remember the Location of the Extension without passing more Parameters.</summary>
	static string PathToExtension = "";
	static string Usings = "";
	static Dictionary<string, object> JsonDeserialised;
	static CancellationTokenSource cts = new();
	const int MaxTries = 20;
	/// <summary>A enum to differentiate beteen the various Method extension types.</summary>
	public enum MethodType{
		/// <summary>Runs with <see cref="Entry.Start"/>.</summary>
		Start = 0,
		/// <summary>Runs with <see cref="Entry.Update"/>.</summary>
		Update = 1,
		/// <summary>Runs with <see cref="Entry.TUpdate"/>.</summary>
		TimedUpdate = 2,
		/// <summary>Runs when the application ends.</summary>
		Closing = 3,
		/// <summary>This means the method runs outside the rendering loop, the method types are not protected against and can damage the application.</summary>
		/// <remarks>UNIMPLEMENTED, DO NOT USE.</remarks>
		Workspace = 4
	}
	static bool StartUp;
	static ExtensionHandler() { extensions = []; IndexToName = []; }
	/// <summary>Convert from a Priority key to an Extension name, mainly for Exception logging.</summary>
	static Dictionary<int, string> IndexToName;
	/// <summary>Stores all the data needed for the system to interface with the method and understand it for what it needs to do.</summary>
	static Dictionary<int, (int ErrorCalls, MethodType type, MethodInfo method)> extensions;
	/// <summary>Loads extensions from the Cache\Saves\Extensions.txt cache, allowing extensions to be automatically loaded from start-up.</summary>
	public static bool[] PreLoadExtensions(){
		StartUp = true;
		List<(string Path, string Message)> ErrorMessage = [];
		bool[] result = Task.Run<bool[]>(() => {
			bool[] result = [];
			string[] ExtensionFilePaths = File.ReadAllLines(System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Extensions.txt"));
			result = new bool[ExtensionFilePaths.Length];
			List<string> validPaths = [];
			int cc =0;
			foreach(string s in ExtensionFilePaths){
				PathToExtension = Directory.Exists(s)? s: (File.Exists(s)? StorageManager.GetParentDirectory(s): "");
				if(PathToExtension == ""){return [false];}
				if (BuildJsonObject().Result){
					validPaths.Add(s);
					result[cc] = true;
				}else{
					result[cc] = false;
					ErrorMessage.Add((s, "File was not in the same Format as expected of a Json extension"));
				}
				cc++;
			}
			File.WriteAllLines(StorageManager.ApplicationPath + @"Cache\Extensions.txt", validPaths);
			if(!Entry.SkipStartUpWarnings){MessageBox.Show("The path and the Message line-up, where there is a \"...\" Message that means that a more impactful error was thrown\n\n" + CustomFunctions.ToString(CustomFunctions.GetTupleArrayT(ErrorMessage)) + "\n" + CustomFunctions.ToString(CustomFunctions.GetTupleArrayR(ErrorMessage)), "Error PreLoading error: Debugged info", MessageBoxButtons.OK, MessageBoxIcon.Warning);}
			StartUp = false;
			return result;
		}).Result;
		return result;
	}
	static void SaveExtension(){
		if (MessageBox.Show("Should the extension be Pre-Loaded on startup of application?", "Pre-load extensions?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.OK){
			using(FileStream fS = File.OpenWrite(System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Extensions.txt"))){
				byte[] array = Encoding.Default.GetBytes(PathToExtension);
				bool completed = true;
				int num = 0;
				do{
					try{
						fS.Write(array, 0, array.Length);
						completed = true;
					}catch(Exception ex){
						if(num < MaxTries){
							if (MessageBox.Show($"The extension could not be written to {System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Extensions.txt")}\nRetry?",
								$"Error Message:\n{ex.Message}",
								MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry) { completed = false; num++; }else{break;}
							Task.Delay(1000);
						}
					}
				}while(!completed);
				if(completed){_ = MessageBox.Show("Extension loaded successfully.", null, MessageBoxButtons.OK, MessageBoxIcon.Information);}
			}
		}
	}
	//!This is the only point where the Extension loading process it allowed to be started (aside from PreloadExtensions), it retrieves all the data it needs locally so it doesnt need any parameters.
	/// <summary>Begins the process of attaching an extension, doing it's logic checks and validation locally.</summary>
	/// <remarks>Open a <see cref="OpenFileDialog"/> and use it to select a folder containing a Metadata.json file.</remarks>
	/// <returns>Was the folder containing viable data for an extension attaching.</returns>
	public static async Task<bool> AttachExtension(){
		bool Suceeded = true;
		await Task.Run(() => {
			using(OpenFileDialog fD = new()){
				    fD.Title = "Open Metadata.json";
				    fD.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
				    fD.FileName = "Metadata.json";
				    fD.CheckFileExists = true;
				    fD.CheckPathExists = true;
				if(fD.ShowDialog() == DialogResult.OK){
					Suceeded = BuildJsonObject().Result;
				}else{ Suceeded = false; }
			}
			if(Suceeded){SaveExtension();}
			return Suceeded;
		});
		return Suceeded;
	}
	/// <summary>Builds and stores the Json file object, checking if the File contains the right properties.</summary>
	/// <returns>Was the Json file of the right format?</returns>
	static async Task<bool> BuildJsonObject(){
		//foreach (char c in Jsonbody) { if (!(char.IsLetterOrDigit(c) | char.IsPunctuation(c))) { throw new JsonFormatexception($"The file at: {filePath + "MetaData.json"} had the wrong format and is incompatible"); } }
		JsonDeserialised = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(System.IO.Path.Combine(PathToExtension + "Metadata.Json")), JsonSerializerOptions.Default) ?? throw new ArgumentException();
		//Code file Path
		//Error handling.
		//The ClassName.
		string Name = "";
		if(JsonDeserialised.TryGetValue("ClassName", out object? classname) && classname != null){
			Name = classname is JsonElement jE ? jE.GetString() ?? $"UndefinedExtension_{extensions.Count}": throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		}else if(JsonDeserialised.TryGetValue("Name", out object? name) && name != null){
			Name = name is JsonElement jE ? jE.GetString() ?? $"UndefinedExtension_{extensions.Count}": throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		}else{throw new FileFormatException("File was not in the same Format as expected of a Json extension");}
		//Priority of the extension.
		_ = JsonDeserialised.TryGetValue("Priority", out object? priority) == true ? true : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		_ = priority ?? (!Entry.SkipStartUpWarnings? MessageBox.Show($"The Json at {PathToExtension} does not have a Priority of expected value, changing value of Priority to: {extensions.Count}", null, MessageBoxButtons.OK, MessageBoxIcon.Warning): DialogResult.OK);
		int Priority = priority is JsonElement je ? Convert.ToInt32(je.ToString()) : throw new Exception("Priority is not a JsonElement");
		GatherUsings();
		bool Start = await LoadToMemory(Priority, Name, MethodType.Start);
		if(!Start){ return false; }
		bool Update = await LoadToMemory(Priority, Name, MethodType.Update);
		bool TimedUpdate = await LoadToMemory(Priority, Name, MethodType.TimedUpdate);
		bool OnClosing = await LoadToMemory(Priority, Name, MethodType.Closing);
		return Start && true | Update | TimedUpdate | OnClosing;
	}
	/// <summary>Invoke all the extension functions of type: <paramref name="type"/>.</summary>
	/// <param name="type">The extension type that is being called.</param>
	/// <param name="parameters">The parameters needed to run the program</param>
	/// <returns>Where methids invoked?</returns>
	/// <remarks>If the type is an action the parameters don't matter as a new parameter list would be passed for invocation anyways.</remarks>
	public static bool InvokeExtensionMethods(MethodType type, params object[] parameters){
		bool Invoked = false;
		int cc = 0;
		foreach(var item in extensions){
			int errorCalls = item.Value.ErrorCalls;
			if(item.Value.type == type){
				try{
					if(item.Value.method.GetParameters().Length > 0){
						item.Value.method.Invoke(null, BindingFlags.Static, null, type == MethodType.Update? []: parameters, CultureInfo.InvariantCulture);
					}else{item.Value.method.Invoke(null, BindingFlags.Static, null, [], CultureInfo.InvariantCulture);}
				}catch(Exception ex){
					if(item.Value.ErrorCalls > (int)(MaxTries/item.Key)){
						MessageBox.Show(ex.Message + $"{IndexToName[item.Key]} has called too many errors({(int)(MaxTries/item.Key)} Exceptions) and has been removed from the Application to maintain performance.", $"Error in Extension: {IndexToName[item.Key]}", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}else{
						DialogResult dR = MessageBox.Show(ex.Message + $"Should this extension be removed? \n(It's possible that it will be removed later if it calls {(int)(MaxTries/item.Key)} Exceptions.)", $"Error in Extension: {IndexToName[item.Key]}", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
						if(dR == DialogResult.OK){ extensions.Remove(item.Key); continue; }else{ errorCalls++; }
					}
				}
				extensions[item.Key] = (errorCalls, item.Value.type, item.Value.method);
				cc++;
				Invoked = true;
			}
		}
		return Invoked;
		
	}
	/// <summary>This overload of <see cref="ExtensionHandler.LoadToMemory(int, string, string, string, MethodType)"/> uses some built-in helper methods to allow for a level of abstraction, it's workflow is similar to <see cref="ExtensionHandler.LoadToMemory(int, string, string, string, MethodType)"/>.</summary>
	/// <param name="Priority">The extension's Priority level.</param>
	/// <param name="ClassName">The name of the class that the extension will be compiled to.</param>
	/// <param name="methodType">The type of the method.</param>
	/// <returns></returns>
	static async Task<bool> LoadToMemory(int Priority, string ClassName, MethodType methodType) {
		return await LoadToMemory(Priority, ClassName, NameFromType(methodType), GetFunctionBody(NameFromType(methodType)), methodType);
	}
	static string NameFromType(MethodType methodType) {
        return methodType switch
        {
            MethodType.Start => Start,
            MethodType.Update => Update,
            MethodType.TimedUpdate => TUpdate,
            MethodType.Closing => Closing,
            _ => throw new ArgumentException(),
        };
    }
	static MethodType TypeFromName(string MethodName) {
		switch (MethodName){
			case "SetUp":
				return MethodType.Start;
			case "Update":
				return MethodType.Update;
			case "TimedUpdate":
				return MethodType.TimedUpdate;
			case "Closing":
				return MethodType.Closing;
			default:
				throw new ArgumentException();
		}
	}
	/// <summary>Saves a singular extension function to allow it to be runnable.</summary>
	/// <param name="Priority">The extionsion function's property.</param>
	/// <param name="ClassName">The name of the function's parent extension.</param>
	/// <param name="functionName">The name of the function.</param>
	/// <param name="FunctionBody">The actual logic of the function.</param>
	/// <param name="methodType">The type of extension function it is, selected from <see cref="ExtensionHandler.MethodType"/></param>
	/// <returns>Was the function processed successfully.</returns>
	/// <exception cref="Exception">Did the compilation of the function fail? then call the Exception.</exception>
	static async Task<bool> LoadToMemory(int Priority, string ClassName, string functionName, string FunctionBody, MethodType methodType, int Recalls = 0){
		return await Task<bool>.Run(() => {
			ScriptOptions scriptOptions = ScriptOptions.Default
				.WithReferences(typeof(MessageBox).Assembly, typeof(MessageBoxButtons).Assembly, typeof(MessageBoxIcon).Assembly, typeof(Entry).Assembly, typeof(gameObj).Assembly)
				.WithImports("System");
			string returnType = methodType switch{
				MethodType.Start => "void ",
				MethodType.Update => "void ",
				MethodType.TimedUpdate => "void ",
				MethodType.Closing => "void ",
				_ => "void ",
			};
			FunctionBody = "class " + ClassName + "{ " + Usings + returnType + FunctionBody + "}";
			Script<object> script = CSharpScript.Create(FunctionBody, scriptOptions);
			Compilation compilation = script.GetCompilation();
			using (MemoryStream ms = new()){
				if(!compilation.Emit(ms).Success){
					DialogResult dR;
					bool Warn = !(Entry.SkipStartUpWarnings && StartUp);
					if (Recalls < 10) {
						if (Warn) {
							dR = MessageBox.Show("Compilation of the Extension at:" + PathToExtension + $" failed.\nThe Extension has been re-compiled {Recalls} times", "Extenson failed to load.", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
						} else { dR = DialogResult.Ignore; }
						//!(Entry.SkipStartUpWarnings && StartUp)? MessageBox.Show("Compilation of the Extension at:" + PathToExtension + $" failed.\nThe Extension has been re-compiled {Recalls} times", "Extenson failed to load.", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error)
					} else {
						if (Warn) {
							dR = MessageBox.Show("The extension at:" + PathToExtension + "failed to be compiled\nThere have been too many re-tries to compile the Extension\nCompilation has been cancelled.", "CompilationTimeoutException", MessageBoxButtons.OK, MessageBoxIcon.Error);
						} else { dR = DialogResult.OK; }
						cts.Cancel();
					}
					if(dR == DialogResult.Retry && Recalls < 10){
                        return LoadToMemory(Priority, ClassName, functionName, FunctionBody, methodType, Recalls + 1).Result;
					}else if(dR == DialogResult.Abort && Recalls < 10){
						cts.Cancel();
						return false;
					}else if(dR == DialogResult.Ignore){
                        return LoadToMemory(Priority, ClassName, functionName, FunctionBody, methodType, 10).Result;
					}else if(dR == DialogResult.OK){return false;}
				}
				ms.Seek(0, SeekOrigin.Begin);
				Assembly assembly = Assembly.Load(ms.ToArray());
				try {
					Type type = assembly.GetType(ClassName) ?? throw new ArgumentNullException("");
					object instance = Activator.CreateInstance(type) ?? throw new ArgumentNullException();
					//There is already an entry with the same Priority, give the function a lower Priority until it can be put into the Dictionary.
					if (extensions.TryGetValue(Priority, out (int ErrorCalls, MethodType type, MethodInfo method) value) && (value.type == methodType)) {
						bool skip = true;
						if (!extensions.TryGetValue(Priority, out (int ErrorCalls, MethodType type, MethodInfo method) _value) && (_value.type == methodType)) { skip = false; } else {
							DialogResult dR = !(Entry.SkipStartUpWarnings && StartUp)? MessageBox.Show(
								"Should all other Priorities be shifted upwards to accomodate this extension(CONTINUE)?\nShould this extension be given the next available Priority allotment(TRY)?\n(THIS MAY AFFECT HOW THE EXTENSION INTERACTS WITH THE APPLICATION AND IT MAY CRASH!)\n\nShould the extension not be added(CANCEL)?", $"ExtensionPriorityConflictionException: There are already an extension with Priority {Priority}",
								MessageBoxButtons.CancelTryContinue, MessageBoxIcon.Warning): Entry.DefaultPriorityConflictBehaviour;
							if (dR == DialogResult.Continue){
								// Create a new dictionary with incremented keys
								Dictionary<int, (int ErrorCalls, MethodType type, MethodInfo method)> updatedExtensions = [];
								Dictionary<int, string> updatedIndexToName = [];
								int sameShift = 0;
								foreach (KeyValuePair<int, (int ErrorCalls, MethodType type, MethodInfo method)> item in extensions) {
									if (item.Key > Priority) {
										updatedExtensions.Add(item.Key + 1, item.Value);
										if (IndexToName.TryGetValue(item.Key, out string? name)) {
											updatedIndexToName.Add(item.Key + 1, name);
										}
									} else if (item.Key == Priority) {
										sameShift++;
										updatedExtensions.Add(item.Key + sameShift, item.Value);
										if (IndexToName.TryGetValue(item.Key, out string? name)) {
											updatedIndexToName.Add(item.Key + sameShift, name);
										}
									}
								}
								extensions = updatedExtensions;
								IndexToName = updatedIndexToName;
							} else if (dR == DialogResult.TryAgain) {
								while ((extensions.TryGetValue(Priority, out (int ErrorCalls, MethodType type, MethodInfo method) value_) && (value_.type == methodType)) | !skip) {
									Priority++;
								}
							} else {return false;}
						}
					}
					_ = extensions.TryAdd(Priority, (0, methodType, type.GetMethod(functionName) ?? throw new ArgumentNullException()));
					_ = IndexToName.TryAdd(Priority, ClassName);
					ms.Dispose();
				} catch (ArgumentNullException) {
					ms.Dispose();
					return false;
				}
			}
			return true;
		});
	}
	/// <summary>Returns the Method body of a function from a Json.</summary>
	/// <param name="functionName">The name of the function.</param>
	/// <param name="Json">The Dictionary that describes the original json object for te function.</param>
	/// <param name="CodeText">The whole text that the Function Body resides in.</param>
	/// <returns>A string containing the function body</returns>
	/// <exception cref="FileFormatException">If the .json wasn't formatted correctly, then <see cref="FileFormatException"/> will be called.</exception>
	static string GetFunctionBody(string FunctionName){
		bool FunctionFound = JsonDeserialised.TryGetValue(FunctionName, out var Function) == true ? true : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		(int Start, int End) FunctionBounds = (0, 0);
		string CodeText = "";
		if (FunctionFound && Function is JsonElement element){
			FunctionBounds = (Convert.ToInt32(element.GetProperty("StartInFile").GetString()), Convert.ToInt32(element.GetProperty("EndInFile").GetString()));
			_ = element.TryGetProperty("RelativeFilePath", out JsonElement element_) == true ? true : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
			CodeText = File.ReadAllText(PathToExtension + element_.GetString() ?? throw new FileFormatException("File was not in the same Format as expected of a Json extension"));
		}
		string s =CustomFunctions.ToString<char>(new Span<char>(CodeText.ToArray()).Slice(FunctionBounds.Start, FunctionBounds.End).ToArray(), false);
		if(!ModerateFunction(s)){
			(int Start, int End) fBounds = GetFunctionBounds(TypeFromName(FunctionName), CodeText);
			s = CustomFunctions.ToString<char>(new Span<char>(CodeText.ToArray()).Slice(fBounds.Start, fBounds.End - fBounds.Start).ToArray(), false);
		}
		return s;
	}
	
	static bool ModerateFunction(string function){
		int Scopes = 0;
		char c = function[0];
		int Length = function.Length;
		for(int cc =0; cc < Length; cc++, c = function[cc == Length? Length - 1: cc]){
			if(c == '{'){Scopes++;}else if(c == '}'){Scopes--;}
		}
		return Scopes == 0;
	}
	static (int, int) GetFunctionBounds(MethodType mT, string WholeFunction){
		int Scopes = 0;
		string MethodName = NameFromType(mT);
		char c = WholeFunction[0];
		(int MethodStart, int MethodEnd) FunctionBounds = (0, 0);
		int Length = WholeFunction.Length;
		for(int cc =0; cc < Length;cc++, c = WholeFunction[cc == Length? Length - 1: cc]){
			if(c == '{'){Scopes++;}else if(c == '}'){Scopes--;}
			if(c == MethodName[0]){
				if(CustomFunctions.ToString(new Span<char>(WholeFunction.ToArray()).Slice(cc, MethodName.Length).ToArray(), false) == MethodName){
					FunctionBounds.MethodStart = cc;
					StringBuilder sB = new();
					int Buffer = Scopes;
					int cc_ = cc+MethodName.Length;
					sB.Append(MethodName);
					while(WholeFunction[cc_ == WholeFunction.Length? WholeFunction.Length - 1: cc_] != '{'){
						sB.Append(WholeFunction[cc_]);
						cc_++;
					}
					do{
						if (cc_ == WholeFunction.Length) { break; }
						char cTemp = WholeFunction[cc_];
						if (cTemp == '{') { Buffer++; } else if (cTemp == '}') { Buffer--; }
						sB.Append(cTemp);
						cc_++;
					}while(Buffer != Scopes);
					FunctionBounds.MethodEnd = sB.Length + FunctionBounds.MethodStart;
					break;
				}
			}
		}
		return FunctionBounds;
	}

	public static string GatherUsings(){
		string CodeTxt = File.ReadAllText(PathToExtension + "Code.txt");
		int counter = 0;
		StringBuilder usings = new();
		int Length = CodeTxt.Length;
		foreach(char _ in CodeTxt){
			string temp = ((counter < Length)? CodeTxt[counter].ToString(): "") + ((counter+1 < Length)? CodeTxt[counter+1].ToString(): "") + ((counter+2 < Length)? CodeTxt[counter+2].ToString(): "") + ((counter+3 < Length)? CodeTxt[counter+3].ToString(): "") + ((counter+4 < Length)? CodeTxt[counter+4].ToString(): "");
			if(temp == "using"){
				int lineLength;
				for(int cc_ = counter;cc_ < Length;cc_++){
					lineLength = 0;
					while(CodeTxt[lineLength] != ';'){lineLength++;}
					Span<char> Line = new Span<char>(CodeTxt.ToArray()).Slice(cc_, lineLength+1);
					if(CustomFunctions.ToString(Line.ToArray(), false).Contains("using")){usings.AppendLine(CustomFunctions.ToString(Line.ToArray()));}
				}
				//Line.Append(";\r\n");
			}
			counter++;
		}
		ExtensionHandler.Usings = usings.ToString();
		return usings.ToString();
	}
}