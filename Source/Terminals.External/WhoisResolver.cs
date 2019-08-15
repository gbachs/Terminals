/*
	Copyright � 2002, The KPD-Team
	All rights reserved.
	http://www.mentalis.org/

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions
  are met:

	- Redistributions of source code must retain the above copyright
	   notice, this list of conditions and the following disclaimer. 

	- Neither the name of the KPD-Team, nor the names of its contributors
	   may be used to endorse or promote products derived from this
	   software without specific prior written permission. 

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
  FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
  THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
  STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
  OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Terminals.Network.WhoIs
{
	/// <summary>
	/// Queries the appropriate whois server for a given domain name and returns the results.
	/// </summary>
	public static class WhoisResolver
	{
		public static string Whois(string domain, string host)
		{
			if (domain == null)
				return "No Domain Specified";

			var ret = string.Empty;
			Socket s = null;
			try
			{
				s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				//s.Connect(new IPEndPoint(Dns.Resolve(host).AddressList[0], 43));
				s.Connect(new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], 43));
				s.Send(Encoding.ASCII.GetBytes(domain + Environment.NewLine));

				var buffer = new byte[1024];
				var recv = s.Receive(buffer);
				while (recv > 0)
				{
					ret += Encoding.ASCII.GetString(buffer, 0, recv);
					recv = s.Receive(buffer);
				}

				s.Shutdown(SocketShutdown.Both);
			}
			catch(Exception e)
			{
				ret = "Could not connect to WhoIs Server.  Please try again later.\r\n\r\nDetails:\r\n" + e;
				//Logging.Error(ret, e);
			}
			finally
			{
				s?.Close();
			}

			return ret;
		}

		/// <summary>
		/// Queries an appropriate whois server for the given domain name.
		/// </summary>
		/// <param name="domain">The domain name to retrieve the information of.</param>
		/// <returns>A string that contains the whois information of the specified domain name.</returns>
		/// <exception cref="ArgumentNullException"><c>domain</c> is null.</exception>
		/// <exception cref="ArgumentException"><c>domain</c> is invalid.</exception>
		/// <exception cref="SocketException">A network error occured.</exception>
		public static string Whois(string domain)
		{
			var ccStart = domain.LastIndexOf(".");
			if (ccStart < 0 || ccStart == domain.Length)
				throw new ArgumentException();

			var cc = domain.Substring(ccStart + 1);
			var host = (cc + ".whois-servers.net");
			return Whois(domain, host);
		}
	}
}