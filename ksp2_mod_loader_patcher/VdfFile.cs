using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ksp2_mod_loader_patcher
{
	public class VdfFile
	{
		#region Parsing
		public static bool TryParse(string path, out VdfFile output)
		{
			try
			{
				using (FileStream file = File.Open(path, FileMode.Open))
				{
					output = Parse(new StreamReader(file));
				}

				return true;
			}
			catch
			{
				output = null;

				return false;
			}
		}

		public static VdfFile Parse(StreamReader filestream)
		{
			string blockName = readString(filestream);
			readWhitespace(filestream);
			Dictionary<string, object> block = readBlock(filestream);

			return new VdfFile
			{
				_name = blockName,
				_block = block
			};
		}
		static Dictionary<string,object> readBlock(StreamReader filestream)
		{
			Dictionary<string, object> blockData = new Dictionary<string, object>();

			expectCharacter(filestream, '{');
			readWhitespace(filestream);

			while (peekNext(filestream) == '\"')
			{
				string first = readString(filestream);
				readWhitespace(filestream);
				char next = peekNext(filestream);
				switch (next)
				{
					case '\"':
						string second = readString(filestream);
						blockData.Add(first, second);
						break;
					case '{':
						Dictionary<string, object> block = readBlock(filestream);
						blockData.Add(first, block);
					break;

					default: throw new Exception($"Unexpected character \"{next}\"");
				}
				readWhitespace(filestream);
			}
			expectCharacter(filestream, '}');

			return blockData;
		}
		static string readString(StreamReader filestream)
		{
			expectCharacter(filestream, '\"');

			char previousChar = '\0';
			string str = readUntil(filestream, delegate(char c)
			{
				previousChar = c;

				return previousChar == '\\' || c != '"';
			});

			string output = str.Substring(0, str.Length - 1);

			return Regex.Unescape(output);
		}
		static void readWhitespace(StreamReader filestream)
		{
			readWhile(filestream, f => char.IsWhiteSpace(f));
		}

		static string readWhile(StreamReader filestream, Func<char,bool> keepReading)
		{
			StringBuilder builder = new StringBuilder();

			while (filestream.Peek() > -1 && keepReading((char)filestream.Peek()))
			{
				int output = filestream.Read();
				if (output <= -1)
					break;

				builder.Append((char)output);
			}

			return builder.ToString();
		}
		static string readUntil(StreamReader filestream, Func<char, bool> keepReading)
		{
			StringBuilder builder = new StringBuilder();

			char nextChar = '\0';
			do
			{
				int output = filestream.Read();
				if (output <= -1)
					break;

				nextChar = (char)output;

				builder.Append(nextChar);

			} while (keepReading(nextChar));

			return builder.ToString();
		}
		static void expectCharacter(StreamReader filestream, char expectedCharacter)
		{
			char c = (char)filestream.Read();
			if (c != expectedCharacter)
				throw new Exception($"Expected \"{expectedCharacter}\" character, found {c}");
		}
		static char peekNext(StreamReader filestream)
		{
			int output = filestream.Peek();
			if (output <= -1)
				return '\0';

			return (char)output;
		}
		#endregion

		private string _name;
		private Dictionary<string, object> _block;

		public string this[string name]
		{
			get
			{
				if(_block.TryGetValue(name, out object val))
				{
					return val as string;
				}

				return null;
			}
		}

		public VdfFile GetChild(string name)
		{
			if (_block.TryGetValue(name, out object val))
			{
				Dictionary<string, object> dict = val as Dictionary<string, object>;
				if (dict == null)
					return null;

				return new VdfFile
				{
					_name = name,
					_block = dict
				};
			}

			return null;
		}

		public IEnumerable<string> GetChildNodes()
		{
			return _block.Keys;
		}
	}
}
