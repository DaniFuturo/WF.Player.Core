///
/// WF.Player.Core - A Wherigo Player Core for different platforms.
/// Copyright (C) 2012-2013  Dirk Weltz <web@weltz-online.de>
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Lesser General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Lesser General Public License for more details.
/// 
/// You should have received a copy of the GNU Lesser General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
/// 

using System;
using System.Collections.Generic;
using NLua;

namespace WF.Player.Core
{
	public class Input : Table
	{
		#region Constructor

		public Input (Engine e, LuaTable t) : base (e, t)
		{
		}

		#endregion

		#region Properties

		public List<string> Choices {
			get {
				List<string> result = new List<string> (); 

				var s = ((LuaTable)wigTable ["Choices"]).GetEnumerator ();
				while (s.MoveNext())
					result.Add ((string)s.Value);

				return result;
			}
		}

		/// <summary>
		/// Gets the input type.
		/// </summary>
		/// <value>The input type.</value>
		public string InputType {
			get {
				return GetString ("InputType");
			}
		}

		/// <summary>
		/// Gets the image.
		/// </summary>
		/// <value>The image.</value>
		public Media Image {
			get {
				LuaTable media = (LuaTable)wigTable ["Media"];

				if (media == null)
					return null;

				return engine.GetMedia (Convert.ToInt32 ((double)media["ObjIndex"]));
			}
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return GetString ("Name");
			}
		}

		/// <summary>
		/// Gets the index of the object.
		/// </summary>
		/// <value>The index of the object.</value>
		public int ObjIndex {
			get {
				return GetInt ("ObjIndex");
			}
		}

		/// <summary>
		/// Gets the text.
		/// </summary>
		/// <value>The text.</value>
		public string Text {
			get {
				return GetString ("Text");
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="WF.Player.Core.Input"/> is visible.
		/// </summary>
		/// <value><c>true</c> if visible; otherwise, <c>false</c>.</value>
		public bool Visible {
			get {
				return GetBool ("Visible");
			}
		}

		#endregion

		#region Methods

		public void Callback(string retValue)
		{
			engine.Call (wigTable, "GetInput", new object[] { wigTable, retValue });
		}

		#endregion

	}

}

