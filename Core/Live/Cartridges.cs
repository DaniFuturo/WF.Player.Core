
using Newtonsoft.Json;
using PCLStorage;
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using WF.Player.Core.Formats;

namespace WF.Player.Core.Live
{
    public class Cartridges : ObservableCollection<Cartridge>
    {
		// TODO: Localize statusMessage?
		private const string apiEndpoint = "http://foundation.rangerfox.com/API/APIv1JSON.svc";
		private string userToken = "";
        private int status = 0;
        private string statusMessage = "";

        #region Constructor

		public Cartridges() : base ()
		{
		}

		public Cartridges(string token) : base ()
        {
            userToken = token;
        }

        #endregion

		#region Properties

		public string Token {
			get {
				return userToken;
			}
			set {
				if (userToken != value)
					userToken = value;
			}
		}

		#endregion

        #region Methods

        /// <summary>
        /// Fill cartridge list by a list of filenames. All filenames in the list contains a valid path and filename.
        /// </summary>
        /// <param name="dir">Path to directory to get cartridges from.</param>
        public async void GetByFileList(List<string> files)
        {
            if (files == null)
            {
                statusMessage = "No files found";

				throw new ArgumentNullException("files");
            }

            // Delete old ones
			Clear();

            foreach (string fileName in files)
            {
                var checkFileExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(fileName);

                if (checkFileExists == ExistenceCheckResult.FileExists)
                {
                    Cartridge cart = new Cartridge(fileName);

                    var file = await FileSystem.Current.LocalStorage.GetFileAsync(fileName);
                    var fileStream = await file.OpenAsync(FileAccess.Read);

					CartridgeLoaders.LoadMetadata(fileStream, cart);

                    Add(cart);
                }
            }
        }

        /// <summary>
        /// Get cartridge list by a coordinate and a radius.
        /// </summary>
        /// <param name="lat">Latitude</param>
        /// <param name="lon">Longitude</param>
        /// <param name="radius">Radius in km</param>
        public void BeginGetByCoords(double lat, double lon, double radius)
        {
			if (!checkConnection())
				throw new InvalidOperationException("Invalid connection to the API.");

			Live.SearchCartridgesRequest search = new Live.SearchCartridgesRequest()
			{
				PageNumber = 1,
				ResultsPerPage = 50,
				SearchArguments = new Live.CartridgeSearchArguments()
				{
					Latitude = lat,
					Longitude = lon,
					SearchRadiusInKm = radius
				}
			};

            beginGetCartridges(search);
        }

        /// <summary>
        /// Get cartridge list by a part of the name.
        /// </summary>
        /// <param name="name">Part of the name</param>
        public void BeginGetByName(string name)
        {
            if (!checkConnection())
				throw new InvalidOperationException("Invalid connection to the API.");

			Live.SearchCartridgesRequest search = new Live.SearchCartridgesRequest()
			{
				PageNumber = 1,
				ResultsPerPage = 50,
				SearchArguments = new Live.CartridgeSearchArguments()
				{
					CartridgeName = name.Trim(),
					OrderSearchBy = Live.CartridgeSearchArguments.OrderBy.Distance
				}
			};

            beginGetCartridges(search);
        }

        /// <summary>
        /// Get cartridge list with only play anywhere cartridges. Sort list by date.
        /// </summary>
        public void BeginGetByPlayAnywhere()
        {
            if (!checkConnection())
				throw new InvalidOperationException("Invalid connection to the API.");

			Live.SearchCartridgesRequest search = new Live.SearchCartridgesRequest()
			{
				PageNumber = 1,
				ResultsPerPage = 50,
				SearchArguments = new Live.CartridgeSearchArguments()
				{
					OrderSearchBy = Live.CartridgeSearchArguments.OrderBy.PublishDate,
					IsPlayAnywhere = true
				}
			};

            beginGetCartridges(search);
        }

		public void DownloadCartridge (Cartridge cart, string path, Stream output)
		{
			Live.DownloadCartridgeRequest downloadRequest = new Live.DownloadCartridgeRequest ();

			downloadRequest.WGCode = cart.WGCode;
			downloadRequest.UserToken = userToken;

			string result = callAPI ("DownloadCartridge",downloadRequest);

			Live.DownloadCartridgeResponse resp = JsonConvert.DeserializeObject<Live.DownloadCartridgeResponse> (result);

			if (resp != null && resp.CartridgeBytes != null)
			{
				cart.Filename = Path.Combine (path, cart.WGCode+".gwc");
				output.Write(resp.CartridgeBytes, 0, resp.CartridgeBytes.Length);
				output.Flush();
			}
		}

        #endregion

        #region Private Functions

 		private string callAPI (string name, object obj)
		{
			string uri = apiEndpoint + "/" + name;
			var jsonRequest = new Dictionary<string, object>{
				{"req", obj}
			};
			string json = JsonConvert.SerializeObject(jsonRequest);
			using (PortableWebClient client = new PortableWebClient()) {
				client.ContentType = "application/json; charset=utf-8";
				return client.UploadString(uri, json);
			}
		}

        /// <summary>
        /// Check, if the connection is ok.
        /// </summary>
        /// <returns>True, if the connections is ok, else false.</returns>
        private bool checkConnection()
        {
            if (String.Empty.Equals(userToken))
            {
                statusMessage = "Missing token";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Search cartridges via web client.
        /// </summary>
        /// <param name="search">Search parameters</param>
        private void beginGetCartridges(Live.SearchCartridgesRequest search)
        {
			// Starts the asynchronous search operation.
			// The result happens in the event handler onSearchCartridgesCompleted.
			if (!String.IsNullOrEmpty (userToken))
				search.UserToken = userToken;
			string result = callAPI("SearchCartridges", search);

			Live.SearchCartridgesResponse resp = JsonConvert.DeserializeObject<Live.SearchCartridgesResponse>(result);

			status = resp.Status.StatusCode;

			if (status == 0)
			{
				if (resp.Cartridges != null)
				{
					// Delete old ones
					Clear();

					// Get new ones
					foreach (Live.CartridgeSearchResult res in resp.Cartridges)
					{
						Cartridge cart = new Cartridge();

						cart.Name = res.Name;
						cart.AuthorName = res.AuthorName;
						cart.AuthorCompany = res.AuthorCompany;
						cart.DateAdded = res.DateAdded;
						cart.DateLastPlayed = res.DateLastPlayed;
						cart.DateLastUpdated = res.DateLastUpdated;
						cart.CompletionTime = res.CompletionTime;
						cart.ActivityType = res.ActivityType;
						cart.CountryID = res.CountryID;
						cart.StateID = res.StateID;
						cart.IsArchived = res.IsArchived;
						cart.IsDisabled = res.IsDisabled;
						cart.IsOpenSource = res.IsOpenSource;
						cart.IsPlayAnywhere = res.IsPlayAnywhere;
						cart.IconFileURL = res.IconFileURL;
						cart.PosterFileURL = res.PosterFileURL;
						cart.StartingLocationLatitude = res.Latitude;
						cart.StartingLocationLongitude = res.Longitude;
						cart.LinkedGeocacheNames = res.LinkedGeocacheNames;
						cart.LinkedGeocacheGCs = res.LinkedGeocacheGCs;
						cart.LinkedGeocacheGUIDs = res.LinkedGeocacheGuids;
						cart.LongDescription = res.LongDescription;
						cart.ShortDescription = res.ShortDescription;
						cart.NumberOfCompletions = res.NumberOfCompletions;
						cart.NumberOfUsersWatching = res.NumberOfUsersWatching;
						cart.UniqueDownloads = res.UniqueDownloads;
						cart.Complete = res.UserHasCompleted;
						cart.UserHasPartiallyPlayed = res.UserHasPartiallyPlayed;
						cart.WGCode = res.WGCode;

						Add(cart);
					}
				}
			}
			else
			{
				statusMessage = resp.Status.StatusMessage;
			}
		}

        #endregion

    }

    #region WebClient replacement

    /// <summary>
    /// Found at https://github.com/jhskailand/PortableWebClient/blob/master/PortableWebClient.cs
    /// </summary>
    public class PortableWebClient: IDisposable
    {
        public PortableWebClient()
        {
            StatusCode = HttpStatusCode.Unused;
            StatusDescription = string.Empty;
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Accept { get; set; }

        public string ContentType { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string StatusDescription { get; set; }

        public string DownloadString(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (this.Username != null && this.Password != null)
            {
                request.Credentials = new NetworkCredential(this.Username, this.Password);
            }
            request.Accept = this.Accept;
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)GetResponse(request);
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                this.StatusCode = response.StatusCode;
                this.StatusDescription = response.StatusDescription;
                string data = reader.ReadToEnd();
                return data;
            }
        }

        public string UploadString(string url, string body)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (this.Username != null && this.Password != null)
            {
                request.Credentials = new NetworkCredential(this.Username, this.Password);
            }
            request.Accept = this.Accept;
            request.Method = "POST";
            request.ContentType = this.ContentType;
            using (Stream writer = (Stream)GetRequestStream(request))
            {
                byte[] data = Encoding.UTF8.GetBytes(body);
                writer.Write(data, 0, data.Length);
            }
            HttpWebResponse response = (HttpWebResponse)GetResponse(request);
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                this.StatusCode = response.StatusCode;
                this.StatusDescription = response.StatusDescription;
                string data = reader.ReadToEnd();
                return data;
            }
        }

        private WebResponse GetResponse(WebRequest request)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            IAsyncResult asyncResult = request.BeginGetResponse(r => autoResetEvent.Set(), null);
            autoResetEvent.WaitOne();
            return request.EndGetResponse(asyncResult);
        }

        private Stream GetRequestStream(WebRequest request)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            IAsyncResult asyncResult = request.BeginGetRequestStream(r => autoResetEvent.Set(), null);
            autoResetEvent.WaitOne();
            return request.EndGetRequestStream(asyncResult);
        }

        public void Dispose()
        {
        }
    }

    #endregion
}
