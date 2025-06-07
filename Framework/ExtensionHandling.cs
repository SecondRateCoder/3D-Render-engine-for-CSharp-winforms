using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

static class ExtensionHandler{
	static readonly string Start = "SetUp";
	static readonly string TUpdate = "TimedUpdate";
	static readonly string Update = "Update";
	static readonly string Closing = "Closing";
	/// <summary>This is simply for the sake of allowng the process to remember the Location of the Extension without passing more Parameters.</summary>
	static string PathToExtension = "";
	static string CodeTxt;
	static string Properties_Fields = "";
	static string Usings;
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
		Workspace = 4,
		All = -1
	}
	static bool StartUp;
	#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	static ExtensionHandler() { ExtensionMethods = []; CallStack = new Dictionary<MethodType, List<int>>(5); }
	#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	/// <summary>Use to interface with <see cref="ExtensionHandler.ExtensionMethods"/> to efficiently retrieve the neccessary data to run an extension function.</summary>
	/// <remarks>The <see cref="int[]"/> array inside corresponds to the indexes in <see cref="ExtensionHandler.ExtensionMethods"/> of the Methods; ordered by thier Priority values. DO NOT APPEND VALUES TO THIS ARRAY.</remarks>
	static Dictionary<MethodType, List<int>> CallStack;
	/// <summary>Stores all the data needed for the system to interface with the method and understand it for what it needs to do.</summary>
	static List<(MethodType mT, int ErrorCalls, MethodInfo method)> ExtensionMethods;
	/// <summary>Loads extensions from the Cache\Saves\Extensions.txt cache, allowing extensions to be automatically loaded from start-up.</summary>
	///<returns>A list of booleans that correspond to an extension's line in Extensions.txt, t</returns>
	public static async Task<bool[]> PreLoadExtensions(){
		StartUp = true;
		return await Task.Run(() =>{
			bool[] result = [];
			string[] ExtensionFilePaths = File.ReadAllLines(System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Extensions.txt"));
			result = new bool[ExtensionFilePaths.Length];
			List<string> validPaths = [];
			int cc = 0;
			foreach (string s in ExtensionFilePaths){
				PathToExtension = Directory.Exists(s) ? s : (File.Exists(s) ? StorageManager.GetParentDirectory(s) : "");
				if (PathToExtension == "") { return [false]; }
				if (BuildJsonObject().Result){
					validPaths.Add(s);
					result[cc] = true;
				}else{
					result[cc] = false;
					ErrorQueue.Add(MethodType.All, "File was not in the same Format as expected of a Json extension");
				}
				cc++;
			}
			File.WriteAllLines(StorageManager.ApplicationPath + @"Cache\Extensions.txt", validPaths);
			ErrorQueue.Add(MethodType.All, "The path and the Message line-up, where there is a \"...\" Message that means that a more impactful error was thrown\n\n", new object());
			StartUp = false;
			return result;
		});
	}
	/// <summary>Save the extension at <see cref = "ExtensionHandler.PathToExtension"/> to be pre-loaded, producing the appropiate Dialog.</summary>
	static void SaveExtension(){
		if (MessageBox.Show("Should the extension be Pre-Loaded on startup of application?", "Pre-load extensions?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.OK){
			using (FileStream fS = File.OpenWrite(System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Extensions.txt"))){
				byte[] array = Encoding.Default.GetBytes(PathToExtension);
				bool completed = true;
				int num = 0;
				do{
					try{
						fS.Write(array, 0, array.Length);
						completed = true;
					}catch (Exception ex){
						if (num < MaxTries){
							if (MessageBox.Show($"The extension could not be written to {System.IO.Path.Combine(StorageManager.ApplicationPath, @"Cache\Extensions.txt")}\nRetry?",
								$"Error Message:\n{ex.Message}",
								MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry) { completed = false; num++; }
							else { break; }
							Task.Delay(1000);
						}
					}
				} while (!completed);
				if (completed) { _ = MessageBox.Show("Extension loaded successfully.", null, MessageBoxButtons.OK, MessageBoxIcon.Information); }
			}
		}
	}
	//!This is the only point where the Extension loading process it allowed to be started (aside from PreloadExtensions), it retrieves all the data it needs locally so it doesnt need any parameters.
	/// <summary>Begins the process of attaching an extension, doing it's logic checks and validation locally.</summary>
	/// <remarks>Open a <see cref="OpenFileDialog"/> and use it to select a folder containing a Metadata.json file.</remarks>
	/// <returns>Was the folder containing viable data for an extension attaching.</returns>
	public static bool AttachExtension(){
		bool function(bool func){
			bool Suceeded = true;
			lock (ExtensionMethods){
				using (OpenFileDialog fD = new()){
					fD.Title = "Open Metadata.json";
					fD.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
					fD.FileName = "Metadata.json";
					fD.CheckFileExists = true;
					fD.CheckPathExists = true;
					if (fD.ShowDialog() == DialogResult.OK){
						CodeTxt = fD.FileName;
						Suceeded = BuildJsonObject().Result;
					}
					else { Suceeded = false; }
				}
				if (Suceeded) { SaveExtension(); }
				return Suceeded;
			}
		}
		;
		return BackdoorJob<bool, bool>.BackdoorJobHandler.PassJob(function, true, nameof(ExtensionHandler), 1000).Result;
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
		if (JsonDeserialised.TryGetValue("ClassName", out object? classname) && classname != null){
			Name = classname is JsonElement jE ? jE.GetString() ?? $"UndefinedExtension_{ExtensionMethods.Count}" : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		}else if (JsonDeserialised.TryGetValue("Name", out object? name) && name != null){
			Name = name is JsonElement jE ? jE.GetString() ?? $"UndefinedExtension_{ExtensionMethods.Count}" : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		}
		else { throw new FileFormatException("File was not in the same Format as expected of a Json extension"); }
		//Priority of the extension.
		_ = JsonDeserialised.TryGetValue("Priority", out object? priority) == true ? true : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		_ = priority ?? (!Entry.SkipStartUpWarnings ? MessageBox.Show($"The Json at {PathToExtension} does not have a Priority of expected value, changing value of Priority to: {ExtensionMethods.Count}", null, MessageBoxButtons.OK, MessageBoxIcon.Warning) : DialogResult.OK);
		int Priority = priority is JsonElement je ? Convert.ToInt32(je.ToString()) : throw new Exception("Priority is not a JsonElement");
		GatherUsings();
		GatherProperty_Field();
		bool Start = await LoadToMemory(Priority, Name, MethodType.Start);
		if (!Start) { return false; }
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
		foreach (var item in ExtensionMethods){
			int errorCalls = item.ErrorCalls;
			if (item.mT == type){
				try{
					if (item.method.GetParameters().Length > 0){
						item.method.Invoke(null, BindingFlags.Static, null, type == MethodType.Update ? [] : parameters, CultureInfo.InvariantCulture);
					}
					else { item.method.Invoke(null, BindingFlags.Static, null, [], CultureInfo.InvariantCulture); }
				}catch (Exception ex){
					if (item.ErrorCalls > (int)(MaxTries / cc)){
						MessageBox.Show(ex.Message + $"{CustomFunctions.ToString(CallStack[type])} has called too many errors({(int)(MaxTries / cc)} Exceptions) and has been removed from the Application to maintain performance.", $"Error in Extension: {CustomFunctions.ToString(CallStack[type])}", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}else{
						DialogResult dR = MessageBox.Show(ex.Message + $"Should this extension be removed? \n(It's possible that it will be removed later if it calls {(int)(MaxTries / cc)} Exceptions.)", $"Error in Extension: {CustomFunctions.ToString(CallStack[type])}", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
						if (dR == DialogResult.OK) { ExtensionMethods.RemoveAt(cc); continue; } else { errorCalls++; }
					}
				}
				ExtensionMethods[cc] = (type, errorCalls, item.method);
				cc++;
				Invoked = true;
			}
		}
		return Invoked;

	}
	/// <summary>This overload of <see cref="ExtensionHandler.LoadToMemory(int, string, string, string, MethodType)"/> uses some built-in helper methods to allow for a level of abstraction, it's workflow is similar to <see cref="ExtensionHandler.LoadToMemory(int, string, string, string, MethodType)"/>.</summary>
	/// <param name="Priority">The extension's Priority level.</param>
	/// <param name="ClassName">The name of the class that the extension will be compiled to.</param>
	/// <param name="mT">The type of the method.</param>
	/// <returns></returns>
	static async Task<bool> LoadToMemory(int Priority, string ClassName, MethodType mT){
		return await LoadToMemory(Priority, ClassName, NameFromType(mT), GetFunctionBody(NameFromType(mT), mT), mT);
	}
	static string NameFromType(MethodType methodType){
		return methodType switch{
			MethodType.Start => Start,
			MethodType.Update => Update,
			MethodType.TimedUpdate => TUpdate,
			MethodType.Closing => Closing,
			_ => throw new ArgumentException(),
		};
	}
	/// <summary>Saves a singular extension function to allow it to be runnable.</summary>
	/// <param name="Priority">The extionsion function's property.</param>
	/// <param name="ClassName">The name of the function's parent extension.</param>
	/// <param name="functionName">The name of the function.</param>
	/// <param name="FunctionBody">The actual logic of the function.</param>
	/// <param name="mT">The type of extension function it is, selected from <see cref="MethodType"/></param>
	/// <returns>Was the function processed successfully.</returns>
	/// <exception cref="Exception">Did the compilation of the function fail? then call the Exception.</exception>
	static async Task<ErrorQueue.SuccessState> LoadToMemory(int Priority, (MethodType mT, int Priority)[] Functions){
		 await Task<ErrorQueue.SuccessState>.Run(() => {
			ScriptOptions scriptOptions = ScriptOptions.Default
 				.WithReferences(typeof(MessageBox).Assembly, typeof(MessageBoxButtons).Assembly, typeof(ErrorQueue).Assembly/*, typeof(ExtensionInterface).Assembly*/)
 				.WithImports("System");
			foreach((MethodType mT, int Priority) item in Functions){
				if(CallStack[item.mT].A)
			}
		});
	}
	// 	static async Task<bool> LoadToMemory(int Priority, string ClassName, string functionName, string FunctionBody, MethodType mT, int Recalls = 0){
	// 		return await Task.Run(async () =>{
	// 			ScriptOptions scriptOptions = ScriptOptions.Default
	// 				.WithReferences(typeof(MessageBox).Assembly, typeof(MessageBoxButtons).Assembly, typeof(MessageBoxIcon).Assembly, typeof(Entry).Assembly, typeof(gameObj).Assembly)
	// 				.WithImports("System");
	// 			string returnType = mT switch{
	// 				MethodType.Start => "void ",
	// 				MethodType.Update => "void ",
	// 				MethodType.TimedUpdate => "void ",
	// 				MethodType.Closing => "void ",
	// 				_ => "void ",
	// 			};
	// 			if (CallStack[mT].Contains(Priority) && ErrorQueue.Warn){
	// 				MessageBox.Show("The extension has a name that conflicts with extensions with a similar name.\nIt will be assigned a temporary name.", "ExtensionNamingConflict", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	// 				ClassName = $"[{CallStack.Count}]thExtension";
	// 			}
	// 			FunctionBody = Usings + "partial class " + ClassName + "{ public " + returnType + FunctionBody + "}" + (mT == MethodType.Start? Properties_Fields: "");
	// 			Script<object> script = CSharpScript.Create(FunctionBody, scriptOptions);
	// 			Compilation compilation = script.GetCompilation();
	// 			using (MemoryStream ms = new()){
	// 				if (!compilation.Emit(ms).Success){
	// 					DialogResult dR;
	// 					if (Recalls < 10){
	// 						if (ErrorQueue.Warn)
	// 						{
	// 							dR = MessageBox.Show("Compilation of the Extension at:" + PathToExtension + $" failed.\nThe Extension has been re-compiled {Recalls} times", "Extenson failed to load.", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
	// 						}
	// 						else { dR = DialogResult.Ignore; }
	// 						//!(Entry.SkipStartUpWarnings && StartUp)? MessageBox.Show("Compilation of the Extension at:" + PathToExtension + $" failed.\nThe Extension has been re-compiled {Recalls} times", "Extenson failed to load.", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error)
	// 					}else{
	// 						if (ErrorQueue.Warn){
	// 							dR = MessageBox.Show("The extension at:" + PathToExtension + "failed to be compiled\nThere have been too many re-tries to compile the Extension\nCompilation has been cancelled.", "CompilationTimeoutException", MessageBoxButtons.OK, MessageBoxIcon.Error);
	// 						}
	// 						else { dR = DialogResult.OK; }
	// 						cts.Cancel();
	// 					}
	// 					if (dR == DialogResult.Retry && Recalls < 10){
	// 						return await LoadToMemory(Priority, ClassName, functionName, FunctionBody, mT, Recalls + 1);
	// 					}else if (dR == DialogResult.Abort && Recalls < 10){
	// 						cts.Cancel();
	// 						return false;
	// 					}else if (dR == DialogResult.Ignore){
	// 						return await LoadToMemory(Priority, ClassName, functionName, FunctionBody, mT, 10);
	// 					}
	// 					else if (dR == DialogResult.OK) { return false; }
	// 				}
	// 				ms.Seek(0, SeekOrigin.Begin);
	// 				Assembly assembly = Assembly.Load(ms.ToArray());
	// 				try{
	// 					Type type = assembly.GetType(ClassName) ?? throw new ArgumentNullException("");
	// 					object instance = Activator.CreateInstance(type) ?? throw new ArgumentNullException();
	// 					ExtensionMethods.Add((mT, 0, type.GetMethod(functionName) ?? throw new ArgumentNullException()));
	// #pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
	// 					int myPosition = ExtensionMethods.IndexOf((mT, 0, type.GetMethod(functionName)));
	// #pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
	// 					bool Conflict = false;
	// 					try { _ = CallStack[mT][Priority]; Conflict = false; } catch (ArgumentOutOfRangeException) { Conflict = true; }
	// 					//If there's a Priority conflict.
	// 					if (Conflict){
	// 						DialogResult dR = !(Entry.SkipStartUpWarnings && StartUp) ? MessageBox.Show(
	// 							"Should all other Priorities be shifted upwards to accomodate this extension(CONTINUE)?\nShould this extension be given the last Priority allotment(TRY)?\n\t(THIS MAY AFFECT HOW THE EXTENSION INTERACTS WITH THE APPLICATION AND IT MAY CRASH!)\n\nShould the extension not be added(CANCEL)?", $"ExtensionPriorityConflictionException: There are already an extension with Priority {Priority}",
	// 							MessageBoxButtons.CancelTryContinue, MessageBoxIcon.Warning) : Entry.DefaultPriorityConflictBehaviour;
	// 						if (dR == DialogResult.Continue){
	// 							CallStack[mT].Insert(Priority >= CallStack[mT].Count ? CallStack[mT].Count - 1 : Priority, myPosition);
	// 						}
	// 						else if (dR == DialogResult.TryAgain) { _ = CallStack[mT].Append(myPosition); } else { return false; }
	// 					}
	// 					ms.Dispose();
	// 				}catch (ArgumentNullException){
	// 					ms.Dispose();
	// 					return false;
	// 				}
	// 			}
	// 			return true;
	// 		});
	// 	}
	/// <summary>Returns the Method body of a function from a Json.</summary>
	/// <param name="functionName">The name of the function.</param>
	/// <returns>A string containing the function body</returns>
	/// <exception cref="FileFormatException">If the .json wasn't formatted correctly, then <see cref="FileFormatException"/> will be called.</exception>
	static string GetFunctionBody(string FunctionName, MethodType mT){
		bool FunctionFound = JsonDeserialised.TryGetValue(FunctionName, out var Function) == true ? true : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
		(int Start, int End) FunctionBounds = (0, 0);
		if (FunctionFound && Function is JsonElement element){
			FunctionBounds = (Convert.ToInt32(element.GetProperty("StartInFile").GetString()), Convert.ToInt32(element.GetProperty("EndInFile").GetString()));
			_ = element.TryGetProperty("RelativeFilePath", out JsonElement element_) == true ? true : throw new FileFormatException("File was not in the same Format as expected of a Json extension");
			CodeTxt = File.ReadAllText(PathToExtension + element_.GetString() ?? throw new FileFormatException("File was not in the same Format as expected of a Json extension"));
		}
		string s = CustomFunctions.ToString(new Span<char>([.. CodeTxt]).Slice(FunctionBounds.Start, FunctionBounds.End).ToArray(), false);
		if (!ModerateFunction(s)){
			(int Start, int End) fBounds = GetFunctionBounds(mT);
			s = CustomFunctions.ToString(new Span<char>([.. CodeTxt]).Slice(fBounds.Start, fBounds.End - fBounds.Start).ToArray(), false);
		}
		return s;
	}

	static bool ModerateFunction(string function){
		int Scopes = 0;
		char c = function[0];
		int Length = function.Length;
		if (!(function.Contains('{') && function.Contains('}'))) { return false; }
		for (int cc = 0; cc < Length; cc++, c = function[cc == Length ? Length - 1 : cc]){
			if (c == '{') { Scopes++; } else if (c == '}') { Scopes--; }
		}
		return Scopes == 0;
	}
	static (int, int) GetFunctionBounds(MethodType mT){
		int Scopes = 0;
		string MethodName = NameFromType(mT);
		char c = CodeTxt[0];
		(int MethodStart, int MethodEnd) FunctionBounds = (0, 0);
		int Length = CodeTxt.Length;
		for (int cc = 0; cc < Length; cc++, c = CodeTxt[cc == Length ? Length - 1 : cc]){
			if (c == '{') { Scopes++; } else if (c == '}') { Scopes--; }
			if (c == MethodName[0]){
				if (CustomFunctions.ToString(new Span<char>([.. CodeTxt]).Slice(cc, MethodName.Length).ToArray(), false) == MethodName){
					FunctionBounds.MethodStart = cc;
					StringBuilder sB = new();
					int Buffer = Scopes;
					int cc_ = cc + MethodName.Length;
					sB.Append(MethodName);
					while (CodeTxt[cc_ == CodeTxt.Length ? CodeTxt.Length - 1 : cc_] != '{'){
						sB.Append(CodeTxt[cc_]);
						cc_++;
					}
					do{
						if (cc_ == CodeTxt.Length) { break; }
						char cTemp = CodeTxt[cc_];
						if (cTemp == '{') { Buffer++; } else if (cTemp == '}') { Buffer--; }
						sB.Append(cTemp);
						cc_++;
					} while (Buffer != Scopes);
					FunctionBounds.MethodEnd = sB.Length + FunctionBounds.MethodStart;
					break;
				}
			}
		}
		return FunctionBounds;
	}
	static void AcquireUsings(){
		string using_ = "using";
		char LineSeperator = ';';
		int StartFrom = 0;
		int Length = CodeTxt.Length;
		foreach(char c in CodeTxt){
			if(c == using[0]){
				StringBuilder sB = new();
				int LineEnd = StartFrom;
				while(CodeTxt[LineEnd > Length? Length - 1: LineEnd] != LineSeperator){LineEnd++;}
				sB = sB.AppendLine(CustomFunctions.ToString(new Span<char>(CodeTxt.ToArray).Slice(StartFrom, LineEnd).ToArray()));
			}
			Usings = sB.Length == 0 | sB.Length< using_.Length + LineSeperator.Length? "": sB.ToString(); 
			StartFrom++;
		}
	}
	///<summary>Acquire all the Properties and Populate <see cref="Properties_Fields"/> with them.</summary>
	/// <remarks>UNFINISHED.</remarks>
	static void AcquireProperties(){
		///public/static/private/protected internal/protected/internal/private protected/file
		string[] AccessModifiers = ["public", "static", "private", "protected", "internal", "file"];
		string[] AdvAccessModifiers = ["private protected", "protected internal"];
		char[] InvalidPropertySyntax = ['(', ')', '{', '}', '', '', '', '', '', '', ];
		string word = CustomFunctions.IsolateWord(CodeTxt);
		int WordCounter = 0;
		StringBuilder Properties = new();
		//To store the Property temporarily to ensure that unvalidated Properties are not loaded.
		StringBuilder Temp = new();
		int TypeOrName = 0;
		for(int cc =0; cc < CodeTxt.Length; cc++, WordCounter += word.Length, word = IsolateWord(CodeTxt, WordCounter)){
			if(word.Contains)
			if(AccessModifiers.Contains(word) || AdvAccessModifiers.Contains(word + $" {CustomFunctions.IsolateWord(CodeTxt, WordCounter + word.Length)}")){
				//Is access modifier.
				Temp.Append($"{word} ");
			}else{
				//Found type or name.
				Temp.Append($"{word} ");
				TypeOrName++;
			}
			if(word == ";"){break;}
		}
	}
	static class ErrorQueue{
		public enum SuccessState{
			/// <summary>There were no problems with the execution of the calling function.</summary>
			RawSuccess = 0,
			/// <summary>Not all of the calling function's execution was problematic but not all of it successful.</summary>
			SoftSuccess = 1,
			/// <summary>No part of the calling function executed properly.</summary>
			RawFail = 2,
			/// <summary>Calling function's execution was problematic enough to call an error</summary>
			WarnableState = -1
		}
		public static bool Warn { get { return !(Entry.SkipStartUpWarnings && StartUp); } }
		static List<(MethodType mT, string Message, object[] Data)> ErrorInfo = [];
		public static void Add(MethodType mT, string Message, params object[] WarnableParameters){
			if (ErrorInfo == null) { ErrorInfo = [(mT, Message, WarnableParameters)]; } else { ErrorInfo.Add((mT, Message, WarnableParameters)); }
		}
		public static void ReadErrorInfo(){
			if (Warn){
				StringBuilder sB = new();
				foreach ((MethodType mT, string Message, object[] Data) item in ErrorInfo){
					sB.AppendLine($"Extension function: {(item.mT == MethodType.All ? "" : Enum.GetName(item.mT))} called an error:\n\t{item.Message}\nEXTRA INFO: \n\n{CustomFunctions.ToString(item.Data)}");
				}
			}
		}
	}
}
// namespace ExtensionInterface{
// 	public enum ApplicationServices{
// 		InputService = 0,
// 		EventService = 1,
// 		GrapchicsService = 2,
// 		AllServices = 3
// 	}
// 	static class Services{
// 		static Dictionary<string, Dictionary<Type, object>> DataStoreService;
// 		/// <summary>Give the Extension more memory from <see cref="Services.DataStoreService"/>, if Memory is available to store it.</summary>
// 		/// <param name="t">The calling Type</param>
// 		/// <param name="DataSize">The size of data that should be given.</param>
// 		/// <returns>Was there space to be acquired.</returns>
// 		/// <exception cref="ArgumentNullException"></exception>
// 		public static bool Poll(Type t, long DataSize){
// 			if((Entry.TotalMemUsage - Entry.PeakMemUsage) < DataSize){
// 				return false;
// 			}else{
// 				bool Exists = DataStoreService.TryGetValue(t.FullName ?? throw new ArgumentNullException(), out Dictionary<Type, object>? Store);
// 				if(!Exists | Store == null){return false;}
// 				if(Store == null){Store = [];}
// 				for(int cc =0; cc < DataSize; cc++){
// 					Store.Add(typeof(bool), 0);
// 				}
// 				return true;
// 			}
// 		}
// 	}
// }