///
/// WF.Player.Core - A Wherigo Player Core for different platforms.
/// Copyright (C) 2012-2013  Dirk Weltz <web@weltz-online.de>
/// Copyright (C) 2012-2013  Brice Clocher <contact@cybisoft.net>
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
using WF.Player.Core.Engines;
using WF.Player.Core.Lua;

namespace WF.Player.Core
{
	/// <summary>
	/// An item of the game: a Thing that can be opened.
	/// </summary>
	public class Item : Thing
	{

		#region Constructor

		internal Item (Engine e, LuaTable t) : base (e, t)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether this <see cref="WF.Player.Core.Item"/> is locked.
		/// </summary>
		/// <value><c>true</c> if locked; otherwise, <c>false</c>.</value>
		public bool Locked {
			get 
			{
				return GetBool ("Locked");
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="WF.Player.Core.Item"/> is opened.
		/// </summary>
		/// <value><c>true</c> if opened; otherwise, <c>false</c>.</value>
		public bool Opened {
			get 
			{
				return GetBool ("Opened");
			}
		}

		#endregion

	}

}
