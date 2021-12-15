using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;

using Ufex.API;
using Ufex.API.Tables;

namespace Ufex.FileTypes.Config
{
	struct IniSection
	{
		public string name;
		public List<IniEntry> entries;
	}

	struct IniEntry
	{
		public string keyName;
		public string keyValue;
	}

	/// <summary>
	/// INI File Type module for Ufex
	/// </summary>
	public class INI : FileType
	{
		ArrayList sections;

		public INI()
		{
			Log.SetLogName("INI.log");
			ShowGraphic = false;
			ShowTechnical = true;
			ShowFileCheck = false;
			sections = new ArrayList();
		}

		public override bool ProcessFile()
		{
			StreamReader sr = new StreamReader(m_FileStream);
			
			bool done = false;		
			// Read until the end is reached
			while(!done)
			{
				string nextLine = sr.ReadLine();

				if(nextLine != null)
				{
					if(nextLine.StartsWith("["))
					{
						int sectionNameEnd = nextLine.IndexOf("]");
						
						if(sectionNameEnd == -1) return false;
						
						// Create a new section
						IniSection newSection = new IniSection();
						
						// Set the section name
						newSection.name = nextLine.Substring(1, sectionNameEnd - 1);
						
						newSection.entries = new List<IniEntry>();

						// Read the section entries
						ReadSection(sr, newSection);
						
						// Add the section to the tree
						TreeNodes.Add(new TreeNode(newSection.name));
						sections.Add(newSection);
					}
					else if(nextLine.StartsWith(";"))
					{
						// Skip Comments
					}
				}
				else
				{
					done = true;
				}

			}
			return true;
		}

		private void ReadSection(StreamReader sr, IniSection section)
		{
			while(sr.Peek() != '[')
			{
				string nextEntry = sr.ReadLine();
				
				// Check for the eof
				if(nextEntry == null)
					return;

				// Find the equal sign in the string
				int splitIndex = nextEntry.IndexOf("=");
				
				// The new entry
				IniEntry newEntry = new IniEntry();

				if(splitIndex != -1)
				{
					// Read the key
					newEntry.keyName = nextEntry.Substring(0, splitIndex);

					// Read the value
					newEntry.keyValue = nextEntry.Substring(splitIndex + 1, nextEntry.Length - splitIndex - 1);
					
					// Add the new entry
					section.entries.Add(newEntry);
				}

				// add the entry if no equals was found???
				
				// See if next line is a comment
				while(sr.Peek() == ';')
				{
					sr.ReadLine();
				}
			}
			return;
		}

		public override TableData GetData(TreeNode tn)
		{
			TextTableData td = new TextTableData(2);
			td.SetColumn(0, "Key", 100);
			td.SetColumn(1, "Value", 200);

			int tnIndex = tn.Index;

			IniSection selSection = (IniSection)sections[tnIndex];

			for(int i = 0; i < selSection.entries.Count; i++)
			{
				IniEntry nextEntry = (IniEntry)selSection.entries[i];
				td.AddRow(nextEntry.keyName, nextEntry.keyValue);
			}

			return td;
		}
	}
}
