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
using NLua;
using WF.Player.Core.Engines;

namespace WF.Player.Core
{
	/// <summary>
	/// A defining point of a Zone.
	/// </summary>
	public class ZonePoint : Table
	{

		#region Constructor

		internal ZonePoint (Engine e, LuaTable t) : base (e, t)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the altitude.
		/// </summary>
		/// <value>The altitude.</value>
		public double Altitude {
			get {
				return GetDouble ("altitude");
			}
		}

		/// <summary>
		/// Gets the latitude.
		/// </summary>
		/// <value>The latitude.</value>
		public double Latitude {
			get {
				return GetDouble ("latitude");
			}
		}

		/// <summary>
		/// Gets the longitude.
		/// </summary>
		/// <value>The longitude.</value>
		public double Longitude {
			get {
				return GetDouble ("longitude");
			}
		}

		#endregion

	}

}
