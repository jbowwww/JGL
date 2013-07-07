using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Gtk;
using JGL.Debugging;
using Mono.CSharp;

namespace Dynamic.UI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DynamicCodePage : VPaned
	{
		public AutoTraceSource Trace { get { return CodeWindow.Trace; } }

		private int codeHistoryIndex = -1;

		private Project Project;

		private CodeWindow CodeWindow;

		/// <summary>
		/// The code completion window.
		/// </summary>
//		CodeCompletionWindow CompletionWindow;

		/// <summary>
		/// Source code input <see cref="Gtk.TextView"/>
		/// </summary>
		TextView tvInput;

		/// <summary>
		/// Event handlers set this to true on key press event for control key, returns to false on key release event
		/// </summary>
		bool tvInputCtrlKeyPressed = false;

		/// <summary>
		/// The tv input previous text.
		/// </summary>
		private string tvInputPreviousText = string.Empty;

		/// <summary>
		/// Code execution output <see cref="Gtk.TextView"/>
		/// </summary>
		TextView tvOutput;

		/// <summary>
		/// The lock output.
		/// </summary>
		private ReaderWriterLock lockOutput = new ReaderWriterLock();

		private static  bool initEvaluator = false;

		StringBuilder sbOut = new StringBuilder();
		StringBuilder sbError = new StringBuilder();
		StringBuilder sbEval = new StringBuilder();

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCodeTests.DynamicCodePage"/> class.
		/// </summary>
		public DynamicCodePage(Project project, CodeWindow codeWindow)
		{
			Project = project;
			CodeWindow = codeWindow;

			lockOutput.AcquireWriterLock(0);

			int mark = 0;

			ScrolledWindow w = new ScrolledWindow();
			tvInput = new TextView();
			w.AddWithViewport(tvInput);
			Pack2(w, false, true);

			w = new ScrolledWindow();
			tvOutput = new TextView();
			tvOutput.Editable = false;
			w.AddWithViewport(tvOutput);
			Pack1(w, true, true);

			Position = 234;
			PositionSet = true;

			tvInput.GrabFocus();

			List<string> codeHistory = project.CodeHistory;
			
			tvInput.KeyReleaseEvent += delegate(object o, KeyReleaseEventArgs args)
			{
				if (args.Event.State.HasFlag(Gdk.ModifierType.ControlMask))
				{
					if (args.Event.Key.Equals(Gdk.Key.space))
					{
						ShowCompletions(tvInput.Buffer.Text);
						// tvInput.Buffer.GetSlice(tvInput.Buffer.StartIter, tvInput.Buffer.EndIter, true);
					}
					else if (args.Event.Key.Equals(Gdk.Key.Return))
					{
						TextWriter consoleOutput = null, consoleError = null;
						try
						{
							string code = tvInputPreviousText.Trim('\n', '\r', ' ');
							if (code != string.Empty)
							{
								codeHistoryIndex = -1;
								if (codeHistory.Count == 0 || codeHistory[codeHistory.Count - 1] != code)
									codeHistory.Add(code);

								sbOut.Clear();
								StringWriter sw = new StringWriter(sbOut);
								Evaluator.MessageOutput = sw;
								consoleOutput = Console.Out;
								Console.SetOut(sw);
								consoleError = Console.Error;
								Console.SetError(sw);

								tvInput.Buffer.Text = string.Empty;
								tvOutput.Buffer.InsertAtCursor("> " + code.Replace("\n", "\n> ") + "\n");
								TextMark tm = tvOutput.Buffer.CreateMark(string.Format("mark-code{0}-start", mark), tvOutput.Buffer.EndIter, true);

								tvOutput.Buffer.InsertAtCursor(ExecuteCode(code));

								TextMark tm2 = tvOutput.Buffer.CreateMark(string.Format("mark-code{0}-end)", mark), tvOutput.Buffer.EndIter, true);
								TextTag tt = new TextTag(string.Format("code{0}", mark));
								tt.Foreground = "#4262EE";
								tvOutput.Buffer.TagTable.Add(tt);
								tvOutput.Buffer.ApplyTag(tt, tvOutput.Buffer.GetIterAtMark(tm), tvOutput.Buffer.GetIterAtMark(tm2));
								Console.Out.Flush();
								Console.Error.Flush();
								Evaluator.MessageOutput.Flush();
								if (sbOut.Length > 0)
								{
									string output = sbOut.ToString(); // + "\n";
									foreach (string splitOutput in output.Split(new char[] { (char)27 }, StringSplitOptions.RemoveEmptyEntries))
									{
										if (splitOutput[0] == '[')
										{
											int colorcode = int.Parse(splitOutput.Substring(1, splitOutput.IndexOf('m') - 1));
											string splitSub = splitOutput.Substring(splitOutput.IndexOf('m') + 1);
											switch (colorcode)
											{
												case 31:
													tm = tvOutput.Buffer.CreateMark(string.Format("mark-console{0}-start", mark), tvOutput.Buffer.EndIter, true);
													tvOutput.Buffer.InsertAtCursor(splitSub);
													consoleOutput.Write(splitSub);
													tm2 = tvOutput.Buffer.CreateMark(string.Format("mark-console{0}-end", mark), tvOutput.Buffer.EndIter, false);
													TextTag tt2 = new TextTag(string.Format("console{0}-error", mark));
													tt2.Foreground = "#F83333";
													tvOutput.Buffer.TagTable.Add(tt2);
													tvOutput.Buffer.ApplyTag(tt2, tvOutput.Buffer.GetIterAtMark(tm), tvOutput.Buffer.GetIterAtMark(tm2));
													break;
												case 0:
													tm = tvOutput.Buffer.CreateMark(string.Format("mark-console{0}-start", mark), tvOutput.Buffer.EndIter, true);
													tvOutput.Buffer.InsertAtCursor(splitSub);
													consoleOutput.Write(splitSub);
													tm2 = tvOutput.Buffer.CreateMark(string.Format("mark-console{0}-end", mark), tvOutput.Buffer.EndIter, false);
													tt2 = new TextTag(string.Format("console{0}-out", mark));
													tt2.Foreground = "#575757";
													tvOutput.Buffer.TagTable.Add(tt2);
													tvOutput.Buffer.ApplyTag(tt2, tvOutput.Buffer.GetIterAtMark(tm), tvOutput.Buffer.GetIterAtMark(tm2));
													break;
												default:
													throw new InvalidDataException(string.Format("Invalid color code on output string split \"{0}\"", splitOutput));
													break;
											}
										}
										else
										{
											tm = tvOutput.Buffer.CreateMark(string.Format("mark-console{0}-start", mark), tvOutput.Buffer.EndIter, true);
											tvOutput.Buffer.InsertAtCursor(splitOutput);
											consoleOutput.Write(splitOutput);
											tm2 = tvOutput.Buffer.CreateMark(string.Format("mark-console{0}-end", mark), tvOutput.Buffer.EndIter, false);
											TextTag tt2 = new TextTag(string.Format("console{0}", mark));
											tt2.Foreground = "#575757";
											tvOutput.Buffer.TagTable.Add(tt2);
											tvOutput.Buffer.ApplyTag(tt2, tvOutput.Buffer.GetIterAtMark(tm), tvOutput.Buffer.GetIterAtMark(tm2));
										}
										mark++;
									}
								}
								mark++;
							}
						}
						catch (Exception ex)
						{
							Trace.Log(TraceEventType.Error, string.Format("{0}: {1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace));
						}
						finally
						{
							Console.SetOut(consoleOutput);
							Console.SetError(consoleError);
							CodeWindow.PopulateHeirarchy();
						}
					}
					else if (codeHistory.Count > 0)
					{
						if (args.Event.Key.Equals(Gdk.Key.Up))
						{
							codeHistoryIndex = codeHistoryIndex <= 0 ? codeHistory.Count - 1 : codeHistoryIndex - 1;
							tvInput.Buffer.Text = codeHistory[codeHistoryIndex];
						}
						else if (args.Event.Key.Equals(Gdk.Key.Down))
						{
							codeHistoryIndex = codeHistoryIndex >= codeHistory.Count - 1 ? 0 : codeHistoryIndex + 1;
							tvInput.Buffer.Text = codeHistory[codeHistoryIndex];
						}
					}
				}
				else
				{
					codeHistoryIndex = -1;
				}

				tvInputPreviousText = tvInput.Buffer.Text;
			};

			lockOutput.ReleaseWriterLock();

			ShowAll();
		}

		/// <summary>
		/// Shows the completions.
		/// </summary>

		public void ShowCompletions(string partialWord)
		{
			string prefix;
			string[] completions = Evaluator.GetCompletions(partialWord, out prefix);
			string usings = Evaluator.GetUsing();
			int l = 0;
		}

		/// <summary>
		/// Executes the given code.
		/// </summary>
		/// <param name='source'>Source code to execute</param>
		/// <remarks>
		/// TODO: Even before you try running code using the same way as Dynamic.Compiler static class does
		/// (compiling assemblies manually instead of Evaluator static class), move this method into Dynamic.Compiler,
		/// as it only compiles and runs code, it does not depend on or affect any instance members of this class
		///  (and also remove the hardwired Evaluator.LoadAssembly calls, the project reference assembly loading, and
		/// the hardwired prefix code for setting up variables , and replace them with parameters)
		/// </remarks>
		string ExecuteCode(string code)
		{
			if (!initEvaluator)
			{
				initEvaluator = true;
				string usings = Evaluator.GetUsing();

				Evaluator.Init(new string[] { });
				usings = Evaluator.GetUsing();

				Evaluator.LoadAssembly("DynamicCodeTests-Scene.dll");
				foreach (string referencePath in Project.ReferencePaths)
				{
					string filename = System.IO.Path.GetFileName(referencePath).ToLower();
					if (filename != "system.dll")	// && filename != "opentk.dll" && filename != "jgl.dll")
						Evaluator.LoadAssembly(referencePath);
				}
			}
			Evaluator.Run(Project.CodeUsings);

			string r = string.Empty;
			try
			{
				string prefix = "Scene[] Scenes = new Scene[EntityContext.RootContext.Count]; EntityContext.RootContext.CopyTo(Scenes, 0); EntityContext RootContext = EntityContext.RootContext; EntityContext CurrentContext = EntityContext.CurrentContext;";
				string source = prefix + code;
				if (code.Contains(";"))
				{
//					r += Evaluator.Run(prefix + code) ? "OK." : "Error!";
					if (!Evaluator.Run(source))
						throw new InvalidProgramException(string.Format("Evaluator.Run() == false (source = \"{0}\")", source.Replace("\n", " ")));
				}
				else
				{
					object result;
					bool result_set;
					string input;
					code += ";";
					if (!Evaluator.Run(prefix))
						throw new InvalidProgramException(string.Format("Evaluator.Run() == false (prefix = \"{0}\")", prefix.Replace("\n", " ")));
					input = Evaluator.Evaluate(code, out result, out result_set);
					if (input != null)
						throw new InvalidProgramException(string.Format("Evaluator.Evaluate() != null (code = \"{0}\") = \"{1}\"", code.Replace("\n", " "), input.Replace("\n", " ")));
					if (result_set)
						r += result != null ? result.ToString() : "(null)";
				}
			}
			catch (Exception ex)
			{
				//r += (char)27 + "[31m" + ex.ToString();		// ex.ToString() seems to include stacktrace anyway - + "Stacktrace:\n" + ex.StackTrace;
				Console.Error.WriteLine((char)27 + "[31m" + ex.ToString());
			}
			return r.Length > 0 ? string.Concat(r, "\n") : string.Empty;
		}
	}
}
