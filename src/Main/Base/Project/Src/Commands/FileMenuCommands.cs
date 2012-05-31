// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project.Dialogs;

namespace ICSharpCode.SharpDevelop.Project.Commands
{
	public class CreateNewSolution : AbstractMenuCommand
	{
		public override void Run()
		{
			
			
			using (NewProjectDialog npdlg = new NewProjectDialog(true)) {
				npdlg.Owner = WorkbenchSingleton.MainForm;
				npdlg.ShowDialog(ICSharpCode.SharpDevelop.Gui.WorkbenchSingleton.MainForm);
			}
		}
	}
	
	public class LoadSolution : AbstractMenuCommand
	{
		public override void Run()
		{
			using (OpenFileDialog fdiag  = new OpenFileDialog()) {
				fdiag.AddExtension    = true;
				fdiag.Filter          = ProjectService.GetAllProjectsFilter(this);
				fdiag.Multiselect     = false;
				fdiag.CheckFileExists = true;
				if (fdiag.ShowDialog(ICSharpCode.SharpDevelop.Gui.WorkbenchSingleton.MainForm) == DialogResult.OK) {
					ProjectService.LoadSolutionOrProject(fdiag.FileName);
				}
			}
		}
	}
	
	public class CloseSolution : AbstractMenuCommand
	{
		public override void Run()
		{
			ProjectService.SaveSolutionPreferences();
			WorkbenchSingleton.Workbench.CloseAllViews();
			if (WorkbenchSingleton.Workbench.WorkbenchWindowCollection.Count == 0) {
				ProjectService.CloseSolution();
			}
			
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
			    if (Path.GetFileName(a.Location) == "StartPage.dll") {
			        LoggingService.Debug("CloseSolution: found StartPage.dll");
			        foreach (Type t in a.GetTypes()) {
			            if (t.Name == "ShowStartPageCommand") {
			                LoggingService.Debug("CloseSolution: found ShowStartPageCommand");
			                ConstructorInfo ci = t.GetConstructor(
			                    BindingFlags.Public | BindingFlags.Instance,
			                    null, new Type[0], null);
			                if (ci != null) {
			                    LoggingService.Debug("CloseSolution: found default constructor");
			                    var command = ci.Invoke(new object[0]) as AbstractMenuCommand;
			                    if (command != null) {
			                        LoggingService.Debug("CloseSolution: running ShowStartPageCommand");
			                        command.Run();
			                    }
			                }
			            }
			        }
			    }
			}
		}
	}
}
