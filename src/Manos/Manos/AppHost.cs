//
// Copyright (C) 2010 Jackson Harper (jackson@manosdemono.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//



using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;

using Manos.IO;
using Manos.Http;
using Manos.Caching;
using Manos.Logging;

namespace Manos
{
	/// <summary>
	/// The app runner. This is where the magic happens.
	/// </summary>
	public static class AppHost
	{
		private static ManosApp app;
		private static bool started;
		
		private static int port = 8080;
		private static IPAddress ip_address = IPAddress.Parse ("0.0.0.0");
		
		private static HttpServer server;
		private static IManosCache cache;
		private static IManosLogger log;
		private static List<IManosPipe> pipes;

		private static IOLoop ioloop = IOLoop.Instance;
		
		public static ManosApp App {
			get { return app; }	
		}
		
		public static HttpServer Server {
			get { return server; }	
		}
		
		public static IManosCache Cache {
			get {
				if (cache == null)
					cache = new ManosInProcCache ();
				return cache;
			}
		}

		public static IManosLogger Log {
			get {
				if (log == null)
					log = new Manos.Logging.ManosConsoleLogger ("manos", LogLevel.Debug);
				return log;
			}
		}

		public static IOLoop IOLoop {
			get { return ioloop; }	
		}
		
		public static IList<IManosPipe> Pipes {
			get { return pipes; }
		}
		
		public static IPAddress IPAddress {
			get { return ip_address; }
			set {
				if (started)
					throw new InvalidOperationException ("IPAddress can not be changed once the server has been started.");
				if (value == null)
					throw new ArgumentNullException ("value");
				ip_address = value;
			}
		}
		
		public static int Port {
			get { return port; }
			set {
				if (started)
					throw new InvalidOperationException ("Port can not be changed once the server has been started.");
				if (port <= 0)
					throw new ArgumentOutOfRangeException ("Invalid port specified, port must be a positive integer.");
				port = value;
			}
		}
		
		public static void Start (ManosApp application)
		{
			if (application == null)
				throw new ArgumentNullException ("application");
			
			app = application;
			
			started = true;
			server = new HttpServer (ioloop);
			server.Transaction += HandleTransaction;
			
			server.Listen (IPAddress.ToString (), port);

			ioloop.Start ();
		}
		
		public static void Stop ()
		{
			ioloop.Stop ();
		}
		
		public static void HandleTransaction (IHttpTransaction con)
		{
			app.HandleTransaction (app, con);
		}

		public static void AddPipe (IManosPipe pipe)
		{
			if (pipes == null)
				pipes = new List<IManosPipe> ();
			pipes.Add (pipe);
		}

		public static Timeout AddTimeout (TimeSpan timespan, IRepeatBehavior repeat, object data, TimeoutCallback callback)
		{
			return AddTimeout (timespan, timespan, repeat, data, callback);
		}

		public static Timeout AddTimeout (TimeSpan begin, TimeSpan timespan, IRepeatBehavior repeat, object data, TimeoutCallback callback)
		{
			Timeout t = new Timeout (begin, timespan, repeat, data, callback);
			
			ioloop.AddTimeout (t);

			return t;
		}
		
		public static void RunTimeout (Timeout t)
		{
			t.Run (app);	
		}
	}
}

