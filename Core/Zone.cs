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
using NLua;
using WF.Player.Core.Engines;

namespace WF.Player.Core
{
	/// <summary>
	/// A Thing that has a location and can contain other Things.
	/// </summary>
	public class Zone : Thing
	{

		#region Constructor

		internal Zone (Engine e, LuaTable t) : base (e, t)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether this <see cref="WF.Player.Core.Zone"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool Active {
			get 
			{
				return GetBool ("Active");
			}
		}

		/// <summary>
		/// Gets the original point.
		/// </summary>
		/// <value>The original point as ZonePoint.</value>
		public ZonePoint OriginalPoint {
			get 
			{
				return GetTable("OriginalPoint") as ZonePoint;
			}
		}

		/// <summary>
		/// Gets the point defining the zone.
		/// </summary>
		/// <value>The inventory.</value>
		public List<ZonePoint> Points {
			get 
			{
				return GetTableList<ZonePoint>("Points");
			}
		}

		/// <summary>
		/// Gets the position state of the player for this zone.
		/// </summary>
		public PlayerZoneState State
		{
			get
			{
				return GetEnum<PlayerZoneState>("State");
			}
		}

		#endregion

	}

}